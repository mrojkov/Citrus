using System;
using Lime;

namespace Orange
{
	public class PluginUIBuidler : IPluginUIBuilder
	{
		public IPluginPanel SidePanel { get; } = new PluginPanel();
	}

	public class PluginPanel : Widget, IPluginPanel
	{
		private readonly SimpleText label = new SimpleText { Anchors = Anchors.LeftRight };

		public PluginPanel()
		{
			MinMaxWidth = 150;
			Layout = new VBoxLayout { Spacing = 10 };
			AddNode(label);
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
			var checkBox = new PluginCheckBox(label);
			AddNode(checkBox);
			return checkBox;
		}

		private class PluginCheckBox : Widget, ICheckBox
		{
			private SimpleText label;
			private CheckBox checkBox;

			public PluginCheckBox(string label)
			{
				Layout = new HBoxLayout { Spacing = 8 };
				SetupCheckBox();
				SetupLabel(label);
			}

			private void SetupCheckBox()
			{
				checkBox = new CheckBox { Checked = false };
				checkBox.Changed += value => Toggled?.Invoke(this, EventArgs.Empty);
				AddNode(checkBox);
			}

			private void SetupLabel(string text)
			{
				label = new SimpleText(text) {
					HitTestTarget = true,
					Clicked = checkBox.Toggle
				};
				AddNode(label);
			}

			public event EventHandler Toggled;

			public bool Active
			{
				get { return checkBox.Checked; }
				set { checkBox.Checked = value; }
			}
		}
	}
}