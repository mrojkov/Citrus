using Yuzu;

namespace Lime
{
	public class PointObject : Node
	{
		private Vector2 position;

		public PointObject()
		{
#if !TANGERINE
			RenderChainBuilder = null;
#endif // !TANGERINE
		}

		[YuzuMember]
		[TangerineKeyframeColor(27)]
		public Vector2 Position { get { return position; } set { position = value; } }

		public float X { get { return position.X; } set { position.X = value; } }
		public float Y { get { return position.Y; } set { position.Y = value; } }

		[YuzuMember]
		[TangerineStaticProperty]
		public SkinningWeights SkinningWeights { get; set; }

		public virtual Vector2 Offset { get; set; }

		public Vector2 TransformedPosition { get; private set; }

		public override void Update(float delta)
		{
			base.Update(delta);
			RecalcTransformedPosition();
		}

		private void RecalcTransformedPosition()
		{
			TransformedPosition = Offset;
			if (Parent?.AsWidget != null) {
				TransformedPosition = Parent.AsWidget.Size * Position + Offset;
			}
			if (SkinningWeights != null && Parent?.Parent != null) {
				BoneArray a = Parent.Parent.AsWidget.BoneArray;
				Matrix32 m1 = Parent.AsWidget.CalcLocalToParentTransform();
				Matrix32 m2 = m1.CalcInversed();
				TransformedPosition = m2.TransformVector(a.ApplySkinningToVector(m1.TransformVector(TransformedPosition), SkinningWeights));
			}
		}

		public Vector2 CalcPositionInSpaceOf(Widget container)
		{
			var matrix = Parent.AsWidget.CalcTransitionToSpaceOf(container);
			return matrix.TransformVector(TransformedPosition);
		}

		public override void AddToRenderChain(RenderChain chain)
		{
		}
	}
}
