using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Lime
{
	public interface ICaretPresenter : IPresenter
	{
		Vector2 Position { get; set; }
		Color4 Color { get; set; }
		bool Visible { get; set; }
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

	public class VerticalLineCaret : IPresenter, ICaretPresenter
	{
		public Vector2 Position { get; set; }
		public Color4 Color { get; set; } = Color4.Black;
		public bool Visible { get; set; }
		public float Thickness { get; set; } = 1.0f;
		public float Width { get; set; } = 0;

		public Lime.RenderObject GetRenderObject(Node node)
		{
			if (!Visible) {
				return null;
			}
			var text = (SimpleText)node;
			var ro = RenderObjectPool<RenderObject>.Acquire();
			ro.CaptureRenderState(text);
			ro.Position = Position;
			ro.Color = Color;
			ro.Thickness = Thickness;
			ro.Width = Width;
			ro.FontHeight = text.FontHeight;
			return ro;
		}

		public bool PartialHitTest(Node node, ref HitTestArgs args) => false;

		public IPresenter Clone() => (IPresenter)MemberwiseClone();

		private class RenderObject : WidgetRenderObject
		{
			public Vector2 Position;
			public Color4 Color;
			public float Thickness;
			public float Width;
			public float FontHeight;

			public override void Render()
			{
				PrepareRenderState();
				var b = Position + new Vector2(Width, FontHeight);
				// Zero-width outline is still twice as wide.
				if (Width <= 0)
					Renderer.DrawLine(Position, b, Color, Thickness);
				else
					Renderer.DrawRectOutline(Position, b, Color, Thickness);
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
		public float OutlineThickness { get; set; } = 1f;
	}

	public class SelectionPresenter : IPresenter
	{
		private ICaretPosition selectionStart;
		private ICaretPosition selectionEnd;
		private SelectionParams selectionParams;

		public SelectionPresenter(
			Widget container, ICaretPosition selectionStart, ICaretPosition selectionEnd,
			SelectionParams selectionParams)
		{
			this.selectionStart = selectionStart;
			this.selectionEnd = selectionEnd;
			this.selectionParams = selectionParams;
			container.CompoundPresenter.Add(this);
		}

		public Lime.RenderObject GetRenderObject(Node node)
		{
			if (!selectionStart.IsVisible || !selectionEnd.IsVisible) {
				return null;
			}
			var s = selectionStart.WorldPos;
			var e = selectionEnd.WorldPos;
			if (s == e) {
				return null;
			}
			if (s.Y > e.Y || s.Y == e.Y && s.X > e.X) {
				var t = s;
				s = e;
				e = t;
			}
			var text = (SimpleText)node;
			var ro = RenderObjectPool<RenderObject>.Acquire();
			ro.CaptureRenderState(text);
			ro.SelectionStart = s;
			ro.SelectionEnd = e;
			ro.OutlineThickness = selectionParams.OutlineThickness;
			ro.OutlineColor = selectionParams.OutlineColor;
			ro.Color = selectionParams.Color;
			ro.Padding = selectionParams.Padding;
			ro.Bounds = text.MeasureText().ShrinkedBy(new Thickness(selectionParams.OutlineThickness));
			ro.FontHeight = text.FontHeight;
			return ro;
		}

		public bool PartialHitTest(Node node, ref HitTestArgs args) => false;

		public IPresenter Clone() => (IPresenter)MemberwiseClone();

		private class RenderObject : WidgetRenderObject
		{
			public Vector2 SelectionStart;
			public Vector2 SelectionEnd;
			public float OutlineThickness;
			public Color4 OutlineColor;
			public Color4 Color;
			public Thickness Padding;
			public Rectangle Bounds;
			public float FontHeight;

			private static List<Rectangle> PrepareRows(Vector2 s, Vector2 e, float fh)
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

			public override void Render()
			{
				PrepareRenderState();
				var rows = PrepareRows(SelectionStart, SelectionEnd, FontHeight).
					Select(r => Rectangle.Intersect(r.ExpandedBy(Padding), Bounds)).ToList();
				foreach (var r in rows) {
					var r1 = r.ExpandedBy(new Thickness(OutlineThickness));
					Renderer.DrawRectOutline(r1.A, r1.B, OutlineColor, OutlineThickness);
				}
				foreach (var r in rows)
					Renderer.DrawRect(r.A, r.B, Color);
			}
		}
	}

	public class UndoHistory<T> where T : IEquatable<T>
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
		bool SelectAllOnFocus { get; set; }
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
		Func<Vector2, Vector2> OffsetContextMenu { get; set; }

		bool IsAcceptableLength(int length);
		bool IsAcceptableLines(int lines);
		bool IsAcceptableHeight(float height);
	}

	public class EditorParams : IEditorParams
	{
		public bool SelectAllOnFocus { get; set; }
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
		public Func<Vector2, Vector2> OffsetContextMenu { get; set; }

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


	public static class WordUtils
	{
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

		public static int PreviousWord(string text, int pos)
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

		public static int NextWord(string text, int pos)
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

		public struct IntRange { public int Left, Right; }

		public static IntRange WordAt(string text, int pos)
		{
			var r = new IntRange { Left = pos, Right = pos };
			var cc = GetCharClassAt(text, pos);
			if (cc == CharClass.Space || cc == CharClass.End)
				cc = GetCharClassAt(text, pos - 1);
			while (GetCharClassAt(text, r.Left - 1) == cc)
				--r.Left;
			while (GetCharClassAt(text, r.Right) == cc)
				++r.Right;
			return r;
		}

	}
}
