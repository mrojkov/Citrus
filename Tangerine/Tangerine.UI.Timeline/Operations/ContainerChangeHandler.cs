using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class ContainerChangeHandler : SymmetricOperationProcessor
	{
		public override void Do(IOperation op)
		{
			if (op is Core.Operations.ISetContainer) {
				var timeline = Timeline.Instance;
				var rows = Document.Current.SelectedRows;
				if (rows.Count > 0) {
					timeline.EnsureRowVisible(rows[0]);
				}
				timeline.EnsureColumnVisible(Document.Current.AnimationFrame);
			}
		}
	}	
}