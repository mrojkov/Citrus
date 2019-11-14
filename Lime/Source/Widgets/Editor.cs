using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;

using static Lime.WordUtils;

namespace Lime
{
	/// <summary>
	/// Editor behaviour implemented over the given text display widget.
	/// </summary>
	public class Editor
	{
		public readonly Widget DisplayWidget;
		public readonly Widget FocusableWidget;
		public readonly Widget ClickableWidget;
		public readonly IText Text;
		public readonly IEditorParams EditorParams;
		public SecureString Password;
		public bool OverwriteMode { get; private set; }
		public bool ProcessInput { get; set; } = true;
		public bool Enabled => DisplayWidget.GloballyEnabled &&
		                       FocusableWidget.GloballyEnabled &&
		                       ClickableWidget.GloballyEnabled;

		public ICaretPosition CaretPos { get; } = new CaretPosition();
		public ICaretPosition SelectionStart { get; } = new CaretPosition();
		public ICaretPosition SelectionEnd { get; } = new CaretPosition();

		public struct UndoItem : IEquatable<UndoItem>
		{
			public int TextPos;
			public string Value;
			public bool Equals(UndoItem other) =>
				TextPos == other.TextPos && Value == other.Value;
		}

		public UndoHistory<UndoItem> History = new UndoHistory<UndoItem>();

		public Editor(Widget displayWidget, IEditorParams editorParams, Widget focusableWidget = null, Widget clickableWidget = null)
		{
			DisplayWidget = displayWidget;
			FocusableWidget = focusableWidget ?? displayWidget;
			ClickableWidget = clickableWidget ?? displayWidget;
			DisplayWidget.HitTestTarget = true;
			Text = (IText)displayWidget;
			Text.TrimWhitespaces = false;
			Text.Localizable = false;
			if (editorParams is EditorParams ep) {
				displayWidget.Components.Remove(ep.GetType());
				displayWidget.Components.Add(ep);
			}
			EditorParams = editorParams;
			History.MaxDepth = EditorParams.MaxUndoDepth;

			var mc = new MultiCaretPosition();
			mc.Add(CaretPos);
			mc.Add(SelectionStart);
			mc.Add(SelectionEnd);
			Text.Caret = mc;

			if (EditorParams.PasswordChar.HasValue) {
				Text.TextProcessor += ProcessHiddenPassword;
				if (EditorParams.PasswordLastCharShowTime > 0)
					displayWidget.Tasks.Add(TrackLastCharInput, this);
			}
			else if (EditorParams.UseSecureString) {
				Text.TextProcessor += ProcessUnsecuredPassword;
			}
			if (EditorParams.UseSecureString)
				Password = new SecureString();

			displayWidget.Tasks.Add(HandleInputTask(), this);
		}

		public static class Cmds {
#if MAC
			private const Modifiers WordModifier = Modifiers.Alt;
#else
			private const Modifiers WordModifier = Modifiers.Control;
#endif
			private const Modifiers SelectModifier = Modifiers.Shift;

			public static ICommand MoveCharPrev = new Command(Key.Left);
			public static ICommand MoveCharNext = new Command(Key.Right);

			public static ICommand MoveWordPrev = new Command(WordModifier, Key.Left);
			public static ICommand MoveWordNext = new Command(WordModifier, Key.Right);

			public static ICommand MoveLinePrev = new Command(Key.Up);
			public static ICommand MoveLineNext = new Command(Key.Down);

			public static ICommand MoveLineStart = new Command(Key.Home);
			public static ICommand MoveLineEnd = new Command(Key.End);

			public static ICommand SelectCharPrev = new Command(SelectModifier, Key.Left);
			public static ICommand SelectCharNext = new Command(SelectModifier, Key.Right);

			public static ICommand SelectWordPrev = new Command(SelectModifier | WordModifier, Key.Left);
			public static ICommand SelectWordNext = new Command(SelectModifier | WordModifier, Key.Right);

			public static ICommand SelectLineStart = new Command(SelectModifier, Key.Home);
			public static ICommand SelectLineEnd = new Command(SelectModifier, Key.End);

			public static ICommand SelectCurrentWord = new Command(Modifiers.Command | Modifiers.Shift, Key.W);

			public static ICommand DeleteWordPrev = new Command(Modifiers.Control, Key.BackSpace);
			public static ICommand DeleteWordNext = new Command(Modifiers.Control, Key.Delete);

			public static ICommand Submit = new Command(Key.Enter);
			public static ICommand Cancel = new Command(Key.Escape);

			public static ICommand BackSpace = new Command(Key.BackSpace);
			public static ICommand ToggleOverwrite = new Command(Key.Insert);

