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
		private FileChooser projectPicker;
		private PlatformPicker platformPicker;
		private PluginPanel pluginPanel;
		private ThemedTextView textView;
		private TextWriter textWriter;
		private CheckBoxWithLabel updateVcs;
		private Button goButton;
		private Button abortButton;
		private ICommand actionsCommand;

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
			var mainVBox = new Widget {
				Layout = new VBoxLayout {
					Spacing = 6
				}
			};
			mainVBox.AddNode(CreateHeaderSection());
			mainVBox.AddNode(CreateVcsSection());
			mainVBox.AddNode(CreateTextView());
			mainVBox.AddNode(CreateFooterSection());
			windowWidget.AddNode(mainVBox);

			Application.MainMenu = new Menu {
				new Command("&File", new Menu {
					new Command("&Quit", () => { Application.Exit(); })
				}),
				(actionsCommand = new Command("&Actions", new Menu { })),
				new Command("&Help", new Menu {
					new Command("&Update", () => {
						Orange.Updater.ShowUpdaterWindow();
					})
				})
			};
		}

		private Widget CreateHeaderSection()
		{
			var header = new Widget {
				Layout = new TableLayout {
					ColCount = 2,
					RowCount = 2,
					RowSpacing = 6,
					ColSpacing = 6,
					RowDefaults = new List<LayoutCell> {
						new LayoutCell { StretchY = 0 },
						new LayoutCell { StretchY = 0 },
					},
					ColDefaults = new List<LayoutCell> {
						new LayoutCell { StretchX = 0 },
						new LayoutCell(),
					}
				},
				LayoutCell = new LayoutCell { StretchY = 0 }
			};
			AddPicker(header, "Target platform", platformPicker = new PlatformPicker());
			AddPicker(header, "Citrus Project", projectPicker = CreateProjectPicker());
			return header;
		}

		private Widget CreateVcsSection()
		{
			updateVcs = new CheckBoxWithLabel("Update project before build");
			return updateVcs;
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
			var container = new Widget {
				Layout = new HBoxLayout {
					Spacing = 5
				},
			};

			actionPicker = new ThemedDropDownList();
			foreach (var menuItem in The.MenuController.GetVisibleAndSortedItems()) {
				actionPicker.Items.Add(new CommonDropDownList.Item(menuItem.Label, menuItem.Action));
			}
			actionPicker.Index = 0;
			container.AddNode(actionPicker);

			goButton = new ThemedButton("Go");
			goButton.Clicked += () => Execute((Func<string>) actionPicker.Value);
			container.AddNode(goButton);

			abortButton = new ThemedButton("Abort") {
				Enabled = false,
				Visible = false
			};
			abortButton.Clicked += () => AssetCooker.CancelCook();
			container.AddNode(abortButton);

			return container;
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
				The.UI.ScrollLogToEnd();
			}, DoesNeedSvnUpdate,
				Task.ExecuteAsync
			);
		}

		private void EnableControls(bool value)
		{
			goButton.Visible = value;
			abortButton.Visible = !value;

			if (value) {
				abortButton.Input.DerestrictScope();
			}
			else {
				abortButton.Input.RestrictScope();
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
			Dictionary<char, int> letterUsedCount = new Dictionary<char, int>();
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
						if (!letterUsedCount.ContainsKey(key)) { letterUsedCount.Add(key, 0);}
						insertionPoints.Add(new KeyValuePair<char, int>(key, i));
					}
					wordStart = false;
					if (i < label.Length - 1 && label[i + 1] == ' ') {
						wordStart = true;
					}
				}
				insertionPoints.Sort((a, b) => {
					if (!letterUsedCount.ContainsKey(a.Key)) { letterUsedCount.Add(a.Key, 0); }
					if (!letterUsedCount.ContainsKey(b.Key)) { letterUsedCount.Add(b.Key, 0); }
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

		public override bool DoesNeedSvnUpdate()
		{
			return updateVcs.Checked;
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
			config.UpdateBeforeBuild = DoesNeedSvnUpdate();
			config.ActiveTargetIndex = platformPicker.Index;
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
			updateVcs.CheckBox.Checked = config.UpdateBeforeBuild;
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
		}

		private class TextViewWriter : TextWriter
		{
			private readonly ThemedTextView textView;
			private readonly TextWriter consoleOutput;

			public TextViewWriter(ThemedTextView textView, TextWriter consoleOutput)
			{
				this.consoleOutput = consoleOutput;
				this.textView = textView;
			}

			public override void WriteLine(string value)
			{
				Write(value + '\n');
			}

			public override void Write(string value)
			{
				Application.InvokeOnMainThread(() => {
#if DEBUG
					System.Diagnostics.Debug.Write(value);
#endif // DEBUG
					consoleOutput.Write(value);
					textView.Append(value);
					textView.ScrollToEnd();
				});
			}

			public override Encoding Encoding { get; }
		}
	}
}
