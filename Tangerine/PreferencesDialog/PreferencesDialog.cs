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
			rootWidget = new InvalidableWindowWidget(window) {
				Padding = new Thickness(8),
				Layout = new VBoxLayout(),
				Nodes = {
					new TabBar {
						Nodes = {
							new Tab {
								Text = "General",
								Active = true
							}
						}
					},
					new BorderedFrame {
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
							(okButton = new Button { Text = "Ok" }),
							(cancelButton = new Button { Text = "Cancel" }),
						}
					}
				}
			};
			okButton.Clicked += () => {
				window.Close();
				if (theme != UserPreferences.Instance.Theme) {
					var alert = new AlertDialog("Tangerine", "The color theme change will take effect next time you run Tangerine.", "Ok");
					alert.Show();
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
			var pane = new ScrollViewWidget();
			pane.Content.Layout = new VBoxLayout { Spacing = 4 };
			new EnumPropertyEditor<ColorThemeEnum>(
				new PropertyEditorContext(pane.Content, UserPreferences.Instance, "Theme", "User interface theme"));
			new Vector2PropertyEditor(
				new PropertyEditorContext(pane.Content, UserPreferences.Instance, "DefaultSceneDimensions", "Default scene dimensions"));
			new BooleanPropertyEditor(
				new PropertyEditorContext(pane.Content, UserPreferences.Instance, "AutoKeyframes", "Automatic keyframes"));
			new BooleanPropertyEditor(
				new PropertyEditorContext(pane.Content, UserPreferences.Instance, "AnimationMode", "Animation mode"));
			return pane;
		}
	}
}