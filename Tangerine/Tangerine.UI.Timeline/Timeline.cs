using System;
using System.Linq;
using Lime;
using Tangerine.Core;
using System.Collections.Generic;

namespace Tangerine.UI.Timeline
{
	public class Timeline : ISelectedNodesProvider
	{
		public static Timeline Instance { get; private set; }
			
		private readonly Dictionary<Uid, Row> RowCache = new Dictionary<Uid, Row>();

		public readonly Toolbar Toolbar = new Toolbar();
		public readonly Rulerbar Ruler = new Rulerbar();
		public readonly OverviewPane Overview = new OverviewPane();
		public readonly GridPane Grid = new GridPane();
		public readonly RollPane Roll = new RollPane();
		public readonly KeyboardFocusController Focus;
		public readonly Widget RootWidget;

		public Vector2 ScrollOrigin;
		public Node Container { get; set; }
		public int CurrentColumn { get; set; }
		public int ColumnCount { get; set; }
		public GridSelection GridSelection = new GridSelection();
		public readonly List<Row> Rows = new List<Row>();
		public readonly List<Row> SelectedRows = new List<Row>();
		public readonly Entity Globals = new Entity();

		public static void Initialize(Widget rootWidget)
		{
			Instance = new Timeline(rootWidget);
		}

		private Timeline(Widget rootWidget)
		{
			RootWidget = rootWidget;
			RootWidget.Updating += delta => Document.Current.History.Commit();
			Focus = new KeyboardFocusController(RootWidget);
			CreateTasks();
			InitializeWidgets();
		}

		void InitializeWidgets()
		{
			RootWidget.Layout = new StackLayout();
			RootWidget.AddNode(new VSplitter {
				Nodes = {
					Overview.RootWidget,
					new HSplitter {
						Nodes = {
							new Widget {
								Layout = new VBoxLayout(),
								LayoutCell = new LayoutCell { StretchX = 0.33f },
								Nodes = {
									Toolbar.Widget,
									Roll.RootWidget,
								}
							},
							new Widget {
								Layout = new VBoxLayout(),
								Nodes = {
									Ruler.Widget,
									Grid.RootWidget,
								}
							},
						}
					}
				}
			});
		}

		void CreateTasks()
		{
			// Rule of thumb: the code which is interacts with a widget, should be placed into the widget's TaskList.
			// That help us to avoid bugs with HitTest and MouseCapture order.
			var tasks = RootWidget.Tasks;
			var gridTasks = Grid.RootWidget.Tasks;
			var rollTasks = Roll.RootWidget.Tasks;
			var overviewTasks = Overview.RootWidget.Tasks;
			tasks.Add(new BuildRowsTask().Main());
			tasks.Add(new RefreshColumnCountTask().Main());
			tasks.Add(new BuildRowViewsTask().Main());
			tasks.Add(new ProcessRollWidgetsTask().Main());
			tasks.Add(new ProcessGridWidgetsTask().Main());
			tasks.Add(new ProcessOverviewWidgetsTask().Main());
			tasks.Add(new KeyboardShortcutsTask().Main());
			overviewTasks.Add(new OverviewScrollTask().Main());
			tasks.Add(new MouseWheelTask().Main());
			gridTasks.Add(new ResizeGridCurveViewTask().Main());
			gridTasks.Add(new GridMouseScrollTask().Main());
			rollTasks.Add(new RollMouseScrollTask().Main());
			gridTasks.Add(new SelectAndDragKeyframesTask().Main());
			gridTasks.Add(new HasKeyframeRespondentTask().Main());
			gridTasks.Add(new DragKeyframesRespondentTask().Main());
			rollTasks.Add(new SelectAndDragRowsTask().Main());
			rollTasks.Add(new ClampScrollOriginTask().Main());
		}

		IEnumerable<Node> ISelectedNodesProvider.Get()
		{
			foreach (var i in SelectedRows) {
				var n = i.Components.Get<Components.NodeRow>()?.Node;
				if (n != null) {
					yield return n;
				}
			}
		}

		void ISelectedNodesProvider.Select(Node node, bool select)
		{
			var row = SelectedRows.FirstOrDefault(i => i.Components.Get<Components.NodeRow>()?.Node == node);
			if (select) {
				if (row == null) {
					row = Rows.First(i => i.Components.Get<Components.NodeRow>()?.Node == node);
					SelectedRows.Add(row);
				}
			} else {
				if (row != null) {
					SelectedRows.Remove(row);
				}
			}
		}

		public void RegisterDocument(Document document)
		{
			document.SelectedNodesProvider = this;
			Container = document.RootNode;
			SelectFirstRow();
		}

		void SelectFirstRow()
		{
			var r = GetCachedRow(Container.Nodes[0].EditorState().Uid);
			Document.Current.History.Execute(new Commands.SelectRow(r));
		}

		public Row GetCachedRow(Uid uid)
		{
			Row row;
			if (!RowCache.TryGetValue(uid, out row)) {
				row = new Row(uid);
				RowCache.Add(uid, row);
			}
			return row;
		}

		public void EnsureColumnVisible(int column)
		{
			if ((column + 1) * Metrics.TimelineColWidth - ScrollOrigin.X >= Grid.RootWidget.Width) {
				ScrollOrigin.X = (column + 1) * Metrics.TimelineColWidth - Grid.RootWidget.Width;
			}
			if (column * Metrics.TimelineColWidth < ScrollOrigin.X) {
				ScrollOrigin.X = Math.Max(0, column * Metrics.TimelineColWidth);
			}
		}

		public void EnsureRowVisible(Row row)
		{
			if (row.Bottom > ScrollOrigin.Y + Grid.Size.Y) {
				ScrollOrigin.Y = row.Bottom - Grid.Size.Y;
			}
			if (row.Top < ScrollOrigin.Y) {
				ScrollOrigin.Y = Math.Max(0, row.Top);
			}
		}

		public bool IsColumnVisible(int col)
		{
			var pos = col * Metrics.TimelineColWidth - ScrollOrigin.X;
			return pos >= 0 && pos < Grid.Size.X;
		}
		
		public bool IsRowVisible(int row)
		{
			var pos = Rows[row].Top - ScrollOrigin.Y;
			return pos >= 0 && pos < Grid.Size.Y;
		}
	}
}