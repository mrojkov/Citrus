using Lime;
using Tangerine.Core;

namespace Tangerine.UI.FilesystemView
{
	public enum SortType
	{
		Name,
		Size,
		Date,
		Extension
	}

	public enum OrderType
	{
		Ascending,
		Descending
	}

	public class Toolbar : Widget
	{
		public Toolbar()
		{
			Padding = new Thickness(4);
			MinMaxHeight = Metrics.ToolbarHeight;
			MinWidth = 50;// TimelineMetrics.ToolbarMinWidth;
			Presenter = new SyncDelegatePresenter<Widget>(Render);
			Layout = new HBoxLayout { Spacing = 2, DefaultCell = new DefaultLayoutCell(Alignment.Center) };
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
		public AddressBar AddressBar;

		public FilesystemToolbar(FilesystemView view, FilesystemModel filesystemModel)
		{
			this.view = view;
			Nodes.AddRange(
				new Widget {
					Presenter = new SyncDelegatePresenter<Widget>((w) => {
						w.PrepareRendererState();
						Renderer.DrawLine(0.0f, w.Size.Y, w.Size.X, w.Size.Y, Theme.Colors.SeparatorColor);
					}),
					Layout = new VBoxLayout(),
					Nodes = {
						new Widget {
							Layout = new HBoxLayout(){
								Spacing = 2
							},
							Nodes = {
								CreateGotoCurrentProjectDirectoryButton(),
								CreateUpButton(),
								CreateGoBackwardButton(),
								CreateGoForwardButton(),
								CreateToggleCookingRulesButton(),
								CreateTogglePreviewButton(),
								CreateSplitHButton(),
								CreateSplitVButton(),
								CreateSortByText(),
								CreateSortDropDownList(),
								CreateSortOrderDropDownList(),
								new Widget {
									LayoutCell = new LayoutCell { Stretch = new Vector2(9999, 9999)}
								},
								CreateCloseButton()
							}
						},
						(AddressBar = new AddressBar(view.Open, filesystemModel)),
					}
				}
			);
		}

		private Widget CreateGoBackwardButton()
		{
			return new ToolbarButton(IconPool.GetTexture("Filesystem.ArrowLeft")) {
				Clicked = () => {
					view.ScrollView.SetFocus();
					view.GoBackward();
				},
			};
		}

		private Widget CreateSortByText()
		{
			return new ThemedSimpleText {
				Text = "Sort By ",
				Padding = new Thickness {
					Top = 2,
					Left = 2,
					Right = 2
				}
			};
		}

		private Widget CreateSortDropDownList()
		{
			var list = new ThemedDropDownList {
				Items = {
					new ThemedDropDownList.Item("Name", SortType.Name),
					new ThemedDropDownList.Item("Extension", SortType.Extension),
					new ThemedDropDownList.Item("Size", SortType.Size),
					new ThemedDropDownList.Item("Date", SortType.Date)
				},
				Index = 0
			};
			list.Value = (view.RootWidget.Components.Get<ViewNodeComponent>().ViewNode as FSViewNode)?.SortType ?? SortType.Name;
			list.Changed += args => {
				view.SortByType((SortType)args.Value, view.OrderType);
				if (view.RootWidget.Components.Get<ViewNodeComponent>().ViewNode is FSViewNode fsViewNode) {
					fsViewNode.SortType = (SortType)args.Value;
				}
			};
			return list;
		}

		private Widget CreateSortOrderDropDownList()
		{
			var list = new ThemedDropDownList {
				Items = {
					new ThemedDropDownList.Item("Ascending", OrderType.Ascending),
					new ThemedDropDownList.Item("Descending", OrderType.Descending)
				},
				Layout = new HBoxLayout(),
				MinMaxWidth = FontPool.Instance.DefaultFont.MeasureTextLine("Descending", Theme.Metrics.TextHeight, 0).X + 30,
				Index = 0
			};
			list.Changed += args => {
				view.SortByType(view.SortType, (OrderType)args.Value);
			};

			return list;
		}

		private Widget CreateGoForwardButton()
		{
			return new ToolbarButton(IconPool.GetTexture("Filesystem.ArrowRight")) {
				Clicked = () => {
					view.ScrollView.SetFocus();
					view.GoForward();
				},
			};
		}

		private Widget CreateGotoCurrentProjectDirectoryButton()
		{
			return new ToolbarButton(IconPool.GetTexture("Filesystem.Home")) {
				Clicked = () => {
					if (Project.Current != Project.Null) {
						view.ScrollView.SetFocus();
						view.GoTo(Project.Current.AssetsDirectory ?? System.IO.Directory.GetDirectoryRoot(System.IO.Directory.GetCurrentDirectory()));
					}
				}
			};
		}

		private Widget CreateUpButton()
		{
			return new ToolbarButton(IconPool.GetTexture("Filesystem.ArrowUp")) {
				Clicked = () => {
					view.ScrollView.SetFocus();
					view.GoUp();
				}
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
