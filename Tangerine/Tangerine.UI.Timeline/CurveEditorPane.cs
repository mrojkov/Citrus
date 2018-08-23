using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Components;

namespace Tangerine.UI.Timeline
{
	public class CurveEditorPane
	{
		static Dictionary<Type, IAnimatorAdapter> adapters = new Dictionary<Type, IAnimatorAdapter>();

		public readonly Widget RootWidget;
		public readonly Widget MainAreaWidget;
		public readonly Widget ContentWidget;

		readonly Widget toolbar;

		readonly Timeline timeline;
		PropertyRow property;

		public float MinValue { get; set; }
		public float MaxValue { get; set; }

		List<SimpleText> labels = new List<SimpleText>();

		public readonly List<Curve> Curves = new List<Curve>();

		static CurveEditorPane()
		{
			adapters[typeof(float)] = new NumericAnimatorAdapter();
			adapters[typeof(Vector2)] = new Vector2AnimatorAdapter();
		}

		public static bool CanEditRow(Row row)
		{
			var animator = row.Components.Get<PropertyRow>()?.Animator;
			return animator != null && adapters.ContainsKey(animator.GetValueType());
		}

		public CurveEditorPane(Timeline timeline)
		{
			this.timeline = timeline;
			timeline.OffsetChanged += value => ContentWidget.X = -value.X;
			MainAreaWidget = new Frame {
				Layout = new StackLayout { HorizontallySizeable = true, VerticallySizeable = true },
				ClipChildren = ClipMethod.ScissorTest,
				HitTestTarget = true,
			};
			ContentWidget = new Widget {
				Id = nameof(CurveEditorPane) + "Content",
				Padding = new Thickness { Top = 1, Bottom = 1 },
				Layout = new VBoxLayout { Spacing = TimelineMetrics.RowSpacing },
				Presenter = new DelegatePresenter<Node>(RenderBackgroundAndGrid),
			};
			MainAreaWidget.AddNode(ContentWidget);
			toolbar = new Widget {
				Padding = new Thickness(2, 0),
				MinMaxHeight = Metrics.ToolbarHeight,
				Layout = new HBoxLayout { Spacing = 4, DefaultCell = new LayoutCell { Alignment = Alignment.Center } },
				Presenter = new WidgetFlatFillPresenter(ColorTheme.Current.Toolbar.Background),
			};
			RootWidget = new Widget {
				Id = nameof(CurveEditorPane),
				Layout = new VBoxLayout(),
				Nodes = { MainAreaWidget, toolbar }
			};
			RootWidget.AddChangeWatcher(() => RootWidget.Size, 
				// Some document operation processors (e.g. ColumnCountUpdater) require up-to-date timeline dimensions.
				_ => Core.Operations.Dummy.Perform());
			RootWidget.Tasks.Add(
				new CurveEditorVerticalZoomProcessor(this),
				new CurveEditorSelectAndDragKeysProcessor(this),
				new CurveEditorPanProcessor(timeline)
			);
		}

		public void EditRow(Row row)
		{
			property = row?.Components.Get<PropertyRow>();
			var adapter = adapters[property.Animator.GetValueType()];
			float min, max;
			CalcRange(property.Animator, adapter, out min, out max);
			MinValue = min;
			MaxValue = max;
			toolbar.Nodes.Clear();
			for (int i = 0; i < adapter.ComponentCount; i++) {
				var color = ColorTheme.Current.TimelineCurveEditor.Curves[i];
				var curve = new Curve(property.Animator, i, adapter, color);
				Curves.Add(curve);
				toolbar.AddNode(new ColorBoxButton(color));
				var name = adapter.GetComponentName(i) ?? property.Animator.TargetPropertyPath;
				var label = new ThemedSimpleText(name) { MinWidth = 60 };
				int c = i;
				label.AddChangeWatcher(
					() => adapter.GetComponentValue(curve.Animator, Document.Current.Container.AnimationTime, c),
					v => label.Text = name + ": " + v.ToString("0.00"));
				toolbar.AddNode(label);
			}
		}

		private void RenderBackgroundAndGrid(Node node)
		{
			MainAreaWidget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, MainAreaWidget.Size, Theme.Colors.WhiteBackground);
			if (property == null) {
				return;
			}
			// Render scale.
			MainAreaWidget.PrepareRendererState();
			Renderer.DrawLine(0, 0.5f, MainAreaWidget.Width, 0.5f, ColorTheme.Current.TimelineGrid.Lines);
			double step = 100000;
			int j = 0;
			while (true) {
				var s = ValueToCoord(0) - ValueToCoord((float)step);
				if (s < 100) {
					break;
				}
				step /= (j++ % 2 == 0) ? 2 : 5;
			}
			var a = Math.Floor(MinValue / step) * step;
			var b = Math.Ceiling(MaxValue / step) * step;
			for (var v = a; v <= b; v += step) {
				var y = ValueToCoord((float)v);
				if (y >= 0 && y < MainAreaWidget.Height) {
					DrawHorizontalLine((float)v);
				}
			}
			ContentWidget.PrepareRendererState();
			int numCols = timeline.ColumnCount;
			// Render vertical lines.
			float x = 0.5f + TimelineMetrics.ColWidth / 2;
			for (int i = 0; i <= numCols; i++) {
				if (timeline.IsColumnVisible(i)) {
					Renderer.DrawLine(
						x, 1, x, MainAreaWidget.Height - 2,
						ColorTheme.Current.TimelineGrid.LinesLight);
				}
				x += TimelineMetrics.ColWidth;
			}
			// Render dark vertical lines below markers.
			foreach (var m in Document.Current.Container.Markers) {
				x = TimelineMetrics.ColWidth * m.Frame + 0.5f;
				if (timeline.IsColumnVisible(m.Frame)) {
					Renderer.DrawLine(
						x, 1, x, MainAreaWidget.Height - 2, ColorTheme.Current.TimelineGrid.Lines);
				}
			}
			var animator = property.Animator;
			ContentWidget.PrepareRendererState();
			foreach (var c in Curves) {
				RenderCurve(c);
			}
			foreach (var c in Curves) {
				RenderSelectedKeys(c);
			}
			RenderCursor();
			MainAreaWidget.PrepareRendererState();
			// Log10(step) is the approximate number of decimal places in step
			// We round it up to an integer number and get the number of needed decimal places in output
			int precision = Math.Max(-(int)Math.Ceiling(Math.Log10(step)), 0);
			for (var v = a; v <= b; v += step) {
				var y = ValueToCoord((float)v);
				if (y >= 0 && y < MainAreaWidget.Height) {
					DrawScaleMark(v, precision);
				}
			}
		}

