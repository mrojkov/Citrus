using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Lime;
using Orange.Source;

namespace Orange
{
	public class OrangeInterface: UserInterface
	{
		private readonly Window window;
		private readonly WindowWidget windowWidget;
		private Widget hBox;
		private Widget mainVBox;
		private Widget bundlePicker;
		private FileChooser projectPicker;
		private PlatformPicker platformPicker;
		private PluginPanel pluginPanel;
		private ThemedTextView textView;
		private TextWriter textWriter;
		private Button goButton;
		private Button abortButton;
		private Widget footerSection;
		private ProgressBarField progressBarField;
		private ICommand actionsCommand;

		private Command cacheLocalAndRemote;
		private Command cacheRemote;
		private Command cacheLocal;
		private Command cacheNone;
		private Command bundlePickerCommand;

		public OrangeInterface()
		{
			var windowSize = new Vector2(500, 400);
			window = new Window(new WindowOptions {
				ClientSize = windowSize,
				FixedSize = false,
				Title = "Orange",
#if WIN
			Icon = new System.Drawing.Icon(new EmbeddedResource("Orange.GUI.Resources.Orange.ico", "Orange.GUI").GetResourceStream()),
#endif // WIN
		});
			window.Closed += The.Workspace.Save;
			windowWidget = new ThemedInvalidableWindowWidget(window) {
				Id = "MainWindow",
				Layout = new HBoxLayout {
					Spacing = 6
				},
				Padding = new Thickness(6),
				Size = windowSize
			};
			hBox = new Widget {
				Layout = new HBoxLayout {
					Spacing = 6
				}
			};
			mainVBox = new Widget {
				Layout = new VBoxLayout {
					Spacing = 6
				}
			};
			mainVBox.AddNode(CreateHeaderSection());
			mainVBox.AddNode(CreateTextView());
			progressBarField = new ProgressBarField();
			mainVBox.AddNode(progressBarField);
			mainVBox.AddNode(CreateFooterSection());
			hBox.AddNode(mainVBox);
			bundlePicker = BundlePicker.CreateBundlePicker();
			hBox.AddNode(bundlePicker);
			windowWidget.AddNode(hBox);
			CreateMenu();
		}

		private void CreateMenu()
		{
			// TODO Duplicates code from Tangerine.TangerineMenu.cs. Both should be presented at one file
			cacheLocalAndRemote = new Command("Local &and remote", () => UpdateCacheModeCheckboxes(AssetCacheMode.Local | AssetCacheMode.Remote));
			cacheRemote = new Command("&Remote", () => UpdateCacheModeCheckboxes(AssetCacheMode.Remote));
			cacheLocal = new Command("&Local", () => UpdateCacheModeCheckboxes(AssetCacheMode.Local));
			cacheNone = new Command("&None", () => UpdateCacheModeCheckboxes(AssetCacheMode.None));
			bundlePickerCommand = new Command("&Bundle picker", () => UpdateBundlePicker(!bundlePicker.Visible));

			Application.MainMenu = new Menu {
				new Command("&File", new Menu {
					new Command("&Quit", () => { Application.Exit(); })
				}),
				new Command("&View", new Menu {
					new Command("&Panels", new Menu {
						bundlePickerCommand
					})
				}),
				(actionsCommand = new Command("&Actions", new Menu { })),
				new Command("&Cache", new Menu {
					new Command("&Actions", new Menu {
						new Command("&Upload cache to server", () => Execute(() => {
								UploadCacheToServer.UploadCacheToServerAction();
								return null;
							}
						))
					}),
					new Command("&Mode", new Menu {
						cacheLocalAndRemote,
						cacheRemote,
						cacheLocal,
						cacheNone
					})
				})
			};
		}

		// TODO Duplicates code from Tangerine.OrangeInterface.cs. Both should be presented at one file
		private void UpdateCacheModeCheckboxes(AssetCacheMode state)
		{
			cacheLocalAndRemote.Checked = state == (AssetCacheMode.Local | AssetCacheMode.Remote);
			cacheRemote.Checked = state == AssetCacheMode.Remote;
			cacheLocal.Checked = state == AssetCacheMode.Local;
			cacheNone.Checked = state == AssetCacheMode.None;
			The.Workspace.AssetCacheMode = state;
		}

