using Yuzu;

namespace Lime
{
	[TangerineRegisterNode(Order = 17)]
	public class SplineGear : Node
	{
		[YuzuMember]
		[TangerineKeyframeColor(4)]
		public NodeReference<Widget> WidgetRef { get; set; }

		[YuzuMember]
		[TangerineKeyframeColor(5)]
		public NodeReference<Spline> SplineRef { get; set; }

		public Widget Widget => WidgetRef?.GetNode(Parent);
		public Spline Spline => SplineRef?.GetNode(Parent);

		[YuzuMember]
		[TangerineKeyframeColor(6)]
		public float SplineOffset { get; set; }

		[YuzuMember]
		[TangerineKeyframeColor(7)]
		public bool OrientToPath { get; set; }

		public SplineGear()
		{
			RenderChainBuilder = null;
		}

		public override Node Clone()
		{
			var clone = (SplineGear)base.Clone();
			clone.WidgetRef = clone.WidgetRef?.Clone();
			clone.SplineRef = clone.SplineRef?.Clone();
			return clone;
		}

		public override void Update(float delta)
		{
			base.Update(delta);
			if (Parent == null) {
				return;
			}
			var spline = Spline;
			var widget = Widget;
			if (spline != null && widget != null) {
				var length = spline.CalcPolylineLength();
				var point = spline.CalcPoint(SplineOffset * length);
				widget.Position = spline.CalcLocalToParentTransform().TransformVector(point);
				if (OrientToPath) {
					var vec = spline.CalcDerivative(SplineOffset * length);
					widget.Rotation = Mathf.Atan2(vec) * Mathf.RadToDeg;
				}
			}
		}

		public override void AddToRenderChain(RenderChain chain)
		{
		}
	}
}
