using System;
using Lime;

namespace Tangerine.Core.Operations
{
	public class SelectRow : Operation
	{
		public readonly Row Row;
		public readonly bool Select;

		public override bool IsChangingDocument => false;

		static int selectCounter = 1;

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
			class Backup { public int selectCounter; }

			protected override void InternalRedo(SelectRow op)
			{
				op.Save(new Backup { selectCounter = op.Row.SelectCounter });
				op.Row.SelectCounter = op.Select ? selectCounter++ : 0;
			}

			protected override void InternalUndo(SelectRow op)
			{
				op.Row.SelectCounter = op.Restore<Backup>().selectCounter;
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
			if (select) {
				var rootFolder = Document.Current.Container.EditorState().RootFolder;
				var folder = rootFolder.Find(node).Folder;
				while (folder != null) {
					if (!folder.Expanded) {
						SetProperty.Perform(folder, nameof(Folder.Expanded), true);
					}
					folder = rootFolder.Find(folder).Folder;
				}
			}
			var row = Document.Current.GetRowForObject(node);
			SelectRow.Perform(row, select);
		}
	}
}