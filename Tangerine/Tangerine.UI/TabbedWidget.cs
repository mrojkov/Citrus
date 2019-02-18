using Lime;
using System.Collections.Generic;
using System.Linq;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class TabbedWidget : Widget
	{
		protected Frame ContentContainer { get; }
		protected List<Widget> Contents { get; } = new List<Widget>();

		private TabBar tabBar;
		public TabBar TabBar
		{
			get => tabBar;
			set
			{
				if (tabBar != value) {
					if (tabBar != null) {
						tabBar.OnReorder -= TabBar_OnReorder;
						tabBar.Unlink();
					}
					tabBar = value;
					tabBar.OnReorder += TabBar_OnReorder;
					PlaceTabBarAndContent(barPlacement);
				}
			}
		}

		private TabBarPlacement barPlacement;
		public TabBarPlacement BarPlacement
		{
			get => barPlacement;
			set
			{
				if (barPlacement != value) {
					barPlacement = value;
					PlaceTabBarAndContent(barPlacement);
				}
			}
		}

		private int activeTabIndex;
		public int ActiveTabIndex
		{
			get => activeTabIndex;
			set {
				activeTabIndex = value;
				ActivateTab(value);
			}
		}

		public bool AllowReordering
		{
			get => TabBar.AllowReordering;
			set => TabBar.AllowReordering = value;
		}

		public TabbedWidget()
		{
			ContentContainer = new Frame {
				ClipChildren = ClipMethod.ScissorTest,
			};
			Layout = new VBoxLayout();
			TabBar = new TabBar();
		}

		private void PlaceTabBarAndContent(TabBarPlacement tabBarPlacement)
		{
			TabBar.Unlink();
			ContentContainer.Unlink();
			if (tabBarPlacement == TabBarPlacement.Top) {
				Nodes.Add(TabBar);
				Nodes.Add(ContentContainer);
			} else {
				Nodes.Add(ContentContainer);
				Nodes.Add(TabBar);
			}
		}

		protected virtual void TabBar_OnReorder(TabBar.ReorderEventArgs args)
		{
			var item = Contents[args.IndexFrom];
			Contents.RemoveAt(args.IndexFrom);
			Contents.Insert(args.IndexTo, item);
			if (activeTabIndex == args.IndexFrom) {
				activeTabIndex = args.IndexTo;
			}
		}

		public void AddTab(Tab newTab, Widget content, bool isActive = false)
		{
			TabBar.Nodes.Add(newTab);
			Contents.Add(content);
			newTab.Clicked += () => ActivateTab(newTab);
			if (isActive) {
				ActivateTab(newTab);
			}
		}

		public void ActivateTab(Tab tab)
		{
			var idx = TabBar.Nodes.IndexOf(tab);
			ActivateTab(idx);
		}

		public override void Dispose()
		{
			base.Dispose();
			foreach (var content in Contents) {
				content.Dispose();
			}
		}

		public void ActivateTab(int index)
		{
			var tab = TabBar.Nodes[index] as Tab;
			TabBar.ActivateTab(tab);
			ContentContainer.Nodes.Clear();
			ContentContainer.Nodes.Add(Contents[index]);
			Contents[index].ExpandToContainerWithAnchors();
			activeTabIndex = index;
		}

		public void RemoveAt(int index)
		{
			TabBar.Nodes.RemoveAt(index);
			Contents.RemoveAt(index);
		}

		public int IndexOf(Tab tab)
		{
			if (TabBar.Nodes.Contains(tab))
				return TabBar.Nodes.IndexOf(tab);
			return -1;
		}

		public enum TabBarPlacement
		{
			Top,
			Bottom
		}
	}

	public class ThemedTabbedWidget : TabbedWidget
	{
		public ThemedTabbedWidget()
		{
			TabBar = new ThemedTabBar();
			ContentContainer.CompoundPresenter.Add(new SyncDelegatePresenter<Frame>(frame => {
				frame.PrepareRendererState();
				Renderer.DrawRect(Vector2.Zero, frame.Size, Theme.Colors.GrayBackground);
			}));
			ContentContainer.CompoundPostPresenter.Add(new SyncDelegatePresenter<Frame>(frame => {
				frame.PrepareRendererState();
				Renderer.DrawRectOutline(Vector2.Zero, frame.Size, Theme.Colors.ControlBorder);
			}));
		}

		public void AddTab(string tabName, Widget content, bool isActive = false, bool canClose = false)
		{
			AddTab(new ThemedTab {
				Text = tabName,
				Closable = canClose
			}, content, isActive);
		}
	}
}
