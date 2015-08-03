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
			get { return Left == Top && Left == Right && Left == Bottom ? Left : float.NaN; }
			set { Left = Top = Right = Bottom = value; }
		}

		public float Horizontal
		{
			get { return Left == Right ? Left : float.NaN; }
			set { Left = Right = value; }
		}

		public float Vertical
		{
			get { return Top == Bottom ? Top : float.NaN; }
			set { Top = Bottom = value; }
		}
	}
}