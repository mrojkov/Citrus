using Lime;
using System.Collections.Generic;
using System.Linq;

namespace Tangerine.UI
{
	public class TabbedWidget : Widget
	{
		public enum TabBarPlacement
		{
			Top,
			Bottom
		}

		protected Frame ContentContainer { get; private set; }
		protected List<Widget> Contents { get; } = new List<Widget>();
		public TabBar TabBar { get; protected set; }

		private int activeTabIndex;
		public int ActiveTabIndex
		{
			get
			{
				return activeTabIndex;
			}
			set
			{
				activeTabIndex = value;
				ActivateTab(value);
			}
		}

		public bool AllowReordering
		{
			get { return TabBar.AllowReordering; }
			set { TabBar.AllowReordering = value; }
		}

		public TabbedWidget(TabBarPlacement tabBarPlacement = TabBarPlacement.Top)
		{
			ContentContainer = new ThemedFrame();
			ContentContainer.ClipChildren = ClipMethod.ScissorTest;
			TabBar = new ThemedTabBar();
			TabBar.OnReorder += TabBar_OnReorder;
			Layout = new VBoxLayout();
			if (tabBarPlacement == TabBarPlacement.Top) {
				Nodes.Add(TabBar);
				Nodes.Add(ContentContainer);
			} else {
				Nodes.Add(ContentContainer);
				Nodes.Add(TabBar);
			}
		}

		private void TabBar_OnReorder(TabBar.ReorderEventArgs args)
		{
			var tab = Contents[args.OldIndex];
			Contents.Remove(tab);
			Contents.Insert(args.NewIndex, tab);
		}

		public void AddTab(string name, Widget content, bool isActive = false, bool canClose = false)
		{
			var tab = new ThemedTab {
				Text = name,
				Closable = canClose
			};
			AddTab(tab, content, isActive);
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

		internal Tab GetById(string tabId)
		{
			return TabBar.Nodes.OfType<Tab>().FirstOrDefault(t => t.Text == tabId);
		}

		internal int IndexOf(Tab tab)
		{
			if (TabBar.Nodes.Contains(tab))
				return TabBar.Nodes.IndexOf(tab);
			return -1;
		}
	}
}
