using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core.Components;

namespace Tangerine.Core
{
	public class RowsSynchronizer : SymmetricOperationProcessor
	{
		readonly Stack<Row> folderStack = new Stack<Row>();
		readonly List<Row> rows = new List<Row>();
		int freezeCounter;

		public override void Process(IOperation op)
		{
			if (CanSyncRows(op)) {
				BuildRows();
				if (!rows.SequenceEqual(Document.Current.Rows)) {
					Document.Current.Rows.Clear();
					Document.Current.Rows.AddRange(rows);
				}
			}
		}

		bool CanSyncRows(IOperation op)
		{
			if (op is Operations.FreezeRows) {
				freezeCounter++;
				return true;
			}
			if (op is Operations.UnfreezeRows) {
				freezeCounter--;
				return true;
			}
			return freezeCounter == 0;
		}

		void BuildRows()
		{
			folderStack.Clear();
			rows.Clear();
			int skipFolderCounter = 0;
			foreach (var node in Document.Current.Container.Nodes) {
				if (skipFolderCounter > 0) {
					if (node is FolderBegin)
						skipFolderCounter++;
					if (node is FolderEnd)
						skipFolderCounter--;
				}
				if (skipFolderCounter > 0)
					continue;
				if (node is FolderBegin) {
					var row = AddFolderRow((FolderBegin)node, ParentFolderRow());
					folderStack.Push(row);
					if (!node.EditorState().Expanded)
						skipFolderCounter = 1;
				} else if (node is FolderEnd) {
					var row = folderStack.Pop();
					row.Components.Get<FolderRow>().FolderEnd = (FolderEnd)node;
				} else {
					var nodeRow = AddNodeRow(node, ParentFolderRow());
					if (node.EditorState().Expanded) {
						foreach (var animator in node.Animators) {
							AddAnimatorRow(node, animator, nodeRow);
						}
					}
				}
			}
			if (folderStack.Count > 0) {
				throw new InvalidOperationException("Missing FolderEnd node");
			}
		}

		Row ParentFolderRow() => folderStack.Count > 0 ? folderStack.Peek() : null;

		Row AddAnimatorRow(Node node, IAnimator animator, Row parent)
		{
			var row = Document.Current.GetRowForObject(animator);
			if (!row.Components.Has<PropertyRow>()) {
				row.Components.Add(new PropertyRow(node, animator));
			}
			AddRow(row, parent);
			return row;
		}

		Row AddNodeRow(Node node, Row parent)
		{
			var row = Document.Current.GetRowForObject(node);
			if (!row.Components.Has<NodeRow>()) {
				row.Components.Add(new NodeRow(node));
			}
			AddRow(row, parent);
			return row;
		}

		Row AddFolderRow(FolderBegin node, Row parent)
		{
			var row = Document.Current.GetRowForObject(node);
			if (!row.Components.Has<FolderRow>()) {
				row.Components.Add(new FolderRow(node));
			}
			AddRow(row, parent);
			return row;
		}

		void AddRow(Row row, Row parent)
		{
			row.Index = rows.Count;
			row.Parent = parent;
			rows.Add(row);
		}
	}

	namespace Operations
	{
		public class FreezeRows : Operation
		{
			public override bool IsChangingDocument => false;

			public static void Perform()
			{
				Document.Current.History.Perform(new FreezeRows());
			}
		}

		public class UnfreezeRows : Operation
		{
			public override bool IsChangingDocument => false;

			public static void Perform()
			{
				Document.Current.History.Perform(new UnfreezeRows());
			}
		}
	}
}