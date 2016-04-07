using System;
using System.Linq;
using Lime;
using Tangerine.Core;
using System.Collections.Generic;

namespace Tangerine.UI.Timeline
{
	public class Timeline
	{
		private readonly Dictionary<Uid, Row> RowCache = new Dictionary<Uid, Row>();

		public readonly Window Window;
		public readonly Toolbar Toolbar = new Toolbar();
		public readonly Rulerbar Ruler = new Rulerbar();
		public readonly OverviewPane Overview = new OverviewPane();
		public readonly GridPane Grid = new GridPane();
		public readonly RollPane Roll = new RollPane();
		public static Timeline Instance { get; private set; }
		public readonly KeyboardFocusController Focus;
		public Widget RootWidget { get; private set; }

		public Vector2 ScrollOrigin;
		public Node Container { get; set; }
		public int CurrentColumn { get; set; }
		public int ColumnCount { get; set; }
		public GridSelection GridSelection = new GridSelection();
		public readonly List<Row> Rows = new List<Row>();
		public readonly List<Row> SelectedRows = new List<Row>();
		public readonly Entity Globals = new Entity();

		public Timeline()
		{
			Instance = this;
			Window = new Window(new WindowOptions { FixedSize = false, RefreshRate = 30, ClientSize = new Size(1300, 400) });
			RootWidget = CreateRootWidget();
			RootWidget.Updating += delta => Document.Current.Update(delta);
			Focus = new KeyboardFocusController(RootWidget);
		}

		public void RegisterDocument(Document document)
		{
			Container = document.RootNode;

			var tasks = document.Tasks;
			tasks.Add(new BuildRowsTask().Main());
			tasks.Add(new RefreshColumnCountTask().Main());
			tasks.Add(new BuildRowViewsTask().Main());
			tasks.Add(new ProcessRollWidgetsTask().Main());
			tasks.Add(new ProcessGridWidgetsTask().Main());
			tasks.Add(new ProcessOverviewWidgetsTask().Main());
			tasks.Add(new KeyboardShortcutsTask().Main());
			tasks.Add(new OverviewScrollTask().Main());
			tasks.Add(new MouseWheelTask().Main());
			// Grid specific tasks
			tasks.Add(new ResizeGridCurveViewTask().Main());
			tasks.Add(new GridMouseScrollTask().Main());
			tasks.Add(new RollMouseScrollTask().Main());
			tasks.Add(new SelectAndDragKeyframesTask().Main());
			tasks.Add(new HasKeyframeRespondentTask().Main());
			tasks.Add(new DragKeyframesRespondentTask().Main());
			// Roll specific tasks
			tasks.Add(new SelectAndDragRowsTask().Main());
			tasks.Add(new ClampScrollOriginTask().Main());
		}

		Widget CreateRootWidget()
		{
			return new DefaultWindowWidget(Window, continuousRendering: false) {
				CornerBlinkOnRendering = true,
				Layout = new StackLayout(),
				Nodes = {
					new VSplitter {
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
					}
				}
			};
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
			if ((column + 1) * Metrics.ColWidth - ScrollOrigin.X >= Grid.RootWidget.Width) {
				ScrollOrigin.X = (column + 1) * Metrics.ColWidth - Grid.RootWidget.Width;
			}
			if (column * Metrics.ColWidth < ScrollOrigin.X) {
				ScrollOrigin.X = Math.Max(0, column * Metrics.ColWidth);
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
			var pos = col * Metrics.ColWidth - ScrollOrigin.X;
			return pos >= 0 && pos < Grid.Size.X;
		}
		
		public bool IsRowVisible(int row)
		{
			var pos = Rows[row].Top - ScrollOrigin.Y;
			return pos >= 0 && pos < Grid.Size.Y;
		}
	}
}