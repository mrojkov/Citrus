namespace Orange
{
	public partial class MainWindow
	{
		private PluginPanel pluginPanel;

		public override IPluginUIBuilder GetPluginUIBuilder()
		{
			return new PluginUIBuidler();
		}

		public override void CreatePluginUI(IPluginUIBuilder builder)
		{
			if (!builder.SidePanel.Enabled) {
				return;
			}
			pluginPanel = builder.SidePanel as PluginPanel;
			mainHBox.PackStart(pluginPanel, false, true, 0);
			NativeWindow.Resize(650, NativeWindow.DefaultHeight);
			NativeWindow.ShowAll();
		}

		public override void DestroyPluginUI()
		{
			pluginPanel?.Destroy();
			pluginPanel = null;
			NativeWindow.Resize(NativeWindow.DefaultWidth, NativeWindow.DefaultHeight);
		}
	}
}
