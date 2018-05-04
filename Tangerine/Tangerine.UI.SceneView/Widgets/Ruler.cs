using System.Collections.Generic;
using System.Linq;
using Tangerine.Core;
using Lime;
using System;

namespace Tangerine.UI.SceneView
{

	public class Rulers
	{
		private static SceneView sv => SceneView.Instance;
		private static Widget cntr => Document.Current.RootNode.AsWidget;
		private const int RulerSize = 20;
		private RulerSet Ruler => SceneUserPreferences.Instance.Ruler;
		private Widget topRuler;
		private Widget leftRuler;

		public Rulers()
		{
			topRuler = new Widget {
				Anchors = Anchors.LeftRight,
				Id = "TopRuler",
				HitTestTarget = true,
			};

			leftRuler = new Widget {
				Anchors = Anchors.Left | Anchors.Bottom | Anchors.Top,
				HitTestTarget = true,
				Id = "LeftRuler",
			};

			topRuler.Tasks.Add(CreateLineTask(topRuler, Orientation.Vertical));
			leftRuler.Tasks.Add(CreateLineTask(leftRuler));
		}

		private IEnumerator<object> CreateLineTask(Widget widget, Orientation lineOrientation = Orientation.Horizontal)
		{
			while (true) {
				var rect = new Rectangle(Vector2.Zero, widget.Size);
				if ((rect.Contains(widget.LocalMousePosition()))) {
					if (widget.Input.WasMousePressed()) {
						var line = new RulerLine(cntr.LocalMousePosition(), lineOrientation);
						Ruler.Lines.Add(line);
						yield return null;
						SceneView.Instance.Components.Add(new LineSelectionComponent { Line = line });
						while (widget.Input.IsMousePressed()) {
							line.SetVector(cntr.LocalMousePosition());
							Window.Current.Invalidate();
							yield return null;
						}
						SceneView.Instance.Components.Remove<LineSelectionComponent>();
					}
					Window.Current.Invalidate();
				}
				yield return null;
			}
		}

		private RulerLine GetLineUnderMouse(Widget widget, Orientation orientation)
		{
			var mPos = cntr.LocalMousePosition();
			var rect = new Rectangle(Vector2.Zero, widget.Size);
			var dir = orientation == Orientation.Vertical ? Vector2.Right : Vector2.Down;
			if (!rect.Contains(widget.LocalMousePosition()))
				return null;
			return Ruler.Lines.FirstOrDefault(line => line.Orientation == orientation &&
				(line.Vector() * dir - mPos * dir).Length <= 6f);
		}

		public void AppendToNode(Widget n)
		{
			topRuler.Size = new Vector2(n.Size.X, RulerSize);
			leftRuler.Size = new Vector2(RulerSize, n.Size.Y);
			n.Nodes.Insert(0, topRuler);
			n.Nodes.Insert(0, leftRuler);
			n.Nodes.Add(new Widget());
			n.CompoundPostPresenter.Push(new RulerWidgetPresenter(topRuler, Orientation.Horizontal));
			n.CompoundPostPresenter.Push(new RulerWidgetPresenter(leftRuler, Orientation.Vertical));
			n.CompoundPostPresenter.Push(new RulerPresenter(leftRuler, topRuler));
			n.AddChangeWatcher(() => n.LocalMousePosition(), lp => Window.Current.Invalidate());
			n.AddChangeWatcher(() => SceneUserPreferences.Instance.RulerVisible,
				v => {
					topRuler.Visible = v;
					leftRuler.Visible = v;
				});
		}

		private class RulerPresenter : CustomPresenter<Widget>
		{
			private readonly Widget leftRuler;
			private readonly Widget topRuler;
			private static Widget cntr => Document.Current.RootNode.AsWidget;
			private RulerSet Ruler => SceneUserPreferences.Instance.Ruler;

			public RulerPresenter(Widget leftRuler, Widget topRuler)
			{
				this.leftRuler = leftRuler;
				this.topRuler = topRuler;
			}

			protected override void InternalRender(Widget widget)
			{
				if (Document.Current.PreviewAnimation || !SceneUserPreferences.Instance.RulerVisible)
					return;

				widget.PrepareRendererState();
				var t = cntr.CalcTransitionToSpaceOf(widget);
				foreach (var line in Ruler.Lines.ToList()) {
					var val = (line.Vector()) * t;
					if (line.Orientation == Orientation.Horizontal) {
						if (val.Y >= RulerSize) {
							Renderer.DrawLine(new Vector2(RulerSize, val.Y), new Vector2(widget.Size.X, val.Y), ColorTheme.Current.SceneView.RulerEditable);
						}
					} else {
						if (val.X >= RulerSize) {
							Renderer.DrawLine(new Vector2(val.X, RulerSize), new Vector2(val.X, widget.Size.Y), ColorTheme.Current.SceneView.RulerEditable);
						}
					}
				}

				var mPos = widget.LocalMousePosition();
				if (mPos.X > RulerSize) {
					Renderer.DrawLine(new Vector2(mPos.X, 0), new Vector2(mPos.X, topRuler.Size.Y), Color4.Green);
				}
				if (mPos.Y > RulerSize) {
					Renderer.DrawLine(new Vector2(0, mPos.Y), new Vector2(leftRuler.Size.X, mPos.Y), Color4.Green);
				}

				var lineComponent = SceneView.Instance.Components.Get<LineSelectionComponent>();
				if (lineComponent != null) {
					var fontsize = 15;
					var letterspacing = 2f;
					var padding = Vector2.One * 5f;
					var isVertical = lineComponent.Line.Orientation == Orientation.Vertical;
					var value = lineComponent.Line.Vector();
					var text = $"{(isVertical ? "X:" : "Y:")} {(int)(isVertical ? value.X : value.Y)}";
					var size = Renderer.MeasureTextLine(text, fontsize, letterspacing);
					size += 2 * padding;
					var mpos = SceneView.Instance.Frame.LocalMousePosition() + Vector2.One * 10f;
					Renderer.DrawRect(mpos, mpos + size, ColorTheme.Current.SceneView.RulerBackground);
					Renderer.DrawRectOutline(mpos, mpos + size, ColorTheme.Current.SceneView.RulerTextColor);
					Renderer.DrawTextLine(mpos + new Vector2(padding.X, (size.Y - fontsize) / 2), text, fontsize, ColorTheme.Current.SceneView.RulerTextColor, letterspacing);
				}
			}
		}

