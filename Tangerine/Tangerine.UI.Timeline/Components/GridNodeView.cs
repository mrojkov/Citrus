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
			foreach (var a in node.Animators.AsArray) {
				for (int j = 0; j < a.ReadonlyKeys.Count; j++) {
					var key = a.ReadonlyKeys[j];
					var colorIndex = PropertyAttributes<TangerineKeyframeColorAttribute>.Get(node.GetType(), a.TargetProperty)?.ColorIndex ?? 0;
					keyStrips[key.Frame][colorIndex] = true;
				}
			}
			for (int i = 0; i < maxCol; i++) {
				if (keyStrips[i] != BitSet32.Empty) {
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