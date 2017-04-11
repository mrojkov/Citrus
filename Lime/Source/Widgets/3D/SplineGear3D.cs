using Yuzu;

namespace Lime
{
	public class SplineGear3D : Node
	{
		[YuzuMember]
		public NodeReference<Node3D> NodeRef { get; set; }

		[YuzuMember]
		public NodeReference<Spline3D> SplineRef { get; set; }

		public Node3D Node
		{
			get { return NodeRef.Node; }
			set { NodeRef = new NodeReference<Node3D>(value); }
		}

		public Spline3D Spline
		{
			get { return SplineRef.Node; }
			set { SplineRef = new NodeReference<Spline3D>(value); }
		}

		[YuzuMember]
		public float SplineOffset { get; set; }

		protected override void RefreshReferences()
		{
			NodeRef = NodeRef.Resolve(Parent);
			SplineRef = SplineRef.Resolve(Parent);
		}

		protected override void SelfLateUpdate(float delta)
		{
			if (Parent == null) {
				return;
			}
			if (Spline != null && Node != null) {
				var length = Spline.CalcLengthRough();
				var transform = Spline.CalcPointTransform(SplineOffset * length);
				NodeRef.Node.Position = Vector3.Zero * transform;
				NodeRef.Node.Rotation = Quaternion.CreateFromRotationMatrix(transform);;
			}
		}
	}
}