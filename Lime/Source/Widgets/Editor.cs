using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;

namespace Lime
{
	public interface ICaretPresenter : IPresenter
	{
		Vector2 Position { get; set; }
		Color4 Color { get; set; }
		bool Visible { get; set; }
		float Thickness { get; set; }
	}

	public interface ICaretParams
	{
		ICaretPresenter CaretPresenter { get; set; }
		float BlinkInterval { get; set; }
		bool FollowTextColor { get; set; }
	}

	public class CaretParams : ICaretParams
	{
		public ICaretPresenter CaretPresenter { get; set; }
		public float BlinkInterval { get; set; } = 0.5f;
		public bool FollowTextColor { get; set; }
	}

	public class VerticalLineCaret : CustomPresenter, ICaretPresenter
	{
		public Vector2 Position { get; set; }
		public Color4 Color { get; set; } = Color4.Black;
		public bool Visible { get; set; }
		public float Thickness { get; set; } = 1.0f;

		public override void Render(Node node)
		{
			if (Visible) {
				var text = (SimpleText)node;
				text.PrepareRendererState();
				Renderer.DrawLine(Position, Position + Vector2.Down * text.FontHeight, Color, Thickness);
			}
		}
	}

	public class CaretDisplay
	{
		private Widget container;
		private ICaretPosition caretPos;
		private ICaretParams caretParams;

		public CaretDisplay(Widget container, ICaretPosition caretPos, ICaretParams caretParams)
		{
			this.container = container;
			this.caretPos = caretPos;
			this.caretParams = caretParams;
			container.CompoundPostPresenter.Add(caretParams.CaretPresenter);
			container.Tasks.Add(CaretDisplayTask());
		}

		private IEnumerator<object> CaretDisplayTask()
		{
			var p = caretParams.CaretPresenter;
			var time = 0f;
			bool blinkOn = true;
			bool wasVisible = false;
			while (true) {
				if (caretPos.IsVisible) {
					time += Task.Current.Delta;
					if (time > caretParams.BlinkInterval && caretParams.BlinkInterval > 0f) {
						time = 0f;
						blinkOn = !blinkOn;
						Window.Current.Invalidate();
					}
					var newPos = caretPos.WorldPos;
					if (!p.Position.Equals(newPos) || !wasVisible) {
						p.Position = newPos;
						time = 0f;
						blinkOn = true;
						Window.Current.Invalidate();
					}
					p.Visible = blinkOn;
					if (caretParams.FollowTextColor) {
						p.Color = container.Color;
					}
				} else if (p.Visible) {
					p.Visible = false;
					Window.Current.Invalidate();
				}
				wasVisible = caretPos.IsVisible;
				yield return null;
			}
		}
	}

	public class SelectionParams
	{
		public Color4 Color { get; set; } = Color4.Yellow;
		public Color4 OutlineColor { get; set; } = Color4.Orange;
		public Thickness Padding { get; set; } = new Thickness(1f);
		public float OutlineThickness{ get; set; } = 1f;
	}

	public class SelectionPresenter: CustomPresenter, IPresenter
	{
		private Widget container;
		private ICaretPosition selectionStart;
		private ICaretPosition selectionEnd;
		private SelectionParams selectionParams;

		public SelectionPresenter(
			Widget container, ICaretPosition selectionStart, ICaretPosition selectionEnd,
			SelectionParams selectionParams)
		{
			this.container = container;
			this.selectionStart = selectionStart;
			this.selectionEnd = selectionEnd;
			this.selectionParams = selectionParams;
			container.CompoundPresenter.Add(this);
		}

		private List<Rectangle> PrepareRows(Vector2 s, Vector2 e, float fh)
		{
			var rows = new List<Rectangle>();
			if (s.Y == e.Y) {
				rows.Add(new Rectangle(s, e + Vector2.Down * fh));
			} else { // Multi-line selection.
				rows.Add(new Rectangle(s, new Vector2(float.PositiveInfinity, s.Y + fh)));
				if (s.Y + fh < e.Y)
					rows.Add(new Rectangle(0, s.Y + fh, float.PositiveInfinity, e.Y));
				rows.Add(new Rectangle(new Vector2(0, e.Y), e + Vector2.Down * fh));
			}
			return rows;
		}

		public override void Render(Node node)
		{
			if (!selectionStart.IsVisible || !selectionEnd.IsVisible) return;

			var s = selectionStart.WorldPos;
			var e = selectionEnd.WorldPos;
			if (s == e) return;
			if (s.Y > e.Y || s.Y == e.Y && s.X > e.X) {
				var t = s;
				s = e;
				e = t;
			}
			var text = (SimpleText)node;
			text.PrepareRendererState();

			var th = selectionParams.OutlineThickness;
			var b = text.MeasureText().ShrinkedBy(new Thickness(th));
			var rows = PrepareRows(s, e, text.FontHeight).
				Select(r => Rectangle.Intersect(r.ExpandedBy(selectionParams.Padding), b)).ToList();

			foreach (var r in rows) {
				var r1 = r.ExpandedBy(new Thickness(th));
				Renderer.DrawRectOutline(r1.A, r1.B, selectionParams.OutlineColor, th);
			}
			foreach (var r in rows)
				Renderer.DrawRect(r.A, r.B, selectionParams.Color);
		}
	}

	public class UndoHistory<T> where T: IEquatable<T>
	{

		private List<T> queue = new List<T>();
		private int current;

		public int MaxDepth { get; set; }

