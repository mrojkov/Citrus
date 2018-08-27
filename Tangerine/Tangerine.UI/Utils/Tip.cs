using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.UI
{
	public static class Tip
	{
		public static IEnumerator<object> ShowTipOnMouseOverTask(Widget source, Func<string> textGetter)
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
						WidgetContext.Current.Root.Tasks.Add(ShowTipTask(source, textGetter));
					}
				}
			}
		}

		private static IEnumerator<object> ShowTipTask(Widget source, Func<string> textGetter)
		{
			var window = WidgetContext.Current.Root;
			var tip = new Widget {
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
			tip.Position = new Vector2(tip.Position.X.Truncate(), tip.Position.Y.Truncate());
			tip.Updated += _ => tip.Size = tip.EffectiveMinSize;
			window.PushNode(tip);
			try {
				while (source.IsMouseOver()) {
					yield return null;
				}
			} finally {
				tip.Unlink();
			}
		}
	}
}
