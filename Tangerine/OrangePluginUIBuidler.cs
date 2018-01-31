using System;
using Orange;

namespace Tangerine
{
	// Dummy OrangePluginUIBuidler for launching game inside Tangerine
	public class OrangePluginUIBuidler : IPluginUIBuilder
	{
		public IPluginPanel SidePanel { get; } = new OrangePluginPanel();
	}

	public class OrangePluginPanel :  IPluginPanel
	{
		private class PluginCheckBox : ICheckBox
		{
			public bool Active { get; set; }
			public event EventHandler Toggled;
		}

		public bool Enabled { get; set; }
		public string Title { get; set; }

		public ICheckBox AddCheckBox(string label)
		{
			return new PluginCheckBox();
		}
	}
}
