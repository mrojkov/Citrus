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
		public readonly Widget InputWidget;
		public readonly IText Text;
		public readonly IEditorParams EditorParams;
		public SecureString Password;

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

		public Editor(Widget container, IEditorParams editorParams, Widget inputWidget = null)
		{
			DisplayWidget = container;
			InputWidget = inputWidget ?? container;
			DisplayWidget.HitTestTarget = true;
			Text = (IText)container;
			Text.TrimWhitespaces = false;
			Text.Localizable = false;

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
					container.Tasks.Add(TrackLastCharInput, this);
			}
			else if (EditorParams.UseSecureString) {
				Text.TextProcessor += ProcessUnsecuredPassword;
			}
			if (EditorParams.UseSecureString)
				Password = new SecureString();

			container.Tasks.Add(HandleInputTask(), this);
		}

		public static class Cmds {
#if MAC
			private const Modifiers WordModifier = Modifiers.Alt;
#else
			private const Modifiers WordModifier = Modifiers.Control;
#endif
			private const Modifiers SelectModifier = Modifiers.Shift;

			public static Key MoveCharPrev = Key.MapShortcut(Key.Left);
			public static Key MoveCharNext = Key.MapShortcut(Key.Right);

			public static Key MoveWordPrev = Key.MapShortcut(WordModifier, Key.Left);
			public static Key MoveWordNext = Key.MapShortcut(WordModifier, Key.Right);

			public static Key MoveLinePrev = Key.MapShortcut(Key.Up);
			public static Key MoveLineNext = Key.MapShortcut(Key.Down);

			public static Key MoveLineStart = Key.MapShortcut(Key.Home);
			public static Key MoveLineEnd = Key.MapShortcut(Key.End);

			public static Key SelectCharPrev = Key.MapShortcut(SelectModifier, Key.Left);
			public static Key SelectCharNext = Key.MapShortcut(SelectModifier, Key.Right);

			public static Key SelectWordPrev = Key.MapShortcut(SelectModifier | WordModifier, Key.Left);
			public static Key SelectWordNext = Key.MapShortcut(SelectModifier | WordModifier, Key.Right);

			public static Key SelectLineStart = Key.MapShortcut(SelectModifier, Key.Home);
			public static Key SelectLineEnd = Key.MapShortcut(SelectModifier, Key.End);

			public static Key SelectAll = Key.MapShortcut(Modifiers.Command, Key.A);

			public static Key DeleteWordPrev = Key.MapShortcut(Modifiers.Control, Key.BackSpace);
			public static Key DeleteWordNext = Key.MapShortcut(Modifiers.Control, Key.Delete);

			public static Key Submit = Key.MapShortcut(Key.Enter);
			public static Key Cancel = Key.MapShortcut(Key.Escape);

			public static Key BackSpace = Key.MapShortcut(Key.BackSpace);

			public static Key ContextMenu = Key.MapShortcut(Key.Menu);
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

		private void ProcessHiddenPassword(ref string text)
		{
			text = lastChar.Visible ?
				PasswordChars(lastChar.Pos) + lastChar.Value + PasswordChars(TextLength - lastChar.Pos - 1) :
				PasswordChars(TextLength);
		}

		private void ProcessUnsecuredPassword(ref string text) { text = Unsecure(Password); }

		public void Unlink()
		{
			if (InputWidget.IsFocused()) {
				InputWidget.RevokeFocus();
				CaretPos.IsVisible = false;
			}
			DisplayWidget.Tasks.StopByTag(this);
		}

		private UndoItem MakeUndoItem() =>
			new UndoItem { TextPos = CaretPos.TextPos, Value = Text.Text };

		private void ApplyUndoItem(UndoItem i)
		{
			lastChar.ShowTimeLeft = 0;
			Text.Text = i.Value;
			CaretPos.TextPos = i.TextPos;
			CaretPos.InvalidatePreservingTextPos();
		}

		private WidgetInput input => InputWidget.Input;

		private bool WasKeyRepeated(Key key) => input.WasKeyRepeated(key);

		private bool InsertChar(char ch)
		{
			if (HasSelection())
				DeleteSelection();
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

		private float CalcTextHeight(string s)
		{
			var text = Text.Text;
			Text.Text = s;
			var height = Text.MeasureText().Height;
			Text.Text = text;
			return height;
		}

		static readonly List<Key> consumingKeys = Key.Enumerate().Where(
			k => k.IsPrintable() || k.IsTextEditing()).Concat(
				new[] {
					Cmds.MoveCharPrev, Cmds.MoveCharNext,
					Cmds.MoveWordPrev, Cmds.MoveWordNext,
					Cmds.MoveLineStart, Cmds.MoveLineEnd,
					Cmds.SelectCharPrev, Cmds.SelectCharNext,
					Cmds.SelectWordPrev, Cmds.SelectWordNext,
					Cmds.SelectLineStart, Cmds.SelectLineEnd,
					Cmds.SelectAll,
					Cmds.DeleteWordPrev, Cmds.DeleteWordNext,
					Cmds.Submit, Cmds.Cancel, Cmds.ContextMenu,
					Cmds.BackSpace,
					Key.Commands.Cut, Key.Commands.Copy, Key.Commands.Paste, Key.Commands.Delete,
					Key.Commands.Undo, Key.Commands.Redo,
				}
			).ToList();

		private bool IsMultiline() => EditorParams.IsAcceptableLines(2);

		private bool HasSelection() =>
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

		private void InsertText(string text)
		{
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
		}

		public void SelectAll()
		{
			SelectionStart.IsVisible = SelectionEnd.IsVisible = true;
			SelectionStart.TextPos = 0;
			SelectionEnd.TextPos = int.MaxValue;
		}

		private void HandleKeys()
		{
			try {
				if (WasKeyRepeated(Cmds.MoveCharPrev))
					MoveCaret(() => CaretPos.TextPos--);
				if (WasKeyRepeated(Cmds.MoveCharNext))
					MoveCaret(() => CaretPos.TextPos++);
				if (WasKeyRepeated(Cmds.MoveWordPrev) && IsTextReadable)
					MoveCaret(() => CaretPos.TextPos = PreviousWord(Text.Text, CaretPos.TextPos));
				if (WasKeyRepeated(Cmds.MoveWordNext) && IsTextReadable)
					MoveCaret(() => CaretPos.TextPos = NextWord(Text.Text, CaretPos.TextPos));
				if (IsMultiline() && WasKeyRepeated(Cmds.MoveLinePrev) && IsTextReadable)
					MoveCaret(() => CaretPos.Line--);
				if (IsMultiline() && WasKeyRepeated(Cmds.MoveLineNext) && IsTextReadable)
					MoveCaret(() => CaretPos.Line++);
				if (WasKeyRepeated(Cmds.MoveLineStart))
					MoveCaret(() => CaretPos.Col = 0);
				if (WasKeyRepeated(Cmds.MoveLineEnd))
					MoveCaret(() => CaretPos.Col = int.MaxValue);

				if (WasKeyRepeated(Cmds.SelectCharPrev))
					MoveCaretSelection(() => CaretPos.TextPos--);
				if (WasKeyRepeated(Cmds.SelectCharNext))
					MoveCaretSelection(() => CaretPos.TextPos++);
				if (WasKeyRepeated(Cmds.SelectWordPrev) && IsTextReadable)
					MoveCaretSelection(() => CaretPos.TextPos = PreviousWord(Text.Text, CaretPos.TextPos));
				if (WasKeyRepeated(Cmds.SelectWordNext) && IsTextReadable)
					MoveCaretSelection(() => CaretPos.TextPos = NextWord(Text.Text, CaretPos.TextPos));
				if (WasKeyRepeated(Cmds.SelectLineStart))
					MoveCaretSelection(() => CaretPos.Col = 0);
				if (WasKeyRepeated(Cmds.SelectLineEnd))
					MoveCaretSelection(() => CaretPos.Col = int.MaxValue);
				if (WasKeyRepeated(Cmds.SelectAll))
					SelectAll();

				if (WasKeyRepeated(Key.Commands.Delete)) {
					if (HasSelection())
						DeleteSelection();
					else if (CaretPos.TextPos >= 0 && CaretPos.TextPos < Text.Text.Length)
						RemoveText(CaretPos.TextPos, 1, CaretPos.TextPos);
				}
				if (WasKeyRepeated(Cmds.DeleteWordPrev) && IsTextReadable) {
					var p = PreviousWord(Text.Text, CaretPos.TextPos);
					if (p < CaretPos.TextPos)
						RemoveText(p, CaretPos.TextPos - p, p);
				}
				if (WasKeyRepeated(Cmds.DeleteWordNext) && IsTextReadable) {
					var p = NextWord(Text.Text, CaretPos.TextPos);
					if (p > CaretPos.TextPos)
						RemoveText(CaretPos.TextPos, p - CaretPos.TextPos);
				}

				if (WasKeyRepeated(Cmds.Submit)) {
					if (IsMultiline()) {
						if (EditorParams.IsAcceptableLines(Text.Text.Count(ch => ch == '\n') + 2))
							InsertText("\n");
						input.ConsumeKey(Cmds.Submit);
					} else {
						HideSelection();
						input.ConsumeKey(Cmds.Submit);
						InputWidget.RevokeFocus();
					}
				}
				if (WasKeyRepeated(Cmds.Cancel) && IsTextReadable) {
					if (History.CanUndo())
						ApplyUndoItem(History.ClearAndRestore());
					HideSelection();
					History.Clear();
					input.ConsumeKey(Cmds.Cancel);
					InputWidget.RevokeFocus();
				}

				if (WasKeyRepeated(Cmds.ContextMenu))
					ShowContextMenu(atCaret: true);
				if (input.WasKeyPressed(Key.Commands.Copy) && HasSelection() && IsTextReadable) {
					var r = GetSelectionRange();
					Clipboard.Text = Text.Text.Substring(r.Start, r.Length);
				}
				if (input.WasKeyPressed(Key.Commands.Cut) && HasSelection() && IsTextReadable) {
					var r = GetSelectionRange();
					Clipboard.Text = Text.Text.Substring(r.Start, r.Length);
					DeleteSelection();
				}
				if (WasKeyRepeated(Key.Commands.Paste))
					InsertText(Clipboard.Text);
				if (WasKeyRepeated(Key.Commands.Undo) && History.CanUndo())
					ApplyUndoItem(History.Undo(MakeUndoItem()));
				if (WasKeyRepeated(Key.Commands.Redo) && History.CanRedo())
					ApplyUndoItem(History.Redo());
			} finally {
				var hs = HasSelection();
				input.SetKeyEnabled(Key.Commands.Cut, hs && IsTextReadable);
				input.SetKeyEnabled(Key.Commands.Copy, hs && IsTextReadable);
				input.SetKeyEnabled(Key.Commands.Delete, hs);
				input.SetKeyEnabled(Key.Commands.Paste, !string.IsNullOrEmpty(Clipboard.Text));
				input.SetKeyEnabled(Key.Commands.Undo, History.CanUndo());

				input.ConsumeKeys(consumingKeys);
				if (IsMultiline())
					input.ConsumeKeys(new[] { Cmds.MoveLinePrev, Cmds.MoveLineNext, });
			}
		}

		private void HandleTextInput()
		{
			if (input.TextInput == null)
				return;
			foreach (var ch in input.TextInput) {
				// Some platforms, notably iOS, do not generate Key.BackSpace.
				// OTOH, '\b' is emulated everywhere.
				if (ch == '\b') {
					if (HasSelection())
						DeleteSelection();
					else if (CaretPos.TextPos > 0 && CaretPos.TextPos <= TextLength)
						RemoveText(CaretPos.TextPos - 1, 1, CaretPos.TextPos - 1);
				}
				else if (ch >= ' ' && ch != '\u007f') // Ignore control and 'delete' characters.
					InsertText(ch.ToString());
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
			// Layout has not been not done yet.
			if (s.Frame.Size == Vector2.Zero) return;
			// ScrollView limits scrolling by Content.Size.
			// Container.Size must be large enough to satistfy scissor test,
			// but not less than Frame.Size to support alignment.
			s.Content.Size = DisplayWidget.Size = Vector2.Max(
				Text.MeasureText().ExpandedBy(DisplayWidget.Padding).Size,
				s.Frame.Size);
			if (!CaretPos.IsVisible) return;
			s.ScrollTo(
				s.PositionToView(s.ProjectToScrollAxis(CaretPos.WorldPos),
				DisplayWidget.Padding.Left, DisplayWidget.Padding.Right), instantly: true);
		}

		private IEnumerator<object> HandleInputTask()
		{
			bool wasFocused = false;
			Vector2 lastClickPos = Vector2.Zero;

			while (true) {
				if (DisplayWidget.Input.WasMousePressed()) {
					InputWidget.SetFocus();
				}
				if (InputWidget.IsFocused()) {
					HandleKeys();
					HandleTextInput();
					var p = input.MousePosition;
					if (input.IsMousePressed()) {
						CaretPos.WorldPos = DisplayWidget.LocalToWorldTransform.CalcInversed().TransformVector(p);
						if (input.WasKeyPressed(Key.Mouse0DoubleClick)) {
							if (IsTextReadable)
								SelectWord();
							else
								SelectAll();
						} else if (input.WasMousePressed()) {
							lastClickPos = p;
							HideSelection();
							input.CaptureMouse();
						} else if ((p - lastClickPos).SqrLength > EditorParams.MouseSelectionThreshold) {
							EnsureSelection();
							SelectionEnd.AssignFrom(CaretPos);
						}
					}
					else {
						input.ReleaseMouse();
					}
					if (input.WasMouseReleased(1))
						ShowContextMenu(atCaret: false);
					Text.SyncCaretPosition();
				}
				AdjustSizeAndScrollToCaret();
				var isFocused = CaretPos.IsVisible = InputWidget.IsFocused();
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
			var i = Window.Current.Input;
			var m = new Menu {
				new LocalKeySendingCommand(i, "Undo", Key.Commands.Undo),
				Command.MenuSeparator,
				new LocalKeySendingCommand(i, "Cut", Key.Commands.Cut),
				new LocalKeySendingCommand(i, "Copy", Key.Commands.Copy),
				new LocalKeySendingCommand(i, "Paste", Key.Commands.Paste),
				new LocalKeySendingCommand(i, "Delete", Key.Commands.Delete),
				Command.MenuSeparator,
				new LocalKeySendingCommand(i, "Select all", Key.Commands.SelectAll),
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
