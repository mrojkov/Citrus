using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine.Timeline
{
	public class GridIdleState : GridState
	{
		public override void OnMousePress(Lime.IntVector2 cell)
		{
			if (doc.SelectedCells.Contains(cell)) {
				//The.Timeline.Grid.ChangeState(new TimelineGridDragSelectionState(cell));
			} else {
				The.Timeline.Grid.ChangeState(new SelectRectGridState(cell));
			}
		}
	}
}
