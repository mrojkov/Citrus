using System;
using System.Collections.Generic;
using Orange;

namespace Tangerine
{
	public class OrangePluginUIBuidler : IPluginUIBuilder
	{
		public IPluginPanel SidePanel { get; } = new OrangePluginPanel();
	}

	public class OrangePluginPanel :  IPluginPanel
	{
		public class PluginCheckBox : ICheckBox
		{
			public string Label { get; }
			public bool Active { get; set; }
			public event EventHandler Toggled;

			public PluginCheckBox(string label)
			{
				Label = label;
			}

			public void Toogle()
			{
				Toggled?.Invoke(this, EventArgs.Empty);
			}
		}

		public bool Enabled { get; set; }
		public string Title { get; set; }
		public List<PluginCheckBox> CheckBoxes { get; } = new List<PluginCheckBox>();

		public ICheckBox AddCheckBox(string label)
		{
			var checkBox = new PluginCheckBox(label);
			CheckBoxes.Add(checkBox);
			return checkBox;
		}
	}
}