		private class RulerWidgetPresenter : CustomPresenter<Widget>
		{
			public Orientation Orientation { get; set; }
			public IPresenter RulerPresenter { get; private set; }
			public Widget Widget { get; private set; }

			private struct RulerData
			{
				public Vector2 origin;
				public float leftStoper;
				public float rightStoper;
				public float offset;
				public Orientation orientation;

				public RulerData(Vector2 origin, float leftStoper, float rightStoper, float offset, Orientation orientation)
				{
					this.origin = origin;
					this.leftStoper = leftStoper;
					this.rightStoper = rightStoper;
					this.offset = offset;
					this.orientation = orientation;
				}
			}

			protected override void InternalRender(Widget canvas)
			{
				if (Document.Current.PreviewAnimation || !SceneUserPreferences.Instance.RulerVisible)
					return;
				var containerHull = Document.Current.RootNode.AsWidget.CalcHullInSpaceOf(canvas);
				Widget.PrepareRendererState();
				Renderer.DrawRect(Vector2.Zero, Widget.Size, ColorTheme.Current.SceneView.RulerBackground);
				var start = (Orientation == Orientation.Horizontal ? Widget.Size.Y : Widget.Size.X) * Vector2.One;
				Renderer.DrawLine(start, Widget.Size, ColorTheme.Current.SceneView.RulerTextColor);
				DrawSegments(new RulerData(containerHull.V1, 0, Orientation == Orientation.Horizontal ?
					Widget.Size.X : Widget.Size.Y, RulerSize, Orientation), RulerSize);
			}

			public RulerWidgetPresenter(Widget widget, Orientation orientation)
			{
				Widget = widget;
				Orientation = orientation;
			}

			private void DrawSegments(RulerData data, float strokeDelta, float stokeLength, float? strokeValue = null)
			{
				int j;
				Vector2 delta;
				Vector2 a;
				Vector2 b;
				Func<Vector2, float> value;
				var textOffset = Vector2.Right * 5;
				if (data.orientation == Orientation.Vertical) {
					delta = Vector2.Down * strokeDelta;
					j = -(int)Math.Round((data.origin.Y - data.leftStoper) / strokeDelta);
					a = new Vector2(data.offset - stokeLength, data.origin.Y + j * strokeDelta);
					b = new Vector2(data.offset, data.origin.Y + j * strokeDelta);
					value = (v) => v.Y;
				} else {
					delta = Vector2.Right * strokeDelta;
					j = -(int)Math.Round((data.origin.X - data.leftStoper) / strokeDelta);
					a = new Vector2(data.origin.X + j * strokeDelta, data.offset - stokeLength);
					b = new Vector2(data.origin.X + j * strokeDelta, data.offset);
					value = (v) => v.X;
				}
				do {
					if (value(a) - data.offset >= 0) {
						Renderer.DrawLine(a, b, ColorTheme.Current.SceneView.RulerTextColor);
						if (strokeValue != null) {
							var text = PrepareSegmentText((Mathf.Abs(j * (int)strokeValue.Value)).ToString(), Orientation);
							Renderer.DrawTextLine(a + textOffset, text, 14f, ColorTheme.Current.SceneView.RulerTextColor, 1f);
						}
					}
					j++;
					a += delta;
					b += delta;
				} while (data.rightStoper > value(a));
			}

			private string PrepareSegmentText(string text, Orientation orientation)
			{
				if (orientation == Orientation.Vertical) {
					return string.Join("\n", text.ToCharArray());
				} else {
					return text;
				}
			}

			private void DrawSegments(RulerData data, float strokeLength)
			{
				var maxDelta = 100;
				var minDelta = 50;
				var step = minDelta * 2048f;
				while (true) {
					var v = step * sv.Scene.Scale.X;
					if (v <= maxDelta && v >= minDelta) {
						break;
					}
					if (v < minDelta) {
						step *= 2;
					}

					if (v > maxDelta) {
						step /= 2;
					}
				}
				var strokeDelta = step * sv.Scene.Scale.X;
				DrawSegments(data, strokeDelta, strokeLength, step);
				DrawSegments(data, strokeDelta / 5, strokeLength / 5);
				DrawSegments(data, strokeDelta / 10, strokeLength / 10);
			}
		}
	}

	public class LineSelectionComponent : Component
	{
		public RulerLine Line { get; set; }
	}
}
