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
		readonly Frame Frame;
		readonly TabbedWidget Content;

		public PreferencesDialog()
		{
			theme = Core.UserPreferences.Instance.Get<UserPreferences>().Theme;
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

		private Widget CreateColorsPane()
		{
			var pane = new ThemedScrollView {
				Padding = new Thickness {
					Right = 15,
				},
			};
			pane.Content.Layout = new VBoxLayout { Spacing = 4 };
			new EnumPropertyEditor<ColorThemeEnum>(
			new PropertyEditorParams(pane.Content, Core.UserPreferences.Instance.Get<Tangerine.UserPreferences>(), nameof(Tangerine.UserPreferences.Theme), "User interface theme"));
			IPropertyEditor tmp;
			tmp = new BooleanPropertyEditor(
				new PropertyEditorParams(pane.Content, Core.UserPreferences.Instance.Get<UI.SceneView.UserPreferences>(), nameof(UI.SceneView.UserPreferences.EnableChessBackground), "Chess background"));
			tmp.ContainerWidget.AddChangeWatcher(
				() => Core.UserPreferences.Instance.Get<UI.SceneView.UserPreferences>().EnableChessBackground, (v) => Application.InvalidateWindows());
			tmp = new Color4PropertyEditor(
				new PropertyEditorParams(
					pane.Content,
					Core.UserPreferences.Instance.Get<UI.SceneView.UserPreferences>(),
					nameof(UI.SceneView.UserPreferences.BackgroundColorA),
					"Background color A"
				) {
					DefaultValueGetter = () => ColorTheme.Current.SceneView.BackgroundColorA
				});
			tmp.ContainerWidget.AddChangeWatcher(
				() => Core.UserPreferences.Instance.Get<UI.SceneView.UserPreferences>().BackgroundColorA, (v) => Application.InvalidateWindows());
			tmp = new Color4PropertyEditor(
				new PropertyEditorParams(
					pane.Content,
					Core.UserPreferences.Instance.Get<UI.SceneView.UserPreferences>(),
					nameof(UI.SceneView.UserPreferences.BackgroundColorB),
					"Background color B"
				) {
					DefaultValueGetter = () => ColorTheme.Current.SceneView.BackgroundColorB
				});
			tmp.ContainerWidget.AddChangeWatcher(
				() => Core.UserPreferences.Instance.Get<UI.SceneView.UserPreferences>().BackgroundColorB, (v) => Application.InvalidateWindows());
			tmp = new Color4PropertyEditor(
				new PropertyEditorParams(
					pane.Content,
					Core.UserPreferences.Instance.Get<UI.SceneView.UserPreferences>(),
					nameof(UI.SceneView.UserPreferences.RootWidgetOverlayColor),
					"Root overlay color"
				) {
					DefaultValueGetter = () => ColorTheme.Current.SceneView.RootWidgetOverlayColor
				});
			tmp.ContainerWidget.AddChangeWatcher(
				() => Core.UserPreferences.Instance.Get<UI.SceneView.UserPreferences>().RootWidgetOverlayColor, (v) => Application.InvalidateWindows());
			new Color4PropertyEditor(
				new PropertyEditorParams(
					pane.Content,
					Core.UserPreferences.Instance.Get<UI.SceneView.UserPreferences>(),
					nameof(UI.SceneView.UserPreferences.AnimationPreviewBackground),
					"Animation preview background"
				) {
					DefaultValueGetter = () => Color4.Black.Transparentify(0.6f)
				});
			return pane;
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
				new PropertyEditorParams(pane.Content, Core.UserPreferences.Instance.Get<Tangerine.UserPreferences>(), nameof(Tangerine.UserPreferences.DefaultSceneDimensions), "Default scene dimensions"));
			new BooleanPropertyEditor(
				new PropertyEditorParams(pane.Content, Core.UserPreferences.Instance.Get<UI.Timeline.UserPreferences>(), nameof(UI.Timeline.UserPreferences.AutoKeyframes), "Automatic keyframes"));
			new BooleanPropertyEditor(
				new PropertyEditorParams(pane.Content, Core.UserPreferences.Instance.Get<UI.Timeline.UserPreferences>(), nameof(UI.Timeline.UserPreferences.AnimationMode), "Animation mode"));
			var boneWidthPropertyEditor = new FloatPropertyEditor(
				new PropertyEditorParams(pane.Content, Core.UserPreferences.Instance.Get<UI.SceneView.UserPreferences>(), nameof(UI.SceneView.UserPreferences.DefaultBoneWidth), "Bone Width"));
			boneWidthPropertyEditor.ContainerWidget.AddChangeWatcher(
						() => Core.UserPreferences.Instance.Get<UI.SceneView.UserPreferences>().DefaultBoneWidth, (v) => Application.InvalidateWindows());
			var overlaysEditor = new BooleanPropertyEditor(new PropertyEditorParams(
				pane.Content, Core.UserPreferences.Instance.Get<UI.SceneView.UserPreferences>(), nameof(UI.SceneView.UserPreferences.ShowOverlays), "Show overlays"));
			overlaysEditor.ContainerWidget.AddChangeWatcher(
						() => Core.UserPreferences.Instance.Get<UI.SceneView.UserPreferences>().ShowOverlays, (v) => Application.InvalidateWindows());

			return pane;
		}
	}
}