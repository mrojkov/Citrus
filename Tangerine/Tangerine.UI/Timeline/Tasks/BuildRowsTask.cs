using System;
using System.Linq;
using Lime;
using Tangerine.Core;
using System.Collections.Generic;

namespace Tangerine.UI.Timeline
{
	public class BuildRowsTask
	{
		Timeline timeline => Timeline.Instance;

		public IEnumerator<object> Main()
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
			timeline.Rows.Clear();
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
			var row = timeline.GetCachedRow(curve.Uid);
			if (!row.Components.Has<Components.CurveRow>()) {
				row.Components.Add(new Components.CurveRow(node, animator, curve));
			}
			AddRow(row);
		}

		void AddAnimatorRow(Node node, IAnimator animator)
		{
			var row = timeline.GetCachedRow(animator.EditorState().Uid);
			if (!row.Components.Has<Components.PropertyRow>()) {
				row.Components.Add(new Components.PropertyRow(node, animator));
			}
			AddRow(row);
		}

		void AddNodeRow(Node node)
		{
			var row = timeline.GetCachedRow(node.EditorState().Uid);
			if (!row.Components.Has<Components.NodeRow>()) {
				row.Components.Add(new Components.NodeRow(node));
			}
			AddRow(row);
		}

		public void AddRow(Row row)
		{
			row.Index = timeline.Rows.Count;
			timeline.Rows.Add(row);
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
			return i == Timeline.Instance.Rows.Count;
		}

		bool ValidateNodeRow(int row, Node node)
		{
			var rows = Timeline.Instance.Rows;
			return row < rows.Count && rows[row].Components.Get<Components.NodeRow>()?.Node == node;
		}

		bool ValidatePropertyRow(int row, IAnimator animator)
		{
			var rows = Timeline.Instance.Rows;
			return row < rows.Count && rows[row].Components.Get<Components.PropertyRow>()?.Animator == animator;
		}

		bool ValidateCurveRow(int row, IAnimator animator, CurveEditorState curveState)
		{
			var rows = Timeline.Instance.Rows;
			if (row >= rows.Count) {
				return false;
			}
			var cr = rows[row].Components.Get<Components.CurveRow>();
			return cr?.Animator == animator && cr?.State == curveState;
		}
	}
}