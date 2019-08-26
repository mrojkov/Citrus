using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Lime;
using Tangerine.UI;
using Tangerine.Core;
using System;
using System.IO;
using Tangerine.Dialogs;
using Tangerine.UI.SceneView;

namespace Tangerine
{
	public class PreferencesDialog
	{
		readonly Window window;
		readonly WindowWidget rootWidget;
		readonly Button okButton;
		readonly Button cancelButton;
		readonly Button resetButton;
		readonly Frame Frame;
		readonly ThemedTabbedWidget Content;
		readonly ToolbarModelEditor toolbarModelEditor;

		private List<IPropertyEditor> editors = new List<IPropertyEditor>();

		private static readonly Thickness contentPadding = new Thickness {
			Left = 10,
			Top = 10,
			Bottom = 10,
			Right = 15,
		};

		private bool saved = false;
		private HotkeyProfile currentProfile;
		private bool themeChanged;
		private bool themeEdited;

		public PreferencesDialog()
		{
			window = new Window(new WindowOptions {
				ClientSize = new Vector2(800, 600),
				FixedSize = false,
				Title = "Preferences",
				MinimumDecoratedSize = new Vector2(400, 300),
				Visible = false
			});
			Frame = new ThemedFrame {
				Padding = new Thickness(8),
				LayoutCell = new LayoutCell { StretchY = float.MaxValue },
				Layout = new StackLayout(),
			};
			Content = new ThemedTabbedWidget();
			Content.AddTab("General", CreateGeneralPane(), true);
			Content.AddTab("Appearance", CreateColorsPane());
			Content.AddTab("Theme", CreateThemePane());
			Content.AddTab("Keyboard shortcuts", CreateKeyboardPane());
			Content.AddTab("Toolbar", toolbarModelEditor = new ToolbarModelEditor());

			rootWidget = new ThemedInvalidableWindowWidget(window) {
				Padding = new Thickness(8),
				Layout = new VBoxLayout(),
				Nodes = {
					Content,
					new Widget {
						Layout = new HBoxLayout { Spacing = 8 },
						LayoutCell = new LayoutCell(Alignment.LeftCenter),
						Padding = new Thickness { Top = 5 },
						Nodes = {
							(resetButton = new ThemedButton { Text = "Reset To Defaults", MinMaxWidth = 150f }),
							new Widget { MinMaxHeight = 0 },
							(okButton = new ThemedButton { Text = "Ok" }),
							(cancelButton = new ThemedButton { Text = "Cancel" }),
						}
					}
				}
			};
			HotkeyRegistry.CurrentProfile.Save();
			okButton.Clicked += () => {
				saved = true;
				SaveAfterEdit();
				window.Close();
				VisualHintsPanel.Refresh();
				Core.UserPreferences.Instance.Save();
				if (themeEdited) {
					AppUserPreferences.Instance.ColorThemeKind = ColorTheme.ColorThemeKind.Custom;
				}
				if (themeChanged) {
					AlertDialog.Show("Color theme changes will be applied after Tangerine restart.");
				}
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

			window.Closed += () => {
				if (saved) {
					foreach (var profile in HotkeyRegistry.Profiles) {
						profile.Save();
					}
					HotkeyRegistry.CurrentProfile = currentProfile;
				} else {
					foreach (var profile in HotkeyRegistry.Profiles) {
						profile.Load();
					}
					HotkeyRegistry.CurrentProfile = HotkeyRegistry.CurrentProfile;
				}
			};

			foreach (var command in HotkeyRegistry.CurrentProfile.Commands) {
				command.Command.Shortcut = new Shortcut(Key.Unknown);
			}
			window.ShowModal();
		}

		private void ResetToDefaults()
		{
			AppUserPreferences.Instance.ResetToDefaults();
			UI.SceneView.SceneUserPreferences.Instance.ResetToDefaults();
			UI.Timeline.TimelineUserPreferences.Instance.ResetToDefaults();
			Core.CoreUserPreferences.Instance.ResetToDefaults();
			HotkeyRegistry.ResetToDefaults();
			toolbarModelEditor.ResetToDefaults();
		}

		private Widget CreateColorsPane()
		{
			var pane = new ThemedScrollView();
			pane.Content.Layout = new VBoxLayout { Spacing = 4 };
			pane.Content.Padding = contentPadding;
			var tmp = new BooleanPropertyEditor(
				new PreferencesPropertyEditorParams(pane.Content, UI.SceneView.SceneUserPreferences.Instance, propertyName: nameof(UI.SceneView.SceneUserPreferences.EnableChessBackground), displayName: "Chess background"));
			tmp.ContainerWidget.AddChangeWatcher(
				() => UI.SceneView.SceneUserPreferences.Instance.EnableChessBackground, (v) => Application.InvalidateWindows());
			tmp = new BooleanPropertyEditor(
				new PreferencesPropertyEditorParams(pane.Content, UI.SceneView.SceneUserPreferences.Instance, propertyName: nameof(UI.SceneView.SceneUserPreferences.DrawFrameBorder), displayName: "Draw frame border"));
			tmp.ContainerWidget.AddChangeWatcher(
				() => UI.SceneView.SceneUserPreferences.Instance.DrawFrameBorder, (v) => Application.InvalidateWindows());
			editors.Add(tmp);
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

		private Widget CreateThemePane()
		{
			var pane = new Widget {
				Layout = new VBoxLayout { Spacing = 10 },
				Padding = contentPadding,
				Nodes = { CreateThemeEditor() }
			};
			return pane;
		}

		public bool DarkIcons {
			get {
				return AppUserPreferences.Instance.ColorTheme.IsDark;
			}
			set {
				AppUserPreferences.Instance.ColorTheme.IsDark = value;
				themeChanged = true;
			}
		}

		private Widget CreateThemeEditor()
		{
			var pane = new Widget {
				Layout = new VBoxLayout { Spacing = 10 },
				Padding = contentPadding
			};
			var themeEditor = new ColorThemeEditor() {
				Layout = new VBoxLayout { Spacing = 10 },
				Padding = contentPadding
			};
			bool firstCall = true;
			pane.AddChangeWatcher(() => themeEditor.Version, _ => {
				if (firstCall) {
					firstCall = false;
					return;
				}
				themeChanged = true;
				themeEdited = true;
			});
			var darkIcons = CreateDarkIconsSwitch(pane);
			var loadDarkButton = new ThemedButton("Dark preset") {
				Clicked = () => {
					AppUserPreferences.Instance.ColorThemeKind = ColorTheme.ColorThemeKind.Dark;
					AppUserPreferences.Instance.LimeColorTheme = Theme.ColorTheme.CreateDarkTheme();
					AppUserPreferences.Instance.ColorTheme = ColorTheme.CreateDarkTheme();
					themeEditor.Rebuild();
					themeChanged = true;
				}
			};
			var loadLightButton = new ThemedButton("Light preset") {
				Clicked = () => {
					AppUserPreferences.Instance.ColorThemeKind = ColorTheme.ColorThemeKind.Light;
					AppUserPreferences.Instance.LimeColorTheme = Theme.ColorTheme.CreateLightTheme();
					AppUserPreferences.Instance.ColorTheme = ColorTheme.CreateLightTheme();
					themeEditor.Rebuild();
					themeChanged = true;
				}
			};
			var saveButton = new ThemedButton("Save theme") {
				Clicked = () => {
					var dlg = new FileDialog {
						AllowedFileTypes = new string[] { "theme" },
						Mode = FileDialogMode.Save
					};
					if (dlg.RunModal()) {
						string path = dlg.FileName;
						var serializer = new Yuzu.Json.JsonSerializer();
						try {
							var limeTheme = AppUserPreferences.Instance.LimeColorTheme;
							var theme = AppUserPreferences.Instance.ColorTheme;
							using (var fileStream = new FileStream(path, FileMode.OpenOrCreate)) {
								serializer.ToStream(new List<object> { limeTheme, theme }, fileStream);
							}
						} catch (System.Exception e) {
							AlertDialog.Show(e.Message);
						}
					}
				}
			};
			var loadButton = new ThemedButton("Load theme") {
				Clicked = () => {
					var dlg = new FileDialog {
						AllowedFileTypes = new string[] { "theme" },
						Mode = FileDialogMode.Open
					};
					if (dlg.RunModal()) {
						string path = dlg.FileName;
						var deserializer = new Yuzu.Json.JsonDeserializer();
						try {
							using (var fs = new FileStream(path, FileMode.OpenOrCreate)) {
								var read = deserializer.FromStream(new List<object>(), fs) as List<object>;
								AppUserPreferences.Instance.ColorThemeKind = ColorTheme.ColorThemeKind.Custom;
								AppUserPreferences.Instance.LimeColorTheme = (Theme.ColorTheme)read[0];
								AppUserPreferences.Instance.ColorTheme = (ColorTheme)read[1];
							}
						} catch (System.Exception e) {
							AlertDialog.Show(e.Message);
						}
					}
					themeEditor.Rebuild();
					themeChanged = true;
				}
			};
			var buttons = new Widget {
				Layout = new HBoxLayout { Spacing = 4 },
				Nodes = { loadDarkButton, loadLightButton, saveButton, loadButton }
			};
			pane.AddNode(buttons);
			pane.AddNode(themeEditor);
			return pane;
		}

		private IPropertyEditor CreateDarkIconsSwitch(Widget pane)
		{
			return new BooleanPropertyEditor(
				new PreferencesPropertyEditorParams(pane, this, propertyName: nameof(DarkIcons), displayName: "Dark icon theme"));
		}

		public void CreateColorPropertyEditor(string targetProperty, string text, object source, System.Func<object> valueGetter, ThemedScrollView container)
		{
			var tmp = new Color4PropertyEditor(
				new PreferencesPropertyEditorParams(
					container.Content,
					source,
					propertyName: targetProperty,
					displayName: text
				) {
					DefaultValueGetter = valueGetter
				});
			tmp.ContainerWidget.AddChangeWatcher(
				new Property<Color4>(source, targetProperty), (v) => Application.InvalidateWindows());
			editors.Add(tmp);
		}

		Widget CreateGeneralPane()
		{
			var parent = new Widget();
			parent.Layout = new VBoxLayout { Spacing = 0 };
			parent.Padding = contentPadding;
			var platform = new Widget();
			platform.Layout = new HBoxLayout { Spacing = 4, DefaultCell = new DefaultLayoutCell(Alignment.Center) };
			platform.Padding = contentPadding;
			platform.AddNode(new ThemedSimpleText("Target platform"));
			var platformPicker = (Orange.The.UI as OrangeInterface).PlatformPicker;
			platformPicker.Unlink();
			platform.AddNode(platformPicker);
			parent.AddNode(platform);
			var pane = new ThemedScrollView();
			pane.Content.Layout = new VBoxLayout { Spacing = 4 };
			pane.Content.Padding = contentPadding;
			editors.AddRange(new IPropertyEditor[] {
				new BooleanPropertyEditor(
					new PreferencesPropertyEditorParams(pane.Content, CoreUserPreferences.Instance, propertyName: nameof(CoreUserPreferences.ReloadModifiedFiles),  displayName: "Reload modified files")),
				new Vector2PropertyEditor(
					new PreferencesPropertyEditorParams(pane.Content, Tangerine.AppUserPreferences.Instance, propertyName: nameof(Tangerine.AppUserPreferences.DefaultSceneDimensions),  displayName: "Default scene dimensions")),
				new BooleanPropertyEditor(
					new PreferencesPropertyEditorParams(pane.Content, CoreUserPreferences.Instance, propertyName: nameof(CoreUserPreferences.AutoKeyframes),  displayName: "Automatic keyframes")),
				new BooleanPropertyEditor(
					new PreferencesPropertyEditorParams(pane.Content, CoreUserPreferences.Instance, propertyName: nameof(CoreUserPreferences.AnimationMode),  displayName: "Animation mode")),
				new IntPropertyEditor(
					new PreferencesPropertyEditorParams(pane.Content, Tangerine.AppUserPreferences.Instance, propertyName: nameof(Tangerine.AppUserPreferences.AutosaveDelay),  displayName: "Autosave delay")),
				new BooleanPropertyEditor(
					new PreferencesPropertyEditorParams(pane.Content, CoreUserPreferences.Instance, propertyName: nameof(CoreUserPreferences.StopAnimationOnCurrentFrame),  displayName: "Stop animaion on current frame")),
				new BooleanPropertyEditor(
					new PreferencesPropertyEditorParams(pane.Content, CoreUserPreferences.Instance, propertyName: nameof(CoreUserPreferences.ShowSplinesGlobally),  displayName: "Show splines globally")),
				new BooleanPropertyEditor(
					new PreferencesPropertyEditorParams(pane.Content, CoreUserPreferences.Instance, propertyName: nameof(CoreUserPreferences.DontPasteAtMouse),  displayName: "Don't paste at mouse pointer")),
				new BooleanPropertyEditor(
					new PreferencesPropertyEditorParams(pane.Content, CoreUserPreferences.Instance, propertyName: nameof(CoreUserPreferences.InverseShiftKeyframeDrag), displayName: "Inverse Shift behaviour when dragging keyframes")),
				new BooleanPropertyEditor(
					new PreferencesPropertyEditorParams(pane.Content, CoreUserPreferences.Instance, propertyName: nameof(CoreUserPreferences.SwapMouseButtonsForKeyframeSwitch), displayName: "Swap mouse buttons for keyframe switch")),
			});
			var boneWidthPropertyEditor =
				new FloatPropertyEditor(
					new PreferencesPropertyEditorParams(pane.Content, UI.SceneView.SceneUserPreferences.Instance, propertyName: nameof(UI.SceneView.SceneUserPreferences.DefaultBoneWidth), displayName: "Bone Width"));

			boneWidthPropertyEditor.ContainerWidget.AddChangeWatcher(
				() => UI.SceneView.SceneUserPreferences.Instance.DefaultBoneWidth, (v) => Application.InvalidateWindows());
			new EnumPropertyEditor<KeyFunction>(
				new PreferencesPropertyEditorParams(pane.Content, CoreUserPreferences.Instance, propertyName: nameof(CoreUserPreferences.DefaultKeyFunction), displayName: "Default interpolation"));
			parent.AddNode(pane);
			return parent;
		}

		private void SaveAfterEdit()
		{
			editors.ForEach(i => i.Submit());
		}

		private Widget CreateKeyboardPane()
		{
			var hotkeyEditor = new HotkeyEditor();
			var pane = new Widget {
				Layout = new VBoxLayout { Spacing = 10 },
				Padding = contentPadding
			};
			pane.Awoke += node => hotkeyEditor.SetFocus();

			var profileLabel = new ThemedSimpleText("Profile: ") {
				VAlignment = VAlignment.Center,
				HAlignment = HAlignment.Right,
				LayoutCell = new LayoutCell(Alignment.RightCenter, 0)
			};
			var profilePicker = new ThemedDropDownList();
			profilePicker.TextWidget.Padding = new Thickness(3, 0);

			var exportButton = new ThemedButton("Export...");
			exportButton.Clicked = () => {
				var dlg = new FileDialog {
					Mode = FileDialogMode.Save,
					InitialFileName = currentProfile.Name
				};
				if (dlg.RunModal()) {
					currentProfile.Save(dlg.FileName);
				}
			};
			var importButton = new ThemedButton("Import...");
			importButton.Clicked = () => {
				var dlg = new FileDialog { Mode = FileDialogMode.Open };
				if (dlg.RunModal()) {
					string name = Path.GetFileName(dlg.FileName);
					if (HotkeyRegistry.Profiles.Any(i => i.Name == name)) {
						if (new AlertDialog($"Profile with name \"{name}\" already exists. Do you want to rewrite it?", "Yes", "Cancel").Show() != 0) {
							return;
						} else {
							profilePicker.Items.Remove(profilePicker.Items.First(i => i.Text == name));
						}
					}
					var profile = HotkeyRegistry.CreateProfile(Path.GetFileName(dlg.FileName));
					profile.Load(dlg.FileName);
					profile.Save();
					HotkeyRegistry.Profiles.Add(profile);
					profilePicker.Items.Add(new CommonDropDownList.Item(profile.Name, profile));
					profilePicker.Value = profile;
				}
			};
			var deleteButton = new ThemedButton("Delete");
			deleteButton.Clicked = () => {
				if (new AlertDialog($"Are you sure you want to delete profile \"{currentProfile.Name}\"?", "Yes", "Cancel").Show() == 0) {
					currentProfile.Delete();
					profilePicker.Items.Remove(profilePicker.Items.First(i => i.Value == currentProfile));
					profilePicker.Index = 0;
					HotkeyRegistry.CurrentProfile = profilePicker.Value as HotkeyProfile;
					foreach (var command in HotkeyRegistry.CurrentProfile.Commands) {
						command.Command.Shortcut = new Shortcut(Key.Unknown);
					}
				}
			};

			var categoryLabel = new ThemedSimpleText("Commands: ") {
				VAlignment = VAlignment.Center,
				HAlignment = HAlignment.Right,
				LayoutCell = new LayoutCell(Alignment.RightCenter, 0)
			};
			var categoryPicker = new ThemedDropDownList();
			categoryPicker.TextWidget.Padding = new Thickness(3, 0);

			var allShortcutsView = new ThemedScrollView();
			allShortcutsView.Content.Padding = contentPadding;
			allShortcutsView.Content.Layout = new VBoxLayout { Spacing = 8 };

			var selectedShortcutsView = new ThemedScrollView();
			selectedShortcutsView.Content.Padding = contentPadding;
			selectedShortcutsView.Content.Layout = new VBoxLayout { Spacing = 4 };

			hotkeyEditor.SelectedShortcutChanged = () => {
				selectedShortcutsView.Content.Nodes.Clear();
				var commands = hotkeyEditor.SelectedCommands.ToLookup(i => i.CategoryInfo);
				foreach (var category in commands) {
					selectedShortcutsView.Content.AddNode(new ThemedSimpleText {
						Text = category.Key.Title,
						VAlignment = VAlignment.Center,
						Color = Theme.Colors.GrayText
					});
					foreach (var command in category) {
						var shortcut = new ThemedSimpleText {
							Text = command.Shortcut.ToString(),
							VAlignment = VAlignment.Center,
							LayoutCell = new LayoutCell(Alignment.LeftCenter, 1)
						};
						var name = new ThemedSimpleText {
							Text = command.Title,
							VAlignment = VAlignment.Center,
							LayoutCell = new LayoutCell(Alignment.LeftCenter, 2)
						};
						var deleteShortcutButton = new ThemedTabCloseButton {
							LayoutCell = new LayoutCell(Alignment.LeftCenter, 0),
							Clicked = () => {
								command.Shortcut = new Shortcut();
								hotkeyEditor.UpdateButtonCommands();
								hotkeyEditor.UpdateShortcuts();
							}
						};
						selectedShortcutsView.Content.AddNode(new Widget {
							Layout = new TableLayout { Spacing = 4, RowCount = 1, ColumnCount = 3 },
							Nodes = { shortcut, name, deleteShortcutButton },
							Padding = new Thickness(15, 0)
						});
					}
				}
				selectedShortcutsView.ScrollPosition = allShortcutsView.MinScrollPosition;
			};

			var filterBox = new ThemedEditBox {
				MaxWidth = 200
			};
			filterBox.AddChangeWatcher(() => filterBox.Text, text => {
				UpdateAllShortcutsView(allShortcutsView, selectedShortcutsView, hotkeyEditor, text.ToLower());
				allShortcutsView.ScrollPosition = allShortcutsView.MinScrollPosition;
			});

			categoryPicker.Changed += args => {
				hotkeyEditor.Category = (args.Value as CommandCategoryInfo);
				hotkeyEditor.SetFocus();
				int index = -1;
				foreach (var node in allShortcutsView.Content.Nodes.SelectMany(i => i.Nodes)) {
					var button = node as ThemedExpandButton;
					if (button == null) {
						continue;
					}
					index++;
					if (index == args.Index) {
						if (!button.Expanded) {
							button.Clicked?.Invoke();
						}
						allShortcutsView.ScrollPosition = button.ParentWidget.Position.Y;
						break;
					}
				}
			};

			profilePicker.Changed += args => {
				var profile = args.Value as HotkeyProfile;
				if (profile != null) {
					hotkeyEditor.Profile = profile;
					filterBox.Text = null;
					UpdateAllShortcutsView(allShortcutsView, selectedShortcutsView, hotkeyEditor, filterBox.Text);
					deleteButton.Enabled = profile.Name != HotkeyRegistry.DefaultProfileName && profilePicker.Items.Count > 1;
					categoryPicker.Items.Clear();
					foreach (var category in profile.Categories) {
						categoryPicker.Items.Add(new CommonDropDownList.Item(category.Title, category));
					}
					categoryPicker.Value = null;
					categoryPicker.Value = profile.Categories.First();
					currentProfile = profile;
				}
			};
			UpdateProfiles(profilePicker);

			HotkeyRegistry.Reseted = () => UpdateProfiles(profilePicker);

			pane.AddNode(new Widget {
				Layout = new TableLayout { Spacing = 4, RowCount = 2, ColumnCount = 3 },
				Nodes = {
					profileLabel, profilePicker,
					new Widget {
						Layout = new HBoxLayout { Spacing = 4 },
						Nodes = { exportButton, importButton, deleteButton }
					},
					categoryLabel, categoryPicker
				},
				LayoutCell = new LayoutCell { StretchY = 0 }
			});

			pane.AddNode(hotkeyEditor);
			pane.AddNode(new Widget {
				Layout = new HBoxLayout { Spacing = 12 },
				Nodes = {
					new Widget {
						Layout = new VBoxLayout {Spacing = 4 },
						Nodes = {
							new Widget{
								Layout = new HBoxLayout {Spacing = 8 },
								Nodes = {
									new ThemedSimpleText("Search: ") {
										VAlignment = VAlignment.Center,
										LayoutCell = new LayoutCell(Alignment.LeftCenter, 0)
									},
									filterBox
								},
								LayoutCell = new LayoutCell { StretchY = 0 }
							},
							new ThemedFrame {
								Nodes = { allShortcutsView },
								Layout = new VBoxLayout()
							}
						}
					},
					new Widget {
						Layout = new VBoxLayout {Spacing = 4 },
						Nodes = {
							new ThemedSimpleText("Selected commands:") { LayoutCell = new LayoutCell { StretchY = 0 } },
							new ThemedFrame {
								Nodes = { selectedShortcutsView },
								Layout = new VBoxLayout()
							}
						}
					}
				}
			});

			return pane;
		}

		private void UpdateAllShortcutsView(ThemedScrollView allShortcutsView, ThemedScrollView selectedShortcutsView, HotkeyEditor hotkeyEditor, string filter)
		{
			allShortcutsView.Content.Nodes.Clear();
			if (hotkeyEditor.Profile == null) {
				return;
			}
			foreach (var category in hotkeyEditor.Profile.Categories) {
				var expandableContent = new Frame {
					Layout = new VBoxLayout { Spacing = 4 },
					Visible = true
				};
				var expandButton = new ThemedExpandButton {
					Anchors = Anchors.Left,
					MinMaxSize = Vector2.One * 20f,
					Expanded = expandableContent.Visible
				};
				var title = new ThemedSimpleText {
					Text = category.Title,
					VAlignment = VAlignment.Center,
					LayoutCell = new LayoutCell(Alignment.LeftCenter, stretchX: 0)
				};
				expandButton.Clicked += () => {
					expandableContent.Visible = !expandableContent.Visible;
					expandButton.Expanded = expandableContent.Visible;
				};
				var header = new Widget {
					Layout = new HBoxLayout(),
					Nodes = { expandButton, title }
				};
				allShortcutsView.Content.AddNode(header);
				allShortcutsView.Content.AddNode(expandableContent);
				var filteredCommands = String.IsNullOrEmpty(filter) ?
					category.Commands.Values : category.Commands.Values.Where(i => i.Title.ToLower().Contains(filter));
				title.Color = filteredCommands.Any() ? Theme.Colors.BlackText : Theme.Colors.GrayText;
				expandButton.Enabled = filteredCommands.Any();
				foreach (var command in filteredCommands) {
					var editor = new ShortcutPropertyEditor(
						new PreferencesPropertyEditorParams(expandableContent, command, propertyName: "Shortcut", displayName: command.Title));
					editor.PropertyLabel.OverflowMode = TextOverflowMode.Ellipsis;
					editor.PropertyLabel.LayoutCell = new LayoutCell(Alignment.LeftCenter, 1);
					editor.PropertyLabel.Padding = new Thickness(expandButton.Width, 0);

					editor.PropertyLabel.CompoundPresenter.RemoveAll(i => i as SelectionPresenter != null);
					editor.PropertyLabel.Caret = new CaretPosition();

					if (!String.IsNullOrEmpty(filter)) {
						var mc = new MultiCaretPosition();
						var start = new CaretPosition { IsVisible = true, WorldPos = new Vector2(1, 1) };
						var finish = new CaretPosition { IsVisible = true, WorldPos = new Vector2(1, 1) };
						mc.Add(start);
						mc.Add(finish);
						editor.PropertyLabel.Caret = mc;
						start.TextPos = editor.PropertyLabel.Text.ToLower().IndexOf(filter);
						finish.TextPos = start.TextPos + filter.Length;
						new SelectionPresenter(editor.PropertyLabel, start, finish, new SelectionParams() {
							Color = Theme.Colors.TextSelection,
							OutlineThickness = 0
						});
					}

					editor.ContainerWidget.LayoutCell = new LayoutCell(Alignment.LeftCenter, 1);
					editor.PropertyChanged = () => {
						hotkeyEditor.UpdateButtonCommands();
						hotkeyEditor.UpdateShortcuts();
					};

					var dragGesture = new DragGesture();
					editor.ContainerWidget.Gestures.Add(dragGesture);

					var task = new Task(UpdateDragCursor(selectedShortcutsView, hotkeyEditor));
					dragGesture.Recognized += () => editor.ContainerWidget.LateTasks.Add(task);
					dragGesture.Ended += () => {
						editor.ContainerWidget.LateTasks.Remove(task);
						var nodeUnderMouse = WidgetContext.Current.NodeUnderMouse;
						if (nodeUnderMouse == selectedShortcutsView && hotkeyEditor.Main != Key.Unknown) {
							if (hotkeyEditor.Main != Key.Unknown) {
								command.Shortcut = new Shortcut(hotkeyEditor.Modifiers, hotkeyEditor.Main);
								hotkeyEditor.UpdateButtonCommands();
								hotkeyEditor.UpdateShortcuts();
								hotkeyEditor.SetFocus();
							}
						} else if (nodeUnderMouse as KeyboardButton != null) {
							var button = nodeUnderMouse as KeyboardButton;
							if (Shortcut.ValidateMainKey(button.Key) && !button.Key.IsModifier()) {
								command.Shortcut = new Shortcut(hotkeyEditor.Modifiers, button.Key);
								hotkeyEditor.UpdateButtonCommands();
								hotkeyEditor.UpdateShortcuts();
								hotkeyEditor.SetFocus();
							}
						}
					};
				}
			}
		}

		IEnumerator<object> UpdateDragCursor(ThemedScrollView selectedShortcutsView, HotkeyEditor hotkeyEditor)
		{
			while (true) {
				var nodeUnderMouse = WidgetContext.Current.NodeUnderMouse;
				bool allowDrop =
					(nodeUnderMouse == selectedShortcutsView && hotkeyEditor.Main != Key.Unknown) ||
					(nodeUnderMouse as KeyboardButton != null && !(nodeUnderMouse as KeyboardButton).Key.IsModifier());
				if (allowDrop) {
					Utils.ChangeCursorIfDefault(Cursors.DragHandOpen);
				}
				else {
					Utils.ChangeCursorIfDefault(Cursors.DragHandClosed);
				}
				yield return null;
			}
		}

		private void UpdateProfiles(ThemedDropDownList profilePicker)
		{
			profilePicker.Items.Clear();
			foreach (var profile in HotkeyRegistry.Profiles) {
				profilePicker.Items.Add(new CommonDropDownList.Item(profile.Name, profile));
			}
			profilePicker.Value = HotkeyRegistry.CurrentProfile;
		}
	}
}
