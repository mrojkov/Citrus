using System;

namespace Lime
{
	public class ScrollableLayout : CommonLayout, ILayout
	{
		public override void MeasureSizeConstraints(Widget widget)
		{
			ConstraintsValid = true;
		}

		public override void ArrangeChildren(Widget widget)
		{
			ArrangementValid = true;
			foreach (var w in GetChildren(widget)) {
				w.Size = w.EffectiveMinSize;
			}
		}
	}
}