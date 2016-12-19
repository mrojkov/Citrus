using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class EnsureRowVisibleIfSelected : OperationProcessor<Core.Operations.SelectRow>
	{
		protected override void InternalRedo(Core.Operations.SelectRow op)
		{
			if (op.Select) {
				var timeline = Timeline.Instance;
				timeline.EnsureRowVisible(op.Row);
			}
		}

		protected override void InternalUndo(Core.Operations.SelectRow op) { }
	}
}