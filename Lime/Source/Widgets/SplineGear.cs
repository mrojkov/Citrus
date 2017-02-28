using Yuzu;

namespace Lime
{
	public class SplineGear : Node
	{
		[YuzuMember]
		public NodeReference<Widget> WidgetRef;

		[YuzuMember]
		public NodeReference<Spline> SplineRef;

		public Widget Widget
		{
			get { return WidgetRef.Node; }
			set { WidgetRef.Node = value; }
		}

		public Spline Spline
		{
			get { return SplineRef.Node; }
			set { SplineRef.Node = value; }
		}

		[YuzuMember]
		public float SplineOffset { get; set; }

		protected override void LookupReferences()
		{
			WidgetRef.LookupNode(Parent);
			SplineRef.LookupNode(Parent);
		}

		protected override void SelfLateUpdate(float delta)
		{
			if (Parent == null) {
				return;
			}
			if (Spline != null && Widget != null) {
				float length = Spline.CalcLengthRough();
				Vector2 point = Spline.CalcPoint(SplineOffset * length);
				Widget.Position = Spline.CalcLocalToParentTransform().TransformVector(point);
				Widget.Update(0);
			}
		}
	}
}