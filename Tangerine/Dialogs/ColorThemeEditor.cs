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
					pane, Theme.Colors,
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
						pane, category.GetValue(ColorTheme.Current),
						color.Name,
						$"{category.Name} {color.Name}",
						() => color.GetValue(category.GetValue(ColorTheme.Current))
					);
				}
			}
			AddNode(pane);
		}

		private static void CreateColorEditor(ThemedScrollView container, object source, string propertyName, string displayName, Func<object> valueGetter)
		{
			var tmp = new Color4PropertyEditor(
				new PropertyEditorParams(
					container.Content,
					source,
					propertyName,
					displayName
				) {
					DefaultValueGetter = valueGetter
				}
			);
			tmp.ContainerWidget.AddChangeWatcher(
				new Property<Color4>(source, propertyName),
				(v) => Application.InvalidateWindows()
			);
		}
	}
}
