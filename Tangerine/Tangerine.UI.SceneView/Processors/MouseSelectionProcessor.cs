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
							RefreshSelectedWidgets(rect);
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

		void RefreshSelectedWidgets(Rectangle rect)
		{
			var currentSelection = Document.Current.SelectedNodes().OfType<Widget>();
			var selectionQuad = rect.ToQuadrangle();
			var newSelection = Document.Current.Container.Nodes.OfType<Widget>().Where(w => selectionQuad.Intersects(w.CalcHullInSpaceOf(SceneView.Instance.Scene)));
			if (!currentSelection.SequenceEqual(newSelection)) {
				Core.Operations.ClearRowSelection.Perform();
				foreach (var node in newSelection) {
					Core.Operations.SelectNode.Perform(node);
				}
			}
		}
	}
}