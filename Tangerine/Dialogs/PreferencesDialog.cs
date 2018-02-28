using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
		readonly Button resetButton;
		readonly ColorThemeEnum theme;
		readonly Frame Frame;
		readonly TabbedWidget Content;

		public PreferencesDialog()
		{
			theme = AppUserPreferences.Instance.Theme;
			window = new Window(new WindowOptions {
				ClientSize = new Vector2(600, 400),
				FixedSize = false,
				Title = "Preferences",
				MinimumDecoratedSize = new Vector2(400, 300)
			});
			Frame = new ThemedFrame {
				Padding = new Thickness(8),
				LayoutCell = new LayoutCell { StretchY = float.MaxValue },
				Layout = new StackLayout(),
			};
			Content = new TabbedWidget();
			Content.AddTab("General", CreateGeneralPane(), true);
			Content.AddTab("Appearance", CreateColorsPane());

			rootWidget = new ThemedInvalidableWindowWidget(window) {
				Padding = new Thickness(8),
				Layout = new VBoxLayout(),
				Nodes = {
					Content,
					new Widget {
						Layout = new HBoxLayout { Spacing = 8 },
						LayoutCell = new LayoutCell(Alignment.LeftCenter),
						Nodes = {
							(resetButton = new ThemedButton { Text = "Reset To Defaults", MinMaxWidth = 150f }),
							new Widget { MinMaxHeight = 0 },
							(okButton = new ThemedButton { Text = "Ok" }),
							(cancelButton = new ThemedButton { Text = "Cancel" }),
						}
					}
				}
			};
			okButton.Clicked += () => {
				window.Close();
				if (theme != AppUserPreferences.Instance.Theme) {
					AlertDialog.Show("The color theme change will take effect next time you run Tangerine.");
				}
				Core.UserPreferences.Instance.Save();
			};
			resetButton.Clicked += () => {
				if (new AlertDialog($"Are you sure you want to reset to defaults?", "Yes", "Cancel").Show() == 0) {
					ResetToDefaults();
				}
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

		private static void ResetToDefaults()
		{
			AppUserPreferences.Instance.ResetToDefaults();
			UI.SceneView.SceneUserPreferences.Instance.ResetToDefaults();
			UI.Timeline.TimelineUserPreferences.Instance.ResetToDefaults();
			Core.CoreUserPreferences.Instance.ResetToDefaults();
		}

		private Widget CreateColorsPane()
		{
			var pane = new ThemedScrollView {
				Padding = new Thickness {
					Right = 15,
				},
			};
			pane.Content.Layout = new VBoxLayout { Spacing = 4 };
			new EnumPropertyEditor<ColorThemeEnum>(
			new PropertyEditorParams(pane.Content, AppUserPreferences.Instance, nameof(Tangerine.AppUserPreferences.Theme), "User interface theme"));
			var tmp = new BooleanPropertyEditor(
				new PropertyEditorParams(pane.Content, UI.SceneView.SceneUserPreferences.Instance, nameof(UI.SceneView.SceneUserPreferences.EnableChessBackground), "Chess background"));
			tmp.ContainerWidget.AddChangeWatcher(
				() => UI.SceneView.SceneUserPreferences.Instance.EnableChessBackground, (v) => Application.InvalidateWindows());
			tmp = new BooleanPropertyEditor(
				new PropertyEditorParams(pane.Content, UI.SceneView.SceneUserPreferences.Instance, nameof(UI.SceneView.SceneUserPreferences.DrawFrameBorder), "Draw frame border"));
			tmp.ContainerWidget.AddChangeWatcher(
				() => UI.SceneView.SceneUserPreferences.Instance.DrawFrameBorder, (v) => Application.InvalidateWindows());
			CreateColorPropertyEditor(
				nameof(UI.SceneView.SceneUserPreferences.BackgroundColorA),
				"Background color A",
				Core.UserPreferences.Instance.Get<UI.SceneView.SceneUserPreferences>(),
				() => ColorTheme.Current.SceneView.BackgroundColorA,
				pane);
			CreateColorPropertyEditor(
				nameof(UI.SceneView.SceneUserPreferences.BackgroundColorB),
				"Background color B",
				Core.UserPreferences.Instance.Get<UI.SceneView.SceneUserPreferences>(),
				() => ColorTheme.Current.SceneView.BackgroundColorB,
				pane);
			CreateColorPropertyEditor(
				nameof(UI.SceneView.SceneUserPreferences.RootWidgetOverlayColor),
				"Root overlay color",
				Core.UserPreferences.Instance.Get<UI.SceneView.SceneUserPreferences>(),
				() => ColorTheme.Current.SceneView.RootWidgetOverlayColor,
				pane);
			CreateColorPropertyEditor(
				nameof(UI.SceneView.SceneUserPreferences.AnimationPreviewBackground),
				"Animation preview background",
				Core.UserPreferences.Instance.Get<UI.SceneView.SceneUserPreferences>(),
				() => Color4.Black.Transparentify(0.6f),
				pane);
			return pane;
		}

		public void CreateColorPropertyEditor(string targetProperty, string text, object source, System.Func<object> valueGetter, ThemedScrollView container)
		{
			var tmp = new Color4PropertyEditor(
				new PropertyEditorParams(
					container.Content,
					source,
					targetProperty,
					text
				) {
					DefaultValueGetter = valueGetter
				});
			tmp.ContainerWidget.AddChangeWatcher(
				new Property<Color4>(source, targetProperty), (v) => Application.InvalidateWindows());
		}

		Widget CreateGeneralPane()
		{
			var pane = new ThemedScrollView {
				Padding = new Thickness {
					Right = 15,
				},
			};
			pane.Content.Layout = new VBoxLayout { Spacing = 4 };
			new Vector2PropertyEditor(
				new PropertyEditorParams(pane.Content, Tangerine.AppUserPreferences.Instance, nameof(Tangerine.AppUserPreferences.DefaultSceneDimensions), "Default scene dimensions"));
			new BooleanPropertyEditor(
				new PropertyEditorParams(pane.Content, CoreUserPreferences.Instance, nameof(CoreUserPreferences.AutoKeyframes), "Automatic keyframes"));
			new BooleanPropertyEditor(
				new PropertyEditorParams(pane.Content, UI.Timeline.TimelineUserPreferences.Instance, nameof(UI.Timeline.TimelineUserPreferences.AnimationMode), "Animation mode"));
			new IntPropertyEditor(
				new PropertyEditorParams(pane.Content, Tangerine.AppUserPreferences.Instance, nameof(Tangerine.AppUserPreferences.AutosaveDelay), "Autosave delay"));
			var boneWidthPropertyEditor = new FloatPropertyEditor(
				new PropertyEditorParams(pane.Content, UI.SceneView.SceneUserPreferences.Instance, nameof(UI.SceneView.SceneUserPreferences.DefaultBoneWidth), "Bone Width"));
			boneWidthPropertyEditor.ContainerWidget.AddChangeWatcher(
				() => UI.SceneView.SceneUserPreferences.Instance.DefaultBoneWidth, (v) => Application.InvalidateWindows());
			return pane;
		}
	}
}