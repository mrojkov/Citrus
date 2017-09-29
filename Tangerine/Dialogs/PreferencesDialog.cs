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
			var tmp = new BooleanPropertyEditor(
				new PropertyEditorParams(pane.Content, Core.UserPreferences.Instance.Get<UI.SceneView.UserPreferences>(), nameof(UI.SceneView.UserPreferences.EnableChessBackground), "Chess background"));
			tmp.ContainerWidget.AddChangeWatcher(
				() => Core.UserPreferences.Instance.Get<UI.SceneView.UserPreferences>().EnableChessBackground, (v) => Application.InvalidateWindows());
			CreateColorPropertyEditor(
				nameof(UI.SceneView.UserPreferences.BackgroundColorA),
				"Background color A",
				Core.UserPreferences.Instance.Get<UI.SceneView.UserPreferences>(),
				() => ColorTheme.Current.SceneView.BackgroundColorA,
				pane);
			CreateColorPropertyEditor(
				nameof(UI.SceneView.UserPreferences.BackgroundColorB),
				"Background color B",
				Core.UserPreferences.Instance.Get<UI.SceneView.UserPreferences>(),
				() => ColorTheme.Current.SceneView.BackgroundColorB,
				pane);
			CreateColorPropertyEditor(
				nameof(UI.SceneView.UserPreferences.RootWidgetOverlayColor),
				"Root overlay color",
				Core.UserPreferences.Instance.Get<UI.SceneView.UserPreferences>(),
				() => ColorTheme.Current.SceneView.RootWidgetOverlayColor,
				pane);
			CreateColorPropertyEditor(
				nameof(UI.SceneView.UserPreferences.AnimationPreviewBackground),
				"Animation preview background",
				Core.UserPreferences.Instance.Get<UI.SceneView.UserPreferences>(),
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
				new PropertyEditorParams(pane.Content, Core.UserPreferences.Instance.Get<Tangerine.UserPreferences>(), nameof(Tangerine.UserPreferences.DefaultSceneDimensions), "Default scene dimensions"));
			new BooleanPropertyEditor(
				new PropertyEditorParams(pane.Content, Core.UserPreferences.Instance.Get<UI.Timeline.UserPreferences>(), nameof(UI.Timeline.UserPreferences.AutoKeyframes), "Automatic keyframes"));
			new BooleanPropertyEditor(
				new PropertyEditorParams(pane.Content, Core.UserPreferences.Instance.Get<UI.Timeline.UserPreferences>(), nameof(UI.Timeline.UserPreferences.AnimationMode), "Animation mode"));
			var boneWidthPropertyEditor = new FloatPropertyEditor(
				new PropertyEditorParams(pane.Content, Core.UserPreferences.Instance.Get<UI.SceneView.UserPreferences>(), nameof(UI.SceneView.UserPreferences.DefaultBoneWidth), "Bone Width"));
			boneWidthPropertyEditor.ContainerWidget.AddChangeWatcher(
				() => Core.UserPreferences.Instance.Get<UI.SceneView.UserPreferences>().DefaultBoneWidth, (v) => Application.InvalidateWindows());
			return pane;
		}
	}
}