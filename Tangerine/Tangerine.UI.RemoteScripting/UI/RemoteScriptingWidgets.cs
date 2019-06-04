using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.RemoteScripting
{
	internal class RemoteScriptingWidgets
	{
		public class TabbedWidget : ThemedTabbedWidget
		{
			private readonly List<TabbedWidgetPage> pages;

			public TabbedWidget(IEnumerable<TabbedWidgetPage> pages)
			{
				TabBar = new ThemedTabBar();
				var isFirstPage = true;
				this.pages = pages.ToList();
				foreach (var page in this.pages) {
					page.Initialize();
					AddTab(page.Tab, page.Content, isFirstPage);
					isFirstPage = false;
				}
			}

			public override void Dispose()
			{
				base.Dispose();
				foreach (var page in pages) {
					page.OnDispose();
				}
			}
		}

		public abstract class TabbedWidgetPage
		{
			public ThemedTab Tab { get; protected set; }
			public Widget Content { get; protected set; }

			public abstract void Initialize();
			public virtual void OnDispose() { }
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
				Layout = new HBoxLayout {
					Spacing = 2,
					DefaultCell = new DefaultLayoutCell(Alignment.Center)
				};
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
			private static readonly object scrollToEndTaskTag = new object();
			private readonly ICommand commandCopy = new Command("Copy");
			private readonly ICommand commandClear = new Command("Clear");

			public TextView(int maxRowsCount = 500, int removeRowsCount = 250)
			{
				TrimWhitespaces = false;
				var menu = new Menu {
					commandCopy,
					commandClear
				};
				Updated += dt => {
					if (Input.WasKeyPressed(Key.Mouse0) || Input.WasKeyPressed(Key.Mouse1)) {
						SetFocus();
						Window.Current.Activate();
					}
					if (Input.WasKeyPressed(Key.Mouse1)) {
						menu.Popup();
					}
					if (commandCopy.WasIssued()) {
						commandCopy.Consume();
						Clipboard.Text = Text;
					}
					if (commandClear.WasIssued()) {
						commandClear.Consume();
						Clear();
					}
					var i = Content.Nodes.Count;
					// numbers choosen by guess
					if (i >= maxRowsCount) {
						Content.Nodes.RemoveRange(0, removeRowsCount);
					}
				};
			}

			public void AppendLine(string text)
			{
				var isScrolledToEnd = Mathf.Abs(Behaviour.ScrollPosition - Behaviour.MaxScrollPosition) < Mathf.ZeroTolerance;
				Append($"{text}\n");
				if (isScrolledToEnd || Behaviour.Content.LateTasks.AnyTagged(scrollToEndTaskTag)) {
					Behaviour.Content.LateTasks.StopByTag(scrollToEndTaskTag);
					IEnumerator<object> ScrollToEnd()
					{
						yield return null;
						this.ScrollToEnd();
					}
					Behaviour.Content.LateTasks.Add(ScrollToEnd, scrollToEndTaskTag);
				}
			}
		}
	}
}