		static void CalcRange(IAnimator animator, IAnimatorAdapter adapter, out float minValue, out float maxValue)
		{
			minValue = float.MaxValue;
			maxValue = -float.MaxValue;
			for (int i = 0; i < adapter.ComponentCount; i++) {
				foreach (var k in animator.ReadonlyKeys) {
					var value = adapter.GetComponentValue(animator, AnimationUtils.FramesToSeconds(k.Frame), i);
					if (value < minValue) minValue = value;
					if (value > maxValue) maxValue = value;
				}
			}
			var range = maxValue - minValue;
			if (range < 1) {
				range = 1;
			}
			maxValue += range * 0.2f;
			minValue -= range * 0.2f;
		}

		void DrawHorizontalLine(float value)
		{
			var y = ValueToCoord(value);
			Renderer.DrawLine(0, y, MainAreaWidget.Width, y, ColorTheme.Current.TimelineGrid.LinesLight);
		}

		void DrawScaleMark(double value, int precision)
		{
			var y = ValueToCoord((float)value);
			var text = value.ToString($"F{precision}");
			Renderer.DrawTextLine(2, y - 14, text, 14, Theme.Colors.BlackText, 0);
		}

		void RenderCurve(Curve curve)
		{
			var color = ColorTheme.Current.TimelineCurveEditor.Curves[curve.Component];
			IKeyframe key = null;
			foreach (var nextKey in curve.Animator.ReadonlyKeys) {
				Renderer.DrawRound(CalcPosition(curve, nextKey.Frame), 2.5f, 10, color);
				if (key != null) {
					if (key.Function == KeyFunction.Linear || key.Function == KeyFunction.Steep) {
						var p0 = CalcPosition(curve, key.Frame);
						var p1 = CalcPosition(curve, nextKey.Frame);
						if (key.Function == KeyFunction.Steep) {
							p1.Y = p0.Y;
						}
						Renderer.DrawLine(p0, p1, color);
					} else {
						// Render spline segment
						var p0 = CalcPosition(curve, key.Frame);
						for (int f = key.Frame + 1; f <= nextKey.Frame; f++) {
							var p1 = CalcPosition(curve, f);
							Renderer.DrawLine(p0, p1, color);
							p0 = p1;
						}
					}
				}
				key = nextKey;
			}
		}

		void RenderSelectedKeys(Curve curve)
		{
			foreach (var key in curve.SelectedKeys) {
				var p = CalcPosition(curve, key.Frame);
				Renderer.DrawRectOutline(p - new Vector2(4, 4), p + new Vector2(4, 4), ColorTheme.Current.TimelineCurveEditor.Selection);
			}
		}

		public float ValueToCoord(float value)
		{
			var t = (value - MinValue) / (MaxValue - MinValue);
			return (1 - t) * MainAreaWidget.Height;
		}

		public float CoordToValue(float coord)
		{
			var t = (MainAreaWidget.Height - coord) / MainAreaWidget.Height;
			return (1 - t) * MinValue + t * MaxValue;
		}

		public Vector2 CalcPosition(Curve curve, int frame)
		{
			return new Vector2(
				(frame + 0.5f) * TimelineMetrics.ColWidth,
				ValueToCoord(curve.Adapter.GetComponentValue(curve.Animator, AnimationUtils.FramesToSeconds(frame), curve.Component)));
		}

		private void RenderCursor()
		{
			var x = TimelineMetrics.ColWidth * (timeline.CurrentColumn + 0.5f);
			ContentWidget.PrepareRendererState();
			Renderer.DrawLine(
				x, 0, x, MainAreaWidget.Height - 1,
				Document.Current.PreviewAnimation ? 
				ColorTheme.Current.TimelineRuler.RunningCursor : 
				ColorTheme.Current.TimelineRuler.Cursor);
		}

		class ColorBoxButton : Button
		{
			public ColorBoxButton(Color4 color)
			{
				Nodes.Clear();
				Size = MinMaxSize = new Vector2(12, 12);
				PostPresenter = new DelegatePresenter<Widget>(widget => {
					widget.PrepareRendererState();
					Renderer.DrawRect(Vector2.Zero, widget.Size, Color4.White);
					var checkSize = new Vector2(widget.Width / 4, widget.Height / 3);
					for (int i = 0; i < 3; i++) {
						var checkPos = new Vector2(widget.Width / 2 + ((i == 1) ? widget.Width / 4 : 0), i * checkSize.Y);
						Renderer.DrawRect(checkPos, checkPos + checkSize, Color4.Black);
					}
					Renderer.DrawRect(Vector2.Zero, widget.Size, color);
					Renderer.DrawRectOutline(Vector2.Zero, widget.Size, ColorTheme.Current.Inspector.BorderAroundKeyframeColorbox);
				});
			}
		}
	}
}
