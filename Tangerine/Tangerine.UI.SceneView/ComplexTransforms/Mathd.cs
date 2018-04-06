using System;

namespace Lime
{
	public static class Mathd
	{

		public static double Wrap180(double angle)
		{
			angle = (angle + 180) % 360;
			if (angle < 0) {
				angle += 360;
			}
			return angle - 180;
		}

		public static double Lerp(double amount, double value1, double value2)
		{
			return value1 + (value2 - value1) * amount;
		}

		public static Vector2d Lerp(double amount, Vector2d value1, Vector2d value2)
		{
			return value1 + (value2 - value1) * amount;
		}

	}
}
