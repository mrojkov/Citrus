using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	class Bone3DPresenter : CustomPresenter<Node3D>
	{
		public Bone3DPresenter(SceneView sceneView)
		{
			sceneView.Frame.CompoundPostPresenter.Add(new DelegatePresenter<Widget>(RenderSelection));
		}

		private static void RenderSelection(Widget canvas)
		{
			if (Document.Current.ExpositionMode) {
				return;
			}
			canvas.PrepareRendererState();
			var nodes = Document.Current.SelectedNodes().Editable().OfType<Node3D>().ToList();
			foreach (var node in nodes) {
				RenderSkeleton(node, node);
			}
		}

		private static void RenderSkeleton(Node node, Node3D root)
		{
			if (node is Mesh3D) {
				foreach (var sm in (node as Mesh3D).Submeshes) {
					foreach (var bone in sm.Bones) {
						Renderer.Flush();
						var viewportToSceneFrame = GetCurrentViewport3D().CalcTransitionToSpaceOf(SceneView.Instance.Frame);
						var a = (Vector2)GetCurrentViewport3D().WorldToViewportPoint(bone.GlobalTransform.Translation) * viewportToSceneFrame;
						var b = (Vector2)GetCurrentViewport3D().WorldToViewportPoint((bone.Parent as Node3D).GlobalTransform.Translation) * viewportToSceneFrame;
						Renderer.DrawRound(a, 3, 10, Color4.Green);
						Renderer.DrawRound(b, 3, 10, Color4.Green);
						Renderer.DrawLine(a, b, Color4.Yellow);
					}
				}
			}
			foreach (var child in node.Nodes) {
				RenderSkeleton(child, root);
			}
		}

		private static Viewport3D GetCurrentViewport3D()
		{
			for (var p = Document.Current.Container; p != null; p = p.Parent) {
				if (p is Viewport3D) {
					return (Viewport3D)p;
				}
			}
			return null;
		}
	}
}
