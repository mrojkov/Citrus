using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime;

namespace Tangerine.UI
{
	public class TextEditorDialog
	{
		private readonly Window window;
		private readonly WindowWidget rootWidget;
		private readonly Button okButton;
		private readonly Button cancelButton;
		private readonly ThemedEditBox editor;

		public TextEditorDialog(string title, string text, Action<string> onSave)
		{
			window = new Window(new WindowOptions {
				ClientSize = new Vector2(300, 200),
				FixedSize = false,
				Title = title,
				Visible = false,
			});
			rootWidget = new ThemedInvalidableWindowWidget(window) {
				Padding = new Thickness(8),
				Layout = new VBoxLayout(),
				Nodes = {
					new Widget {
						Layout = new AnchorLayout(),
						Nodes = {
							(editor = new ThemedEditBox {
								Anchors = Anchors.LeftRightTopBottom,
								Text = text,
							})
						},
					},
					new Widget {
						Layout = new HBoxLayout { Spacing = 8 },
						LayoutCell = new LayoutCell {
							StretchY = 0
						},
						Padding = new Thickness { Top = 5 },
						Nodes = {
							new Widget { MinMaxHeight = 0 },
							(okButton = new ThemedButton { Text = "Ok" }),
							(cancelButton = new ThemedButton { Text = "Cancel" }),
						},
					}
				}
			};
			// TODO: implement multiline scrollable text editor.
			const int maxLines = 640;
			editor.Editor.EditorParams.MaxLines = maxLines;
			editor.MinHeight = editor.TextWidget.FontHeight * maxLines;
			editor.TextWidget.VAlignment = VAlignment.Top;
			cancelButton.Clicked += () => {
				window.Close();
			};
			okButton.Clicked += () => {
				onSave(editor.Text);
				window.Close();
			};
			window.ShowModal();
		}
	}
}
