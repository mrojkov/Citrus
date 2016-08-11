using System;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class Console : IDocumentView
	{
		public static Console Instance { get; private set; }

		public readonly Widget PanelWidget;
		public readonly Frame RootWidget;
		public readonly Frame ScrollViewWidget;
		public readonly Widget ContentWidget;

		public Console(Widget rootWidget)
		{
			PanelWidget = rootWidget;
			RootWidget = new Frame { Layout = new StackLayout { VerticallySizeable = true } };
			ContentWidget = new ScrollView(RootWidget).Content;
			ContentWidget.Layout = new VBoxLayout { Tag = "ConsoleContent", Spacing = 4 };
			ContentWidget.Padding = new Thickness(4);
			Logger.OnWrite += text => {
				var widget = new SimpleText {
					Text = $"{DateTime.Now.ToString("hh:mm:ss")} {text}",
					AutoSizeConstraints = false,
					FontHeight = DesktopTheme.Metrics.TextHeight * 0.8f
				};
				ContentWidget.PushNode(widget);
				var i = ContentWidget.Nodes.Count;
				if (i >= 100) {
					ContentWidget.Nodes.RemoveAt(i - 1);
				}
			};
		}

		public void Attach()
		{
			Instance = this;
			PanelWidget.PushNode(RootWidget);
		}

		public void Detach()
		{
			Instance = null;
			RootWidget.Unlink();
		}
	}
}
