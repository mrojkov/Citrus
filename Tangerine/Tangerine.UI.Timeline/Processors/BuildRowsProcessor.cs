using System;
using System.Linq;
using Lime;
using Tangerine.Core;
using System.Collections.Generic;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class BuildRowsProcessor : IProcessor
	{
		Timeline timeline => Timeline.Instance;

		public IEnumerator<object> Loop()
		{
			while (true) {
				if (!ValidateRows()) {
					RebuildRows();
				}
				yield return null;
			}
		}

		void RebuildRows()
		{
			Document.Current.Rows.Clear();
			foreach (var node in timeline.Container.Nodes) {
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
			if (!row.Components.Has<Core.Components.CurveRow>()) {
				row.Components.Add(new Core.Components.CurveRow(node, animator, curve));
			}
			AddRow(row);
		}

		void AddAnimatorRow(Node node, IAnimator animator)
		{
			var row = Document.Current.GetRowById(animator.EditorState().Uid);
			if (!row.Components.Has<Core.Components.PropertyRow>()) {
				row.Components.Add(new Core.Components.PropertyRow(node, animator));
			}
			AddRow(row);
		}

		void AddNodeRow(Node node)
		{
			var row = Document.Current.GetRowById(node.EditorState().Uid);
			if (!row.Components.Has<Core.Components.NodeRow>()) {
				row.Components.Add(new Core.Components.NodeRow(node));
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
			int i = 0;
			foreach (var node in Timeline.Instance.Container.Nodes) {
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
			return row < rows.Count && rows[row].Components.Get<Core.Components.NodeRow>()?.Node == node;
		}

		bool ValidatePropertyRow(int row, IAnimator animator)
		{
			var rows = Document.Current.Rows;
			return row < rows.Count && rows[row].Components.Get<Core.Components.PropertyRow>()?.Animator == animator;
		}

		bool ValidateCurveRow(int row, IAnimator animator, CurveEditorState curveState)
		{
			var rows = Document.Current.Rows;
			if (row >= rows.Count) {
				return false;
			}
			var cr = rows[row].Components.Get<Core.Components.CurveRow>();
			return cr?.Animator == animator && cr?.State == curveState;
		}
	}
}