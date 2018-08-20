using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class SaveRulerDialog
	{
		private readonly Window window;
		private bool result;

		public SaveRulerDialog(Ruler ruler)
		{
			ThemedButton cancelButton;
			ThemedButton okButton;
			CheckBox checkBox;
			EditBox editBox;
			window = new Window(new WindowOptions {
				Title = "Save Ruler",
				Visible = false,
				Style = WindowStyle.Dialog,
			});
			WindowWidget rootWidget = new ThemedInvalidableWindowWidget(window) {
				LayoutBasedWindowSize = true,
				Padding = new Thickness(8),
				Layout = new VBoxLayout { Spacing = 16 },
				Nodes = {
					new Widget {
						Layout = new TableLayout {
							ColumnCount = 2,
							RowCount = 2,
							Spacing = 8,
							ColumnDefaults = {
								new LayoutCell(Alignment.RightCenter, 0.5f, 0),
								new LayoutCell(Alignment.LeftCenter, 1, 0)
							}
						},
						Nodes = {
							new ThemedSimpleText("Ruler name"),
							(editBox = new ThemedEditBox { MinWidth = 150}),
							new ThemedSimpleText("Anchor to root"),
							(checkBox = new ThemedCheckBox())
						}
					},
					new Widget {
						Layout = new HBoxLayout { Spacing = 8 },
						LayoutCell = new LayoutCell(Alignment.RightCenter),
						Nodes = {
							(okButton = new ThemedButton("Ok")),
							(cancelButton = new ThemedButton("Cancel")),
						}
					}
				}
			};
			cancelButton.Clicked += window.Close;
			okButton.AddChangeWatcher(() => editBox.Text, (text) => okButton.Enabled = !string.IsNullOrEmpty(text));
			okButton.Clicked += () => {
				ruler.Name = editBox.Text;
				ruler.AnchorToRoot = checkBox.Checked;
				result = true;
				window.Close();
			};
			rootWidget.FocusScope = new KeyboardFocusScope(rootWidget);
			editBox.SetFocus();
		}

		public bool Show()
		{
			window.ShowModal();
			return result;
		}

	}
}
