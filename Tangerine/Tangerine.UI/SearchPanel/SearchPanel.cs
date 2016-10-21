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
		readonly ScrollViewWidget scrollView;
		private List<Node> results = new List<Node>();
		private int selectedIndex;
		private int rowHeight = DesktopTheme.Metrics.TextHeight;

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
			scrollView = new ScrollViewWidget();
			RootWidget = new Frame { Id = "SearchPanel",
				Padding = new Thickness(4),
				Layout = new VBoxLayout { Spacing = 4 },
				Nodes = {
					(searchStringEditor = new EditBox()),
					scrollView
				}
			};
			resultPane = scrollView.Content;
			RootWidget.Tasks.Add(new Property<string>(() => searchStringEditor.Text).DistinctUntilChanged().Consume(RefreshResultPane));
			scrollView.TabTravesable = new TabTraversable();
			resultPane.CompoundPresenter.Insert(0, new DelegatePresenter<Widget>(w => {
				w.PrepareRendererState();
				if (scrollView.IsFocused() && results.Count > 0) {
					Renderer.DrawRect(0, rowHeight * selectedIndex, w.Width, (selectedIndex + 1) * rowHeight, DesktopTheme.Colors.SelectedBackground);
				}
			}));
			scrollView.Input.KeyRepeated += (input, key) => {
				if (Cmds.All.Contains(key)) {
					input.ConsumeKey(key);
				}
				if (results.Count == 0) {
					return;
				}
				if (key == Key.Mouse0) {
					scrollView.SetFocus();
					selectedIndex = (resultPane.Input.LocalMousePosition.Y / rowHeight).Floor().Clamp(0, results.Count - 1);
				} else if (key == Cmds.Down) {
					selectedIndex++;
				} else if (key == Cmds.Up) {
					selectedIndex--;
				} else if (key == Cmds.Cancel) {
					scrollView.RevokeFocus();
				} else if (key == Cmds.Enter || key == Key.Mouse0DoubleClick) {
					Core.Operations.ClearRowSelection.Perform();
					var node = results[selectedIndex];
					Core.Operations.EnterNode.Perform(node.Parent, selectFirstNode: false);
					Core.Operations.SelectNode.Perform(node);
				} else {
					return;
				}
				selectedIndex = selectedIndex.Clamp(0, results != null ? results.Count - 1 : 0);
				EnsureRowVisible(selectedIndex);
				Window.Current.Invalidate();
			};
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
			var searchStringLowercase = searchString.Trim().ToLower();
			results = Document.Current.RootNode.Descendants.Where(i => i.Id != null && i.Id.ToLower().
				Contains(searchStringLowercase)).OrderBy(i => i.Id.ToLower()).ToList();
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
			foreach (var node in results) {
				resultPane.Nodes.Add(new SimpleText(node.Id));
				resultPane.Nodes.Add(new SimpleText(GetTypeName(node)));
				resultPane.Nodes.Add(new SimpleText(GetContainerPath(node)));
			}
			selectedIndex = 0;
			scrollView.ScrollPosition = scrollView.MinScrollPosition;
		}

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
				if (p != node.Parent) {
					r += " : ";
				}
				r += string.IsNullOrEmpty(p.Id) ? p.GetType().Name : p.Id;
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