			public static ICommand ContextMenu = new Command(Key.Menu);
		}

		bool IsTextReadable => !EditorParams.UseSecureString && !EditorParams.PasswordChar.HasValue;
		private int TextLength => EditorParams.UseSecureString ? Password.Length : Text.Text.Length;

		private struct LastChar
		{
			public float ShowTimeLeft;
			public char Value;
			public int Pos;
			public bool Visible => ShowTimeLeft > 0;
		}
		private LastChar lastChar;

		private string PasswordChars(int length) => new string(EditorParams.PasswordChar.Value, length);

		// This totally defeats the point of using SecureString.
		private static string Unsecure(SecureString s)
		{
			if (s.Length == 0)
				return "";
			var bstr = Marshal.SecureStringToBSTR(s);
			try {
				return Marshal.PtrToStringBSTR(bstr);
			} finally {
				Marshal.ZeroFreeBSTR(bstr);
			}
		}

		private void ProcessHiddenPassword(ref string text, Widget w)
		{
			text = lastChar.Visible ?
				PasswordChars(lastChar.Pos) + lastChar.Value + PasswordChars(TextLength - lastChar.Pos - 1) :
				PasswordChars(TextLength);
		}

		private void ProcessUnsecuredPassword(ref string text, Widget w) { text = Unsecure(Password); }

		public void Unlink()
		{
			if (FocusableWidget.IsFocused()) {
				FocusableWidget.RevokeFocus();
				CaretPos.IsVisible = false;
			}
			DisplayWidget.Tasks.StopByTag(this);
		}

		public char CurrentChar() =>
			CaretPos.TextPos >= 0 && CaretPos.TextPos < TextLength ? Text.DisplayText[CaretPos.TextPos] : '\0';

		private UndoItem MakeUndoItem() =>
			new UndoItem { TextPos = CaretPos.TextPos, Value = Text.Text };

		private void ApplyUndoItem(UndoItem i)
		{
			lastChar.ShowTimeLeft = 0;
			Text.Text = i.Value;
			CaretPos.TextPos = i.TextPos;
			CaretPos.InvalidatePreservingTextPos();
		}

		private float CalcTextHeight(string s)
		{
			var text = Text.Text;
			Text.Text = s;
			var height = Text.MeasureText().Height;
			Text.Text = text;
			return height;
		}

		static readonly List<ICommand> consumingCommands =
			Command.Editing.Union(
			Key.Enumerate().Where(k => k.IsPrintable() || k.IsTextEditing()).Select(i => new Command(i)).Union(
			Key.Enumerate().Where(k => k.IsPrintable()).Select(i => new Command(new Shortcut(Modifiers.Shift, i)))).Union(
			new [] {
				Cmds.MoveCharPrev, Cmds.MoveCharNext,
				Cmds.MoveWordPrev, Cmds.MoveWordNext,
				Cmds.MoveLineStart, Cmds.MoveLineEnd,
				Cmds.SelectCharPrev, Cmds.SelectCharNext,
				Cmds.SelectWordPrev, Cmds.SelectWordNext,
				Cmds.SelectLineStart, Cmds.SelectLineEnd,
				Cmds.SelectCurrentWord,
				Cmds.DeleteWordPrev, Cmds.DeleteWordNext,
				Cmds.Submit, Cmds.Cancel,
				Cmds.BackSpace, Cmds.ToggleOverwrite,
				Cmds.ContextMenu,
		})).ToList();

		private bool IsMultiline() => EditorParams.IsAcceptableLines(2);

		public bool HasSelection() =>
			SelectionStart.IsVisible && SelectionStart.IsValid && SelectionEnd.IsValid &&
			SelectionStart.TextPos != SelectionEnd.TextPos;

		private void EnsureSelection()
		{
			if (SelectionStart.IsVisible) return;
			SelectionStart.IsVisible = SelectionEnd.IsVisible = true;
			SelectionStart.TextPos = SelectionEnd.TextPos = CaretPos.TextPos;
		}

		private void HideSelection() =>
			SelectionStart.IsVisible = SelectionEnd.IsVisible = false;

		private void MoveCaret(Action move)
		{
			HideSelection();
			move();
		}

		private void MoveCaretSelection(Action move)
		{
			EnsureSelection();
			move();
			SelectionEnd.AssignFrom(CaretPos);
		}

