using System;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class SaveRulerDialog
	{
		private readonly Window window;
		private readonly WindowWidget rootWidget;
		private readonly Widget buttonsPanel;
		private readonly EditBox EditBox;
		private readonly CheckBox CheckBox;
		private bool result;
		private readonly ThemedButton okButton;
		private readonly ThemedButton cancelButton;
		private readonly Ruler ruler;

		public SaveRulerDialog(Ruler ruler)
		{
			this.ruler = ruler;
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
							RowCount = 2,
							Spacing = 8,
							ColDefaults = {
								new LayoutCell(Alignment.RightCenter, 0.5f, 0),
								new LayoutCell(Alignment.LeftCenter, 1, 0)
							}
						},
						Nodes = {
							new ThemedSimpleText("Ruler name"),
							(EditBox = new ThemedEditBox()),
							new ThemedSimpleText("Anchor to root"),
							(CheckBox = new ThemedCheckBox())
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
				ruler.Name = EditBox.Text;
				ruler.AnchorToRoot = CheckBox.Checked;
				result = true;
				window.Close();
			};
			rootWidget.FocusScope = new KeyboardFocusScope(rootWidget);
			EditBox.SetFocus();
		}

		public bool Show()
		{
			window.ShowModal();
			return result;
		}

	}
}