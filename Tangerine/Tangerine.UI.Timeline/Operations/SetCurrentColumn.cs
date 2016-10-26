using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Operations
{
	public class SetCurrentColumn : Operation
	{
		protected int Column;

		public override bool IsChangingDocument => false;

		public static void Perform(int column)
		{
			Document.Current.History.Perform(new SetCurrentColumn(column));
		}

		private SetCurrentColumn(int column)
		{
			Column = column;
		}

		public class Processor : OperationProcessor<SetCurrentColumn>
		{
			class Backup { public int Column; }

			protected override void InternalDo(SetCurrentColumn op)
			{
				op.Save(new Backup { Column = Timeline.Instance.CurrentColumn });
				Timeline.Instance.CurrentColumn = op.Column;
				Timeline.Instance.EnsureColumnVisible(op.Column);
			}

			protected override void InternalUndo(SetCurrentColumn op)
			{
				var col = op.Restore<Backup>().Column;
				Timeline.Instance.CurrentColumn = col;
				Timeline.Instance.EnsureColumnVisible(col);
			}
		}
	}
}