		private void RemoveText(int start, int length, int newTextPos = -1)
		{
			if (EditorParams.UseSecureString) {
				HideSelection();
				for (int i = 0; i < length; ++i)
					Password.RemoveAt(start);
				Text.Invalidate();
			} else {
				History.Add(MakeUndoItem());
				HideSelection();
				Text.Text = Text.Text.Remove(start, length);
			}
			if (start <= lastChar.Pos && lastChar.Pos < start + length)
				lastChar.ShowTimeLeft = 0;
			if (newTextPos >= 0)
				CaretPos.TextPos = newTextPos;
			CaretPos.InvalidatePreservingTextPos();
		}

		private bool InsertChar(char ch)
		{
#if WIN
			if (ch == '\r') return false;
#endif
			if (CaretPos.TextPos < 0 || CaretPos.TextPos > TextLength) return false;
			if (!EditorParams.IsAcceptableLength(TextLength + 1)) return false;
			if (EditorParams.UseSecureString) {
				if (ch == '\n') return false;
				Password.InsertAt(CaretPos.TextPos, ch);
			} else {
				if (ch != '\n' && !EditorParams.AllowNonDisplayableChars && !Text.CanDisplay(ch)) return false;
				var newText = Text.Text.Insert(CaretPos.TextPos, ch.ToString());
				if (EditorParams.AcceptText != null && !EditorParams.AcceptText(newText)) return false;
				if (EditorParams.MaxHeight > 0 && !EditorParams.IsAcceptableHeight(CalcTextHeight(newText))) return false;
				Text.Text = newText;
			}
			lastChar.ShowTimeLeft = EditorParams.PasswordLastCharShowTime;
			lastChar.Value = ch;
			lastChar.Pos = CaretPos.TextPos++;
			return true;
		}

		private void InsertText(string text)
		{
			if (HasSelection())
				DeleteSelection();
			if (EditorParams.UseSecureString) {
				if (text.Count(InsertChar) > 0)
					Text.Invalidate();
			} else {
				var u = MakeUndoItem();
				if (text.Count(InsertChar) > 0)
					History.Add(u);
			}
		}

		private struct SelectionRange
		{
			public int Start;
			public int Length;
		}

		private SelectionRange GetSelectionRange() =>
			new SelectionRange {
				Start = Math.Min(SelectionStart.TextPos, SelectionEnd.TextPos),
				Length = Math.Abs(SelectionStart.TextPos - SelectionEnd.TextPos)
			};

		private void DeleteSelection()
		{
			var r = GetSelectionRange();
			RemoveText(r.Start, r.Length, r.Start);
		}

		private void SelectWord()
		{
			if (Text.Text == "") return;
			EnsureSelection();
			var w = WordAt(Text.Text, CaretPos.TextPos);
			SelectionStart.TextPos = w.Left;
			SelectionEnd.TextPos = w.Right;
			CaretPos.TextPos = SelectionEnd.TextPos;
		}

		public void SelectAll()
		{
			SelectionStart.IsVisible = SelectionEnd.IsVisible = true;
			SelectionStart.TextPos = 0;
			SelectionEnd.TextPos = int.MaxValue;
			Window.Current.Invalidate();
		}

		public void DeleteChar()
		{
			if (HasSelection())
				DeleteSelection();
			else if (CaretPos.TextPos >= 0 && CaretPos.TextPos < TextLength)
				RemoveText(CaretPos.TextPos, 1, CaretPos.TextPos);
		}

