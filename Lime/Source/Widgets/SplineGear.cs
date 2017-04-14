using Yuzu;

namespace Lime
{
	public class SplineGear : Node
	{
		[YuzuMember]
		public NodeReference<Widget> WidgetRef { get; set; }

		[YuzuMember]
		public NodeReference<Spline> SplineRef { get; set; }

		public Widget Widget
		{
			get { return WidgetRef.Node; }
			set { WidgetRef = new NodeReference<Widget>(value); }
		}

		public Spline Spline
		{
			get { return SplineRef.Node; }
			set { SplineRef = new NodeReference<Spline>(value); }
		}

		[YuzuMember]
		public float SplineOffset { get; set; }

		protected override void RefreshReferences()
		{
			WidgetRef = WidgetRef.Resolve(Parent);
			SplineRef = SplineRef.Resolve(Parent);
		}

		protected override void SelfLateUpdate(float delta)
		{
			if (Parent == null) {
				return;
			}
			// TODO: Rework NodeReference, remove this line.
			RefreshReferences();
			if (Spline != null && Widget != null) {
				var length = Spline.CalcLengthRough();
				var point = Spline.CalcPoint(SplineOffset * length);
				Widget.Position = Spline.CalcLocalToParentTransform().TransformVector(point);
			}
		}
	}
}