		private Widget CreateHeaderSection()
		{
			var header = new Widget {
				Layout = new TableLayout {
					ColumnCount = 2,
					RowCount = 2,
					RowSpacing = 6,
					ColumnSpacing = 6,
					RowDefaults = new List<DefaultLayoutCell> {
						new DefaultLayoutCell { StretchY = 0 },
						new DefaultLayoutCell { StretchY = 0 },
					},
					ColumnDefaults = new List<DefaultLayoutCell> {
						new DefaultLayoutCell { StretchX = 0 },
						new DefaultLayoutCell(),
					}
				},
				LayoutCell = new LayoutCell { StretchY = 0 }
			};
			AddPicker(header, "Target platform", platformPicker = new PlatformPicker());
			AddPicker(header, "Citrus Project", projectPicker = CreateProjectPicker());
			return header;
		}

		private static void AddPicker(Node table, string name, Node picker)
		{
			var label = new ThemedSimpleText(name) {
				VAlignment = VAlignment.Center,
				HAlignment = HAlignment.Left
			};
			label.MinHeight = Theme.Metrics.DefaultButtonSize.Y;
			table.AddNode(label);
			table.AddNode(picker);
		}

		private static FileChooser CreateProjectPicker()
		{
			var picker = new FileChooser();
			picker.FileChosenByUser += The.Workspace.Open;
			return picker;
		}

		private Widget CreateTextView()
		{
			textView = new ThemedTextView();
			textWriter = new TextViewWriter(textView, Console.Out);
			Console.SetOut(textWriter);
			Console.SetError(textWriter);
			var menu = new Menu();
			var shCopy = new Shortcut(Modifiers.Control, Key.C);
			var command = new Command
			{
				Shortcut = shCopy,
				Text = "Copy All",
			};
			menu.Add(command);
			textView.Updated += (dt) => {
				if (textView.Input.WasKeyPressed(Key.Mouse1)) {
					menu.Popup();
				}
				if (command.WasIssued()) {
					command.Consume();
					Clipboard.Text = textView.Text;
				}
			};
			return textView;
		}

		private ThemedDropDownList actionPicker;

		private Widget CreateFooterSection()
		{
			footerSection = new Widget {
				Layout = new HBoxLayout {
					Spacing = 5
				},
			};

			actionPicker = new ThemedDropDownList();
			foreach (var menuItem in The.MenuController.GetVisibleAndSortedItems()) {
				actionPicker.Items.Add(new CommonDropDownList.Item(menuItem.Label, menuItem.Action));
			}
			actionPicker.Index = 0;
			footerSection.AddNode(actionPicker);

			goButton = new ThemedButton("Go");
			goButton.Clicked += () => Execute((Func<string>) actionPicker.Value);
			footerSection.AddNode(goButton);

			abortButton = new ThemedButton("Abort") {
				Enabled = false,
				Visible = false
			};
			abortButton.Clicked += () => AssetCooker.CancelCook();
			footerSection.AddNode(abortButton);

			return footerSection;
		}

		public override void StopProgressBar()
		{
			progressBarField.HideAndClear();
		}

		public override void SetupProgressBar(int maxPosition)
		{
			progressBarField.Setup(maxPosition);
		}

		public override void IncreaseProgressBar(int amount = 1)
		{
			progressBarField.Progress(amount);
		}

		public void UpdateBundlePicker(bool value)
		{
			bundlePicker.Visible = value;
			bundlePickerCommand.Checked = value;
			The.Workspace.BundlePickerVisible = value;
		}

		private void Execute(Func<string> action)
		{
			windowWidget.Tasks.Add(ExecuteTask(action));
		}

		private IEnumerator<object> ExecuteTask(Func<string> action)
		{
			yield return OrangeActionsHelper.ExecuteOrangeAction(action, () => {
				The.Workspace.Save();
				EnableControls(false);
				textView.Clear();
			}, () => {
				EnableControls(true);
				if (textView.ScrollPosition == textView.MaxScrollPosition) {
					The.UI.ScrollLogToEnd();
				}
			},
				Task.ExecuteAsync
			);
		}

