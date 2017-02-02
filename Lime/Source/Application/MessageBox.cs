#if WIN
using System.Drawing;
using System.Windows.Forms;
using SD = System.Drawing;
using WinForms = System.Windows.Forms;

namespace Lime
{
	[System.ComponentModel.DesignerCategory("")] //Disable designer
	internal class MessageBoxForm : Form
	{
		public MessageBoxForm(string title, string text)
		{
			Text = title;
			FormBorderStyle = FormBorderStyle.FixedDialog;
			AutoSize = true;
			MaximizeBox = false;

			var menuItem = new WinForms.MenuItem(
				"Копировать", (s, e) => Clipboard.Text = text, WinForms.Shortcut.CtrlC);

			var textLabel = new Label {
				BackColor = Color.White,
				AutoSize = true,
				Text = text,
				Padding = new Padding(16),
				Dock = DockStyle.Fill,
				Size = new SD.Size(Width, 220),
				ContextMenu = new ContextMenu(),
			};
			textLabel.ContextMenu.MenuItems.Add(menuItem);

			var okButton = new WinForms.Button {
				Text = "OK",
				Location = new Point(Width - 88, 4),
				Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
				Size = new SD.Size(80, 30),
			};
			okButton.Click += (s, e) => {
				Close();
			};

			var panelButtons = new Panel {
				Dock = DockStyle.Bottom,
				Location = new Point(0, 220),
				Size = new SD.Size(Width, 38),
			};
			panelButtons.SuspendLayout();
			panelButtons.Controls.Add(okButton);

			SuspendLayout();
			Controls.Add(textLabel);
			Controls.Add(panelButtons);
			panelButtons.ResumeLayout(true);
			ResumeLayout(true);

			KeyPreview = true;
			KeyDown += (s, e) => {
				if (e.Control && e.KeyCode == Keys.C) {
					Clipboard.Text = text;
				}
			};
		}
	}
}
#endif
