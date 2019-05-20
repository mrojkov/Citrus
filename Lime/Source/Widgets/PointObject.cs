using Yuzu;

namespace Lime
{
	public class PointObject : Node, IUpdatableNode
	{
		private Vector2 position;
		private Vector2 transformedPosition;
		private Vector2 offset;

		public PointObject()
		{
			Components.Add(new UpdatableNodeBehaviour());
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
					DirtyMask |= DirtyFlags.LocalTransform | DirtyFlags.ParentBoundingRect;
					position = value;
				}
			}
		}

		public float X
		{
			get => position.X;
			set {
				if (position.X != value) {
					DirtyMask |= DirtyFlags.LocalTransform | DirtyFlags.ParentBoundingRect;
					position.X = value;
				}
			}
		}
		public float Y
		{
			get => position.Y;
			set {
				if (position.Y != value) {
					DirtyMask |= DirtyFlags.LocalTransform | DirtyFlags.ParentBoundingRect;
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
					DirtyMask |= DirtyFlags.LocalTransform | DirtyFlags.ParentBoundingRect;
					offset = value;
				}
			}

		}
		
		public Vector2 TransformedPosition
		{
			get
			{
				if (CleanDirtyFlags(DirtyFlags.LocalTransform | DirtyFlags.GlobalTransform | DirtyFlags.GlobalTransformInverse)) {
					RecalcTransformedPosition();
				}
				return transformedPosition;
			}
		}
		
		public virtual void OnUpdate(float delta)
		{
			RecalcTransformedPosition();
		}

		private void RecalcTransformedPosition()
		{
			transformedPosition = Offset;
			if (Parent?.AsWidget != null) {
				transformedPosition = Parent.AsWidget.Size * Position + Offset;
			}
			if (SkinningWeights != null && Parent?.Parent != null) {
				BoneArray a = Parent.Parent.AsWidget.BoneArray;
				Matrix32 m1 = Parent.AsWidget.CalcLocalToParentTransform();
				Matrix32 m2 = m1.CalcInversed();
				transformedPosition = m2.TransformVector(a.ApplySkinningToVector(m1.TransformVector(transformedPosition), SkinningWeights));
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
