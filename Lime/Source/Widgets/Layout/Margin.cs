namespace Lime
{
	public class Margin
	{
		public float Left;
		public float Top;
		public float Right;
		public float Bottom;

		public Margin() {}

		public Margin(float overall)
		{
			Overall = overall;
		}

		public float Overall
		{
			set { Left = Top = Right = Bottom = value; }
		}

		public float Horizontal
		{
			set { Left = Right = value; }
		}

		public float Vertical
		{
			set { Top = Bottom = value; }
		}
	}
}