		private void EnableControls(bool value)
		{
			goButton.Visible = value;
			abortButton.Visible = !value;
			EnableChildren(windowWidget, value);
			hBox.Enabled = true;
			mainVBox.Enabled = true;
			EnableChildren(mainVBox, value);
			footerSection.Enabled = true;
			EnableChildren(footerSection, value);
			abortButton.Enabled = !value;
			textView.Enabled = true;
			progressBarField.Enabled = true;
			bundlePicker.Enabled = true;
			EnableChildren(bundlePicker, value);
		}

		private void EnableChildren(Widget widget, bool value)
		{
			foreach (var node in widget.Nodes) {
				(node as Widget).Enabled = value;
			}
		}

		public override void OnWorkspaceOpened()
		{
			platformPicker.Reload();
			AssetCooker.BeginCookBundles += () => abortButton.Enabled = true;
			AssetCooker.EndCookBundles += () => abortButton.Enabled = false;
		}

		public override void ClearLog()
		{
			textView.Clear();
		}

		public override void RefreshMenu()
		{
			actionPicker.Items.Clear();
			actionsCommand.Menu.Clear();
			var letterUsedCount = new Dictionary<char, int>();
			foreach (var menuItem in The.MenuController.GetVisibleAndSortedItems()) {
				actionPicker.Items.Add(new CommonDropDownList.Item(menuItem.Label, menuItem.Action));
				// Arrange win-specific hotkey ampersands, minimizing conflicts
				var label = menuItem.Label.ToLower();
				bool wordStart = true;
				var insertionPoints = new List<KeyValuePair<char, int>>();
				for (int i = 0; i < label.Length; i++) {
					if (label[i] == ' ') {
						continue;
					}
					if (wordStart) {
						var key = label[i];
						if (!letterUsedCount.ContainsKey(key)) {
							letterUsedCount.Add(key, 0);
						}
						insertionPoints.Add(new KeyValuePair<char, int>(key, i));
					}
					wordStart = false;
					if (i < label.Length - 1 && label[i + 1] == ' ') {
						wordStart = true;
					}
				}
				insertionPoints.Sort((a, b) => {
					if (!letterUsedCount.ContainsKey(a.Key)) {
						letterUsedCount.Add(a.Key, 0);
					}
					if (!letterUsedCount.ContainsKey(b.Key)) {
						letterUsedCount.Add(b.Key, 0);
					}
					return letterUsedCount[a.Key] - letterUsedCount[b.Key];
				});
				var labelWithAmpersand = menuItem.Label.Insert(insertionPoints[0].Value, "&");
				letterUsedCount[insertionPoints[0].Key]++;
				actionsCommand.Menu.Add(new Command(labelWithAmpersand, () => { Execute(menuItem.Action); }));
			}
		}

		public override bool AskConfirmation(string text)
		{
			bool? result = null;
			Application.InvokeOnMainThread(() => result = ConfirmationDialog.Show(text));
			while (result == null) {
				Thread.Sleep(1);
			}
			return result.Value;
		}

		public override bool AskChoice(string text, out bool yes)
		{
			yes = true;
			return true;
		}

		public override void ShowError(string message)
		{
			Application.InvokeOnMainThread(() => AlertDialog.Show(message));
		}

		public override Target GetActiveTarget()
		{
			return platformPicker.SelectedTarget;
		}

		public override IPluginUIBuilder GetPluginUIBuilder()
		{
			return new PluginUIBuidler();
		}

		public override void CreatePluginUI(IPluginUIBuilder builder)
		{
			if (!builder.SidePanel.Enabled) {
				return;
			}
			pluginPanel = builder.SidePanel as PluginPanel;
			windowWidget.AddNode(pluginPanel);
			window.ClientSize = new Vector2(window.ClientSize.X + 150, window.ClientSize.Y);
		}

		public override void DestroyPluginUI()
		{
			windowWidget.Nodes.Remove(pluginPanel);
			if (pluginPanel != null) {
				window.ClientSize = new Vector2(window.ClientSize.X - 150, window.ClientSize.Y);
				pluginPanel = null;
			}
		}

