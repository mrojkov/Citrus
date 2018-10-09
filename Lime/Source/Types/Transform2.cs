using System;

namespace Lime
{
	public struct Transform2
	{
		public Vector2 Translation;
		public Vector2 Scale;
		public float Rotation;

		public static Transform2 Lerp(float t, Transform2 a, Transform2 b)
		{
			return new Transform2 {
				Translation = Mathf.Lerp(t, a.Translation, b.Translation),
				Scale = Mathf.Lerp(t, a.Scale, b.Scale),
				Rotation = Mathf.Lerp(t, a.Rotation, b.Rotation),
			};
		}

		public Matrix32 ToMatrix32()
		{
			var v = Vector2.CosSin(Rotation * Mathf.DegToRad) * Scale;
			return new Matrix32 {
				U = v.X * new Vector2(1, 0) + v.Y * new Vector2(0, 1),
				V = v.X * new Vector2(0, 1) - v.Y * new Vector2(1, 0),
				T = Translation
			};
		}
	}
}
