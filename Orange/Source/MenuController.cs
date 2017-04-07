using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Orange
{
	public class MenuItem
	{
		public string Label;
		public Action Action;
		public int Priority;
	}

	public class MenuController
	{
		public static readonly MenuController Instance = new MenuController();

		public readonly List<MenuItem> Items = new List<MenuItem>();

		public List<MenuItem> GetVisibleAndSortedItems()
		{
			var items = Items.ToList();
			items.Sort((a, b) => a.Priority.CompareTo(b.Priority));
			return items;
		}

		public void CreateAssemblyMenuItems()
		{
			Items.Clear();
			if (PluginLoader.CurrentPlugin == null) {
				return;
			}
			foreach (var i in PluginLoader.CurrentPlugin?.MenuItems) {
				Items.Add(new MenuItem() {
					Action = i.Value,
					Label = i.Metadata.Label,
					Priority = i.Metadata.Priority,
				});
			}
			The.UI.RefreshMenu();
		}

	}
}
