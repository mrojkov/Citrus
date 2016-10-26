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
			Document.Current.History.Perform(new ShiftGridSelection(offset));
		}

		ShiftGridSelection(IntVector2 offset)
		{
			Offset = offset;
		}

		public class Processor : OperationProcessor<ShiftGridSelection>
		{
			class Backup { public List<GridSpanList> Spans; }

			protected override void InternalDo(ShiftGridSelection op)
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
					var spans = row.Components.GetOrAdd<GridSpanList>();
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
				var b = new Backup { Spans = Document.Current.Rows.Select(r => r.Components.GetOrAdd<GridSpanList>()).ToList() };
				op.Save(b);
				if (op.Offset.Y != 0) {
					foreach (var row in Document.Current.Rows) {
						var i = row.Index - op.Offset.Y;
						row.Components.Remove<GridSpanList>();
						row.Components.Add(i >= 0 && i < Document.Current.Rows.Count ? b.Spans[i] : new GridSpanList());
					}
				}
			}

			void UnshiftY(ShiftGridSelection op)
			{
				var b = op.Restore<Backup>();
				foreach (var row in Document.Current.Rows) {
					row.Components.Remove<GridSpanList>();
					row.Components.Add(b.Spans[row.Index]);
				}
			}
		}
	}
}