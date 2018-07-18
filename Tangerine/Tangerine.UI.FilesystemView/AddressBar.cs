using Lime;
using Tangerine.UI.FilesystemView;

namespace Tangerine.UI.FilesystemView
{
	public class AddressBar : Toolbar
	{
		private ThemedEditBox editor;
		private FilesystemView view;

		public string Path
		{
			get {
				return editor.TextWidget.Text;
			}
			set {
				editor.TextWidget.Text = value;
			}
		}

		public AddressBar(FilesystemView view)
		{
			this.view = view;
			Layout = new HBoxLayout();
			Nodes.Add(editor = new ThemedEditBox());
			editor.LayoutCell = new LayoutCell(Alignment.Center);
			Padding.Right += 4;
			Updating += (float delta) => {
				if (editor.Input.WasKeyPressed(Key.Enter)) {
					view.Open(Path);
				}
			};
		}
	}
}
