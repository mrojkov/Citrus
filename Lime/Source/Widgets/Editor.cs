using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

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
		public float BlinkInterval { get; set; }
		public bool FollowTextColor { get; set; }

		public CaretParams()
		{
			BlinkInterval = 0.5f;
		}
	}

	public class VerticalLineCaret : CustomPresenter, ICaretPresenter
	{
		public Vector2 Position { get; set; }
		public Color4 Color { get; set; }
		public bool Visible { get; set; }
		public float Thickness { get; set; }

		public VerticalLineCaret()
		{
			Thickness = 1.0f;
			Color = Color4.Black;
		}

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

		public override void Render(Node node)
		{
			if (!selectionStart.IsVisible || !selectionEnd.IsVisible) return;
			var text = (SimpleText)node;
			text.PrepareRendererState();
			var r = new Rectangle(selectionStart.WorldPos, selectionEnd.WorldPos + Vector2.Down * text.FontHeight).
				Normalized.ExpandedBy(selectionParams.Padding);
			Renderer.DrawRect(r.A, r.B, selectionParams.Color);
			Renderer.DrawRectOutline(r.A, r.B, selectionParams.OutlineColor);
		}
	}

	public interface IEditorParams
	{
		int MaxLength { get; set; }
		int MaxLines { get; set; }
		float MaxHeight { get; set; }
		char? PasswordChar { get; set; }
		float PasswordLastCharShowTime { get; set; }
		Predicate<string> AcceptText { get; set; }
		ScrollView Scroll { get; set; }
		bool AllowNonDisplayableChars { get; set; }

		bool IsAcceptableLength(int length);
		bool IsAcceptableLines(int lines);
		bool IsAcceptableHeight(float height);
	}

	public class EditorParams : IEditorParams
	{
		public int MaxLength { get; set; }
		public int MaxLines { get; set; }
		public float MaxHeight { get; set; }
		public char? PasswordChar { get; set; }
		public float PasswordLastCharShowTime { get; set; }
		public Predicate<string> AcceptText { get; set; }
		public ScrollView Scroll { get; set; }
		public bool AllowNonDisplayableChars { get; set; }

		public EditorParams()
		{
#if WIN || MAC || MONOMAC
			PasswordLastCharShowTime = 0.0f;
#else
			PasswordLastCharShowTime = 1.0f;
#endif
		}

		public bool IsAcceptableLength(int length) { return MaxLength <= 0 || length <= MaxLength; }
		public bool IsAcceptableLines(int lines) { return MaxLines <= 0 || lines <= MaxLines; }
		public bool IsAcceptableHeight(float height) { return MaxHeight <= 0 || height <= MaxHeight; }

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

		public ICaretPosition CaretPos { get; }
		public ICaretPosition SelectionStart { get; }
		public ICaretPosition SelectionEnd { get; }

		public Editor(
			Widget container,
			ICaretPosition caretPos,
			ICaretPosition selectionStart, ICaretPosition selectionEnd,
			IEditorParams editorParams, Widget inputWidget = null)
		{
			DisplayWidget = container;
			InputWidget = inputWidget ?? container;
			DisplayWidget.HitTestTarget = true;
			Text = (IText)container;
			Text.TrimWhitespaces = false;
			Text.Localizable = false;

			CaretPos = caretPos;
			SelectionStart = selectionStart;
			SelectionEnd = selectionEnd;
			EditorParams = editorParams;

			if (editorParams.PasswordChar != null) {
				Text.TextProcessor += ProcessTextAsPassword;
				container.Tasks.Add(TrackLastCharInput, this);
			}
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
		}

		private string PasswordChars(int length) { return new string(EditorParams.PasswordChar.Value, length); }

		private void ProcessTextAsPassword(ref string text)
		{
			if (text != "")
				text = isLastCharVisible ? PasswordChars(text.Length - 1) + text.Last() : PasswordChars(text.Length);
		}

		public void Unlink()
		{
			if (InputWidget.IsFocused()) {
				InputWidget.RevokeFocus();
				CaretPos.IsVisible = false;
			}
			DisplayWidget.Tasks.StopByTag(this);
		}

		private bool WasKeyRepeated(Key key)
		{
			return InputWidget.Input.WasKeyRepeated(key);
		}

		private void InsertChar(char ch)
		{
			if (CaretPos.TextPos < 0 || CaretPos.TextPos > Text.Text.Length) return;
			if (!EditorParams.IsAcceptableLength(Text.Text.Length + 1)) return;
			if (ch != '\n' && !EditorParams.AllowNonDisplayableChars && !Text.CanDisplay(ch)) return;
			if (HasSelection())
				DeleteSelection();
			var newText = Text.Text.Insert(CaretPos.TextPos, ch.ToString());
			if (EditorParams.AcceptText != null && !EditorParams.AcceptText(newText)) return;
			if (EditorParams.MaxHeight > 0 && !EditorParams.IsAcceptableHeight(CalcTextHeight(newText))) return;
			Text.Text = newText;
			CaretPos.TextPos++;
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
					Cmds.Submit, Cmds.Cancel,
					Key.Commands.Cut, Key.Commands.Copy, Key.Commands.Paste, Key.Commands.Delete,
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

		private bool IsMultiline() { return EditorParams.IsAcceptableLines(2); }

		private bool HasSelection() =>
			SelectionStart.IsVisible && SelectionStart.IsValid && SelectionEnd.IsValid;

		private void EnsureSelection()
		{
			if (SelectionStart.IsVisible) return;
			SelectionStart.IsVisible = SelectionEnd.IsVisible = true;
			SelectionStart.TextPos = SelectionEnd.TextPos = CaretPos.TextPos;
		}

		private void HideSelection()
		{
			SelectionStart.IsVisible = SelectionEnd.IsVisible = false;
		}

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
			if (!HasSelection()) return;
			var r = GetSelectionRange();
			Text.Text = Text.Text.Remove(r.Start, r.Length);
			CaretPos.TextPos = r.Start;
			CaretPos.InvalidatePreservingTextPos();
			HideSelection();
		}

		private void HandleKeys(string originalText)
		{
			try {
				if (WasKeyRepeated(Cmds.MoveCharPrev))
					MoveCaret(() => CaretPos.TextPos--);
				if (WasKeyRepeated(Cmds.MoveCharNext))
					MoveCaret(() => CaretPos.TextPos++);
				if (WasKeyRepeated(Cmds.MoveWordPrev))
					MoveCaret(() => CaretPos.TextPos = PreviousWord(Text.Text, CaretPos.TextPos));
				if (WasKeyRepeated(Cmds.MoveWordNext))
					MoveCaret(() => CaretPos.TextPos = NextWord(Text.Text, CaretPos.TextPos));
				if (IsMultiline() && WasKeyRepeated(Cmds.MoveLinePrev))
					MoveCaret(() => CaretPos.Line--);
				if (IsMultiline() && WasKeyRepeated(Cmds.MoveLineNext))
					MoveCaret(() => CaretPos.Line++);
				if (WasKeyRepeated(Cmds.MoveLineStart))
					MoveCaret(() => CaretPos.Col = 0);
				if (WasKeyRepeated(Cmds.MoveLineEnd))
					MoveCaret(() => CaretPos.Col = int.MaxValue);

				if (WasKeyRepeated(Cmds.SelectCharPrev))
					MoveCaretSelection(() => CaretPos.TextPos--);
				if (WasKeyRepeated(Cmds.SelectCharNext))
					MoveCaretSelection(() => CaretPos.TextPos++);
				if (WasKeyRepeated(Cmds.SelectWordPrev))
					MoveCaretSelection(() => CaretPos.TextPos = PreviousWord(Text.Text, CaretPos.TextPos));
				if (WasKeyRepeated(Cmds.SelectWordNext))
					MoveCaretSelection(() => CaretPos.TextPos = NextWord(Text.Text, CaretPos.TextPos));
				if (WasKeyRepeated(Cmds.SelectLineStart))
					MoveCaretSelection(() => CaretPos.Col = 0);
				if (WasKeyRepeated(Cmds.SelectLineEnd))
					MoveCaretSelection(() => CaretPos.Col = int.MaxValue);
				if (WasKeyRepeated(Cmds.SelectAll)) {
					SelectionStart.IsVisible = SelectionEnd.IsVisible = true;
					SelectionStart.TextPos = 0;
					SelectionEnd.TextPos = int.MaxValue;
				}

				if (WasKeyRepeated(Key.Commands.Delete)) {
					if (HasSelection())
						DeleteSelection();
					else if (CaretPos.TextPos >= 0 && CaretPos.TextPos < Text.Text.Length) {
						Text.Text = Text.Text.Remove(CaretPos.TextPos, 1);
						CaretPos.InvalidatePreservingTextPos();
					}
				}
				if (WasKeyRepeated(Cmds.DeleteWordPrev)) {
					HideSelection();
					var p = PreviousWord(Text.Text, CaretPos.TextPos);
					if (p < CaretPos.TextPos) {
						Text.Text = Text.Text.Remove(p, CaretPos.TextPos - p);
						CaretPos.TextPos = p;
					}
				}
				if (WasKeyRepeated(Cmds.DeleteWordNext)) {
					HideSelection();
					var p = NextWord(Text.Text, CaretPos.TextPos);
					if (p > CaretPos.TextPos) {
						Text.Text = Text.Text.Remove(CaretPos.TextPos, p - CaretPos.TextPos);
						CaretPos.InvalidatePreservingTextPos();
					}
				}
				if (WasKeyRepeated(Cmds.Submit)) {
					if (EditorParams.IsAcceptableLines(Text.Text.Count(ch => ch == '\n') + 2)) {
						InsertChar('\n');
					} else {
						HideSelection();
						InputWidget.Input.ConsumeKey(Cmds.Submit);
						InputWidget.RevokeFocus();
					}
				}
				if (WasKeyRepeated(Cmds.Cancel)) {
					HideSelection();
					Text.Text = originalText;
					InputWidget.Input.ConsumeKey(Cmds.Cancel);
					InputWidget.RevokeFocus();
				}
				if (InputWidget.Input.WasKeyPressed(Key.Commands.Copy) && HasSelection()) {
					var r = GetSelectionRange();
					Clipboard.Text = Text.Text.Substring(r.Start, r.Length);
				}
				if (InputWidget.Input.WasKeyPressed(Key.Commands.Cut) && HasSelection()) {
					var r = GetSelectionRange();
					Clipboard.Text = Text.Text.Substring(r.Start, r.Length);
					DeleteSelection();
				}
				if (WasKeyRepeated(Key.Commands.Paste)) {
					foreach (var ch in Clipboard.Text)
						InsertChar(ch);
				}
			} finally {
				var input = InputWidget.Input;
				input.SetKeyEnabled(Key.Commands.Cut, !string.IsNullOrEmpty(Text.Text));
				input.SetKeyEnabled(Key.Commands.Copy, !string.IsNullOrEmpty(Text.Text));
				input.SetKeyEnabled(Key.Commands.Paste, !string.IsNullOrEmpty(Clipboard.Text));
				input.ConsumeKeys(consumingKeys);
				if (IsMultiline())
					input.ConsumeKeys(new[] { Cmds.MoveLinePrev, Cmds.MoveLineNext, });
			}
		}

		private float lastCharShowTimeLeft;
		private bool isLastCharVisible;

		private void HandleTextInput()
		{
			if (InputWidget.Input.TextInput == null)
				return;
			foreach (var ch in InputWidget.Input.TextInput) {
				// Some platforms, notably iOS, do not generate Key.BackSpace.
				// OTOH, '\b' is emulated everywhere.
				if (ch == '\b') {
					if (HasSelection())
						DeleteSelection();
					else if (CaretPos.TextPos > 0 && CaretPos.TextPos <= Text.Text.Length) {
						CaretPos.TextPos--;
						Text.Text = Text.Text.Remove(CaretPos.TextPos, 1);
						lastCharShowTimeLeft = 0f;
					}
				}
				else if (ch >= ' ' && ch != '\u007f') { // Ignore control and 'delete' characters.
					InsertChar(ch);
					lastCharShowTimeLeft = EditorParams.PasswordLastCharShowTime;
				}
			}
		}

		private IEnumerator<object> TrackLastCharInput()
		{
			while (true) {
				if (Text.Text != "") {
					lastCharShowTimeLeft -= Task.Current.Delta;
					var shouldShowLastChar = lastCharShowTimeLeft > 0;
					if (shouldShowLastChar != isLastCharVisible) {
						isLastCharVisible = shouldShowLastChar;
						Text.Invalidate();
					}
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
			string originalText = null;
			while (true) {
				var wasClicked = DisplayWidget.WasClicked();
				if (wasClicked)
					InputWidget.SetFocus();
				if (InputWidget.IsFocused()) {
					HandleKeys(originalText);
					HandleTextInput();
					if (wasClicked) {
						var t = DisplayWidget.LocalToWorldTransform.CalcInversed();
						CaretPos.WorldPos = t.TransformVector(InputWidget.Input.MousePosition);
					}
					Text.SyncCaretPosition();
				}
				AdjustSizeAndScrollToCaret();
				var isFocused = InputWidget.IsFocused();
				CaretPos.IsVisible = isFocused;
				if (!wasFocused && isFocused) {
					originalText = Text.Text;
				}
				if (wasFocused && !isFocused) {
					if (originalText != Text.Text) {
						Text.Submit();
					}
				}
				wasFocused = isFocused;
				yield return null;
			}
		}
	}
}
