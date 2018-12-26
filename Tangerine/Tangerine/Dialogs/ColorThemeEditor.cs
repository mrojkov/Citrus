using System;
using System.Reflection;
using Lime;
using Tangerine.Core;
using Tangerine.UI;

namespace Tangerine.Dialogs
{
	public class ColorThemeEditor: Widget
	{
		private ThemedScrollView pane;
		private ThemedEditBox searchBox;

		public int Version { get; private set; }

		public ColorThemeEditor()
		{
			Rebuild();
		}

		public void Rebuild()
		{
			Nodes.Clear();
			pane = new ThemedScrollView();
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
			AddNode(CreateSearchBox());
			AddNode(pane);
		}

		private Widget CreateSearchBox()
		{
			searchBox = new ThemedEditBox();
			searchBox.AddChangeWatcher(
				() => searchBox.Text,
				_ => ApplyColorSearch()
			);
			return new Widget {
				Layout = new HBoxLayout(),
				LayoutCell = new LayoutCell { StretchX = 2 },
				Nodes = {
					new ThemedSimpleText("Search: ") {
						LayoutCell = new LayoutCell(Alignment.LeftCenter)
					},
					searchBox
				}
			};
		}

		private void CreateColorEditor(ThemedScrollView container, object source, string propertyName, string displayName, Func<object> valueGetter)
		{
			var tmp = new Color4PropertyEditor(
				new PreferencesPropertyEditorParams(
					container.Content,
					source,
					propertyName: propertyName,
					displayName: displayName
				) {
					DefaultValueGetter = valueGetter
				}
			);
			tmp.Changed += Editor_Changed;
		}

		private void Editor_Changed()
		{
			Version++;
		}

		private void ApplyColorSearch()
		{
			pane.ScrollPosition = 0;
			var isShowingAll = searchBox.Text.Length == 0;
			var filter = searchBox.Text.ToLower();
			foreach (var node in pane.Content.Nodes) {
				switch (node) {
					case ThemedFrame tf:
						break;
					case Widget w:
						var colorName = (w.Nodes[0].Nodes[0].Nodes[1] as ThemedSimpleText).Text.ToLower();
						w.Visible = colorName.Contains(filter) || isShowingAll;
						break;
				}
			}
		}
	}
}
