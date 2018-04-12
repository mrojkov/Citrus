using System;
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
		readonly ThemedScrollView scrollView;
		private List<Node> results = new List<Node>();
		private int selectedIndex;
		private int rowHeight = Theme.Metrics.TextHeight;

		class Cmds
		{
			public static readonly Key Up = Key.MapShortcut(Key.Up);
			public static readonly Key Down = Key.MapShortcut(Key.Down);
			public static readonly Key Cancel = Key.MapShortcut(Key.Escape);
			public static readonly Key Enter = Key.MapShortcut(Key.Enter);
			public static readonly List<Key> All = new List<Key> { Up, Down, Cancel, Enter };
		}

		public SearchPanel(Widget rootWidget)
		{
			PanelWidget = rootWidget;
			scrollView = new ThemedScrollView();
			RootWidget = new Frame { Id = "SearchPanel",
				Padding = new Thickness(4),
				Layout = new VBoxLayout { Spacing = 4 },
				Nodes = { (searchStringEditor = new ThemedEditBox()), scrollView }
			};
			resultPane = scrollView.Content;
			RootWidget.AddChangeWatcher(() => searchStringEditor.Text, RefreshResultPane);
			scrollView.TabTravesable = new TabTraversable();
			resultPane.CompoundPresenter.Insert(0, new DelegatePresenter<Widget>(w => {
				w.PrepareRendererState();
				if (scrollView.IsFocused() && results.Count > 0) {
					Renderer.DrawRect(
						0, rowHeight * selectedIndex,
						w.Width, (selectedIndex + 1) * rowHeight, 
						Theme.Colors.SelectedBackground);
				}
			}));
			scrollView.LateTasks.Add(new KeyRepeatHandler(ScrollView_KeyRepeated));
		}

		void ScrollView_KeyRepeated(WidgetInput input, Key key)
		{
			if (Cmds.All.Contains(key)) {
				input.ConsumeKey(key);
			}
			if (results.Count == 0) {
				return;
			}
			if (key == Key.Mouse0) {
				scrollView.SetFocus();
				selectedIndex = (resultPane.LocalMousePosition().Y / rowHeight).Floor().Clamp(0, results.Count - 1);
			} else if (key == Cmds.Down) {
				selectedIndex++;
			} else if (key == Cmds.Up) {
				selectedIndex--;
			} else if (key == Cmds.Cancel) {
				scrollView.RevokeFocus();
			} else if (key == Cmds.Enter || key == Key.Mouse0DoubleClick) {
				NavigateToItem(selectedIndex);
			} else {
				return;
			}
			selectedIndex = selectedIndex.Clamp(0, results != null ? results.Count - 1 : 0);
			EnsureRowVisible(selectedIndex);
			Window.Current.Invalidate();
		}

		void NavigateToItem(int selectedIndex)
		{
			var node = results[selectedIndex];
			var ea = node.Ancestors.FirstOrDefault(i => i.ContentsPath != null);
			var curScenePath = Document.Current.Path;
			if (ea != null) {
				var searchString = searchStringEditor.Text;
				var localIndex = GetResults(ea, searchString).ToList().IndexOf(node);
				Document externalSceneDoc = null;
				try {
					externalSceneDoc = Project.Current.OpenDocument(ea.ContentsPath);
				} catch (System.Exception e) {
					AlertDialog.Show(e.Message);
					return;
				}
				externalSceneDoc.SceneNavigatedFrom = curScenePath; 
				node = GetResults(externalSceneDoc.RootNode, searchString).ToList()[localIndex];
			}
			Core.Operations.EnterNode.Perform(node.Parent, selectFirstNode: false);
			Core.Operations.SelectNode.Perform(node);
		}

		void EnsureRowVisible(int row)
		{
			while ((row + 1) * rowHeight > scrollView.ScrollPosition + scrollView.Height) {
				scrollView.ScrollPosition++;
			}
			while (row * rowHeight < scrollView.ScrollPosition) {
				scrollView.ScrollPosition--;
			}
		}

		void RefreshResultPane(string searchString)
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

		static IEnumerable<Node> GetResults(Node rootNode, string searchString)
		{
			var searchStringLowercase = searchString.Trim().ToLower();
			return rootNode.Descendants.Where(i => i.Id?.ToLower().Contains(searchStringLowercase) ?? false).
				OrderBy(i => i.Id.ToLower());
		}

		//static IEnumerable<Node> DescendantsWithoutExternalDuplicates(Node root)
		//{
		//	var stack = new Stack<Node>();
		//	var processed = new HashSet<string>();
		//	foreach (var n in root.Descendants) {
		//		if (n.ContentsPath != null) {
		//			stack.Push(n);
		//		} else {
		//			while (stack.Count > 0 && !n.DescendantOf(stack.Peek())) {
		//				processed.Add(stack.Pop().ContentsPath);
		//			}
		//		}
		//		if (stack.Count == 0 || !processed.Contains(stack.Peek().ContentsPath)) {
		//			yield return n;
		//		}
		//	}
		//}

		static string GetTypeName(Node node)
		{
			var r = node.GetType().ToString();
			if (r.StartsWith("Lime.")) {
				r = r.Substring(5);
			}
			return r;
		}

		static string GetContainerPath(Node node)
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
			RefreshResultPane(searchStringEditor.Text);
		}

		public void Detach()
		{
			Instance = null;
			RootWidget.Unlink();
		}
	}
}
