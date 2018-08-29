using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline.Operations
{
	public class ClearGridSelection : Operation
	{
		public override bool IsChangingDocument => false;

		public static void Perform()
		{
			DocumentHistory.Current.Perform(new ClearGridSelection());
		}

		private ClearGridSelection() {}

		public class Processor : OperationProcessor<ClearGridSelection>
		{
			class Backup { public List<GridSpanList> Spans; }
				
			protected override void InternalRedo(ClearGridSelection op)
			{
				op.Save(new Backup { Spans = Document.Current.Rows.Select(r => r.Components.GetOrAdd<GridSpanListComponent>().Spans).ToList() });
				foreach (var row in Document.Current.Rows) {
					row.Components.Remove<GridSpanListComponent>();
				}
			}

			protected override void InternalUndo(ClearGridSelection op)
			{
				var s = op.Restore<Backup>().Spans;
				foreach (var row in Document.Current.Rows) {
					row.Components.Remove<GridSpanListComponent>();
					row.Components.Add(new GridSpanListComponent(s[row.Index]));
				}
			}
		}
	}
}