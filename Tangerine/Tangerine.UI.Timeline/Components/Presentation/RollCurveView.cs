using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Components
{
	public class RollCurveView : IRollWidget
	{
		readonly Row row;
		readonly SimpleText label;
		readonly Widget widget;

		public RollCurveView(Row row, int identation)
		{
			this.row = row;
			var c = row.Components.Get<CurveRow>();
			label = new SimpleText { AutoSizeConstraints = false, LayoutCell = new LayoutCell(Alignment.Center), Text = c.State.Component };
			widget = new Widget {
				Padding = new Thickness { Left = 4, Right = 2 },
				MinHeight = c.State.RowHeight - 1,
				Layout = new HBoxLayout(),
				Nodes = {
					new HSpacer(identation * Metrics.RollIndentation),
					label,
				},
			};
			widget.Presenter = new DelegatePresenter<Widget>(RenderBackground);
			widget.Updated += delta => widget.MinHeight = c.State.RowHeight;
		}

		Widget IRollWidget.Widget => widget;

		void RenderBackground(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, widget.Size, Timeline.Instance.SelectedRows.Contains(row) ? Colors.SelectedBackground : Colors.WhiteBackground);
		}
	}
}