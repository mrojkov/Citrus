using System;
using System.Linq;
using System.Collections.Generic;
using Lime;

namespace Tangerine.Core
{
	public class RowsSynchronizer : SymmetricOperationProcessor
	{
		readonly Stack<Row> folderStack = new Stack<Row>();
		readonly List<Row> rows = new List<Row>();

		public override void Process(IOperation op)
		{
			BuildRows();
			if (!rows.SequenceEqual(Document.Current.Rows)) {
				Document.Current.Rows.Clear();
				Document.Current.Rows.AddRange(rows);
			}
		}

		void BuildRows()
		{
			folderStack.Clear();
			rows.Clear();
			Row collapsedFolderRow = null;
			foreach (var node in Document.Current.Container.Nodes) {
				if (node is FolderEnd) {
					var row = folderStack.Pop();
					row.Components.Get<Components.FolderRow>().FolderEnd = node;
					if (row == collapsedFolderRow)
						collapsedFolderRow = null;
					continue;
				}
				if (node is FolderBegin) {
					var row = AddFolderRow(node);
					if (collapsedFolderRow == null && !node.EditorState().Expanded) {
						collapsedFolderRow = row;
					}
					folderStack.Push(row);
					continue;
				}
				if (collapsedFolderRow != null) {
					continue;
				}
				AddNodeRow(node);
				if (node.EditorState().Expanded) {
					foreach (var animator in node.Animators) {
						AddAnimatorRow(node, animator);
						if (animator.EditorState().CurvesShown) {
							foreach (var curve in animator.EditorState().Curves) {
								AddCurveRow(node, animator, curve);
							}
						}
					}
				}
			}
		}

		void AddCurveRow(Node node, IAnimator animator, CurveEditorState curve)
		{
			var row = Document.Current.GetRowById(curve.Uid);
			if (!row.Components.Has<Components.CurveRow>()) {
				row.Components.Add(new Components.CurveRow(node, animator, curve));
			}
			AddRow(row);
		}

		void AddAnimatorRow(Node node, IAnimator animator)
		{
			var row = Document.Current.GetRowById(animator.EditorState().Uid);
			if (!row.Components.Has<Components.PropertyRow>()) {
				row.Components.Add(new Components.PropertyRow(node, animator));
			}
			AddRow(row);
		}

		void AddNodeRow(Node node)
		{
			var row = Document.Current.GetRowById(node.EditorState().Uid);
			if (!row.Components.Has<Components.NodeRow>()) {
				row.Components.Add(new Components.NodeRow(node));
			}
			AddRow(row);
		}

		Row AddFolderRow(Node node)
		{
			var row = Document.Current.GetRowById(node.EditorState().Uid);
			if (!row.Components.Has<Components.FolderRow>()) {
				row.Components.Add(new Components.FolderRow(node));
			}
			AddRow(row);
			return row;
		}

		void AddRow(Row row)
		{
			row.Index = rows.Count;
			rows.Add(row);
		}
	}
}