		public void Add(T item)
		{
			if (queue.Count > 0 && item.Equals(queue[queue.Count - 1]))
				return;
			if (current < queue.Count)
				queue.RemoveRange(current, queue.Count - current);
			var overflow = queue.Count - MaxDepth + 1;
			if (MaxDepth > 0 && overflow > 0) {
				queue.RemoveRange(0, overflow);
				current -= overflow;
			}
			queue.Add(item);
			current = queue.Count;
		}

		public bool CanUndo() => current > 0;
		public bool CanRedo() => current < queue.Count - 1;

		public T Undo(T item)
		{
			if (!CanUndo())
				throw new InvalidOperationException();
			if (current == queue.Count && !item.Equals(queue[queue.Count - 1]))
				queue.Add(item);
			return queue[--current];
		}

		public T Redo()
		{
			if (!CanRedo())
				throw new InvalidOperationException();
			return queue[++current];
		}

		public void Clear()
		{
			queue.Clear();
			current = 0;
		}

		public T ClearAndRestore()
		{
			if (!CanUndo())
				throw new InvalidOperationException();
			var result = queue[0];
			queue.Clear();
			current = 0;
			return result;
		}
	}

	public interface IEditorParams
	{
		int MaxLength { get; set; }
		int MaxLines { get; set; }
		float MaxHeight { get; set; }
		int MaxUndoDepth { get; set; }
		bool UseSecureString { get; set; }
		char? PasswordChar { get; set; }
		float PasswordLastCharShowTime { get; set; }
		Predicate<string> AcceptText { get; set; }
		ScrollView Scroll { get; set; }
		bool AllowNonDisplayableChars { get; set; }
		float MouseSelectionThreshold { get; set; }

		bool IsAcceptableLength(int length);
		bool IsAcceptableLines(int lines);
		bool IsAcceptableHeight(float height);
	}

	public class EditorParams : IEditorParams
	{
		public int MaxLength { get; set; }
		public int MaxLines { get; set; }
		public float MaxHeight { get; set; }
		public int MaxUndoDepth { get; set; } = 100;
		public bool UseSecureString { get; set; }
		public char? PasswordChar { get; set; }
		public float PasswordLastCharShowTime { get; set; } =
#if WIN || MAC || MONOMAC
			0.0f;
#else
			1.0f;
#endif
		public Predicate<string> AcceptText { get; set; }
		public ScrollView Scroll { get; set; }
		public bool AllowNonDisplayableChars { get; set; }
		public float MouseSelectionThreshold { get; set; } = 3.0f;

		public bool IsAcceptableLength(int length) => MaxLength <= 0 || length <= MaxLength;
		public bool IsAcceptableLines(int lines) => MaxLines <= 0 || lines <= MaxLines;
		public bool IsAcceptableHeight(float height) => MaxHeight <= 0 || height <= MaxHeight;

		public const NumberStyles numberStyle =
			NumberStyles.AllowDecimalPoint |
			NumberStyles.AllowLeadingSign;

		public static bool AcceptNumber(string s)
		{
			double temp;
			return s == "-" || Double.TryParse(s, numberStyle, CultureInfo.InvariantCulture, out temp);
		}
	}

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

		private enum CharClass
		{
			Begin,
			Space,
			Punctuation,
			Word,
			Other,
			End,
		}

		private static CharClass GetCharClassAt(string text, int pos)
		{
			if (pos < 0) return CharClass.Begin;
			if (pos >= text.Length) return CharClass.End;
			var ch = text[pos];
			if (Char.IsWhiteSpace(ch)) return CharClass.Space;
			if (Char.IsPunctuation(ch) || Char.IsSeparator(ch) || Char.IsSymbol(ch))
				return CharClass.Punctuation;
			if (Char.IsLetterOrDigit(ch))
				return CharClass.Word;
			return CharClass.Other;
		}

		private static int PreviousWord(string text, int pos)
		{
			if (pos <= 0)
				return 0;
			--pos;
			for (var ccRight = GetCharClassAt(text, pos); pos > 0; --pos) {
				var ccLeft = GetCharClassAt(text, pos - 1);
				if (ccLeft != ccRight && ccRight != CharClass.Space)
					break;
				ccRight = ccLeft;
			}
			return pos;
		}

		private static int NextWord(string text, int pos)
		{
			if (pos >= text.Length)
				return text.Length;
			++pos;
			for (var ccLeft = GetCharClassAt(text, pos - 1); pos < text.Length; ++pos) {
				var ccRight = GetCharClassAt(text, pos);
				if (ccRight != ccLeft && ccRight != CharClass.Space)
					break;
				ccLeft = ccRight;
			}
			return pos;
		}

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
			var t = Text.Text;
			if (t == "") return;
			EnsureSelection();
			SelectionStart.TextPos = SelectionEnd.TextPos = CaretPos.TextPos;
			var cc = GetCharClassAt(t, CaretPos.TextPos);
			if (cc == CharClass.Space || cc == CharClass.End)
				cc = GetCharClassAt(t, CaretPos.TextPos - 1);
			while (GetCharClassAt(t, SelectionStart.TextPos - 1) == cc)
				--SelectionStart.TextPos;
			while (GetCharClassAt(t, SelectionEnd.TextPos) == cc)
				++SelectionEnd.TextPos;
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
						History.Clear();
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
				m.Popup(Window.Current, p, 0, null);
			} else
				m.Popup();
#endif
		}
	}
}
