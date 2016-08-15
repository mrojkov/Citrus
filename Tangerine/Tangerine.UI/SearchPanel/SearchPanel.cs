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

		public SearchPanel(Widget rootWidget)
		{
			PanelWidget = rootWidget;
			RootWidget = new Frame { Id = "SearchPanel",
				Padding = new Thickness(4),
				Layout = new VBoxLayout(),
				Nodes = {
					(searchStringEditor = new EditBox()),
					(resultPane = new Frame { ClipChildren = ClipMethod.ScissorTest })
				}
			};
			RootWidget.Tasks.Add(new Property<string>(() => searchStringEditor.Text).DistinctUntilChanged().Consume(RefreshResultPane));
			//RefreshResultPane("t");
		}

		void RefreshResultPane(string searchString)
		{
			if (searchString.IsNullOrWhiteSpace()) {
				resultPane.Nodes.Clear();
				return;
			}
			var searchStringLowercase = searchString.Trim().ToLower();
			var results = Document.Current.RootNode.Descendants.Where(i => i.Id != null && i.Id.ToLower().Contains(searchStringLowercase)).ToList();
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
				resultPane.Nodes.Add(new SimpleText(GetContainerPath(node)) {
					AutoSizeConstraints = false,
					OverflowMode = TextOverflowMode.Ignore // Use 'Ignore' because ellipsis by default is toooo slow.
				});
			}
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

	class DataGridView
	{
		public readonly Widget Widget;

		public class ColumnHeader
		{
			public string Text;
			public float Stretch;
		}

		public class Row
		{
			public List<string> Cells = new List<string>();
		}

		public List<ColumnHeader> ColumnHeaders = new List<ColumnHeader>();
		public List<Row> Rows = new List<Row>();

		public DataGridView()
		{
			
		}
	}
}
