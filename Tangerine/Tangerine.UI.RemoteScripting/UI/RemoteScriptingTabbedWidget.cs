using Lime;
using Tangerine.Core;

namespace Tangerine.UI.RemoteScripting
{
	internal class RemoteScriptingTabbedWidget : ThemedTabbedWidget
	{
		public RemoteScriptingTabbedWidget(RemoteScriptingStatusBar statusBar)
		{
			TabBar = new ThemedTabBar();
			var pages = new Page[] {
				new RemoteScriptingAssemblyPage(statusBar)
			};
			var isFirstPage = true;
			foreach (var page in pages) {
				page.Initialize();
				AddTab(page.Tab, page.Content, isFirstPage);
				isFirstPage = false;
			}
		}

		public abstract class Page
		{
			public ThemedTab Tab { get; protected set; }
			public Widget Content { get; protected set; }

			public abstract void Initialize();
		}

		public class Toolbar : Widget
		{
			public Widget Content { get; }

			public Toolbar()
			{
				Padding = new Thickness(4);
				MinMaxHeight = Metrics.ToolbarHeight;
				MinWidth = 50;
				Presenter = new SyncDelegatePresenter<Widget>(Render);
				Layout = new HBoxLayout { Spacing = 2, DefaultCell = new DefaultLayoutCell(Alignment.Center) };
				Nodes.AddRange(
					new Widget {
						Layout = new VBoxLayout(),
						Nodes = {
							(Content = new Widget {
								Layout = new HBoxLayout { Spacing = 2 }
							})
						}
					}
				);
			}

			private static void Render(Widget widget)
			{
				widget.PrepareRendererState();
				Renderer.DrawRect(Vector2.Zero, widget.Size, ColorTheme.Current.Toolbar.Background);
			}
		}

		public class TextView : ThemedTextView
		{
			private readonly ICommand commandClear = new Command("Clear");

			public TextView()
			{
				var menu = new Menu {
					Command.Copy,
					commandClear
				};
				Updated += dt => {
					if (
						Input.WasKeyPressed(Key.Mouse0) ||
						Input.WasKeyPressed(Key.Mouse1)
					) {
						SetFocus();
						Window.Current.Activate();
					}
					if (IsFocused()) {
						Command.Copy.Enabled = true;
						if (Command.Copy.WasIssued()) {
							Command.Copy.Consume();
							Clipboard.Text = DisplayText;
						}
					}
					if (Input.WasKeyPressed(Key.Mouse1)) {
						menu.Popup();
					}
					if (IsFocused() && Command.Copy.WasIssued()) {
						Command.Copy.Consume();
						Clipboard.Text = Text;
					}
					if (commandClear.WasIssued()) {
						commandClear.Consume();
						Clear();
					}
					var i = Content.Nodes.Count;
					// numbers choosen by guess
					if (i >= 500) {
						Content.Nodes.RemoveRange(0, 250);
					}
				};
			}
		}
	}
}
