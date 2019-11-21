using Yuzu;

namespace Lime
{
	public class PointObject : Node
	{
		private Vector2 position;
		private Vector2 transformedPosition;
		private Vector2 offset;

		public PointObject()
		{
#if !TANGERINE
			RenderChainBuilder = null;
#endif // !TANGERINE
		}

		[YuzuMember]
		[TangerineKeyframeColor(27)]
		public Vector2 Position
		{
			get => position;
			set
			{
				if (position != value) {
					DirtyMask |= DirtyFlags.LocalTransform;
					position = value;
				}
			}
		}

		public float X
		{
			get => position.X;
			set {
				if (position.X != value) {
					DirtyMask |= DirtyFlags.LocalTransform;
					position.X = value;
				}
			}
		}
		public float Y
		{
			get => position.Y;
			set {
				if (position.Y != value) {
					DirtyMask |= DirtyFlags.LocalTransform;
					position.Y = value;
				}
			}
		}

		[YuzuMember]
		[TangerineStaticProperty]
		public SkinningWeights SkinningWeights { get; set; }

		public virtual Vector2 Offset
		{
			get => offset;
			set
			{
				if (offset != value) {
					DirtyMask |= DirtyFlags.LocalTransform;
					offset = value;
				}
			}

		}

		public Vector2 TransformedPosition
		{
			get
			{
				RecalcTransformedPositionIfNeeded();
				return transformedPosition;
			}
		}

		public void RecalcTransformedPositionIfNeeded()
		{
			if (CleanDirtyFlags(DirtyFlags.LocalTransform | DirtyFlags.GlobalTransform | DirtyFlags.GlobalTransformInverse)) {
				RecalcTransformedPosition();
			}
		}

		private void RecalcTransformedPosition()
		{
			var parentWidget = Parent?.AsWidget;
			var prevTransformedPosition = transformedPosition;
			transformedPosition = Offset;
			if (parentWidget != null) {
				transformedPosition = parentWidget.Size * Position + Offset;
			}
			if (SkinningWeights != null && Parent?.Parent != null) {
				BoneArray a = Parent.Parent.AsWidget.BoneArray;
				Matrix32 m1 = parentWidget.CalcLocalToParentTransform();
				Matrix32 m2 = m1.CalcInversed();
				transformedPosition = m2.TransformVector(a.ApplySkinningToVector(m1.TransformVector(transformedPosition), SkinningWeights));
			}
			if (transformedPosition != prevTransformedPosition) {
			 	parentWidget.ExpandBoundingRect(transformedPosition);
			}
		}

		public Vector2 CalcPositionInSpaceOf(Widget container)
		{
			return CalcPositionInSpaceOf(container.LocalToWorldTransform);
		}

		public Vector2 CalcPositionInSpaceOf(Matrix32 matrix)
		{
			var t = Parent.AsWidget.CalcTransitionToSpaceOf(matrix);
			return t.TransformVector(TransformedPosition);
		}

		public override void AddToRenderChain(RenderChain chain)
		{
		}
	}
}
