using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core.Components;

namespace Tangerine.Core
{
	public class RowsSynchronizer : SymmetricOperationProcessor
	{
		readonly List<Row> rows = new List<Row>();

		public override void Process(IOperation op)
		{
			var doc = Document.Current;
			rows.Clear();
			doc.RowTree = GetFolderRow(doc.Container.RootFolder());
			doc.RowTree.Rows.Clear();
			AddFolderContent(doc.RowTree);
			// Use temporal row list to avoid 'Collection was modified' exception during row batch processing.
			if (!rows.SequenceEqual(Document.Current.Rows)) {
				doc.Rows.Clear();
				doc.Rows.AddRange(rows);
			}
		}

		void AddFolderContent(Row parentRow)
		{
			var parentFolder = parentRow.Components.Get<FolderRow>().Folder;
			foreach (var i in parentFolder.Items) {
				var node = i as Node;
				var folder = i as Folder;
				if (node != null) {
					var nodeRow = AddNodeRow(parentRow, node);
					if (node.EditorState().Expanded) {
						foreach (var animator in node.Animators) {
							AddAnimatorRow(nodeRow, node, animator);
						}
					}
				} else if (folder != null) {
					var folderRow = AddFolderRow(parentRow, folder);
					if (folder.Expanded) {
						AddFolderContent(folderRow);
					}
				}
			}
		}

		Row AddAnimatorRow(Row parent, Node node, IAnimator animator)
		{
			var row = Document.Current.GetRowForObject(animator);
			if (!row.Components.Contains<PropertyRow>()) {
				row.Components.Add(new PropertyRow(node, animator));
			}
			AddRow(parent, row);
			return row;
		}

		Row AddNodeRow(Row parent, Node node)
		{
			var row = Document.Current.GetRowForObject(node);
			if (!row.Components.Contains<NodeRow>()) {
				row.Components.Add(new NodeRow(node));
				row.CanHaveChildren = true;
			}
			AddRow(parent, row);
			return row;
		}

		Row AddFolderRow(Row parent, Folder folder)
		{
			var row = GetFolderRow(folder);
			AddRow(parent, row);
			return row;
		}

		Row GetFolderRow(Folder folder)
		{
			var row = Document.Current.GetRowForObject(folder);
			if (!row.Components.Contains<FolderRow>()) {
				row.Components.Add(new FolderRow(folder));
				row.CanHaveChildren = true;
			}
			return row;
		}

		void AddRow(Row parent, Row row)
		{
			row.Index = rows.Count;
			row.Parent = parent;
			row.Rows.Clear();
			rows.Add(row);
			parent.Rows.Add(row);
		}
	}
}