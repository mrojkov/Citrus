using System;
using System.Linq;
using Lime;
using System.Collections.Generic;

namespace Tangerine.Core
{
	public class RowsSynchronizer : SymmetricOperationProcessor
	{
		public override void Do(IOperation operation)
		{
			if (!ValidateRows()) {
				RebuildRows();
			}
		}

		void RebuildRows()
		{
			Document.Current.Rows.Clear();
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

		public void AddRow(Row row)
		{
			row.Index = Document.Current.Rows.Count;
			Document.Current.Rows.Add(row);
		}

		bool ValidateRows()
		{
			var doc = Document.Current;
			if (doc == null) {
				return true;
			}
			int i = 0;
			foreach (var node in Document.Current.Container.Nodes) {
				if (!ValidateNodeRow(i++, node)) {
					return false;
				}
				if (node.EditorState().Expanded) {
					foreach (var animator in node.Animators) {
						if (!ValidatePropertyRow(i++, animator)) {
							return false;
						}
						if (animator.EditorState().CurvesShown) {
							foreach (var curve in animator.EditorState().Curves) {
								if (!ValidateCurveRow(i++, animator, curve))
									return false;
							}
						}
					}
				}
			}
			return i == Document.Current.Rows.Count;
		}

		bool ValidateNodeRow(int row, Node node)
		{
			var rows = Document.Current.Rows;
			return row < rows.Count && rows[row].Components.Get<Components.NodeRow>()?.Node == node;
		}

		bool ValidatePropertyRow(int row, IAnimator animator)
		{
			var rows = Document.Current.Rows;
			return row < rows.Count && rows[row].Components.Get<Components.PropertyRow>()?.Animator == animator;
		}

		bool ValidateCurveRow(int row, IAnimator animator, CurveEditorState curveState)
		{
			var rows = Document.Current.Rows;
			if (row >= rows.Count) {
				return false;
			}
			var cr = rows[row].Components.Get<Components.CurveRow>();
			return cr?.Animator == animator && cr?.State == curveState;
		}
	}
}