using Lime;

namespace Tangerine.UI.SceneView.WidgetTransforms
{
	public struct Transform2d
	{
		public Vector2d Translation;
		public Vector2d Scale;
		public double Rotation;

		public Matrix32d ToMatrix32()
		{
			var v = Vector2d.CosSin(Rotation * Mathf.DegToRad) * Scale;
			return new Matrix32d {
				U = v.X * new Vector2d(1, 0) + v.Y * new Vector2d(0, 1),
				V = v.X * new Vector2d(0, 1) - v.Y * new Vector2d(1, 0),
				T = Translation
			};
		}
	}	
}
