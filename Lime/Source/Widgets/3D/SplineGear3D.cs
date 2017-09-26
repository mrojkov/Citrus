using Yuzu;

namespace Lime
{
	[AllowedParentTypes(typeof(Node3D), typeof(Viewport3D))]
	public class SplineGear3D : Node
	{
		[YuzuMember]
		public NodeReference<Node3D> NodeRef { get; set; }

		[YuzuMember]
		public NodeReference<Spline3D> SplineRef { get; set; }

		public Node3D Node => NodeRef?.GetNode(Parent);
		public Spline3D Spline => SplineRef?.GetNode(Parent);

		[YuzuMember]
		public float SplineOffset { get; set; }

		public SplineGear3D()
		{
			RenderChainBuilder = null;
		}

		public override Node Clone()
		{
			var clone = (SplineGear3D)base.Clone();
			clone.NodeRef = clone.NodeRef?.Clone();
			clone.SplineRef = clone.SplineRef?.Clone();
			return clone;
		}

		protected override void SelfLateUpdate(float delta)
		{
			if (Parent == null) {
				return;
			}
			var spline = Spline;
			var node = Node;
			if (spline != null && node != null) {
				var length = spline.CalcLengthRough();
				var transform = spline.CalcPointTransform(SplineOffset * length);
				node.Position = Vector3.Zero * transform;
				node.Rotation = Quaternion.CreateFromRotationMatrix(transform);
			}
		}

		protected internal override void AddToRenderChain(RenderChain chain)
		{
		}
	}
}