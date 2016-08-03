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
			new TabTraverseController(rootWidget);
			var cancelIndex = buttons.ToList().IndexOf("Cancel");
			if (cancelIndex >= 0) {
				new WidgetKeyHandler(rootWidget, KeyBindings.CloseDialog).KeyPressed += () => Close(cancelIndex);
			}
			rootWidget.Input.CaptureAll();
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
			rootWidget.Input.ReleaseAll();
			window.Close();
		}

		public int Show()
		{
			//rootWidget.Update(0); // Refresh its size layout
			//window.ClientSize = rootWidget.EffectiveMinSize;
			// window.Center();
			window.ShowDialog();
			return result;
		}
	}
}