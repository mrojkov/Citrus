using System;
using System.Linq;
using Lime;
using Tangerine.Core;
using System.Collections.Generic;
using Tangerine.UI.Timeline.Components;
using Tangerine.Core.Components;

namespace Tangerine.UI.Timeline
{
	public class Timeline : IDocumentView
	{
		public static Timeline Instance { get; private set; }
			
		public readonly Toolbar Toolbar;
		public readonly Rulerbar Ruler;
		public readonly OverviewPane Overview;
		public readonly GridPane Grid;
		public readonly RollPane Roll;
		public readonly Widget PanelWidget;
		public readonly DockPanel Panel;
		public readonly Widget RootWidget;

		private Vector2 offset;
		public Vector2 Offset
		{
			get { return offset; }
			set
			{
				if (value != Offset) {
					offset = value;
					OffsetChanged?.Invoke(value);
				}
			}
		}

		public float OffsetX { get { return Offset.X; } set { Offset = new Vector2(value, Offset.Y); } }
		public float OffsetY { get { return Offset.Y; } set { Offset = new Vector2(Offset.X, value); } }

		public int CurrentColumn => Document.Current.AnimationFrame;
		public int ColumnCount { get; set; }
		public readonly ComponentCollection<IComponent> Globals = new ComponentCollection<IComponent>();

		public event Action<Vector2> OffsetChanged;

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
			Toolbar = new Toolbar();
			Ruler = new Rulerbar();
			Overview = new OverviewPane();
			Grid = new GridPane();
			Roll = new RollPane();
			RootWidget = new Widget();
			OffsetChanged += v => Grid.SetOffset(v);
			CreateProcessors();
			InitializeWidgets();
		}

		public void Attach()
		{
			Instance = this;
			PanelWidget.PushNode(RootWidget);
			RootWidget.SetFocus();
			DockManager.Instance.FilesDropped += DropFiles;
		}

		public void Detach()
		{
			DockManager.Instance.FilesDropped -= DropFiles;
			Instance = null;
			RootWidget.Unlink();
		}

		void DropFiles(IEnumerable<string> files)
		{
			if (RootWidget.IsMouseOverThisOrDescendant()) {
				Grid.TryDropFiles(files);
			}
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
			RootWidget.LateTasks.Add(
				new OverviewScrollProcessor(),
				new MouseWheelProcessor(),
				new ResizeGridCurveViewProcessor(),
				new RollMouseScrollProcessor(),
				new SelectAndDragKeyframesProcessor(),
				new HasKeyframeRespondentProcessor(),
				new DragKeyframesRespondentProcessor(),
				new GridMouseScrollProcessor(),
				new SelectAndDragRowsProcessor(),
				new RulerbarMouseScrollProcessor(),
				new ClampScrollPosProcessor(),
				PanelTitleUpdater()
			);
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
			if ((column + 1) * TimelineMetrics.ColWidth - Offset.X >= Grid.RootWidget.Width) {
				OffsetX = (column + 1) * TimelineMetrics.ColWidth - Grid.RootWidget.Width;
			}
			if (column * TimelineMetrics.ColWidth < Offset.X) {
				OffsetX = Math.Max(0, column * TimelineMetrics.ColWidth);
			}
		}

		public void EnsureRowVisible(Row row)
		{
			var gw = row.GetGridWidget();
			if (gw.Bottom > Offset.Y + Grid.Size.Y) {
				OffsetY = gw.Bottom - Grid.Size.Y;
			}
			if (gw.Top < Offset.Y) {
				OffsetY = Math.Max(0, gw.Top);
			}
		}

		public bool IsColumnVisible(int col)
		{
			var pos = col * TimelineMetrics.ColWidth - Offset.X;
			return pos >= 0 && pos < Grid.Size.X;
		}
		
		public bool IsRowVisible(int row)
		{
			var pos = Document.Current.Rows[row].GetGridWidget().Top - Offset.Y;
			return pos >= 0 && pos < Grid.Size.Y;
		}

		public static void RegisterGlobalCommands()
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
			h.Connect(TimelineCommands.DeleteKeyframes, RemoveKeyframes, Document.HasCurrent);
			h.Connect(TimelineCommands.CreateMarkerPlay, () => CreateMarker(MarkerAction.Play), Document.HasCurrent);
			h.Connect(TimelineCommands.CreateMarkerStop, () => CreateMarker(MarkerAction.Stop), Document.HasCurrent);
			h.Connect(TimelineCommands.CreateMarkerJump, () => CreateMarker(MarkerAction.Jump), Document.HasCurrent);
			h.Connect(TimelineCommands.DeleteMarker, DeleteMarker, Document.HasCurrent);
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

		static void RemoveKeyframes()
		{
			foreach (var row in Document.Current.Rows) {
				var spans = row.Components.GetOrAdd<GridSpanList>();
				foreach (var span in spans.GetNonOverlappedSpans()) {
					var node = row.Components.Get<NodeRow>()?.Node ?? row.Components.Get<PropertyRow>()?.Node;
					if (node == null) {
						continue;
					}
					var property = row.Components.Get<PropertyRow>()?.Animator.TargetProperty;
					foreach (var a in node.Animators) {
						if (property != null && a.TargetProperty != property) {
							continue;
						}
						foreach (var k in a.Keys.Where(k => k.Frame >= span.A && k.Frame < span.B).ToList()) {
							Core.Operations.RemoveKeyframe.Perform(a, k.Frame);
						}
					}
				}
			}
		}

		static void AdvanceCurrentColumn(int stride)
		{
			Operations.SetCurrentColumn.Perform(Math.Max(0, Instance.CurrentColumn + stride));
		}

		static void CreateMarker(MarkerAction action)
		{
			var timeline = Instance;
			var newMarker = new Marker(
				action == MarkerAction.Play ? "Start" : "",
				timeline.CurrentColumn,
				action
			);
			Core.Operations.SetMarker.Perform(Document.Current.Container.DefaultAnimation.Markers, newMarker);
		}

		static void DeleteMarker()
		{
			var timeline = Instance;
			var marker = Document.Current.Container.Markers.FirstOrDefault(i => i.Frame == timeline.CurrentColumn);
			if (marker != null) {
				Core.Operations.DeleteMarker.Perform(Document.Current.Container.DefaultAnimation.Markers, marker);
			}
		}

	}

	public static class RowExtensions
	{
		public static Components.IGridWidget GetGridWidget(this Row row) => row.Components.Get<Components.IGridWidget>();
	}
}