using System;
using System.Collections.Generic;
using System.Linq;
using Tangerine.Core;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline.Operations
{
	public class SelectGridSpan : Operation
	{
		public readonly GridSpan Span;
		public readonly int Row;

		public override bool IsChangingDocument => false;

		public static void Perform(int row, GridSpan span)
		{
			Document.Current.History.Perform(new SelectGridSpan(row, span));
		}

		private SelectGridSpan(int row, GridSpan span)
		{
			Row = row;
			Span = span;
		}

		public class Processor : OperationProcessor<SelectGridSpan>
		{
			protected override void InternalDo(SelectGridSpan op)
			{
				Document.Current.Rows[op.Row].Components.GetOrAdd<GridSpanList>().Add(op.Span);
			}

			protected override void InternalUndo(SelectGridSpan op)
			{
				Document.Current.Rows[op.Row].Components.GetOrAdd<GridSpanList>().Remove(op.Span);
			}
		}
	}
}