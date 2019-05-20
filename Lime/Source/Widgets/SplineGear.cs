using Yuzu;

namespace Lime
{
	[TangerineRegisterNode(Order = 17)]
	public class SplineGear : Node, IUpdatableNode
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
		[TangerineValidRange(0f, 1f)]
		public float SplineOffset { get; set; }

		[YuzuMember]
		[TangerineKeyframeColor(7)]
		public bool AlongPathOrientation { get; set; }

		public SplineGear()
		{
			RenderChainBuilder = null;
			Components.Add(new UpdatableNodeBehaviour());
		}

		protected override Node CloneInternal()
		{
			var clone = (SplineGear)base.CloneInternal();
			clone.WidgetRef = clone.WidgetRef?.Clone();
			clone.SplineRef = clone.SplineRef?.Clone();
			return clone;
		}

		public virtual void OnUpdate(float delta)
		{
			if (Parent == null) {
				return;
			}
			var spline = Spline;
			var widget = Widget;
			if (spline != null && widget != null) {
				var length = spline.CalcPolylineLength();
				var point = spline.CalcPoint(SplineOffset * length);
				widget.Position = spline.CalcLocalToParentTransform().TransformVector(point);
				if (AlongPathOrientation) {
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
