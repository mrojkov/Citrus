using Lime;
using Tangerine.Core;

namespace Tangerine.UI.FilesystemView
{
	public class Toolbar : Widget
	{
		public Toolbar()
		{
			Padding = new Thickness { Left = 4 };
			MinMaxHeight = Metrics.ToolbarHeight;
			MinWidth = 50;// TimelineMetrics.ToolbarMinWidth;
			Presenter = new DelegatePresenter<Widget>(Render);
			Layout = new HBoxLayout { Spacing = 2, CellDefaults = new LayoutCell(Alignment.Center) };
		}

		static void Render(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, widget.Size, ColorTheme.Current.Toolbar.Background);
		}
	}
	public class FilesystemToolbar : Toolbar
	{
		public FilesystemToolbar()
		{
			Nodes.AddRange(
				CreateGotoCurrentProjectDirectoryButton(),
				CreateUpButton(),
				CreateGoBackwardButton(),
				CreateGoForwardButton(),
				CreateToggleCookingRulesButton(),
				CreateTogglePreviewButton()
			);
		}

		private Widget CreateGoBackwardButton()
		{
			return new ToolbarButton(IconPool.GetTexture("Filesystem.ArrowLeft")) {
				Clicked = () => FilesystemView.Instance?.GoBackward(),
			};
		}

		private Widget CreateGoForwardButton()
		{
			return new ToolbarButton(IconPool.GetTexture("Filesystem.ArrowRight")) {
				Clicked = () => FilesystemView.Instance?.GoForward(),
			};
		}

		private static Widget CreateGotoCurrentProjectDirectoryButton()
		{
			return new ToolbarButton(IconPool.GetTexture("Filesystem.Home")) {
				Clicked = () => {
					FilesystemView.Instance?.GoTo(Project.Current.AssetsDirectory);
				}
			};
		}

		private static Widget CreateUpButton()
		{
			return new ToolbarButton(IconPool.GetTexture("Filesystem.ArrowUp")) {
				Clicked = () => {
					FilesystemView.Instance?.GoUp();
				}
			};
		}

		private static Widget CreateToggleCookingRulesButton()
		{
			ToolbarButton b = null;
			var up = Core.UserPreferences.Instance.Get<UserPreferences>();
			return b = new ToolbarButton(IconPool.GetTexture("Filesystem.CookingRules")) {
				Checked = up.ShowCookingRulesEditor,
				Clicked = () => {
					FilesystemView.Instance.ToggleCookingRules();
					up.ShowCookingRulesEditor = b.Checked = !b.Checked;
				}
			};
		}

		private static Widget CreateTogglePreviewButton()
		{
			ToolbarButton b = null;
			var up = Core.UserPreferences.Instance.Get<UserPreferences>();
			return b = new ToolbarButton(IconPool.GetTexture("Filesystem.Preview")) {
				Checked = up.ShowSelectionPreview,
				Clicked = () => {
					FilesystemView.Instance.TogglePreview();
					up.ShowSelectionPreview = b.Checked = !b.Checked;
				}
			};
		}
	}
}