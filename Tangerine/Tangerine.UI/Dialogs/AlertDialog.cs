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
		int result = -1;

		public AlertDialog(string text, params string[] buttons)
		{
			if (buttons.Length == 0) {
				buttons = new[] { "Ok" };
			}
			window = new Window(new WindowOptions {
				FixedSize = true,
				Title = "Tangerine",
				Visible = false,
				Style = WindowStyle.Dialog
			});
			rootWidget = new ThemedInvalidableWindowWidget(window) {
				LayoutBasedWindowSize = true,
				Padding = new Thickness(8),
				Layout = new VBoxLayout { Spacing = 16 },
				Nodes = {
					new ThemedSimpleText(text) {
						Padding = new Thickness(4)
					},
					(buttonsPanel = new Widget {
						Layout = new HBoxLayout { Spacing = 8 },
						LayoutCell = new LayoutCell(Alignment.RightCenter, 1, 0),
					})
				}
			};
			rootWidget.FocusScope = new KeyboardFocusScope(rootWidget);
			rootWidget.FocusScope.FocusNext.Add(Key.MapShortcut(Key.Right));
			rootWidget.FocusScope.FocusPrevious.Add(Key.MapShortcut(Key.Left));
			var cancelIndex = buttons.ToList().IndexOf("Cancel");
			if (cancelIndex >= 0) {
				result = cancelIndex;
				rootWidget.LateTasks.AddLoop(() => {
					if (rootWidget.Input.ConsumeKeyPress(Key.Escape)) {
						Close(cancelIndex);
					}
				});
			}
			for (int i = 0; i < buttons.Length; i++) {
				var button = new ThemedButton { Text = buttons[i] };
				int j = i;
				button.Clicked += () => Close(j);
				buttonsPanel.AddNode(button);
				if(i == 0) {
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

		public static int Show(string text, params string[] buttons)
		{
			var dialog = new AlertDialog(text, buttons);
			return dialog.Show();
		}
	}
}
