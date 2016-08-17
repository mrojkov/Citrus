using Yuzu;

namespace Lime
{
	public class PointObject : Node
	{
		private Vector2 position;

		public PointObject()
		{
			// Just in sake of optimization set Presenter to null because all of PointObjects have empty Render() methods.
			Presenter = null;
		}

		[YuzuMember]
		public Vector2 Position { get { return position; } set { position = value; } }

		public float X { get { return position.X; } set { position.X = value; } }

		public float Y { get { return position.Y; } set { position.Y = value; } }

		[YuzuMember]
		public SkinningWeights SkinningWeights { get; set; }
	}
}
