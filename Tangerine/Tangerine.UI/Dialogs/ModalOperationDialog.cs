using Lime;

namespace Tangerine.UI
{
	public class ModalOperationDialog
	{
		public delegate string GetCurrentStatusDelegate();

		private readonly Window window;
		private readonly WindowWidget rootWidget;
		private readonly ThemedSimpleText statusText;

		public ModalOperationDialog(GetCurrentStatusDelegate getCurrentStatus, string title = null)
		{
			window = new Window(new WindowOptions {
				FixedSize = true,
				Title = title ?? "Tangerine",
				Visible = false,
				Style = WindowStyle.Borderless
			});
			rootWidget = new ThemedInvalidableWindowWidget(window) {
				LayoutBasedWindowSize = true,
				Padding = new Thickness(8),
				Layout = new VBoxLayout { Spacing = 16 },
				Nodes = {
					(statusText = new ThemedSimpleText {
						Padding = new Thickness(4)
					})
				}
			};
			rootWidget.FocusScope = new KeyboardFocusScope(rootWidget);
			rootWidget.LateTasks.AddLoop(() => {
				statusText.Text = $"{title}\n{getCurrentStatus()}";
			});
		}

		public void Show()
		{
			window.ShowModal();
		}

		public void Close()
		{
			window.Close();
		}
	}
}