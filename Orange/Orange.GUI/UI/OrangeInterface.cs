using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Lime;

namespace Orange
{
	public class OrangeInterface: UserInterface
	{
		private readonly Window window;
		private readonly WindowWidget windowWidget;
		private PlatformPicker platformPicker;
		private PluginPanel pluginPanel;
		private TextView textView;
		private TextWriter textWriter;

		public OrangeInterface()
		{
			var windowSize = new Vector2(500, 400);
			window = new Window(new WindowOptions {
				ClientSize = windowSize,
				FixedSize = false,
				Title = "Orange"
			});
			window.Closed += The.Workspace.Save;
			windowWidget = new DefaultWindowWidget(window) {
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
			mainVBox.AddNode(CreateTextView());
			mainVBox.AddNode(CreateFooterSection());
			windowWidget.AddNode(mainVBox);
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
						new LayoutCell{ StretchY = 0 },
						new LayoutCell{ StretchY = 0 },
					},
					ColDefaults = new List<LayoutCell> {
						new LayoutCell{ StretchX = 0 },
						new LayoutCell(),
					}
				},
				LayoutCell = new LayoutCell { StretchY = 0 }
			};
			AddPicker(header, "Target platform", platformPicker = new PlatformPicker());
			AddPicker(header, "Citrus Project", CreateProjectPicker());
			return header;
		}

		private static void AddPicker(Node table, string name, Node picker)
		{
			var label = new SimpleText(name) {
				VAlignment = VAlignment.Center,
				HAlignment = HAlignment.Left
			};
			table.AddNode(label);
			table.AddNode(picker);
		}

		private Widget CreateProjectPicker()
		{
			var picker = new FileChooser();
			picker.FileChosen += file => {
				The.Workspace.Open(file);
				platformPicker.Reload();
			};
			return picker;
		}

		private Widget CreateTextView()
		{
			textView = new TextView {
				LayoutCell = new LayoutCell(),
			};
			textWriter = textView.GetTextWriter();
			Console.SetOut(textWriter);
			Console.SetError(textWriter);
			return textView;
		}

		private Widget CreateFooterSection()
		{
			var container = new Widget {
				Layout = new HBoxLayout {
					Spacing = 5
				},
			};
			var actionPicker = new DropDownList();

			foreach (var menuItem in The.MenuController.GetVisibleAndSortedItems()) {
				actionPicker.Items.Add(new CommonDropDownList.Item(menuItem.Label, menuItem.Action));
			}

			container.AddNode(actionPicker);
			actionPicker.Index = 0;
			var go = new Button("Go");
			go.Clicked += () => Execute((Action) actionPicker.Value);
			container.AddNode(go);
			return container;
		}

		private void Execute(Action action)
		{
#if WIN
			if (GetActiveTarget().Platform == TargetPlatform.iOS) {
				ShowError("iOS target is not supported on Windows platform");
				return;
			}
#endif
			windowWidget.Tasks.Add(ExecuteAsync(action));
		}

		private IEnumerator<object> ExecuteAsync(Action action)
		{
			var startTime = DateTime.Now;
			The.Workspace.Save();
			EnableControls(false);
			The.Workspace?.AssetFiles?.Rescan();
			textView.Text = string.Empty;
			yield return Task.ExecuteAsync(() => {
				try {
					action();
				}
				catch (System.Exception ex) {
					textWriter.WriteLine(ex);
				}
			});
			textWriter.WriteLine("Output has been copied to clipboard.");
			Clipboard.Text = textView.Text;
			EnableControls(true);
			ShowTimeStatistics(startTime);

		}

		private void EnableControls(bool b)
		{
			//throw new NotImplementedException();
		}

		private void ShowTimeStatistics(DateTime startTime)
		{
			var endTime = DateTime.Now;
			var delta = endTime - startTime;
			Console.WriteLine("Elapsed time {0}:{1}:{2}", delta.Hours, delta.Minutes, delta.Seconds);
		}

		public override void ClearLog()
		{
			textView.Text = string.Empty;
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
			return false;
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
	}
}