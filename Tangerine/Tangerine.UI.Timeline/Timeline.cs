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
		public readonly ComponentCollection<Component> Globals = new ComponentCollection<Component>();

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
			RootWidget.AddNode(new ThemedVSplitter {
				Stretches = Splitter.GetStretchesList(ref UserPreferences.Instance.TimelineVSplitterStretches, 0.5f, 1),
				Nodes = {
					Overview.RootWidget,
					new ThemedHSplitter {
						Stretches = Splitter.GetStretchesList(ref UserPreferences.Instance.TimelineHSplitterStretches, 0.3f, 1),
						Nodes = {
							new Widget {
								Layout = new VBoxLayout(),
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
			var gw = row.GridWidget();
			if (gw.Bottom() > Offset.Y + Grid.Size.Y) {
				OffsetY = gw.Bottom() - Grid.Size.Y;
			}
			if (gw.Top() < Offset.Y) {
				OffsetY = Math.Max(0, gw.Y);
			}
		}

		public bool IsColumnVisible(int col)
		{
			var pos = col * TimelineMetrics.ColWidth - Offset.X;
			return pos >= 0 && pos < Grid.Size.X;
		}

		public bool IsRowVisible(int row)
		{
			var pos = Document.Current.Rows[row].GridWidget().Top() - Offset.Y;
			return pos >= 0 && pos < Grid.Size.Y;
		}
	}

	public static class RowExtensions
	{
		public static Widget GridWidget(this Row row) => row.Components.Get<RowView>()?.GridRow.GridWidget;
	}
}