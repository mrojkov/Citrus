using System;
using Lime;

namespace Tangerine.UI.SceneView.WidgetTransforms
{
	public class WidgetZeroScalePreserver
	{

		private readonly Widget widget;
		private float? savedScaleX;
		private float? savedScaleY;

		internal WidgetZeroScalePreserver(Widget widget)
		{
			this.widget = widget;
		}

		internal Vector2 AdjustToScale(Vector2 scale)
		{
			if (savedScaleX == null && savedScaleY == null) return scale;

			return new Vector2(savedScaleX ?? scale.X, savedScaleY ?? scale.Y);
		}

		internal void Store()
		{
			Restore();

			if (Math.Abs(widget.Scale.X) < Mathf.ZeroTolerance) {
				savedScaleX = widget.Scale.X;
				widget.Scale = new Vector2(1, widget.Scale.Y);
			}
			if (Math.Abs(widget.Scale.Y) < Mathf.ZeroTolerance) {
				savedScaleY = widget.Scale.Y;
				widget.Scale = new Vector2(widget.Scale.X, 1);
			}
		}

		internal void Restore()
		{
			if (widget != null && savedScaleX != null) {
				widget.Scale = new Vector2(savedScaleX.Value, widget.Scale.Y);
			}
			if (widget != null && savedScaleY != null) {
				widget.Scale = new Vector2(widget.Scale.X, savedScaleY.Value);
			}
			savedScaleX = null;
			savedScaleY = null;
		}

	}

}
