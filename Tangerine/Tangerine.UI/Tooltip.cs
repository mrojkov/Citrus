using System;
using System.Collections.Generic;

namespace Lime
{
	public class Tooltip
	{
		private readonly Widget tooltip;
		private readonly Window tooltipWindow;
		private readonly ThemedInvalidableWindowWidget rootWidget;
		private ThemedSimpleText tooltipText;

		private static Tooltip instance;
		public static Tooltip Instance => instance ?? (instance = new Tooltip());

		private Tooltip()
		{
			tooltipWindow = new Window(new WindowOptions {
				Style = WindowStyle.Borderless,
				FixedSize = false,
				Visible = false,
				Centered = false,
				ToolWindow = true,
			});
			tooltip = new ThemedFrame {
				LayoutCell = new LayoutCell { Ignore = true },
				Layout = new StackLayout(),
				Nodes = {
					(tooltipText = new ThemedSimpleText { Padding = new Thickness(4) }),
				},
				Presenter = new ThemedFramePresenter(Color4.Yellow.Transparentify(0.8f), Color4.Black)
			};
			rootWidget = new ThemedInvalidableWindowWidget(tooltipWindow) {
				Padding = new Thickness(8),
				Layout = new VBoxLayout(),
				Nodes = {
					tooltip
				}
			};
		}

		public IEnumerator<object> ShowOnMouseOverTask(Widget source, Func<string> textGetter)
		{
			while (true) {
				yield return null;
				if (source.IsMouseOver() && textGetter() != null) {
					var showTooltip = true;
					for (float t = 0; t < 0.5f; t += Lime.Task.Current.Delta) {
						if (!source.IsMouseOver()) {
							showTooltip = false;
							break;
						}
						yield return null;
					}
					if (showTooltip) {
						var wasResized = false;
						tooltipText.Text = textGetter();
						var pos = Application.Input.DesktopMousePosition + new Vector2(0, source.Height);
						while (source.IsMouseOver()) {
							yield return null;
							if (!wasResized) {
								wasResized = true;
								tooltipWindow.Visible = true;
								if (pos.X + tooltip.Width >= Environment.GetDesktopSize().X) {
									pos.X = Environment.GetDesktopSize().X - tooltip.Width - Theme.Metrics.ControlsPadding.Right;
								}
								if (pos.Y + tooltip.Height >= Environment.GetDesktopSize().Y) {
									pos.Y -= 2 * tooltip.Height + Theme.Metrics.ControlsPadding.Bottom;
								}
								tooltipWindow.ClientSize = tooltipWindow.DecoratedSize = tooltip.Size = tooltip.EffectiveMinSize;
								tooltipWindow.ClientPosition = tooltipWindow.DecoratedPosition = new Vector2(pos.X.Truncate(), pos.Y.Truncate());
							}
						}
						tooltipWindow.Visible = false;
					}
				}
			}
		}
	}
}
