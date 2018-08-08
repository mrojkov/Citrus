using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class SearchPanel : IDocumentView
	{
		public static SearchPanel Instance { get; private set; }

		private readonly Widget PanelWidget;
		private readonly Frame RootWidget;
		private readonly EditBox searchStringEditor;
		private readonly ThemedScrollView scrollView = new ThemedScrollView();
		private TreeView view;

		private static readonly Dictionary<Key, Action<TreeView>> keyActionMap = new Dictionary<Key, Action<TreeView>>() {
			{ Key.MapShortcut(Key.Enter), NavigateToSelectedNode },
			{ Key.MapShortcut(Key.Up), SelectPreviosTreeNode },
			{ Key.MapShortcut(Key.Down), SelectNextTreeNode },
			{ Key.MapShortcut(Key.Left), LeaveSelectedTreeNode },
			{ Key.MapShortcut(Key.Right), EnterSelectedTreeNode },
			{ Key.MapShortcut(Key.Escape), ClearSelection },
			{ Key.MapShortcut(Key.Space), ToggleSelectedTreeNode },
		};

		public static SearchPanel Instance { get; private set; }

		public readonly Widget PanelWidget;
		public readonly Frame RootWidget;
		private readonly EditBox searchStringEditor;
		private readonly Widget resultPane;
		private readonly ThemedScrollView scrollView;
		private List<Node> results = new List<Node>();
		private int selectedIndex;
		private readonly int rowHeight = Theme.Metrics.TextHeight;

		public SearchPanel(Widget rootWidget)
		{
			PanelWidget = rootWidget;
			RootWidget = new Frame {
				Id = "SearchPanel",
				Padding = new Thickness(5),
				Layout = new VBoxLayout { Spacing = 5 },
				Nodes = {
					(searchStringEditor = new ThemedEditBox())
				}
			};
			rootWidget.TabTravesable = new TabTraversable();
			var treeView = new TreeView(RootWidget, Document.Current.RootNode);
			var searchTreeView = new TreeView(RootWidget, Document.Current.RootNode);
			searchStringEditor.AddChangeWatcher(() => searchStringEditor.Text, t => {
				if (!String.IsNullOrEmpty(t)) {
					if (treeView.IsAttached()) {
						treeView.Detach();
						searchTreeView.Attach();
						view = searchTreeView;
					}
					searchTreeView.Filter(t);
				} else {
					if (searchTreeView.IsAttached()) {
						searchTreeView.Detach();
						treeView.Attach();
						view = treeView;
					}
				}
			});
			treeView.Attach();
			view = treeView;
			RootWidget.LateTasks.Add(new KeyRepeatHandler((input, key) => {
				if (keyActionMap.ContainsKey(key)) {
					input.ConsumeKey(key);
					keyActionMap[key](view);
					view.EnsureSelectionVisible();
					Window.Current.Invalidate();
				}
			}));
		}

		private static void NavigateToSelectedNode(TreeView view) => view.NavigateToSelectedNode();
		private static void SelectNextTreeNode(TreeView view)
		{
			if (!view.HasSelection()) {
				view.SelectFirstMatch();
				return;
			}
			view.SelectNextTreeNode();
		}
		private static void SelectPreviosTreeNode(TreeView view) => view.SelectPreviousTreeNode();
		private static void EnterSelectedTreeNode(TreeView view) => view.EnterSelectedTreeNode();
		private static void LeaveSelectedTreeNode(TreeView view) => view.LeaveSelectedTreeNode();
		private static void ToggleSelectedTreeNode(TreeView view) => view.ToggleSelectedTreeNode();
		private static void ClearSelection(TreeView view) => view.ClearSelection();

		public void Attach()
		{
			Instance = this;
			PanelWidget.PushNode(RootWidget);
		}

		public void Detach()
		{
			Instance = null;
			RootWidget.Unlink();
		}
	}
}
