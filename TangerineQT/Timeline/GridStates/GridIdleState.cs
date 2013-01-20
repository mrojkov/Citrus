using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine.Timeline
{
	public class GridIdleState : GridState
	{
		public override void OnMousePress(Qyoto.QMouseEvent e)
		{
			Lime.IntVector2 cell = Toolbox.PixelToCell(e.Pos());
			if (doc.SelectedCells.Contains(cell)) {
				//The.Timeline.Grid.ChangeState(new TimelineGridDragSelectionState(cell));
			} else {
			}
			base.OnMousePress(e);
		}
	}
}
