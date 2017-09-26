using Yuzu;

namespace Lime
{
	public class SplineGear : Node
	{
		[YuzuMember]
		public NodeReference<Widget> WidgetRef { get; set; }

		[YuzuMember]
		public NodeReference<Spline> SplineRef { get; set; }

		public Widget Widget => WidgetRef?.GetNode(Parent);
		public Spline Spline => SplineRef?.GetNode(Parent);

		[YuzuMember]
		public float SplineOffset { get; set; }

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

		protected override void SelfLateUpdate(float delta)
		{
			if (Parent == null) {
				return;
			}
			var spline = Spline;
			var widget = Widget;
			if (spline != null && widget != null) {
				var length = spline.CalcLengthRough();
				var point = spline.CalcPoint(SplineOffset * length);
				widget.Position = spline.CalcLocalToParentTransform().TransformVector(point);
			}
		}

		protected internal override void AddToRenderChain(RenderChain chain)
		{
		}
	}
}