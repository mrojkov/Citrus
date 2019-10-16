using System;
using System.Collections.Generic;
using System.Linq;

namespace Orange
{
	public class MenuItem
	{
		public string Label;
		public Func<string> Action;
		public int Priority;
		public bool ApplicableToBundleSubset;

		public MenuItem(Func<string> action, string label, int priority, bool applicableToBundleSubset)
		{
			Label = label;
			Action = action;
			Priority = priority;
			ApplicableToBundleSubset = applicableToBundleSubset;
		}

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
			foreach (Lazy<Action, IMenuItemMetadata> menuItem in PluginLoader.CurrentPlugin?.MenuItems) {
				Items.Add(new MenuItem(() => {
						menuItem.Value();
						return null;
					}, menuItem.Metadata.Label,
					menuItem.Metadata.Priority,
					menuItem.Metadata.ApplicableToBundleSubset)
				);
			}
			foreach (Lazy<Func<string>, IMenuItemMetadata> menuItem in PluginLoader.CurrentPlugin?.MenuItemsWithErrorDetails)
			{
				Items.Add(new MenuItem(
					menuItem.Value,
					menuItem.Metadata.Label,
					menuItem.Metadata.Priority,
					menuItem.Metadata.ApplicableToBundleSubset)
				);
			}
			The.UI.RefreshMenu();
		}

	}
}
