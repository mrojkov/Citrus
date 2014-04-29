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

		protected override void SelfLateUpdate(int delta)
		{
			if (Parent == null) {
				return;
			}
			var spline = Parent.Nodes.TryFind(SplineId) as Spline;
			var widget = Parent.Nodes.TryFind(WidgetId) as Widget;
			if (spline != null && widget != null) {
				float length = spline.CalcLengthRough();
				Vector2 point = spline.CalcPoint(SplineOffset * length);
				widget.Position = spline.CalcLocalToParentTransform().TransformVector(point);
				widget.Update(0);
			}
		}
	}
}