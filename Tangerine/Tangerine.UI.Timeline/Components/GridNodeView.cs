using Lime;
using System.Collections.Generic;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Components
{
	public class GridNodeView : IGridRowView
	{
		private readonly Node node;

		public Widget GridWidget { get; }
		public Widget OverviewWidget { get; }
		public AwakeBehavior GridWidgetAwakeBehavior => GridWidget.Components.Get<AwakeBehavior>();
		public AwakeBehavior OverviewWidgetAwakeBehavior => OverviewWidget.Components.Get<AwakeBehavior>();

		public GridNodeView(Node node)
		{
			this.node = node;
			GridWidget = new Widget {
				LayoutCell = new LayoutCell {StretchY = 0},
				MinHeight = TimelineMetrics.DefaultRowHeight,
				Presenter = new SyncDelegatePresenter<Widget>(Render)
			};
			GridWidget.Components.Add(new AwakeBehavior());
			OverviewWidget = new Widget {
				LayoutCell = new LayoutCell {StretchY = 0},
				MinHeight = TimelineMetrics.DefaultRowHeight,
				Presenter = new SyncDelegatePresenter<Widget>(Render)
			};
			OverviewWidget.Components.Add(new AwakeBehavior());
		}

		private class Cell
		{
			public BitSet32 Strips;
			public int StripCount;
			public KeyFunction Func1;
			public KeyFunction Func2;

			public void Clear()
			{
				Strips = BitSet32.Empty;
				StripCount = 0;
			}
		}

		private static readonly Stack<Cell> cellPool = new Stack<Cell>();
		private readonly Dictionary<int, Cell> cells = new Dictionary<int, Cell>();
		private int animatorsVersion = -1;
		private string animationId;

		protected virtual void Render(Widget widget)
		{
			int v = CalcAnimatorsTotalVersion();
			if (animatorsVersion != v || animationId != Document.Current.AnimationId) {
				animatorsVersion = v;
				animationId = Document.Current.AnimationId;
				GenerateCells();
			}
			RenderCells(widget);
		}

		private int CalcAnimatorsTotalVersion()
		{
			int result = node.Animators.Version;
			foreach (var a in node.Animators) {
				result = unchecked(result + a.Version);
			}
			return result;
		}

		private void GenerateCells()
		{
			foreach (var cell in cells.Values) {
				cell.Clear();
				cellPool.Push(cell);
			}
			cells.Clear();
			foreach (var animator in node.Animators) {
				if (animator.IsZombie || animator.AnimationId != Document.Current.AnimationId) {
					continue;
				}
				for (var j = 0; j < animator.ReadonlyKeys.Count; j++) {
					var key = animator.ReadonlyKeys[j];
					var colorIndex = PropertyAttributes<TangerineKeyframeColorAttribute>.Get(animator.Animable.GetType(), animator.TargetPropertyPath)?.ColorIndex ?? 0;
					if (!cells.TryGetValue(key.Frame, out var cell)) {
						cell = cellPool.Count == 0 ? new Cell() : cellPool.Pop();
						cells.Add(key.Frame, cell);
					}
					if (cell.StripCount == 0) {
						cell.Func1 = key.Function;
					} else if (cell.StripCount == 1) {
						var lastColorIndex = 0;
						for (int i = 0; i < cell.Strips.Count; i++) {
							if (cell.Strips[i]) {
								lastColorIndex = i;
								break;
							}
						}
						if (lastColorIndex < colorIndex) {
							cell.Func2 = key.Function;
						} else {
							var a = cell.Func1;
							cell.Func1 = key.Function;
							cell.Func2 = a;
						}
					}
					cell.Strips[colorIndex] = true;
					cell.StripCount++;
					cells[key.Frame] = cell;
				}
			}
		}

		private void RenderCells(Widget widget)
		{
			widget.PrepareRendererState();
			foreach (var kv in cells) {
				int column = kv.Key;
				var cell = kv.Value;
				var a = new Vector2(column * TimelineMetrics.ColWidth + 1, 0);
				var stripHeight = widget.Height / cell.StripCount;
				if (cell.StripCount <= 2) {
					int color1 = -1;
					int color2 = -1;
					for (int j = 0; j < cell.Strips.Count; j++) {
						if (cell.Strips[j]) {
							if (color1 == -1) {
								color1 = j;
							} else {
								color2 = j;
							}
						}
					}
					if (color2 == -1) {
						color2 = color1;
					}
					var b = a + new Vector2(TimelineMetrics.ColWidth - 1, stripHeight);
					DrawFigure(a, b, cell.Func1, KeyframePalette.Colors[color1]);
					if (cell.StripCount == 2) {
						a.Y += stripHeight;
						b.Y += stripHeight;
						DrawFigure(a, b, cell.Func2, KeyframePalette.Colors[color2]);
					}
				} else {
					// Draw strips
					var drawnStripCount = 0;
					for (var colorIndex = 0; colorIndex < cell.Strips.Count; colorIndex++) {
						if (cell.Strips[colorIndex]) {
							var b = a + new Vector2(TimelineMetrics.ColWidth - 1, stripHeight);
							Renderer.DrawRect(a, b, KeyframePalette.Colors[colorIndex]);
							drawnStripCount++;
							if (drawnStripCount == cell.StripCount) break;
							a.Y += stripHeight;
						}
					}
					// Strips of the same color
					if (drawnStripCount < cell.StripCount) {
						int colorIndex;
						for (colorIndex = 0; colorIndex < cell.Strips.Count; colorIndex++) {
							if (cell.Strips[colorIndex]) break;
						}
						for (var j = drawnStripCount; j != cell.StripCount; j++) {
							var b = a + new Vector2(TimelineMetrics.ColWidth - 1, stripHeight);
							Renderer.DrawRect(a, b, KeyframePalette.Colors[colorIndex]);
							a.Y += stripHeight;
						}
					}
				}
			}
		}

		private static Vertex[] vertices = new Vertex[11];

		private void DrawFigure(Vector2 a, Vector2 b, KeyFunction func, Color4 color)
		{
			var segmentWidth = b.X - a.X;
			var segmentHeight = b.Y - a.Y;
			switch (func) {
				case KeyFunction.Linear: {
					vertices[0].Pos = new Vector2(a.X, b.Y - 0.5f);
					vertices[1].Pos = new Vector2(b.X, a.Y);
					vertices[2].Pos = new Vector2(b.X, b.Y - 0.5f);
					for (int i = 0; i < vertices.Length; i++) {
						vertices[i].Color = color;
					}
					Renderer.DrawTriangleFan(vertices, numVertices: 3);
					break;
				}
				case KeyFunction.Steep: {
					var leftSmallRectVertexA = new Vector2(a.X + 0.5f, a.Y + segmentHeight / 2);
					var leftSmallRectVertexB = new Vector2(a.X + segmentWidth / 2, b.Y - 0.5f);
					Renderer.DrawRect(leftSmallRectVertexA, leftSmallRectVertexB, color);
					var rightBigRectVertexA = new Vector2(a.X + segmentWidth / 2, a.Y + 0.5f);
					var rightBigRectVertexB = new Vector2(b.X, b.Y - 0.5f);
					Renderer.DrawRect(rightBigRectVertexA, rightBigRectVertexB, color);
					break;
				}
				case KeyFunction.Spline: {
					var numSegments = 5;
					var center = new Vector2(a.X, b.Y - 0.5f);
					vertices[0] = new Vertex { Pos = center, Color = color };
					for (int i = 0; i < numSegments; i++) {
						var r = Vector2.CosSin(i * Mathf.HalfPi / (numSegments - 1));
						vertices[i + 1].Pos.X = center.X + r.X * segmentWidth;
						vertices[i + 1].Pos.Y = center.Y - r.Y * segmentHeight;
						vertices[i + 1].Color = color;
					}
					Renderer.DrawTriangleFan(vertices, numSegments + 1);
					break;
				}
				case KeyFunction.ClosedSpline: {
					var circleCenter = new Vector2(a.X + segmentWidth / 2, a.Y + segmentHeight / 2);
					var circleRadius = 0f;
					if (segmentWidth < segmentHeight) {
						circleRadius = circleCenter.X - a.X - 0.5f;
					} else {
						circleRadius = circleCenter.Y - a.Y - 0.5f;
					}
					Renderer.DrawRound(circleCenter, circleRadius, numSegments: 6, color);
					break;
				}
			}
		}
	}
}
