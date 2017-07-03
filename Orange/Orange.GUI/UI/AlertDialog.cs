using System.Linq;
using Lime;

namespace Orange
{
	public class AlertDialog
	{
		readonly Window window;
		readonly WindowWidget rootWidget;
		readonly Widget buttonsPanel;
		int result;

		public AlertDialog(string text, params string[] buttons)
		{
			if (buttons.Length == 0) {
				buttons = new[] { "Ok" };
			}
			window = new Window(new WindowOptions {
				FixedSize = true,
				Title = "Orange",
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
			var cancelIndex = buttons.ToList().IndexOf("Cancel");
			if (cancelIndex >= 0) {
				rootWidget.LateTasks.AddLoop(() => {
					if (rootWidget.Input.ConsumeKeyPress(Key.Escape)) {
						Close(cancelIndex);
					}
				});
			}
			for (int i = 0; i < buttons.Length; i++) {
				var button = new ThemedButton(buttons[i]);
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

		public static int Show(string text, params string[] buttons)
		{
			var dialog = new AlertDialog(text, buttons);
			return dialog.Show();
		}
	}

	public class ConfirmationDialog : AlertDialog
	{
		public ConfirmationDialog(string text): base(text, "Yes", "No") { }

		public new bool Show()
		{
			return GetResult(base.Show());
		}

		private bool GetResult(int index)
		{
			return index == 0;
		}

		public static bool Show(string text)
		{
			var dialog = new ConfirmationDialog(text);
			return dialog.Show();
		}
	}
}