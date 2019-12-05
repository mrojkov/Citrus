using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using Tangerine.Common.FilesDropHandlers;
using Tangerine.Core;
using Tangerine.Core.Components;
using Tangerine.UI.Docking;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline
{
	public class Timeline : IDocumentView
	{
		public static Timeline Instance { get; private set; }
		public static Action<Timeline> OnCreate;

		public readonly Toolbar Toolbar;
		public readonly Rulerbar Ruler;
		public readonly OverviewPane Overview;
		public readonly GridPane Grid;
		public readonly CurveEditorPane CurveEditor;
		public readonly RollPane Roll;
		public readonly Widget PanelWidget;
		public readonly Panel Panel;
		public readonly Widget RootWidget;
		public readonly WaveformCache WaveformCache;
		public readonly DropFilesGesture DropFilesGesture;

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

		public float CurrentColumnEased
		{
			get {
				if (Document.Current.PreviewScene) {
					var time = Document.Current.Animation.Time;
					time = Document.Current.Animation.BezierEasingCalculator.EaseTime(time);
					return (float)(time * AnimationUtils.FramesPerSecond);
				} else {
					return Document.Current.AnimationFrame;
				}
			}
		}

		public int ColumnCount { get; set; }
		public readonly ComponentCollection<Component> Globals = new ComponentCollection<Component>();

		public event Action<Vector2> OffsetChanged;
		/// <summary>
		/// Called before Attach code execution.
		/// </summary>
		public event Action Attaching;
		/// <summary>
		/// Called before Detach code execution.
		/// </summary>
		public event Action Detaching;
		/// <summary>
		/// Called after Attach code execution.
		/// </summary>
		public event Action Attached;
		/// <summary>
		/// Called after Detach code execution
		/// </summary>
		public event Action Detached;

		public static IEnumerable<Type> GetOperationProcessorTypes() => new[] {
			typeof(EnsureRowVisibleIfSelected),
			typeof(EnsureCurrentColumnVisibleIfContainerChanged),
			typeof(ColumnCountUpdater),
			typeof(RowViewsUpdater),
			typeof(RollWidgetsUpdater),
			typeof(OverviewWidgetsUpdater),
			typeof(GridWidgetsUpdater),
			typeof(ImageCombinerLinkIndicationProcessor),
			typeof(BoneLinkIndicationProcessor),
			typeof(SplineGearLinkIndicationProcessor),
		};

		public Timeline(Panel panel)
		{
			RootWidget = new Widget();
			Panel = panel;
			PanelWidget = panel.ContentWidget;
			Toolbar = new Toolbar();
			Ruler = new Rulerbar();
			Overview = new OverviewPane();
			Grid = new GridPane(this);
			CurveEditor = new CurveEditorPane(this);
			Roll = new RollPane();
			CreateProcessors();
			InitializeWidgets();
			WaveformCache = new WaveformCache(Project.Current.FileSystemWatcher);
			RootWidget.AddChangeWatcher(() => Document.Current.Container, container => {
				Offset = container.Components.GetOrAdd<TimelineOffset>().Offset;
			});
			RootWidget.AddChangeWatcher(() => Offset, (value) => {
				var offset = Document.Current.Container.Components.Get<TimelineOffset>();
				if (offset != null) {
					offset.Offset = value;
				}
			});
			RootWidget.Gestures.Add(DropFilesGesture = new DropFilesGesture());
			CreateFilesDropHandlers();
			OnCreate?.Invoke(this);
		}

		private void CreateFilesDropHandlers()
		{
			DropFilesGesture.Recognized += new ImagesDropHandler().Handle;
			DropFilesGesture.Recognized += new AudiosDropHandler().Handle;
			DropFilesGesture.Recognized += new ScenesDropHandler().Handle;
		}


		public void Attach()
		{
			Attaching?.Invoke();
			Instance = this;
			PanelWidget.PushNode(RootWidget);
			RootWidget.SetFocus();
			UpdateTitle();
			Attached?.Invoke();
		}

		public void Detach()
		{
			Detaching?.Invoke();
			Instance = null;
			RootWidget.Unlink();
			Detached?.Invoke();
		}

		private static void FilesDropOnHandling()
		{
			if (!Window.Current.Active) {
				Window.Current.Activate();
			}
		}

		void InitializeWidgets()
		{
			RootWidget.Layout = new StackLayout();
			RootWidget.AddNode(new ThemedVSplitter {
				Stretches = Splitter.GetStretchesList(TimelineUserPreferences.Instance.TimelineVSplitterStretches, 0.5f, 1),
				Nodes = {
					Overview.RootWidget,
					new ThemedHSplitter {
						Stretches = Splitter.GetStretchesList(TimelineUserPreferences.Instance.TimelineHSplitterStretches, 0.3f, 1),
						Nodes = {
							new Widget {
								Layout = new VBoxLayout(),
								Nodes = {
									Toolbar.RootWidget,
									Roll.RootWidget,
								}
							},
							new Widget {
								Layout = new HBoxLayout(),
								Nodes = {
									new Widget { MinMaxWidth = 0 },
									new Frame {
										ClipChildren = ClipMethod.ScissorTest,
										Layout = new VBoxLayout(),
										Nodes = {
											Ruler.RootWidget,
											Grid.RootWidget,
											CurveEditor.RootWidget,
										}
									},
								}
							}
						}
					}
				}
			});
		}

		void CreateProcessors()
		{
			RootWidget.LateTasks.Add(
				new SlowMotionProcessor(),
				new AnimationStretchProcessor(),
				new OverviewScrollProcessor(),
				new MouseWheelProcessor(this),
				new RollMouseScrollProcessor(),
				new SelectAndDragKeyframesProcessor(),
				new CompoundAnimations.CreateAnimationTrackWeightRampProcessor(),
				new CompoundAnimations.SelectAndDragAnimationClipsProcessor(0),
				new CompoundAnimations.SelectAndDragAnimationClipsProcessor(1),
				new HasKeyframeRespondentProcessor(),
				new DragKeyframesRespondentProcessor(),
				new GridMouseScrollProcessor(),
				new SelectAndDragRowsProcessor(),
				new RulerbarMouseScrollProcessor(),
				new ClampScrollPosProcessor(),
				new GridContextMenuProcessor(),
				new CompoundAnimations.GridContextMenuProcessor()
			);
			RootWidget.Components.GetOrAdd<LateConsumeBehaviour>().Add(ShowCurveEditorTask());
			RootWidget.Components.GetOrAdd<LateConsumeBehaviour>().Add(PanelTitleUpdater());
		}

		void UpdateTitle()
		{
			Panel.Title = "Timeline";
			var t = "";
			for (var n = Document.Current.Container; n != Document.Current.RootNode; n = n.Parent) {
				var id = string.IsNullOrEmpty(n.Id) ? "?" : n.Id;
				t = id + ((t != "") ? ": " + t : t);
			}
			if (t != "") {
				Panel.Title += " - '" + t + "'";
			}
		}

		IConsumer PanelTitleUpdater() =>
			new Property<Node>(() => Document.Current.Container).WhenChanged(_ => UpdateTitle());

		IConsumer ShowCurveEditorTask()
		{
			var editCurvesProp = new Property<bool>(() => TimelineUserPreferences.Instance.EditCurves);
			return new Property<Row>(FirstSelectedRow).Coalesce(editCurvesProp).WhenChanged(t => {
				var row = t.Item1;
				var showCurves =
					TimelineUserPreferences.Instance.EditCurves &&
					row != null && CurveEditorPane.CanEditRow(row);
				CurveEditor.RootWidget.Visible = showCurves;
				Grid.RootWidget.Visible = !showCurves;
				if (showCurves) {
					CurveEditor.EditRow(row);
				}
			});
		}

		Row FirstSelectedRow() => Document.Current.SelectedRows().FirstOrDefault();

		public void EnsureColumnVisible(int column)
		{
			if ((column + 1) * TimelineMetrics.ColWidth - Offset.X >= Ruler.RootWidget.Width) {
				OffsetX = (column + 1) * TimelineMetrics.ColWidth - Ruler.RootWidget.Width;
			}
			if (column * TimelineMetrics.ColWidth < Offset.X) {
				OffsetX = Math.Max(0, column * TimelineMetrics.ColWidth);
			}
		}

		public void EnsureRowVisible(Row row)
		{
			var gw = row.RollWidget();
			if (gw == null) {
				return;
			}
			if (gw.Bottom() > Offset.Y + Roll.RootWidget.Height) {
				OffsetY = gw.Bottom() - Roll.RootWidget.Height;
			}
			if (gw.Top() < Offset.Y) {
				OffsetY = Math.Max(0, gw.Y);
			}
		}

		public void EnsureRowChildsVisible(Row row)
		{
			var first = row.RollWidget();
			var lastRow = row.Rows.Last();
			while (lastRow.Rows.Count > 0) {
				lastRow = lastRow.Rows.Last();
			}
			var last = lastRow.RollWidget();
			float bottom = last.Bottom();
			float top = first.Top();
			float d = bottom - top;
			float height = Roll.RootWidget.Height;
			if (d > height) {
				OffsetY = top;
			} else if (bottom > OffsetY + height) {
				OffsetY += bottom - OffsetY - height;
			} else if (top < OffsetY) {
				OffsetY = top;
			}
		}

		public void GetVisibleColumnRange(out int min, out int max)
		{
			min = Math.Max(0, (Offset.X / TimelineMetrics.ColWidth).Round() - 1);
			max = Math.Min(ColumnCount - 1, ((Offset.X + Ruler.RootWidget.Width) / TimelineMetrics.ColWidth).Round() + 1);
		}

		public bool IsColumnVisible(int col)
		{
			var pos = col * TimelineMetrics.ColWidth - Offset.X;
			return pos >= 0 && pos < Ruler.RootWidget.Width;
		}

		public bool IsRowVisible(int row)
		{
			var pos = Document.Current.Rows[row].RollWidget().Top() - Offset.Y;
			return pos >= 0 && pos < Roll.RootWidget.Height;
		}
	}

	public static class RowExtensions
	{
		public static Widget GridWidget(this Row row) => row.Components.Get<RowView>()?.GridRow.GridWidget;
		public static Widget RollWidget(this Row row) => row.Components.Get<RowView>()?.RollRow.Widget;
	}
}
