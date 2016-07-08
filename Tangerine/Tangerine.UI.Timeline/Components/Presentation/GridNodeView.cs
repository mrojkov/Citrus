using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Components
{
	public class GridNodeView : IGridWidget, IOverviewWidget
	{
		readonly Node node;
		readonly Widget gridWidget;
		readonly Widget overviewWidget;
		
		public GridNodeView(Node node)
		{
			this.node = node;
			gridWidget = new Widget { LayoutCell = new LayoutCell { StretchY = 0 }, MinHeight = Metrics.TimelineDefaultRowHeight };
			overviewWidget = new Widget { LayoutCell = new LayoutCell { StretchY = 0 }, MinHeight = Metrics.TimelineDefaultRowHeight };
			gridWidget.Presenter = new DelegatePresenter<Widget>(Render);
			overviewWidget.Presenter = new DelegatePresenter<Widget>(Render);
		}

		Widget IGridWidget.Widget => gridWidget;
		Widget IOverviewWidget.Widget => overviewWidget;

		static BitSet32[] keyStrips = new BitSet32[0];

		void Render(Widget widget)
		{
			var maxCol = Timeline.Instance.ColumnCount;
			widget.PrepareRendererState();
			if (maxCol > keyStrips.Length) {
				keyStrips = new BitSet32[maxCol];
			}
			for (int i = 0; i < maxCol; i++) {
				keyStrips[i] = BitSet32.Empty;
			}
			Renderer.DrawRect(Vector2.Zero, widget.ContentSize, Colors.WhiteBackground);
			foreach (var a in node.Animators) {
				for (int j = 0; j < a.ReadonlyKeys.Count; j++) {
					var key = a.ReadonlyKeys[j];
					var attr = PropertyAttributes<TangerineAttribute>.Get(node.GetType(), a.TargetProperty);
					if (attr != null) {
						keyStrips[key.Frame][attr.ColorIndex] = true;
					}
				}
			}
			for (int i = 0; i < maxCol; i++) {
				if (keyStrips[i] != BitSet32.Empty) {
					var s = keyStrips[i];
					int c = 0;
					for (int j = 0; j < 32; j++) {
						c += s[j] ? 1 : 0;
					}
					var a = new Vector2(i * Metrics.TimelineColWidth, 0);
					var d = widget.Height / c;
					for (int j = 0; j < 32; j++) {
						if (s[j]) {
							var b = a + new Vector2(Metrics.TimelineColWidth, d);
							Renderer.DrawRect(a, b, KeyframePalette.Colors[j]);
							a.Y += d;
						}
					}
				}
			}
		}
	}
}