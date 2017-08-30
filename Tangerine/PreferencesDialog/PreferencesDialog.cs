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
			theme = Core.UserPreferences.Instance.Get<UserPreferences>().Theme;
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
				if (theme != Core.UserPreferences.Instance.Get<UserPreferences>().Theme) {
					AlertDialog.Show("The color theme change will take effect next time you run Tangerine.");
				}
				Core.UserPreferences.Instance.Save();
			};
			cancelButton.Clicked += () => {
				window.Close();
				Core.UserPreferences.Instance.Load();
			};
			rootWidget.FocusScope = new KeyboardFocusScope(rootWidget);
			rootWidget.LateTasks.AddLoop(() => {
				if (rootWidget.Input.ConsumeKeyPress(Key.Escape)) {
					window.Close();
					Core.UserPreferences.Instance.Load();
				}
			});
			okButton.SetFocus();
		}

		Widget CreateGenericPane()
		{
			var pane = new ThemedScrollView();
			pane.Content.Layout = new VBoxLayout { Spacing = 4 };
			new EnumPropertyEditor<ColorThemeEnum>(
				new PropertyEditorParams(pane.Content, Core.UserPreferences.Instance.Get<Tangerine.UserPreferences>(), nameof(Tangerine.UserPreferences.Theme), "User interface theme"));
			new Vector2PropertyEditor(
				new PropertyEditorParams(pane.Content, Core.UserPreferences.Instance.Get<Tangerine.UserPreferences>(), nameof(Tangerine.UserPreferences.DefaultSceneDimensions), "Default scene dimensions"));
			new BooleanPropertyEditor(
				new PropertyEditorParams(pane.Content, Core.UserPreferences.Instance.Get<UI.Timeline.UserPreferences>(), nameof(UI.Timeline.UserPreferences.AutoKeyframes), "Automatic keyframes"));
			new BooleanPropertyEditor(
				new PropertyEditorParams(pane.Content, Core.UserPreferences.Instance.Get<UI.Timeline.UserPreferences>(), nameof(UI.Timeline.UserPreferences.AnimationMode), "Animation mode"));
			new FloatPropertyEditor(
				new PropertyEditorParams(pane.Content, Core.UserPreferences.Instance.Get<UI.SceneView.UserPreferences>(), nameof(UI.SceneView.UserPreferences.DefaultBoneWidth), "Bone Width"));

			var overlaysEditor = new BooleanPropertyEditor(new PropertyEditorParams(
				pane.Content, Core.UserPreferences.Instance.Get<UI.SceneView.UserPreferences>(), nameof(UI.SceneView.UserPreferences.ShowOverlays), "Show overlays"));
			overlaysEditor.ContainerWidget.AddChangeWatcher(
				() => Core.UserPreferences.Instance.Get<UI.SceneView.UserPreferences>().ShowOverlays, (v) => Application.InvalidateWindows());

			return pane;
		}
	}
}