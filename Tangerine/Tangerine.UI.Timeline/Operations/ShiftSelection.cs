using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Operations
{
	public class ShiftSelection : IOperation
	{
		IntVector2 offset;

		public bool IsChangingDocument => false;
		public DateTime Timestamp { get; set; }

		public static void Perform(IntVector2 offset)
		{
			Document.Current.History.Perform(new ShiftSelection(offset));
		}

		private ShiftSelection(IntVector2 offset)
		{
			this.offset = offset;
		}

		public void Do()
		{
			Shift(offset);
		}

		public void Undo()
		{
			Shift(-offset);
		}

		void Shift(IntVector2 offset)
		{
			var s = Timeline.Instance.GridSelection;
			for (int i = 0; i < s.Count; i++) {
				var r = s[i];
				r.A += offset;
				r.B += offset;
				s[i] = r;
			}
		}
	}
}