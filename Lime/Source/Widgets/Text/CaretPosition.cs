using System;

namespace Lime
{
	internal class CaretPosition: ICaretPosition
	{
		public enum ValidState { None, All, LineCol, WorldPos, TextPos, LineWorldX };
		public ValidState Valid { get; private set; }
		public int RenderingLineNumber;
		public int RenderingTextPos;

		private int line;
		private int col;
		private int textPos;
		private Vector2 worldPos;
		private bool isVisible;
		private Vector2 nearestCharPos;

		public int Line {
			get { return line; }
			set {
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
			set {
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

		public int TextPos
		{
			get { return textPos; }
			set
			{
				if (textPos == value) return;
				Valid = ValidState.TextPos;
				textPos = value;
			}
		}

		public Vector2 WorldPos
		{
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
			RenderingLineNumber = 0;
			RenderingTextPos = 0;
			nearestCharPos = Vector2.PositiveInfinity;
		}

		public void Sync(int index, Vector2 charPos, Vector2 size)
		{
			switch (Valid) {
				case ValidState.None:
				case ValidState.All:
					break;
				case ValidState.LineCol:
					if (Line == RenderingLineNumber && Col == index) {
						textPos = RenderingTextPos;
						worldPos = charPos;
						Valid = ValidState.All;
					}
					break;
				case ValidState.TextPos:
					if (TextPos == RenderingTextPos) {
						line = RenderingLineNumber;
						col = index;
						worldPos = charPos;
						Valid = ValidState.All;
					}
					break;
				case ValidState.WorldPos:
					if ((WorldPos - charPos).SqrLength < (WorldPos - nearestCharPos).SqrLength) {
						line = RenderingLineNumber;
						col = index;
						textPos = RenderingTextPos;
						nearestCharPos = charPos;
						Valid = ValidState.WorldPos;
					}
					break;
				case ValidState.LineWorldX:
					if (
						Line == RenderingLineNumber &&
						(WorldPos.X - charPos.X).Abs() < (WorldPos.X - nearestCharPos.X).Abs())
					{
						col = index;
						textPos = RenderingTextPos;
						nearestCharPos = charPos;
					}
					break;
			}
			++RenderingTextPos;
		}

		public void FinishSync()
		{
			if (Valid == ValidState.WorldPos || Valid == ValidState.LineWorldX)
				worldPos = nearestCharPos;
			Valid = ValidState.All;
		}

		public void InvalidatePreservingTextPos()
		{
			Valid = ValidState.TextPos;
		}

		public void Clamp(int textLength, int lineCount)
		{
			if (Valid == ValidState.TextPos)
				textPos = textPos.Clamp(0, textLength);
			if (Valid == ValidState.LineCol || Valid == ValidState.LineWorldX)
				line = line.Clamp(0, lineCount - 1);
		}

		public CaretPosition Clone()
		{
			return (CaretPosition)MemberwiseClone();
		}
	}
}
