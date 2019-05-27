using Lime;

namespace Tangerine.UI.RemoteScripting
{
	internal class RemoteScriptingAssemblyPage : RemoteScriptingTabbedWidget.Page
	{
		private readonly RemoteScriptingStatusBar statusBar;
		private ThemedTextView assemblyBuilderLog;

		public RemoteScriptingAssemblyPage(RemoteScriptingStatusBar statusBar)
		{
			this.statusBar = statusBar;
		}

		public override void Initialize()
		{
			Tab = new ThemedTab { Text = "Assembly" };
			RemoteScriptingTabbedWidget.Toolbar toolbar;
			Content = new Widget {
				Layout = new VBoxLayout(),
				Nodes = {
					(toolbar = new RemoteScriptingTabbedWidget.Toolbar()),
					(assemblyBuilderLog = new RemoteScriptingTabbedWidget.TextView())
				}
			};
			toolbar.Content.Nodes.AddRange(
				new ToolbarButton(IconPool.GetTexture("RemoteScripting.Build")) {
					Clicked = () => { },
				}
			);
		}
	}
}
