using Lime;
using Tangerine.Core;
using Tangerine.UI.Docking;

namespace Tangerine.UI.RemoteScripting
{
	public class RemoteScriptingPane
	{
		public static RemoteScriptingPane Instance;

		private readonly Panel panel;
		private Widget rootWidget;
		private RemoteScriptingTabbedWidget tabbedWidget;
		private RemoteScriptingStatusBar statusBar;

		public RemoteScriptingPane(Panel panel)
		{
			Instance = this;
			this.panel = panel;
			panel.ContentWidget.AddChangeWatcher(() => Project.Current.CitprojPath, path => Initialize());
		}

		private void Initialize()
		{
			CleanUp();
			InitializeWidgets();
		}

		private void InitializeWidgets()
		{
			statusBar = new RemoteScriptingStatusBar {
				MinMaxHeight = 25f
			};
			rootWidget = new Widget {
				Layout = new VBoxLayout(),
				Nodes = {
					(tabbedWidget = new RemoteScriptingTabbedWidget(statusBar)),
					statusBar,
				}
			};
			panel.ContentWidget.PushNode(rootWidget);
		}

		private void CleanUp()
		{
			tabbedWidget?.UnlinkAndDispose();
			tabbedWidget = null;
			statusBar?.UnlinkAndDispose();
			statusBar = null;
			rootWidget?.UnlinkAndDispose();
			rootWidget = null;
		}
	}
}
