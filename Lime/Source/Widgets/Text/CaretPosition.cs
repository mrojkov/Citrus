namespace Lime
{
	internal class CaretPosition: ICaretPosition
	{
		public enum ValidState { None, All, LinePos, WorldPos, TextPos };
		public ValidState Valid;
		public int RenderingLineNumber;
		public int RenderingTextPos;
		public Vector2 NearestCharPos;

		private int line;
		private int col;
		private int textPos;
		private Vector2 worldPos;
		private bool isVisible;

		public int Line {
			get { return line; }
			set {
				if (line == value) return;
				Valid = ValidState.LinePos;
				line = value;
			}
		}

		public int Col {
			get { return col; }
			set {
				if (col == value) return;
				Valid = ValidState.LinePos;
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

		public void Sync(int index, Vector2 charPos, Vector2 size)
		{
			switch (Valid) {
				case ValidState.None:
				case ValidState.All:
					break;
				case ValidState.LinePos:
					if (Line == RenderingLineNumber && Col == index) {
						TextPos = RenderingTextPos;
						WorldPos = charPos;
						Valid = ValidState.All;
					}
					break;
				case ValidState.TextPos:
					if (TextPos == RenderingTextPos) {
						Line = RenderingLineNumber;
						Col = index;
						WorldPos = charPos;
						Valid = ValidState.All;
					}
					break;
				case ValidState.WorldPos:
					if ((WorldPos - charPos).SqrLength < (WorldPos - NearestCharPos).SqrLength) {
						Line = RenderingLineNumber;
						Col = index;
						TextPos = RenderingTextPos;
						NearestCharPos = charPos;
						Valid = ValidState.WorldPos;
					}
					break;
			}
			++RenderingTextPos;
		}

		public CaretPosition Clone()
		{
			return (CaretPosition)MemberwiseClone();
		}
	}
}
