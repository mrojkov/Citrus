using System;

namespace Orange
{
	public interface IPluginUIBuilder
	{
		IPluginPanel SidePanel { get; }
	}

	public interface IPluginPanel
	{
		bool Enabled { get; set; }
		string Title { get; set; }
		ICheckBox AddCheckBox(string label);
	}

	public interface ICheckBox
	{
		event EventHandler Toggled;
		bool Active { get; set; }
	}
}