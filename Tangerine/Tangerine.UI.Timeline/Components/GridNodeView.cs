using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Components
{
	public class GridNodeView : IGridRowView
	{
		readonly Node node;

		public Widget GridWidget { get; private set; }
		public Widget OverviewWidget { get; private set; }

		public GridNodeView(Node node)
		{
			this.node = node;
			GridWidget = new Widget { LayoutCell = new LayoutCell { StretchY = 0 }, MinHeight = TimelineMetrics.DefaultRowHeight };
			GridWidget.Presenter = new DelegatePresenter<Widget>(Render);
			OverviewWidget = new Widget { LayoutCell = new LayoutCell { StretchY = 0 }, MinHeight = TimelineMetrics.DefaultRowHeight };
			OverviewWidget.Presenter = new DelegatePresenter<Widget>(Render);
		}

		static BitSet32[] keyStrips = new BitSet32[0];

		protected virtual void Render(Widget widget)
		{
			var maxCol = Timeline.Instance.ColumnCount;
			widget.PrepareRendererState();
			if (maxCol > keyStrips.Length) {
				keyStrips = new BitSet32[maxCol];
			}
			for (int i = 0; i < maxCol; i++) {
				keyStrips[i] = BitSet32.Empty;
			}
			foreach (var animator in node.Animators) {
				for (int i = 0; i < animator.ReadonlyKeys.Count; i++) {
					var key = animator.ReadonlyKeys[i];
					var colorIndex = PropertyAttributes<TangerineKeyframeColorAttribute>.Get(node.GetType(), animator.TargetProperty)?.ColorIndex ?? 0;
					keyStrips[key.Frame][colorIndex] = true;

					if (keyStrips[key.Frame] != BitSet32.Empty) {
						// How many interpolations in the key?
						var numberOf = 2;
						var numberOfOnesInKeyStrips = 0;
						for (int k = 0; k < keyStrips[key.Frame].Count; k++) {
							if (keyStrips[key.Frame][k] == true) {
								numberOfOnesInKeyStrips++;
								if (numberOfOnesInKeyStrips == numberOf + 1) { break; }
							}
						}
						if (numberOfOnesInKeyStrips <= numberOf) {
							// Draw figures
							var s = keyStrips[key.Frame];
							var a = new Vector2(key.Frame * TimelineMetrics.ColWidth + 1, 0);
							var d = widget.Height;
							for (int j = 0; j < 32; j++) {
								if (s[j]) {
									var b = a + new Vector2(TimelineMetrics.ColWidth - 1, d);
									switch (key.Function) {
										case KeyFunction.Linear:
											var quadrangle = new Quadrangle {
												V1 = new Vector2(a.X + (b.X - a.X) / 2, a.Y),
												V2 = new Vector2(b.X, a.Y + (b.Y - a.Y) / 2),
												V3 = new Vector2(a.X + (b.X - a.X) / 2, b.Y),
												V4 = new Vector2(a.X, a.Y + (b.Y - a.Y) / 2)
											};
											Renderer.DrawQuadrangle(quadrangle, KeyframePalette.Colors[j]);
											break;
										case KeyFunction.Steep:
											var rectSize = (b.X - a.X) / 2;
											var horizontalOffset = rectSize / 2;
											var verticalOffset = (b.Y - a.Y - rectSize) / 2;
											var rectVertexA = new Vector2(a.X + horizontalOffset, a.Y + verticalOffset);
											var rectVertexB = new Vector2(b.X - horizontalOffset, b.Y - verticalOffset);
											Renderer.DrawRect(rectVertexA, rectVertexB, KeyframePalette.Colors[j]);
											break;
										case KeyFunction.Spline:
											var circleCenter = new Vector2(a.X + (b.X - a.X) / 2, a.Y + (b.Y - a.Y) / 2);
											var circleRadius = (b.X - a.X) / 2;
											Renderer.DrawRound(circleCenter, circleRadius, 32, KeyframePalette.Colors[j]);
											break;
										case KeyFunction.ClosedSpline:
											var roundCenter = new Vector2(a.X + (b.X - a.X) / 2, a.Y + (b.Y - a.Y) / 2);
											var roundRadius = (b.X - a.X) / 2;
											Renderer.DrawRound(roundCenter, roundRadius, 32, Color4.Transparent, KeyframePalette.Colors[j]);
											break;
									}
								}
							}
						}
						else {
							// Draw strips
							var s = keyStrips[i];
							int c = 0;
							for (int j = 0; j < 32; j++) {
								c += s[j] ? 1 : 0;
							}
							var a = new Vector2(i * TimelineMetrics.ColWidth + 1, 0);
							var d = widget.Height / c;
							for (int j = 0; j < 32; j++) {
								if (s[j]) {
									var b = a + new Vector2(TimelineMetrics.ColWidth - 1, d);
									Renderer.DrawRect(a, b, KeyframePalette.Colors[j]);
									a.Y += d;
								}
							}
						}
					}
				}
			}
		}
	}
}