		private void HandleKeys()
		{
			try {
				if (Cmds.ContextMenu.WasIssued()) {
					ShowContextMenu(atCaret: true);
				}
				if (Cmds.MoveCharPrev.WasIssued())
					MoveCaret(() => CaretPos.TextPos--);
				if (Cmds.MoveCharNext.WasIssued())
					MoveCaret(() => CaretPos.TextPos++);
				if (Cmds.MoveWordPrev.WasIssued() && IsTextReadable)
					MoveCaret(() => CaretPos.TextPos = PreviousWord(Text.Text, CaretPos.TextPos));
				if (Cmds.MoveWordNext.WasIssued() && IsTextReadable)
					MoveCaret(() => CaretPos.TextPos = NextWord(Text.Text, CaretPos.TextPos));
				if (IsMultiline() && Cmds.MoveLinePrev.WasIssued() && IsTextReadable)
					MoveCaret(() => CaretPos.Line--);
				if (IsMultiline() && Cmds.MoveLineNext.WasIssued() && IsTextReadable)
					MoveCaret(() => CaretPos.Line++);
				if (Cmds.MoveLineStart.WasIssued())
					MoveCaret(() => CaretPos.Col = 0);
				if (Cmds.MoveLineEnd.WasIssued())
					MoveCaret(() => CaretPos.Col = int.MaxValue);

				if (Cmds.SelectCharPrev.WasIssued())
					MoveCaretSelection(() => CaretPos.TextPos--);
				if (Cmds.SelectCharNext.WasIssued())
					MoveCaretSelection(() => CaretPos.TextPos++);
				if (Cmds.SelectWordPrev.WasIssued() && IsTextReadable)
					MoveCaretSelection(() => CaretPos.TextPos = PreviousWord(Text.Text, CaretPos.TextPos));
				if (Cmds.SelectWordNext.WasIssued() && IsTextReadable)
					MoveCaretSelection(() => CaretPos.TextPos = NextWord(Text.Text, CaretPos.TextPos));
				if (Cmds.SelectLineStart.WasIssued())
					MoveCaretSelection(() => CaretPos.Col = 0);
				if (Cmds.SelectLineEnd.WasIssued())
					MoveCaretSelection(() => CaretPos.Col = int.MaxValue);
				if (Command.SelectAll.WasIssued())
					SelectAll();
				if (Cmds.SelectCurrentWord.WasIssued() && IsTextReadable)
					SelectWord();

				if (Cmds.ToggleOverwrite.WasIssued())
					OverwriteMode = !OverwriteMode;
				if (!Command.Delete.IsConsumed()) {
					Command.Delete.Enabled = HasSelection() || CaretPos.TextPos < TextLength;
					if (Command.Delete.WasIssued())
						DeleteChar();
				}
				if (Cmds.DeleteWordPrev.WasIssued() && IsTextReadable) {
					var p = PreviousWord(Text.Text, CaretPos.TextPos);
					if (p < CaretPos.TextPos)
						RemoveText(p, CaretPos.TextPos - p, p);
				}
				if (Cmds.DeleteWordNext.WasIssued() && IsTextReadable) {
					var p = NextWord(Text.Text, CaretPos.TextPos);
					if (p > CaretPos.TextPos)
						RemoveText(CaretPos.TextPos, p - CaretPos.TextPos);
				}

				if (Cmds.Submit.WasIssued()) {
					if (IsMultiline()) {
						if (EditorParams.IsAcceptableLines(Text.Text.Count(ch => ch == '\n') + 2))
							InsertText("\n");
						Cmds.Submit.Consume();
					} else {
						HideSelection();
						Cmds.Submit.Consume();
						FocusableWidget.RevokeFocus();
					}
				}
				if (Cmds.Cancel.WasIssued() && IsTextReadable) {
					if (History.CanUndo())
						ApplyUndoItem(History.ClearAndRestore());
					HideSelection();
					History.Clear();
					Cmds.Cancel.Consume();
					FocusableWidget.RevokeFocus();
				}
				if (!Command.SelectAll.IsConsumed()) {
					Command.SelectAll.Enabled = true;
				}
				if (!Command.Copy.IsConsumed()) {
					Command.Copy.Enabled = HasSelection() && IsTextReadable;
					if (Command.Copy.WasIssued()) {
						var r = GetSelectionRange();
						Clipboard.Text = Text.Text.Substring(r.Start, r.Length);
					}
				}
				if (!Command.Cut.IsConsumed()) {
					Command.Cut.Enabled = HasSelection() && IsTextReadable;
					if (Command.Cut.WasIssued()) {
						var r = GetSelectionRange();
						Clipboard.Text = Text.Text.Substring(r.Start, r.Length);
						DeleteSelection();
					}
				}
				if (!Command.Paste.IsConsumed()) {
					if (Command.Paste.WasIssued())
						InsertText(Clipboard.Text);
				}
				if (!Command.Undo.IsConsumed()) {
					Command.Undo.Enabled = History.CanUndo();
					if (Command.Undo.WasIssued())
						ApplyUndoItem(History.Undo(MakeUndoItem()));
				}
				if (!Command.Redo.IsConsumed()) {
					Command.Redo.Enabled = History.CanRedo();
					if (Command.Redo.WasIssued())
						ApplyUndoItem(History.Redo());
				}
			} finally {
				Command.ConsumeRange(consumingCommands);
				if (IsMultiline()) {
					Cmds.MoveLinePrev.Consume();
					Cmds.MoveLineNext.Consume();
				}
			}
		}

		private void HandleTextInput()
		{
			if (!ProcessInput || FocusableWidget.Input.TextInput == null)
				return;

			foreach (var ch in FocusableWidget.Input.TextInput) {
				// Some platforms, notably iOS, do not generate Key.BackSpace.
				// OTOH, '\b' is emulated everywhere.
				if (ch == '\b') {
					if (HasSelection())
						DeleteSelection();
					else if (CaretPos.TextPos > 0 && CaretPos.TextPos <= TextLength)
						RemoveText(CaretPos.TextPos - 1, 1, CaretPos.TextPos - 1);
				} else if (ch >= ' ' && ch != '\u007f') { // Ignore control and 'delete' characters.
					if (OverwriteMode)
						DeleteChar();
					InsertText(ch.ToString());
				}
			}
		}

