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

		public void RefreshMenu()
		{
			var picker = The.MainWindow.ActionPicker;
			var activeText = picker.ActiveText;
			int count = picker.Model.IterNChildren();
			for (int i = 0; i < count; i++) {
				picker.RemoveText(0);
			}
			int active = 0;
			int c = 0;
			var items = Items.FindAll(i => IsVisibleMenuItem(i));
			items.Sort((a, b) => a.Priority.CompareTo(b.Priority));
			foreach (var item in items) {
				picker.AppendText(item.Label);
				if (item.Label == activeText) {
					active = c;
				}
				c++;
			}
			picker.Active = active;
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
			RefreshMenu();
		}

		private static IEnumerable<MenuItem> ScanForMenuItems(System.Reflection.Assembly assembly)
		{
			foreach (var type in assembly.GetTypes()) {
				foreach (var method in type.GetMethods()) {
					var attrs = method.GetCustomAttributes(typeof(MenuItemAttribute), false);
					if (attrs.Length > 0) {
						if (!method.IsStatic || method.GetParameters().Length > 0) {
							throw new Lime.Exception("MenuItemAttribute is valid only for static parameterless methods");
						}
						var attr = attrs[0] as MenuItemAttribute;
						yield return CreateMenuItem(method, assembly, attr);
					}
				}
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
