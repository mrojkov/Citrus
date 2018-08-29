using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline.Operations
{
	public class ShiftGridSelection : Operation
	{
		public override bool IsChangingDocument => false;

		public readonly IntVector2 Offset;

		public static void Perform(IntVector2 offset)
		{
			DocumentHistory.Current.Perform(new ShiftGridSelection(offset));
		}

		ShiftGridSelection(IntVector2 offset)
		{
			Offset = offset;
		}

		public class Processor : OperationProcessor<ShiftGridSelection>
		{
			class Backup { public List<GridSpanList> Spans; }

			protected override void InternalRedo(ShiftGridSelection op)
			{
				ShiftX(op.Offset.X);
				ShiftY(op);
			}

			protected override void InternalUndo(ShiftGridSelection op)
			{
				UnshiftY(op);
				ShiftX(-op.Offset.X);
			}

			void ShiftX(int offset)
			{
				foreach (var row in Document.Current.Rows) {
					var spans = row.Components.GetOrAdd<GridSpanListComponent>().Spans;
					for (int i = 0; i < spans.Count; i++) {
						var s = spans[i];
						s.A += offset;
						s.B += offset;
						spans[i] = s;
					}
				}
			}

			void ShiftY(ShiftGridSelection op)
			{
				var b = new Backup { Spans = Document.Current.Rows.Select(r => r.Components.GetOrAdd<GridSpanListComponent>().Spans).ToList() };
				op.Save(b);
				if (op.Offset.Y != 0) {
					foreach (var row in Document.Current.Rows) {
						var i = row.Index - op.Offset.Y;
						row.Components.Remove<GridSpanListComponent>();
						row.Components.Add(i >= 0 && i < Document.Current.Rows.Count ? new GridSpanListComponent(b.Spans[i]) : new GridSpanListComponent());
					}
				}
			}

			void UnshiftY(ShiftGridSelection op)
			{
				var b = op.Restore<Backup>();
				foreach (var row in Document.Current.Rows) {
					row.Components.Remove<GridSpanListComponent>();
					row.Components.Add(new GridSpanListComponent(b.Spans[row.Index]));
				}
			}
		}
	}
}