using System;
using Lime;

namespace Tangerine.UI
{
	public class Console
	{
		public static Console Instance { get; private set; }

		public readonly Widget RootWidget;
		public readonly Frame ScrollViewWidget;
		public readonly Widget ContentWidget;

		public static void Initialize(Widget rootWidget)
		{
			Instance = new Console(rootWidget);
		}

		private Console(Widget rootWidget)
		{
			RootWidget = rootWidget;
			ContentWidget = new ScrollView((Frame)RootWidget).Content;
			InitializeWidgets();
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

		void InitializeWidgets()
		{
			RootWidget.Layout = new StackLayout { VerticallySizeable = true };
			ContentWidget.Layout = new VBoxLayout { Tag = "ConsoleContent", Spacing = 4 };
			ContentWidget.Padding = new Thickness(4);
		}
	}
}

