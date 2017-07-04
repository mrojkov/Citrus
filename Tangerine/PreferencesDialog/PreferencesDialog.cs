using System;
using System.Collections.Generic;
using Lime;
using Tangerine.UI;
using Tangerine.Core;

namespace Tangerine
{
	public class PreferencesDialog
	{
		readonly Window window;
		readonly WindowWidget rootWidget;
		readonly Button okButton;
		readonly Button cancelButton;
		readonly ColorThemeEnum theme;

		public PreferencesDialog()
		{
			theme = UserPreferences.Instance.Theme;
			window = new Window(new WindowOptions {
				ClientSize = new Vector2(600, 400),
				FixedSize = false,
				Title = "Preferences",
				MinimumDecoratedSize = new Vector2(400, 300)
			});
			rootWidget = new ThemedInvalidableWindowWidget(window) {
				Padding = new Thickness(8),
				Layout = new VBoxLayout(),
				Nodes = {
					new ThemedTabBar {
						Nodes = {
							new ThemedTab {
								Text = "General",
								Active = true
							}
						}
					},
					new ThemedFrame {
						Padding = new Thickness(8),
						LayoutCell = new LayoutCell { StretchY = float.MaxValue },
						Layout = new StackLayout(),
						Nodes = {
							CreateGenericPane(),
						}
					},
					new Widget { MinHeight = 8 },
					new Widget {
						Layout = new HBoxLayout { Spacing = 8 },
						LayoutCell = new LayoutCell(Alignment.RightCenter),
						Nodes = {
							(okButton = new ThemedButton { Text = "Ok" }),
							(cancelButton = new ThemedButton { Text = "Cancel" }),
						}
					}
				}
			};
			okButton.Clicked += () => {
				window.Close();
				if (theme != UserPreferences.Instance.Theme) {
					AlertDialog.Show("The color theme change will take effect next time you run Tangerine.");
				}
				UserPreferences.Instance.Save();
			};
			cancelButton.Clicked += () => {
				window.Close();
				UserPreferences.Instance.Load();
			};
			rootWidget.FocusScope = new KeyboardFocusScope(rootWidget);
			rootWidget.LateTasks.AddLoop(() => {
				if (rootWidget.Input.ConsumeKeyPress(Key.Escape)) {
					window.Close();
					UserPreferences.Instance.Load();
				}
			});
			okButton.SetFocus();
		}

		Widget CreateGenericPane()
		{
			var pane = new ThemedScrollView();
			pane.Content.Layout = new VBoxLayout { Spacing = 4 };
			new EnumPropertyEditor<ColorThemeEnum>(
				new PropertyEditorParams(pane.Content, UserPreferences.Instance, "Theme", "User interface theme"));
			new Vector2PropertyEditor(
				new PropertyEditorParams(pane.Content, UserPreferences.Instance, "DefaultSceneDimensions", "Default scene dimensions"));
			new BooleanPropertyEditor(
				new PropertyEditorParams(pane.Content, UserPreferences.Instance, "AutoKeyframes", "Automatic keyframes"));
			new BooleanPropertyEditor(
				new PropertyEditorParams(pane.Content, UserPreferences.Instance, "AnimationMode", "Animation mode"));

			var overlaysEditor = new BooleanPropertyEditor(new PropertyEditorParams(
				pane.Content, UserPreferences.Instance, nameof(UI.SceneView.UserPreferences.ShowOverlays), "Show overlays"));
			overlaysEditor.ContainerWidget.AddChangeWatcher(
				() => UserPreferences.Instance.SceneViewUserPreferences.ShowOverlays, (v) => Application.InvalidateWindows());

			return pane;
		}
	}
}