		public override void SaveToWorkspaceConfig(ref WorkspaceConfig config)
		{
			config.ActiveTargetIndex = platformPicker.Index;
			config.BundlePickerVisible = bundlePicker.Visible;
			if (window.State != WindowState.Minimized) {
				config.ClientPosition = window.ClientPosition;
				config.ClientSize = window.ClientSize;
			}
		}

		public override void LoadFromWorkspaceConfig(WorkspaceConfig config)
		{
			var newIndex = config.ActiveTargetIndex;
			if (newIndex < 0 || newIndex >= platformPicker.Items.Count) {
				newIndex = 0;
			}
			platformPicker.Index = newIndex;
			projectPicker.ChosenFile = config.CitrusProject;
			if (config.ClientPosition.X < 0) {
				config.ClientPosition.X = 0;
			}
			if (config.ClientPosition.Y < 0) {
				config.ClientPosition.Y = 0;
			}
			if (config.ClientPosition != Vector2.Zero) {
				window.ClientPosition = config.ClientPosition;
			}
			if (config.ClientSize != Vector2.Zero) {
				window.ClientSize = config.ClientSize;
			}
			UpdateCacheModeCheckboxes(config.AssetCacheMode);
			UpdateBundlePicker(config.BundlePickerVisible);
		}

		private class TextViewWriter : TextWriter
		{
			private readonly ThemedTextView textView;
			private readonly TextWriter consoleOutput;

			public TextViewWriter(ThemedTextView textView, TextWriter consoleOutput)
			{
				this.consoleOutput = consoleOutput;
				this.textView = textView;
				this.textView.Behaviour.Content.Gestures.Add(new DragGesture());
			}

			public override void WriteLine(string value)
			{
				Write(value + '\n');
			}

			private bool autoscrollEnabled = false;

			public override void Write(string value)
			{
				Application.InvokeOnMainThread(() => {
#if DEBUG
					System.Diagnostics.Debug.Write(value);
#endif // DEBUG
					if (autoscrollEnabled && !textView.Behaviour.IsScrolling() && !(textView.Behaviour as ScrollViewWithSlider).SliderIsDragging) {
						textView.ScrollToEnd();
					}
					autoscrollEnabled = textView.ScrollPosition == textView.MaxScrollPosition;
					consoleOutput.Write(value);
					textView.Append(value);
				});
			}

			public override Encoding Encoding { get; }
		}

		private class ProgressBarField : Widget
		{
			public int CurrentPosition;
			public int MaxPosition;

			private ThemedSimpleText textFieldA;
			private ThemedSimpleText textFieldB;

			public ProgressBarField()
			{
				Layout = new HBoxLayout { Spacing = 6 };
				MinMaxHeight = Theme.Metrics.DefaultButtonSize.Y;

				var bar = new ThemedFrame();
				var rect = new Widget();
				rect.CompoundPresenter.Add(new WidgetFlatFillPresenter(Lime.Theme.Colors.SelectedBorder));
				rect.Tasks.AddLoop(() => {
					rect.Size = new Vector2(bar.Width * Mathf.Clamp((float)CurrentPosition / MaxPosition, 0, 1), bar.ContentHeight);
				});
				bar.AddNode(rect);

				textFieldA = new ThemedSimpleText {
					VAlignment = VAlignment.Center,
					HAlignment = HAlignment.Center,
				};
				textFieldB = new ThemedSimpleText {
					VAlignment = VAlignment.Center,
					HAlignment = HAlignment.Center,
				};

				AddNode(bar);
				AddNode(textFieldA);
				AddNode(textFieldB);

				HideAndClear();
			}

			public void Progress(int amount = 1)
			{
				CurrentPosition += amount;
				Application.InvokeOnMainThread(() => {
					textFieldA.Text = (int)((float)CurrentPosition / MaxPosition * 100) + "%";
					textFieldB.Text = CurrentPosition + " / " + MaxPosition;
				});
			}

			public void Setup(int maxPosition)
			{
				CurrentPosition = 0;
				MaxPosition = maxPosition;
				Progress(0);
				Visible = true;
			}

			public void HideAndClear()
			{
				CurrentPosition = 100;
				MaxPosition = 100;
				Visible = false;
			}
		}
	}
}
