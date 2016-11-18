using System;
using System.Linq;
using System.Collections.Generic;
using Lime;

namespace Tangerine.Core
{
	public class RowsSynchronizer : SymmetricOperationProcessor
	{
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
			rows.Clear();
			foreach (var node in Document.Current.Container.Nodes) {
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

		void AddRow(Row row)
		{
			row.Index = rows.Count;
			rows.Add(row);
		}
	}
}