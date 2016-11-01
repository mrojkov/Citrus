using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	class MouseSelectionProcessor : ITaskProvider
	{
		public IEnumerator<object> Task()
		{
			var sv = SceneView.Instance;
			var input = sv.Input;
			while (true) {
				if (input.WasMousePressed() && !input.IsKeyPressed(Key.Shift)) {
					var rect = new Rectangle(sv.MousePosition, sv.MousePosition);
					var presenter = new DelegatePresenter<Widget>(w => {
						w.PrepareRendererState();
						var t = sv.Scene.CalcTransitionToSpaceOf(sv.Frame);
						Renderer.DrawRectOutline(rect.A * t, rect.B * t, SceneViewColors.MouseSelection, 1);
					});
					sv.Frame.CompoundPostPresenter.Add(presenter);
					input.CaptureMouse();
					var clicked = true;
					var selectedNodes = Document.Current.SelectedNodes().Editable().ToList();
					Document.Current.History.BeginTransaction();
					while (input.IsMousePressed()) {
						rect.B = sv.MousePosition;
						clicked &= (rect.B - rect.A).Length <= 5;
						if (!clicked) {
							RefreshSelectedNodes(rect, selectedNodes);
							CommonWindow.Current.Invalidate();
						}
						yield return null;
					}
					input.ReleaseMouse();
					if (clicked) {
						SelectByClick(rect.A);
					}
					sv.Frame.CompoundPostPresenter.Remove(presenter);
					CommonWindow.Current.Invalidate();
					Document.Current.History.EndTransaction();
				}
				yield return null;
			}
		}

		void SelectByClick(Vector2 point)
		{
			var sv = SceneView.Instance;
			var ctrlPressed = sv.Input.IsKeyPressed(Key.Control);
			if (!ctrlPressed) {
				Core.Operations.ClearRowSelection.Perform();
			}
			foreach (var widget in Document.Current.Container.Nodes.Editable().OfType<Widget>()) {
				var hull = widget.CalcHullInSpaceOf(sv.Scene);
				if (hull.Contains(point)) {
					Core.Operations.SelectNode.Perform(widget,
						!ctrlPressed || !Document.Current.SelectedNodes().Contains(widget));
					break;
				}
			}
		}

		void RefreshSelectedNodes(Rectangle rect, IEnumerable<Node> originalSelection)
		{
			var ctrlPressed = SceneView.Instance.Input.IsKeyPressed(Key.Control);
			var currentSelection = Document.Current.SelectedNodes();
			var newSelection = Document.Current.Container.Nodes.Editable().Where(n =>
				ctrlPressed ? TestNode(rect, n) ^ originalSelection.Contains(n) : TestNode(rect, n));
			if (!newSelection.SequenceEqual(currentSelection)) {
				Core.Operations.ClearRowSelection.Perform();
				foreach (var node in newSelection) {
					Core.Operations.SelectNode.Perform(node);
				}
			}
		}

		bool TestNode(Rectangle rect, Node node)
		{
			var nrect = rect.Normalized;
			var canvas = SceneView.Instance.Scene;
			if (node is Widget) {
				var widget = (Widget)node;
				var hull = widget.CalcHullInSpaceOf(canvas);
				for (int i = 0; i < 4; i++) {
					if (nrect.Contains(hull[i])) {
						return true;
					}
				}
				var pivot = widget.CalcPositionInSpaceOf(canvas);
				return nrect.Contains(pivot);
			} else if (node is PointObject) {
				var p = ((PointObject)node).TransformedPosition;
				var t = node.Parent.AsWidget.CalcTransitionToSpaceOf(canvas);
				return nrect.Contains(t * p);
			}
			return false;
		}
	}
}