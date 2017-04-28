using System;
using System.Collections.Generic;
using System.Threading;
using Lime;

namespace Orange
{
	public class OrangeInterface: UserInterface
	{
		private Window window;
		private WindowWidget windowWidget;
		private PlatformPicker platformPicker;

		public OrangeInterface()
		{
			var windowSize = new Vector2(500, 400);
			window = new Window(new WindowOptions {
				ClientSize = windowSize,
				FixedSize = false,
				Title = "Orange"
			});
			windowWidget = new DefaultWindowWidget(window) {
				Id = "MainWindow",
				Layout = new VBoxLayout {
					Spacing = 6
				},
				Padding = new Thickness(6),
				Size = windowSize
			};
			windowWidget.AddNode(CreateHeaderSection());
			windowWidget.AddNode(CreateTextView());
			windowWidget.AddNode(CreateFooterSection());
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
			AddPicker(header, "Citrus Project", GetProjectPicker());
			return header;
		}

		private void AddPicker(Widget table, string name, Widget picker)
		{
			var label = new SimpleText(name) {
				VAlignment = VAlignment.Center,
				HAlignment = HAlignment.Left
			};
			table.AddNode(label);
			table.AddNode(picker);
		}

		private Widget GetProjectPicker()
		{
			var container = new Widget {
				Layout = new HBoxLayout()
			};
			EditBox editor;
			Button button;
			container.AddNode(new Widget {
				Layout = new HBoxLayout(),
				Nodes = {
					(editor = new EditBox { LayoutCell = new LayoutCell(Alignment.Center) }),
					new Widget{MinMaxWidth = 4},
					(button = new Button {
						Text = "...",
						MinMaxWidth = 20,
						Draggable = true,
						LayoutCell = new LayoutCell(Alignment.Center)
					})
				}
			});
			editor.Submitted += text => editor.Text = text;
			button.Clicked += () => {
				var dlg = new FileDialog {
					AllowedFileTypes = new[] { "citproj" },
					Mode = FileDialogMode.Open,
					InitialDirectory = "D:\\Dev\\EmptyProject"// Directory.GetCurrentDirectory(),
				};
				if (dlg.RunModal()) {
					editor.Text = dlg.FileName;
					The.Workspace.Open(dlg.FileName);
					platformPicker.Reload();
				}
			};
			return container;
		}

		private Widget CreateTextView()
		{
			var textView = new TextViewe {
				LayoutCell = new LayoutCell(),
			};
			var writer = textView.GetTextWriter();
			Console.SetOut(writer);
			Console.SetError(writer);
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
			go.Clicked += () => System.Threading.Tasks.Task.Run(() => ((Action)actionPicker.Value)());
			container.AddNode(go);
			return container;
		}

		public override bool AskConfirmation(string text)
		{
			ConfirmationWindow confirmation = null;
			Application.InvokeOnMainThread(() => confirmation = new ConfirmationWindow());
			while (confirmation == null) {
				Thread.Sleep(1);
			}
			bool? result = null;
			confirmation.Closed += () => result = confirmation.Result;
			while (result == null) {
				Thread.Sleep(1);
			}
			return result.Value;
			/*var box = new MessageDialog(NativeWindow,
				DialogFlags.Modal, MessageType.Question,
				ButtonsType.YesNo,
				text);
			box.Title = "Orange";
			box.Modal = true;
			var result = box.Run();
			box.Destroy();
			return result == (int)ResponseType.Yes;
			return true;*/
			//throw new System.NotImplementedException();
		}

		public override bool AskChoice(string text, out bool yes)
		{
			yes = true;
			return true;
			//throw new System.NotImplementedException();
		}

		public override void ShowError(string message)
		{
			//throw new System.NotImplementedException();
		}

		public override TargetPlatform GetActivePlatform()
		{
			return platformPicker.SelectedPlatform ?? TargetPlatform.Win;
		}

		public override SubTarget GetActiveSubTarget()
		{
			return platformPicker.SelectedSubTarget;
		}

		public override bool DoesNeedSvnUpdate()
		{
			return false;
			//throw new System.NotImplementedException();
		}

		public override IPluginUIBuilder GetPluginUIBuilder()
		{
			return new PluginUIBuidler();
		}

		public override void CreatePluginUI(IPluginUIBuilder builder)
		{
			//throw new System.NotImplementedException();
		}

		public override void DestroyPluginUI()
		{
			//throw new System.NotImplementedException();
		}
	}

	public class ConfirmationWindow : Window
	{

		public ConfirmationWindow()
		{
			Result = true;
		}

		public bool Result { get; private set; }
	}
}