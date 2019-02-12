using System;
using System.Collections.Generic;

namespace Lime
{
	public static class Tooltip
	{
		public static IEnumerator<object> ShowTooltipOnMouseOverTask(Widget source, Func<string> textGetter)
		{
			while (true) {
				yield return null;
				if (source.IsMouseOver() && textGetter() != null) {
					var showTip = true;
					for (float t = 0; t < 0.5f; t += Lime.Task.Current.Delta) {
						if (!source.IsMouseOver()) {
							showTip = false;
							break;
						}
						yield return null;
					}
					if (showTip) {
						WidgetContext.Current.Root.Tasks.Add(ShowTooltipTask(source, textGetter));
					}
				}
			}
		}

		private static IEnumerator<object> ShowTooltipTask(Widget source, Func<string> textGetter)
		{
			var window = WidgetContext.Current.Root;
			var tooltip = new Widget {
				Position = source.CalcPositionInSpaceOf(window) +
					new Vector2(source.Width * 0.66f, source.Height),
				Size = Vector2.Zero,
				LayoutCell = new LayoutCell { Ignore = true },
				Layout = new StackLayout(),
				Nodes = {
					new ThemedSimpleText { Text = textGetter(), Padding = new Thickness(4) },
					new ThemedFrame()
				}
			};
			tooltip.Position = new Vector2(tooltip.Position.X.Truncate(), tooltip.Position.Y.Truncate());
			tooltip.Updated += _ => tooltip.Size = tooltip.EffectiveMinSize;
			window.PushNode(tooltip);
			try {
				while (source.IsMouseOver()) {
					yield return null;
				}
			} finally {
				tooltip.Unlink();
			}
		}
	}
}
