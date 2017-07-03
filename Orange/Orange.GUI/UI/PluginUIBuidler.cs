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
		private readonly SimpleText label = new ThemedSimpleText { Anchors = Anchors.LeftRight };

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

		private class PluginCheckBox : CheckBoxWithLabel, ICheckBox
		{
			public PluginCheckBox(string label) : base(label)
			{
				CheckBox.Changed += value => Toggled?.Invoke(this, EventArgs.Empty);
			}

			public event EventHandler Toggled;

			public bool Active
			{
				get { return CheckBox.Checked; }
				set { CheckBox.Checked = value; }
			}
		}
	}

	public class CheckBoxWithLabel: Widget
	{
		public CheckBoxWithLabel(string text)
		{
			Layout = new HBoxLayout { Spacing = 8 };
			AddNode(CheckBox = new ThemedCheckBox());
			Label = new ThemedSimpleText(text) {
				HitTestTarget = true,
				Clicked = CheckBox.Toggle
			};
			AddNode(Label);
		}

		public CheckBox CheckBox { get; }

		public bool Checked
		{
			get { return CheckBox.Checked; }
			set { CheckBox.Checked = value; }
		}

		public SimpleText Label { get; }
	}
}