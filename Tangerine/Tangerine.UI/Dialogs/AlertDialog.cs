using System;
using System.Linq;
using Lime;

namespace Tangerine.UI
{
	public class AlertDialog
	{
		readonly Window window;
		readonly WindowWidget rootWidget;
		readonly Widget buttonsPanel;
		int result;

		public AlertDialog(string title, string text, params string[] buttons)
		{
			window = new Window(new WindowOptions {
				FixedSize = true,
				Title = title,
				Visible = false,
				Style = WindowStyle.Dialog
			});
			rootWidget = new InvalidableWindowWidget(window) {
				LayoutBasedWindowSize = true,
				Padding = new Thickness(8),
				Layout = new VBoxLayout { Spacing = 16 },
				Nodes = {
					new SimpleText(text) {
						Padding = new Thickness(4)
					},
					(buttonsPanel = new Widget {
						Layout = new HBoxLayout { Spacing = 8 },
						LayoutCell = new LayoutCell(Alignment.RightCenter, 1, 0),
					})
				}
			};
			rootWidget.FocusScope = new KeyboardFocusScope(rootWidget);
			var cancelIndex = buttons.ToList().IndexOf("Cancel");
			if (cancelIndex >= 0) {
				rootWidget.LateTasks.AddLoop(() => {
					if (rootWidget.Input.ConsumeKeyPress(Key.Escape)) {
						Close(cancelIndex);
					}
				});
			}
			for (int i = 0; i < buttons.Length; i++) {
				var button = new Button { Text = buttons[i] };
				int j = i;
				button.Clicked += () => Close(j);
				buttonsPanel.AddNode(button);
				if (i == 0) {
					button.SetFocus();
				}
			}
		}

		void Close(int result)
		{
			this.result = result;
			window.Close();
		}

		public int Show()
		{
			window.ShowModal();
			return result;
		}
	}
}