using Lime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class TabbedWidget : Widget
	{
		protected Frame Frame { get; }
		protected TabBar TabBar { get; }

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

		public TabbedWidget()
		{
			Frame = new ThemedFrame {
				Padding = new Thickness(8),
				LayoutCell = new LayoutCell { StretchY = float.MaxValue },
				Layout = new StackLayout(),
			};
			TabBar = new ThemedTabBar();
			Layout = new VBoxLayout();
			Nodes.Add(TabBar);
			Nodes.Add(Frame);
			Nodes.Add(new Widget { MinHeight = 8 });
		}

		public void AddTab(string name, Widget content, bool IsActive = false)
		{
			var newTab = new ThemedTab {
				Text = name,
				Active = IsActive
			};
			TabBar.Nodes.Add(newTab);
			newTab.Clicked += () => ActivateTab(newTab);
			content.Visible = false;
			Frame.Nodes.Add(content);
			if (IsActive) {
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
			for (int i = 0; i < TabBar.Nodes.Count; i++) {
				Frame.Nodes[i].AsWidget.Visible = i == index;
			}
			TabBar.ActivateTab(tab);
		}

		public void RemoveAt(int index)
		{
			TabBar.Nodes.RemoveAt(index);
			Frame.Nodes.RemoveAt(index);
		}
	}
}
