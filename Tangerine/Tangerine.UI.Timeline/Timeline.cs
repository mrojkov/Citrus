using System;
using System.Linq;
using Lime;
using Tangerine.Core;
using System.Collections.Generic;

namespace Tangerine.UI.Timeline
{
	public class Timeline : IDocumentView
	{
		public static Timeline Instance { get; private set; }
			
		public readonly Toolbar Toolbar = new Toolbar();
		public readonly Rulerbar Ruler = new Rulerbar();
		public readonly OverviewPane Overview = new OverviewPane();
		public readonly GridPane Grid = new GridPane();
		public readonly RollPane Roll = new RollPane();
		public readonly Widget PanelWidget;
		public readonly DockPanel Panel;
		public readonly Widget RootWidget = new Widget();

		public Vector2 ScrollPos;
		public int CurrentColumn => Document.Current.AnimationFrame;
		public int ColumnCount { get; set; }
		public readonly Entity Globals = new Entity();

		public static IEnumerable<IOperationProcessor> GetOperationProcessors()
		{
			return new IOperationProcessor[] {
				new EnsureRowVisibleIfSelected(),
				new EnsureCurrentColumnVisibleIfContainerChanged(),
				new ColumnCountUpdater(),
				new RowViewsUpdater(),
				new RollWidgetsUpdater(),
				new OverviewWidgetsUpdater(),
				new GridWidgetsUpdater()
			};
		}

		public Timeline(DockPanel panel)
		{
			Panel = panel;
			PanelWidget = panel.ContentWidget;
			CreateProcessors();
			InitializeWidgets();
		}

		public void Attach()
		{
			Instance = this;
			PanelWidget.PushNode(RootWidget);
			RootWidget.SetFocus();
		}

		public void Detach()
		{
			Instance = null;
			RootWidget.Unlink();
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
									Toolbar.RootWidget,
									Roll.RootWidget,
								}
							},
							new Widget {
								Layout = new VBoxLayout(),
								Nodes = {
									Ruler.RootWidget,
									Grid.RootWidget,
								}
							},
						}
					}
				}
			});
		}

		void CreateProcessors()
		{
			RootWidget.LateTasks.Add(new ITaskProvider[] {
				new OverviewScrollProcessor(),
				new MouseWheelProcessor(),
				new ResizeGridCurveViewProcessor(),
				new RollMouseScrollProcessor(),
				new SelectAndDragKeyframesProcessor(),
				new HasKeyframeRespondentProcessor(),
				new DragKeyframesRespondentProcessor(),
				new GridMouseScrollProcessor(),
				new SelectAndDragRowsProcessor(),
				new RulerMouseScrollProcessor(),
				new ClampScrollPosProcessor(),
				new EditMarkerProcessor(),
				PanelTitleUpdater(),
			});
		}

		ITaskProvider PanelTitleUpdater()
		{
			return new Property<Node>(() => Document.Current.Container).WhenChanged(_ => {
				Panel.Title = "Timeline";
				var t = "";
				for (var n = Document.Current.Container; n != Document.Current.RootNode; n = n.Parent) {
					var id = string.IsNullOrEmpty(n.Id) ? "?" : n.Id;
					t = id + ((t != "") ? ": " + t : t);
				}
				if (t != "") {
					Panel.Title += " - '" + t + "'";
				}
			});
		}

		public void EnsureColumnVisible(int column)
		{
			if ((column + 1) * TimelineMetrics.ColWidth - ScrollPos.X >= Grid.RootWidget.Width) {
				ScrollPos.X = (column + 1) * TimelineMetrics.ColWidth - Grid.RootWidget.Width;
			}
			if (column * TimelineMetrics.ColWidth < ScrollPos.X) {
				ScrollPos.X = Math.Max(0, column * TimelineMetrics.ColWidth);
			}
		}

		public void EnsureRowVisible(Row row)
		{
			var gw = row.GetGridWidget();
			if (gw.Bottom > ScrollPos.Y + Grid.Size.Y) {
				ScrollPos.Y = gw.Bottom - Grid.Size.Y;
			}
			if (gw.Top < ScrollPos.Y) {
				ScrollPos.Y = Math.Max(0, gw.Top);
			}
		}

		public bool IsColumnVisible(int col)
		{
			var pos = col * TimelineMetrics.ColWidth - ScrollPos.X;
			return pos >= 0 && pos < Grid.Size.X;
		}
		
		public bool IsRowVisible(int row)
		{
			var pos = Document.Current.Rows[row].GetGridWidget().Top - ScrollPos.Y;
			return pos >= 0 && pos < Grid.Size.Y;
		}

		static Timeline()
		{
			var h = CommandHandlerList.Global;
			h.Connect(TimelineCommands.EnterNode, () => {
				var node = Document.Current.SelectedNodes().FirstOrDefault();
				if (node != null) {
					Core.Operations.EnterNode.Perform(node);
				}
			}, Document.HasCurrent);
			h.Connect(TimelineCommands.ExitNode, Core.Operations.LeaveNode.Perform, Document.HasCurrent);
			h.Connect(TimelineCommands.ScrollUp, () => SelectRow(-1, false), Document.HasCurrent);
			h.Connect(TimelineCommands.ScrollDown, () => SelectRow(1, false), Document.HasCurrent);
			h.Connect(TimelineCommands.SelectUp, () => SelectRow(-1, true), Document.HasCurrent);
			h.Connect(TimelineCommands.SelectDown, () => SelectRow(1, true), Document.HasCurrent);
			h.Connect(TimelineCommands.ScrollLeft, () => AdvanceCurrentColumn(-1), Document.HasCurrent);
			h.Connect(TimelineCommands.ScrollRight, () => AdvanceCurrentColumn(1), Document.HasCurrent);
			h.Connect(TimelineCommands.FastScrollLeft, () => AdvanceCurrentColumn(-10), Document.HasCurrent);
			h.Connect(TimelineCommands.FastScrollRight, () => AdvanceCurrentColumn(10), Document.HasCurrent);
		}

		static void SelectRow(int advance, bool multiselection)
		{
			var doc = Document.Current;
			if (doc.Rows.Count == 0) {
				return;
			}
			if (!doc.SelectedRows().Any()) {
				Core.Operations.SelectRow.Perform(doc.Rows[0]);
				return;
			}
			var lastSelectedRow = doc.SelectedRows().OrderByDescending(i => i.SelectedAtUpdate).First();
			var nextRow = doc.Rows[Mathf.Clamp(lastSelectedRow.Index + advance, 0, doc.Rows.Count - 1)];
			if (nextRow != lastSelectedRow) {
				if (!multiselection) {
					Core.Operations.ClearRowSelection.Perform();
				}
				if (nextRow.Selected) {
					Core.Operations.SelectRow.Perform(lastSelectedRow, false);
				}
				Core.Operations.SelectRow.Perform(nextRow);
			}
		}

		static void AdvanceCurrentColumn(int stride)
		{
			Operations.SetCurrentColumn.Perform(Math.Max(0, Instance.CurrentColumn + stride));
		}
	}

	public static class RowExtensions
	{
		public static Components.IGridWidget GetGridWidget(this Row row) => row.Components.Get<Components.IGridWidget>();
	}
}