using System;
using Lime;

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
			class Backup { public long selectedAtUpdate; }

			protected override void InternalRedo(SelectRow op)
			{
				op.Save(new Backup { selectedAtUpdate = op.Row.SelectedAtUpdate });
				op.Row.SelectedAtUpdate = op.Select ? Application.UpdateCounter : 0;
			}

			protected override void InternalUndo(SelectRow op)
			{
				op.Row.SelectedAtUpdate = op.Restore<Backup>().selectedAtUpdate;
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
			if (node is FolderEnd)
				return;
			var row = Document.Current.GetRowForObject(node);
			SelectRow.Perform(row, select);
		}
	}
}