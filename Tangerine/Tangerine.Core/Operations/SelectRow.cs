using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.Core.Operations
{
	public class SelectRow : Operation
	{
		public readonly Row Row;
		public readonly bool Select;

		public override bool IsChangingDocument => false;

		public static void Perform(Row row, bool select = true)
		{
			Document.Current.History.Perform(new SelectRow(row, select));
		}

		private SelectRow(Row row, bool select)
		{
			Select = select;
			Row = row;
		}

		public class Processor : OperationProcessor<SelectRow>
		{
			class Backup { public int LastIndex; }

			protected override void InternalRedo(SelectRow op)
			{
				var sr = Document.Current.SelectedRows;
				var b = new Backup { LastIndex = sr.IndexOf(op.Row) };
				op.Save(b);
				if (b.LastIndex >= 0) {
					sr.RemoveAt(b.LastIndex);
				}
				if (op.Select) {
					sr.Insert(0, op.Row);
				}
			}

			protected override void InternalUndo(SelectRow op)
			{
				var b = op.Restore<Backup>();
				var sr = Document.Current.SelectedRows;
				if (op.Select) {
					System.Diagnostics.Debug.Assert(sr[0] == op.Row);
					sr.RemoveAt(0);
				} else if (b.LastIndex >= 0) {
					sr.Insert(b.LastIndex, op.Row);
				}
			}
		}
	}

	public class SelectNode
	{
		public static void Perform(Node node, bool select = true)
		{
			if (node.Parent != Document.Current.Container) {
				throw new InvalidOperationException();
			}
			var row = Document.Current.GetRowById(node.EditorState().Uid);
			SelectRow.Perform(row, select);
		}
	}
}