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
		FilesystemView view;
		private SimpleText pathText;

		public FilesystemToolbar(FilesystemView view)
		{
			this.view = view;
			Nodes.AddRange(
				new Widget {
					Presenter = new DelegatePresenter<Widget>((w) => {
						w.PrepareRendererState();
						Renderer.DrawLine(0.0f, w.Size.Y, w.Size.X, w.Size.Y, Theme.Colors.SeparatorColor);
					}),
					Layout = new VBoxLayout(),
					Nodes = {
						new Widget {
							Layout = new HBoxLayout(),
							Nodes = {
								CreateGotoCurrentProjectDirectoryButton(),
								CreateUpButton(),
								CreateGoBackwardButton(),
								CreateGoForwardButton(),
								CreateToggleCookingRulesButton(),
								CreateTogglePreviewButton(),
								CreateSplitHButton(),
								CreateSplitVButton(),
								new Widget {
									LayoutCell = new LayoutCell { Stretch = new Vector2(9999, 9999)}
								},
								CreateCloseButton()
							}
						},
						(pathText = new ThemedSimpleText {
						}),
					}
				}
			);
		}

		public string Path
		{
			get
			{
				return pathText.Text;
			}
			set
			{
				pathText.Text = value;
			}
		}

		private Widget CreateGoBackwardButton()
		{
			return new ToolbarButton(IconPool.GetTexture("Filesystem.ArrowLeft")) {
				Clicked = () => view.GoBackward(),
			};
		}

		private Widget CreateGoForwardButton()
		{
			return new ToolbarButton(IconPool.GetTexture("Filesystem.ArrowRight")) {
				Clicked = () => view.GoForward(),
			};
		}

		private Widget CreateGotoCurrentProjectDirectoryButton()
		{
			return new ToolbarButton(IconPool.GetTexture("Filesystem.Home")) {
				Clicked = () => {
					if (Project.Current != Project.Null) {
						view.GoTo(Project.Current.AssetsDirectory); 
					}
				}
			};
		}

		private Widget CreateUpButton()
		{
			return new ToolbarButton(IconPool.GetTexture("Filesystem.ArrowUp")) {
				Clicked = () => view.GoUp()
			};
		}

		private Widget CreateToggleCookingRulesButton()
		{
			ToolbarButton b = null;
			var up = view.RootWidget.Components.Get<ViewNodeComponent>().ViewNode as FSViewNode;
			return b = new ToolbarButton(IconPool.GetTexture("Filesystem.CookingRules")) {
				Checked = up.ShowCookingRulesEditor,
				Clicked = () => {
					view.ToggleCookingRules();
					up.ShowCookingRulesEditor = b.Checked = !b.Checked;
				}
			};
		}

		private Widget CreateTogglePreviewButton()
		{
			ToolbarButton b = null;
			var up = view.RootWidget.Components.Get<ViewNodeComponent>().ViewNode as FSViewNode;
			return b = new ToolbarButton(IconPool.GetTexture("Filesystem.Preview")) {
				Checked = up.ShowSelectionPreview,
				Clicked = () => {
					view.TogglePreview();
					up.ShowSelectionPreview = b.Checked = !b.Checked;
				}
			};
		}

		private Widget CreateSplitHButton()
		{
			ToolbarButton b = null;
			return b = new ToolbarButton(IconPool.GetTexture("Filesystem.SplitH")) {
				Clicked = () => {
					view.Split(SplitterType.Horizontal);
				}
			};
		}

		private Widget CreateSplitVButton()
		{
			ToolbarButton b = null;
			return b = new ToolbarButton(IconPool.GetTexture("Filesystem.SplitV")) {
				Clicked = () => {
					view.Split(SplitterType.Vertical);
				}
			};
		}

		private Widget CreateCloseButton()
		{
			return new ToolbarButton(IconPool.GetTexture("Filesystem.Close")) {
				Clicked = () => {
					view.Close();
				}
			};
		}
	}
}