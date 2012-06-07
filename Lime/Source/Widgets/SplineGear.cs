using System;
using Lime;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class SplineGear : Node
	{
		[ProtoMember(1)]
		public string WidgetId { get; set; }

		[ProtoMember(2)]
		public string SplineId { get; set; }

		[ProtoMember(3)]
		public float SplineOffset { get; set; }

		public override void Update(int delta)
		{
			base.Update(delta);
			if (Parent != null) {
				Spline spline = Parent.Nodes.Get<Spline>(SplineId);
				Widget widget = Parent.Nodes.Get<Widget>(WidgetId);
				if (spline != null && widget != null) {
					float length = spline.CalcLength();
					Vector2 point = spline.CalcPoint(SplineOffset * length);
					widget.Position = spline.CalcLocalMatrix().TransformVector(point);
					widget.Update(0);
				}
			}
		}
	}
}