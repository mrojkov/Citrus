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
			MinWidth = TimelineMetrics.ToolbarMinWidth;
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
				CreateUpButton(),
				CreateGotoCurrentProjectDirectoryButton(),
				CreateToggleCookingRulesButton()
			);
		}

		private static Widget CreateGotoCurrentProjectDirectoryButton()
		{
			return new Button
			{
				Text = "Project Dir",
				Clicked = () => {
					FilesystemView.Instance?.GoTo(Project.Current.AssetsDirectory);
				}
			};
		}

		private static Widget CreateUpButton()
		{
			return new Button
			{
				Text = "UP",
				Clicked = () => {
					FilesystemView.Instance?.GoUp();
				}
			};
		}

		private static Widget CreateToggleCookingRulesButton()
		{
			return new Button
			{
				Text = "Cooking Rules",
				Clicked = () => {
					FilesystemView.Instance.ToggleCookingRules();
				}
			};
		}
	}
}