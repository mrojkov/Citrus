using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Components
{
	public class RollCurveView : IRollRowView
	{
		readonly Row row;
		readonly SimpleText label;
		readonly Widget widget;
		readonly Widget spacer;

		public RollCurveView(Row row)
		{
			this.row = row;
			var c = row.Components.Get<Core.Components.CurveRow>();
			label = new ThemedSimpleText { ForceUncutText = false, LayoutCell = new LayoutCell(Alignment.Center), Text = c.State.Component };
			widget = new Widget {
				Padding = new Thickness { Left = 4, Right = 2 },
				MinHeight = c.State.RowHeight - 1,
				Layout = new HBoxLayout(),
				HitTestTarget = true,
				Nodes = {
					(spacer = new Widget()),
					label,
				},
			};
			widget.CompoundPresenter.Push(new DelegatePresenter<Widget>(RenderBackground));
			widget.Updated += delta => widget.MinHeight = c.State.RowHeight;
		}

		public void Rename() { }

		public Widget Widget => widget;
		public float Indentation { set { spacer.MinMaxWidth = value; } }

		void RenderBackground(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawRect(
				Vector2.Zero, widget.Size,
				row.Selected ? ColorTheme.Current.Basic.SelectedBackground : ColorTheme.Current.Basic.WhiteBackground);
		}
	}
}