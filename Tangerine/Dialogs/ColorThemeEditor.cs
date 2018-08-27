using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Lime;
using Tangerine.Core;
using Tangerine.UI;

namespace Tangerine.Dialogs
{
	public class ColorThemeEditor: Widget
	{
		public ColorThemeEditor()
		{
			Rebuild();
		}

		public void Rebuild()
		{
			Nodes.Clear();
			var pane = new ThemedScrollView();
			pane.Content.Layout = new VBoxLayout { Spacing = 4 };
			pane.Content.Padding = new Thickness(10);
			var flags =
				BindingFlags.Public |
				BindingFlags.GetProperty |
				BindingFlags.SetProperty |
				BindingFlags.Instance;
			foreach (var color in typeof(Theme.ColorTheme).GetProperties(flags)) {
				if (color.PropertyType != typeof(Color4)) {
					continue;
				}
				CreateColorEditor(
					pane, AppUserPreferences.Instance.LimeColorTheme,
					color.Name,
					$"Basic {color.Name}",
					() => color.GetValue(Theme.Colors)
				);
			}
			foreach (var category in typeof(ColorTheme).GetProperties(flags)) {
				if (category.Name == "Basic") {
					continue;
				}
				foreach (var color in category.PropertyType.GetProperties(flags)) {
					if (color.PropertyType != typeof(Color4)) {
						continue;
					}
					CreateColorEditor(
						pane, category.GetValue(AppUserPreferences.Instance.ColorTheme),
						color.Name,
						$"{category.Name} {color.Name}",
						() => color.GetValue(category.GetValue(AppUserPreferences.Instance.ColorTheme))
					);
				}
			}
			AddNode(pane);
		}

		private static void CreateColorEditor(ThemedScrollView container, object source, string propertyName, string displayName, Func<object> valueGetter)
		{
			var tmp = new Color4PropertyEditor(
				new PreferencesPropertyEditorParams(
					container.Content,
					source,
					propertyName,
					displayName
				) {
					DefaultValueGetter = valueGetter
				}
			);
		}
	}
}
