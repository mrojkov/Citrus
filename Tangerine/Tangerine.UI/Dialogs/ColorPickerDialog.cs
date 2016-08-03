using System;
using System.Linq;
using Lime;

namespace Tangerine.UI
{
	public class ColorPickerDialog
	{
		readonly Window window;
		readonly WindowWidget rootWidget;
		readonly Widget spectrum;
		readonly Button okButton;
		readonly Button cancelButton;
		readonly Color4 color;
		Color4 result;

		public ColorPickerDialog(Color4 color)
		{
			window = new Window(new WindowOptions {
				FixedSize = true,
				ClientSize = new Vector2(1, 1),
				Title = "Colors",
				Visible = false,
				Style = WindowStyle.Dialog
			});
			rootWidget = new InvalidableWindowWidget(window) {
				LayoutBasedWindowSize = true,
				Padding = new Thickness(8),
				Layout = new VBoxLayout { Spacing = 8 },
				Nodes = {
					new Widget {
						LayoutCell = new LayoutCell(Alignment.Center),
						Layout = new StackLayout(),
						Nodes = {
							(spectrum = new Widget {
								MinMaxSize = new Vector2(150, 150),
								PostPresenter = new WidgetBoundsPresenter(Color4.Green)
							})
						}
					},
					new Widget {
						Layout = new HBoxLayout { Spacing = 8 },
						LayoutCell = new LayoutCell(Alignment.RightCenter, 1, 0),
						Nodes = {
							(okButton = new Button { Text = "Ok" }),
							(cancelButton = new Button { Text = "Cancel" }),
						}
					}
				}
			};
			rootWidget.Update(0);
			new TabTraverseController(rootWidget);
			new WidgetKeyHandler(rootWidget, KeyBindings.CloseDialog).KeyPressed += () => Close(color);
			rootWidget.Input.CaptureAll();
		}

		void Close(Color4 result)
		{
			this.result = result;
			rootWidget.Input.ReleaseAll();
			window.Close();
		}

		public Color4 Show()
		{
			window.ShowDialog();
			return result;
		}
	}
}