using System;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class SaveRulerDialog
	{
		readonly Window window;
		readonly WindowWidget rootWidget;
		readonly Widget buttonsPanel;
		readonly EditBox EditBox;
		string result;
		private readonly ThemedButton okButton;
		private readonly ThemedButton cancelButton;

		public SaveRulerDialog()
		{
			window = new Window(new WindowOptions {
				FixedSize = true,
				Title = "Save Ruler",
				Visible = false,
				Style = WindowStyle.Dialog,
			});
			rootWidget = new ThemedInvalidableWindowWidget(window) {
				LayoutBasedWindowSize = true,
				Padding = new Thickness(8),
				Layout = new VBoxLayout { Spacing = 16 },
				Nodes = {
					new Widget {
						Layout = new TableLayout {
							ColCount = 2,
							RowCount = 1,
							Spacing = 8,
							ColDefaults = {
								new LayoutCell(Alignment.RightCenter, 0.5f, 0),
								new LayoutCell(Alignment.LeftCenter, 1, 0)
							}
						},
						Nodes = {
							new ThemedSimpleText("Enter ruler name"),
							(EditBox = new ThemedEditBox()),
						}
					},
					(buttonsPanel = new Widget {
						Layout = new HBoxLayout { Spacing = 8 },
						LayoutCell = new LayoutCell(Alignment.RightCenter),
						Nodes = {
							(okButton = new ThemedButton("Ok")),
							(cancelButton = new ThemedButton("Cancel")),
						}
					})
				}
			};
			cancelButton.Clicked += window.Close;
			okButton.AddChangeWatcher(() => EditBox.Text, (text) => okButton.Enabled = !string.IsNullOrEmpty(text));
			okButton.Clicked += () => {
				result = EditBox.Text;
				window.Close();
			};
			rootWidget.FocusScope = new KeyboardFocusScope(rootWidget);
			EditBox.SetFocus();
		}

		public string Show()
		{
			window.ShowModal();
			return result;
		}

	}
}