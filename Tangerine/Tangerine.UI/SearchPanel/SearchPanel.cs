using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class SearchPanel : IDocumentView
	{
		public static SearchPanel Instance { get; private set; }

		public readonly Widget PanelWidget;
		public readonly Frame RootWidget;
		readonly EditBox searchStringEditor;
		readonly Widget resultPane;
		private readonly ThemedScrollView scrollView = new ThemedScrollView();
		private List<Node> results = new List<Node>();
		private int selectedIndex;
		private int rowHeight = Theme.Metrics.TextHeight;
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

		private static class Commands
		{
			public static readonly Key Up = Key.MapShortcut(Key.Up);
			public static readonly Key Down = Key.MapShortcut(Key.Down);
			public static readonly Key Cancel = Key.MapShortcut(Key.Escape);
			public static readonly Key Enter = Key.MapShortcut(Key.Enter);
			public static readonly List<Key> All = new List<Key> { Up, Down, Cancel, Enter };
		}

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
				Padding = new Thickness(4),
				Layout = new VBoxLayout { Spacing = 4 },
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
		private static void SelectNextTreeNode(TreeView view) => view.SelectNextTreeNode();
		private static void SelectPreviosTreeNode(TreeView view) => view.SelectPreviousTreeNode();
		private static void EnterSelectedTreeNode(TreeView view) => view.EnterSelectedTreeNode();
		private static void LeaveSelectedTreeNode(TreeView view) => view.LeaveSelectedTreeNode();
		private static void ToggleSelectedTreeNode(TreeView view) => view.ToggleSelectedTreeNode();
		private static void ClearSelection(TreeView view) => view.ClearSelection();

		private void NavigateToItem(int selectedIndex)
		{
			var node = results[selectedIndex];
			var ea = node.Ancestors.FirstOrDefault(i => i.ContentsPath != null);
			var curScenePath = Document.Current.Path;
			if (ea != null) {
				var searchString = searchStringEditor.Text;
				var localIndex = GetResults(ea, searchString).ToList().IndexOf(node);
				Document externalSceneDoc;
				try {
					externalSceneDoc = Project.Current.OpenDocument(ea.ContentsPath);
				} catch (System.Exception e) {
					AlertDialog.Show(e.Message);
					return;
				}
				externalSceneDoc.SceneNavigatedFrom = curScenePath;
				node = GetResults(externalSceneDoc.RootNode, searchString).ToList()[localIndex];
				Document.SetCurrent(externalSceneDoc);
				Document.Current.History.DoTransaction(() => {
					Core.Operations.EnterNode.Perform(node.Parent, selectFirstNode: false);
					Core.Operations.SelectNode.Perform(node);
				});
				return;
			}
			Core.Operations.EnterNode.Perform(node.Parent, selectFirstNode: false);
			Core.Operations.SelectNode.Perform(node);
		}

		private void EnsureRowVisible(int row)
		{
			while ((row + 1) * rowHeight > scrollView.ScrollPosition + scrollView.Height) {
				scrollView.ScrollPosition++;
			}
			while (row * rowHeight < scrollView.ScrollPosition) {
				scrollView.ScrollPosition--;
			}
		}

		private void RefreshResultPane(string searchString)
		{
			if (searchString.IsNullOrWhiteSpace()) {
				resultPane.Nodes.Clear();
				return;
			}
			results = GetResults(Document.Current.RootNode, searchString).ToList();
			resultPane.Nodes.Clear();
			resultPane.Layout = new TableLayout {
				ColCount = 3,
				ColSpacing = 8,
				RowCount = results.Count,
				ColDefaults = new List<LayoutCell>{
					new LayoutCell { Stretch = Vector2.Zero },
					new LayoutCell { Stretch = Vector2.Zero },
					new LayoutCell { StretchY = 0 }
				}
			};
			foreach (var r in results) {
				resultPane.Nodes.Add(new ThemedSimpleText(r.Id));
				resultPane.Nodes.Add(new ThemedSimpleText(GetTypeName(r)));
				resultPane.Nodes.Add(new ThemedSimpleText(GetContainerPath(r)));
			}
			selectedIndex = 0;
			scrollView.ScrollPosition = scrollView.MinScrollPosition;
		}

		private static IEnumerable<Node> GetResults(Node rootNode, string searchString)
		{
			var searchStringLowercase = searchString.Trim().ToLower();
			return rootNode.Descendants
				.Where(i => {
					bool textContains =
						i is IText txt && txt.Text.ToLower().Contains(searchStringLowercase);
					return (i.Id?.ToLower().Contains(searchStringLowercase) ?? false) || textContains;
				})
				.OrderBy(i => i.Id.ToLower());
		}

		private static string GetTypeName(Node node)
		{
			var r = node.GetType().ToString();
			if (r.StartsWith("Lime.")) {
				r = r.Substring(5);
			}
			return r;
		}

		private static string GetContainerPath(Node node)
		{
			string r = "";
			for (var p = node.Parent; p != null; p = p.Parent) {
				if (p == Document.Current.RootNode)
					break;
				if (p != node.Parent)
					r += " : ";
				r += string.IsNullOrEmpty(p.Id) ? p.GetType().Name : p.Id;
				if (p.ContentsPath != null) {
					r += " [" + p.ContentsPath + ']';
				}
			}
			return r;
		}

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
