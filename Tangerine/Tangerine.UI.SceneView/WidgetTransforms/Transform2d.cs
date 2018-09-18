using System;

namespace Tangerine.UI.SceneView.WidgetTransforms
{
	public struct Transform2d
	{
		public Vector2d Translation;
		public Vector2d Scale;
		public double Rotation;

		public Transform2d(Vector2d translation, Vector2d scale, double rotation)
		{
			Translation = translation;
			Scale = scale;
			Rotation = rotation;
		}

		public Matrix32d ToMatrix32()
		{
			var v = Vector2d.CosSin(Rotation * Math.PI / 180.0);
			return new Matrix32d {
				U = new Vector2d(v.X, v.Y) * Scale.X,
				V = new Vector2d(-v.Y, v.X) * Scale.Y,
				T = Translation
			};
		}
	}	
}
