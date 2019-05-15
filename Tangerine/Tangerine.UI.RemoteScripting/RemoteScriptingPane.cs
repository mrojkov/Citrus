using Lime;
using Tangerine.Core;
using Tangerine.UI.Docking;

namespace Tangerine.UI.RemoteScripting
{
	public class RemoteScriptingPane
	{
		public static RemoteScriptingPane Instance;

		private Panel panel;
		private Widget dockPanelWidget;

		public RemoteScriptingPane(Panel panel)
		{
			Instance = this;
			this.panel = panel;
			dockPanelWidget = panel.ContentWidget;
			dockPanelWidget.AddChangeWatcher(() => Project.Current.CitprojPath, path => Initialize());
		}

		private void Initialize() { }
	}
}
