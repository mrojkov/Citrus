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
			var textHeight = DesktopTheme.Metrics.TextHeight;
			var textExtent = Renderer.MeasureTextLine(FontPool.Instance.DefaultFont, text, textHeight);
			var windowSize = new Vector2((textExtent.X + 20).Clamp(200, 800), 100);
			window = new Window(new WindowOptions {
				ClientSize = windowSize,
				FixedSize = true,
				Title = title,
				Visible = false,
				Style = WindowStyle.Dialog
			});
			rootWidget = new InvalidableWindowWidget(window) {
				Padding = new Thickness(8),
				Layout = new VBoxLayout { Spacing = 8 },
				Nodes = {
					new Widget {
						Layout = new StackLayout(),
						Nodes = {
							new SimpleText {
								Text = text,
								FontHeight = textHeight,
								AutoSizeConstraints = false,
								HAlignment = HAlignment.Center,
								VAlignment = VAlignment.Top
							},
						}
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
			window.ShowDialog();
			return result;
		}
	}
}