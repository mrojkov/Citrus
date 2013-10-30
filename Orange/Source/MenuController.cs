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
		public Assembly Assembly;
	}

	public class MenuController
	{
		public static readonly MenuController Instance = new MenuController();

		public readonly List<MenuItem> Items = new List<MenuItem>();

		public List<MenuItem> GetVisibleAndSortedItems()
		{
			var items = Items.FindAll(IsVisibleMenuItem);
			items.Sort((a, b) => a.Priority.CompareTo(b.Priority));
			return items;
		}

		private bool IsVisibleMenuItem(MenuItem item)
		{
			if (item.Assembly == System.Reflection.Assembly.GetExecutingAssembly()) {
				return true;
			}
			if (item.Assembly == PluginLoader.CurrentPlugin) {
				return true;
			}
			return false;
		}

		public void CreateAssemblyMenuItems(System.Reflection.Assembly assembly)
		{
			var items = new List<MenuItem>(ScanForMenuItems(assembly));
			Items.AddRange(items);
			The.UI.RefreshMenu();
		}

		private static IEnumerable<MenuItem> ScanForMenuItems(System.Reflection.Assembly assembly)
		{
			foreach (var method in assembly.GetAllMethodsWithAttribute(typeof(MenuItemAttribute))) {
				if (!method.IsStatic || method.GetParameters().Length > 0) {
					throw new Lime.Exception("MenuItemAttribute is valid only for static parameterless methods");
				}
				var attrs = method.GetCustomAttributes(typeof(MenuItemAttribute), false);
				var attr = attrs[0] as MenuItemAttribute;
				yield return CreateMenuItem(method, assembly, attr);
			}
		}

		private static MenuItem CreateMenuItem(MethodInfo method, Assembly assembly, MenuItemAttribute attr)
		{
			return new MenuItem() {
				Action = () => method.Invoke(null, null),
				Label = attr.Label,
				Priority = attr.Priority,
				Assembly = assembly
			};
		}
	}
}
