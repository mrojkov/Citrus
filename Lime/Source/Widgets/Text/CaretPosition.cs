using System;
using System.Collections.Generic;
using System.Linq;

namespace Lime
{
	public class CaretPosition : ICaretPosition
	{
		public enum ValidState { None, All, LineCol, WorldPos, TextPos, LineWorldX };
		public ValidState Valid { get; private set; }

		private int renderingLineNumber;
		private int renderingTextPos;
		private int line;
		private int col;
		private int textPos;
		private Vector2 worldPos;
		private bool isVisible;
		private Vector2 nearestCharPos;

		public bool IsValid => Valid == ValidState.All;

		public int Line {
			get { return line; }
			set
			{
				if (line == value) return;
				switch (Valid) {
					case ValidState.All:
					case ValidState.WorldPos:
						Valid = ValidState.LineWorldX;
						break;
					case ValidState.LineWorldX:
					case ValidState.LineCol:
						break;
					default:
						throw new InvalidOperationException(Valid.ToString());
				}
				line = value;
			}
		}

		public int Col {
			get { return col; }
			set
			{
				if (col == value) return;
				switch (Valid) {
					case ValidState.All:
					case ValidState.LineCol:
					case ValidState.LineWorldX:
						Valid = ValidState.LineCol;
						break;
					default:
						throw new InvalidOperationException(Valid.ToString());
				}
				col = value;
			}
		}

		public int TextPos {
			get { return textPos; }
			set
			{
				if (textPos == value) return;
				Valid = ValidState.TextPos;
				textPos = value;
			}
		}

		public Vector2 WorldPos {
			get { return worldPos; }
			set
			{
				if (worldPos.Equals(value)) return;
				Valid = ValidState.WorldPos;
				worldPos = value;
			}
		}

		public bool IsVisible {
			get { return isVisible && Valid != ValidState.None; }
			set { isVisible = value; }
		}

		public void EmptyText(Vector2 pos)
		{
			worldPos = pos;
			line = col = textPos = 0;
			Valid = ValidState.All;
		}

		public void StartSync()
		{
			renderingLineNumber = 0;
			renderingTextPos = 0;
			nearestCharPos = Vector2.PositiveInfinity;
		}

		public void Sync(int index, Vector2 charPos, Vector2 size)
		{
			switch (Valid) {
				case ValidState.None:
				case ValidState.All:
					break;
				case ValidState.LineCol:
					if (Line == renderingLineNumber && Col == index) {
						textPos = renderingTextPos;
						worldPos = charPos;
						Valid = ValidState.All;
					}
					break;
				case ValidState.TextPos:
					if (TextPos == renderingTextPos) {
						line = renderingLineNumber;
						col = index;
						worldPos = charPos;
						Valid = ValidState.All;
					}
					break;
				case ValidState.WorldPos:
					if ((WorldPos - charPos).SqrLength < (WorldPos - nearestCharPos).SqrLength) {
						line = renderingLineNumber;
						col = index;
						textPos = renderingTextPos;
						nearestCharPos = charPos;
						Valid = ValidState.WorldPos;
					}
					break;
				case ValidState.LineWorldX:
					if (
						Line == renderingLineNumber &&
						(WorldPos.X - charPos.X).Abs() < (WorldPos.X - nearestCharPos.X).Abs())
					{
						col = index;
						textPos = renderingTextPos;
						nearestCharPos = charPos;
					}
					break;
			}
			++renderingTextPos;
		}

		public void FinishSync()
		{
			if (Valid == ValidState.WorldPos || Valid == ValidState.LineWorldX)
				worldPos = nearestCharPos;
			Valid = ValidState.All;
		}

		public void InvalidatePreservingTextPos()
		{
			if (Valid == ValidState.All)
				Valid = ValidState.TextPos;
		}

		public void ClampTextPos(int textLength)
		{
			if (Valid == ValidState.TextPos)
				textPos = textPos.Clamp(0, textLength);
		}

		public void ClampLine(int lineCount)
		{
			if (Valid == ValidState.LineCol || Valid == ValidState.LineWorldX)
				line = line.Clamp(0, lineCount - 1);
		}

		public void ClampCol(int maxCol)
		{
			if (Valid == ValidState.LineCol && Line == renderingLineNumber)
				Col = Col.Clamp(0, maxCol);
		}

		public void NextLine()
		{
			++renderingLineNumber;
		}

		public ICaretPosition Clone()
		{
			return (CaretPosition)MemberwiseClone();
		}

		public void AssignFrom(ICaretPosition c)
		{
			line = c.Line;
			col = c.Col;
			textPos = c.TextPos;
			worldPos = c.WorldPos;
			isVisible = c.IsVisible;
			var cp = c as CaretPosition;
			if (cp != null) {
				Valid = cp.Valid;
			}
		}
	}

	internal class DummyCaretPosition: ICaretPosition
	{
		public static DummyCaretPosition Instance = new DummyCaretPosition();

		public bool IsValid => true;

		public int Line { get; set; }
		public int Col { get; set; }
		public int TextPos { get; set; }
		public Vector2 WorldPos { get; set; }
		public bool IsVisible {
			get { return false; }
			set { }
		}
		public void EmptyText(Vector2 pos) {}
		public void StartSync() {}
		public void Sync(int index, Vector2 charPos, Vector2 size) {}
		public void FinishSync() {}
		public void InvalidatePreservingTextPos() {}
		public void ClampTextPos(int textLength) {}
		public void ClampLine(int lineCount) {}
		public void ClampCol(int maxCol) {}
		public void NextLine() {}
		public ICaretPosition Clone() { return this; }
		public void AssignFrom(ICaretPosition c) { }
	}

	public class MultiCaretPosition : ICaretPosition
	{
		private List<ICaretPosition> carets = new List<ICaretPosition>();

		public bool IsValid => carets.All(c => c.IsValid);

		public int Line { get; set; }
		public int Col { get; set; }
		public int TextPos { get; set; }
		public Vector2 WorldPos { get; set; }
		public bool IsVisible {
			get { return carets.Any(c => c.IsVisible); }
			set { }
		}
		public void EmptyText(Vector2 pos) => carets.ForEach(c => c.EmptyText(pos));
		public void StartSync() => carets.ForEach(c => c.StartSync());
		public void Sync(int index, Vector2 charPos, Vector2 size) => carets.ForEach(c => c.Sync(index, charPos, size));
		public void FinishSync() => carets.ForEach(c => c.FinishSync());
		public void InvalidatePreservingTextPos() => carets.ForEach(c => c.InvalidatePreservingTextPos());
		public void ClampTextPos(int textLength) => carets.ForEach(c => c.ClampTextPos(textLength));
		public void ClampLine(int lineCount) => carets.ForEach(c => c.ClampLine(lineCount));
		public void ClampCol(int maxCol) => carets.ForEach(c => c.ClampCol(maxCol));
		public void NextLine() => carets.ForEach(c => c.NextLine());
		public ICaretPosition Clone() => new MultiCaretPosition { carets = carets.Select(c => c.Clone()).ToList() };

		public void AssignFrom(ICaretPosition c)
		{
			throw new InvalidOperationException();
		}

		public void Add(ICaretPosition c) => carets.Add(c);
	}
}
