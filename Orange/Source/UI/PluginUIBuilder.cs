using Gtk;

namespace Orange
{
	public class PluginUIBuidler : IPluginUIBuilder
	{
		public IPluginPanel SidePanel { get; } = new PluginPanel();
	}

	public class PluginPanel : HBox, IPluginPanel
	{
		private readonly Label label = new Label();
		private readonly VBox panel = new VBox();

		public PluginPanel()
		{
			panel.PackStart(label, false, true, 6);
			panel.PackStart(new HSeparator(), false, true, 6);
			PackStart(new VSeparator(), false, true, 6);
			PackStart(panel, false, true, 6);
			Title = "Plugin title";
		}
		public bool Enabled { get; set; }

		public string Title
		{
			get { return label.Text; }
			set { label.Text = value; }
		}

		public ICheckBox AddCheckBox(string label)
		{
			var checkBox = new CheckBox(label);
			panel.PackStart(checkBox, false, true, 6);
			return checkBox;
		}

		private class CheckBox : CheckButton, ICheckBox
		{
			public CheckBox(string label) : base(label)
			{
				Active = false;
			}
		}
	}
}