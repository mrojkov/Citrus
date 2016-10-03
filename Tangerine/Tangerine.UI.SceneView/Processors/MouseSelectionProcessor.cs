using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	class MouseSelectionProcessor : IProcessor
	{
		public IEnumerator<object> Loop()
		{
			var sv = SceneView.Instance;
			var input = sv.Input;
			while (true) {
				if (input.WasMousePressed()) {
					var rect = new Rectangle(sv.MousePosition, sv.MousePosition);
					var presenter = new DelegatePresenter<Widget>(w => {
						w.PrepareRendererState();
						var t = sv.Scene.CalcTransitionToSpaceOf(sv.Frame);
						Renderer.DrawRectOutline(rect.A * t, rect.B * t, SceneViewColors.MouseSelection, 1);
					});
					sv.Frame.CompoundPostPresenter.Add(presenter);
					input.CaptureMouse();
					var occasionalClick = true;
					while (input.IsMousePressed()) {
						rect.B = sv.MousePosition;
						occasionalClick &= (rect.B - rect.A).Length <= 5;
						if (!occasionalClick) {
							RefreshSelectedNodes(rect);
							CommonWindow.Current.Invalidate();
						}
						yield return null;
					}
					input.ReleaseMouse();
					sv.Frame.CompoundPostPresenter.Remove(presenter);
					CommonWindow.Current.Invalidate();
				}
				yield return null;
			}
		}

		void RefreshSelectedNodes(Rectangle rect)
		{
			var currentSelection = Document.Current.SelectedNodes().OfType<Widget>();
			var newSelection = Document.Current.Container.Nodes.Unlocked().Where(n => TestNode(rect, n));
			if (!currentSelection.SequenceEqual(newSelection)) {
				Core.Operations.ClearRowSelection.Perform();
				foreach (var node in newSelection) {
					Core.Operations.SelectNode.Perform(node);
				}
			}
		}

		bool TestNode(Rectangle rect, Node node)
		{
			var canvas = SceneView.Instance.Scene;
			if (node is Widget) {
				var widget = (Widget)node;
				var hull = widget.CalcHullInSpaceOf(canvas);
				for (int i = 0; i < 4; i++) {
					if (rect.Contains(hull[i])) {
						return true;
					}
				}
				var pivot = widget.CalcPositionInSpaceOf(canvas);
				return rect.Contains(pivot);
			} else if (node is PointObject) {
				var po = (PointObject)node;
				var p = ((Widget)node.Parent).CalcPositionInSpaceOf(canvas);
				throw new NotImplementedException();
			}
			return false;
		}
	}
}