		private IEnumerator<object> TrackLastCharInput()
		{
			while (true) {
				if (lastChar.Visible) {
					lastChar.ShowTimeLeft -= Task.Current.Delta;
					if (!lastChar.Visible)
						Text.Invalidate();
				}
				yield return null;
			}
		}

		public void AdjustSizeAndScrollToCaret()
		{
			var s = EditorParams.Scroll;
			if (s == null) return;
			// Layout has not been done yet.
			if (s.Frame.Size == Vector2.Zero) return;
			if (!CaretPos.IsVisible) return;
			s.ScrollTo(
				s.PositionToView(s.ProjectToScrollAxis(CaretPos.WorldPos),
				DisplayWidget.Padding.Left, DisplayWidget.Padding.Right), instantly: true);
		}

		private IEnumerator<object> HandleInputTask()
		{
			bool wasFocused = false;

			var rightClickGesture = new ClickGesture(1);
			var clickGesture = new ClickGesture();
			var doubleClickGesture = new DoubleClickGesture();
			var dragGesture = new DragGesture(0, DragDirection.Any, dragThreshold: EditorParams.MouseSelectionThreshold);
			ClickableWidget.Gestures.Add(rightClickGesture);
			ClickableWidget.Gestures.Add(clickGesture);
			ClickableWidget.Gestures.Add(doubleClickGesture);
			ClickableWidget.Gestures.Add(dragGesture);

			while (true) {
				if (EditorParams.SelectAllOnFocus && !wasFocused && FocusableWidget.IsFocused()) {
					SelectAll();
					CaretPos.TextPos = TextLength;
				}
				if (FocusableWidget.IsFocused()) {
					HandleKeys();
					if (Window.Current.Active && Enabled) {
						HandleTextInput();
					}
				}
				if (clickGesture.WasRecognized()) {
					if (!FocusableWidget.IsFocused()) {
						if (EditorParams.SelectAllOnFocus)
							SelectAll();
					} else {
						HideSelection();
					}
					FocusableWidget.SetFocus();
					CaretPos.WorldPos = DisplayWidget.LocalMousePosition();
				}
				if (doubleClickGesture.WasRecognized()) {
					if (IsTextReadable)
						SelectWord();
					else
						SelectAll();
				}
				if (rightClickGesture.WasRecognized()) {
					FocusableWidget.SetFocus();
					ShowContextMenu(true);
				}
				if (dragGesture.WasRecognized()) {
					FocusableWidget.SetFocus();
					CaretPos.WorldPos = DisplayWidget.ToLocalMousePosition(dragGesture.MousePressPosition);
					HideSelection();
					EnsureSelection();
					SelectionStart.AssignFrom(CaretPos);
				} else if (dragGesture.WasChanged()) {
					CaretPos.WorldPos = DisplayWidget.LocalMousePosition();
					EnsureSelection();
					SelectionEnd.AssignFrom(CaretPos);
				}
				Text.SyncCaretPosition();
				AdjustSizeAndScrollToCaret();
				var isFocused = CaretPos.IsVisible = FocusableWidget.IsFocused();
				if (wasFocused && !isFocused) {
					HideSelection();
					if (History.CanUndo()) {
						History.Clear();
						Text.Submit();
					}
				}
				wasFocused = isFocused;
				yield return null;
			}
		}

		private void ShowContextMenu(bool atCaret)
		{
#if MAC || WIN
			var hs = HasSelection();
			Command.Cut.Enabled = hs && IsTextReadable;
			Command.Copy.Enabled = hs && IsTextReadable;
			Command.Delete.Enabled = hs || CaretPos.TextPos < TextLength;
			Command.Paste.Enabled = !string.IsNullOrEmpty(Clipboard.Text);
			Command.Undo.Enabled = History.CanUndo();
			var i = Window.Current.Input;
			var m = new Menu {
				Command.Undo,
				Command.MenuSeparator,
				Command.Cut,
				Command.Copy,
				Command.Paste,
				Command.Delete,
				Command.MenuSeparator,
				Command.SelectAll,
			};
			if (atCaret) {
				var p = DisplayWidget.LocalToWorldTransform.TransformVector(CaretPos.WorldPos);
				if (EditorParams.OffsetContextMenu != null)
					p = EditorParams.OffsetContextMenu(p);
				m.Popup(Window.Current, p, 0, null);
			} else
				m.Popup();
#endif
		}
	}
}
