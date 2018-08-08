using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class HierarchyPanel : IDocumentView
	{
		public static HierarchyPanel Instance { get; private set; }

		private readonly Widget panelWidget;
		private readonly Frame rootWidget;
		private readonly EditBox searchStringEditor;
		private readonly ThemedScrollView scrollView = new ThemedScrollView();
		private DocumentHierarchyTreeView view;

		private static readonly Dictionary<Key, Action<DocumentHierarchyTreeView>> keyActionMap = new Dictionary<Key, Action<DocumentHierarchyTreeView>>() {
			{ Key.MapShortcut(Key.Enter), NavigateToSelectedNode },
			{ Key.MapShortcut(Key.Up), SelectPreviosTreeNode },
			{ Key.MapShortcut(Key.Down), SelectNextTreeNode },
			{ Key.MapShortcut(Key.Left), LeaveSelectedTreeNode },
			{ Key.MapShortcut(Key.Right), EnterSelectedTreeNode },
			{ Key.MapShortcut(Key.Escape), ClearSelection },
			{ Key.MapShortcut(Key.Space), ToggleSelectedTreeNode },
		};

		public HierarchyPanel(Widget rootWidget)
		{
			panelWidget = rootWidget;
			this.rootWidget = new Frame {
				Id = "SearchPanel",
				Padding = new Thickness(5),
				Layout = new VBoxLayout { Spacing = 5 },
				Nodes = {
					(searchStringEditor = new ThemedEditBox())
				}
			};
			rootWidget.TabTravesable = new TabTraversable();
			var treeView = new DocumentHierarchyTreeView(this.rootWidget, Document.Current.RootNode);
			var searchTreeView = new DocumentHierarchyTreeView(this.rootWidget, Document.Current.RootNode);
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
			this.rootWidget.LateTasks.Add(new KeyRepeatHandler((input, key) => {
				if (keyActionMap.ContainsKey(key)) {
					input.ConsumeKey(key);
					keyActionMap[key](view);
					view.EnsureSelectionVisible();
					Window.Current.Invalidate();
				}
			}));
		}

		private static void NavigateToSelectedNode(DocumentHierarchyTreeView view) => view.NavigateToSelectedNode();
		private static void SelectNextTreeNode(DocumentHierarchyTreeView view)
		{
			if (!view.HasSelection()) {
				view.SelectFirstMatch();
				return;
			}
			view.SelectNextTreeNode();
		}
		private static void SelectPreviosTreeNode(DocumentHierarchyTreeView view) => view.SelectPreviousTreeNode();
		private static void EnterSelectedTreeNode(DocumentHierarchyTreeView view) => view.EnterSelectedTreeNode();
		private static void LeaveSelectedTreeNode(DocumentHierarchyTreeView view) => view.LeaveSelectedTreeNode();
		private static void ToggleSelectedTreeNode(DocumentHierarchyTreeView view) => view.ToggleSelectedTreeNode();
		private static void ClearSelection(DocumentHierarchyTreeView view) => view.ClearSelection();

		public void Attach()
		{
			Instance = this;
			panelWidget.PushNode(rootWidget);
		}

		public void Detach()
		{
			Instance = null;
			rootWidget.Unlink();
		}
	}
}
