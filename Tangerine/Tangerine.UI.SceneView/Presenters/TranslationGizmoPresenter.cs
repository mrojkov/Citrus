using System;
using System.Linq;
using System.Runtime.InteropServices;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class TranslationGizmoPresenter : CustomPresenter<Node3D>
	{
		public TranslationGizmoPresenter(SceneView sceneView)
		{
			sceneView.Frame.CompoundPostPresenter.Add(new DelegatePresenter<Widget>(RenderSelection));
		}

		private void RenderSelection(Widget canvas)
		{
			if (Document.Current.ExpositionMode || Document.Current.PreviewAnimation) {
				return;
			}
			var vp = GetCurrentViewport3D();
			if (vp == null || vp.Camera == null) {
				return;
			}
			canvas.PrepareRendererState();
			var nodes = Document.Current.SelectedNodes().Editable().OfType<Node3D>().ToList();
			if (nodes.Count == 0) {
				return;
			}
			Renderer.Flush();
			var oldCullMode = Renderer.CullMode;
			var oldWorld = Renderer.World;
			var oldView = Renderer.View;
			var oldProj = Renderer.Projection;
			var oldDepthState = Renderer.DepthState;
			Renderer.View = vp.Camera.View;
			Renderer.Projection = vp.TransformProjection(Renderer.Projection);
			Renderer.DepthState = DepthState.DepthReadWrite;
			Renderer.Clear(ClearOptions.DepthBuffer);
			foreach (var node in nodes) {
				RenderGizmo(node);
			}
			Renderer.World = oldWorld;
			Renderer.View = oldView;
			Renderer.Projection = oldProj;
			Renderer.CullMode = oldCullMode;
			Renderer.DepthState = oldDepthState;
		}

		Viewport3D GetCurrentViewport3D()
		{
			for (var p = Document.Current.Container; p != null; p = p.Parent) {
				if (p is Viewport3D) {
					return (Viewport3D)p;
				}
			}
			return null;
		}

		void RenderGizmo(Node3D node)
		{
			Renderer.Flush();
			Renderer.World = CalcGizmoTransform(node.GlobalTransform, 100);
			Renderer.CullMode = CullMode.Front;
			WidgetMaterial.Diffuse.Apply(0);
			gizmo.DrawIndexed(0, gizmo.Indices.Length);
		}

		Matrix44 CalcGizmoTransform(Matrix44 modelGlobalTransform, float gizmoRadiusInPixels)
		{
			var viewport = GetCurrentViewport3D();
			var camera = viewport.Camera;
			Vector3 scale, translation;
			Quaternion rotation;
			modelGlobalTransform.Decompose(out scale, out rotation, out translation);
			float distance = (camera.Position - translation).Length;
			var vfov = camera.FieldOfView / camera.AspectRatio;
			float s = gizmoRadiusInPixels * distance  * (2 * (float)Math.Tan(vfov / 2) / viewport.Height);
			return Matrix44.CreateRotation(rotation) * Matrix44.CreateScale(s, s, s) * Matrix44.CreateTranslation(translation);
		}

		static Mesh<VertexPositionColor> gizmo = CreateTranslationGizmo();

		static Mesh<VertexPositionColor> CreateTranslationGizmo()
		{
			var axisX = CreateArrow(Matrix44.CreateRotationZ(-Mathf.HalfPi), Color4.Red);
			var axisY = CreateArrow(Matrix44.Identity, Color4.Green);
			var axisZ = CreateArrow(Matrix44.CreateRotationX(Mathf.HalfPi), Color4.Blue);
			return MeshUtils.Combine(axisX, axisY, axisZ);
		}

		static Mesh<VertexPositionColor> CreateArrow(Matrix44 matrix, Color4 color)
		{
			const float tipHeight = 0.25f;
			const float tipRadius = 0.06f;
			const float poleRadius = 0.01f;
			var tip = MeshUtils.CreateCone(tipHeight, tipRadius, 20, color);
			TransformMesh(tip, Matrix44.CreateTranslation(0, 1 - tipHeight, 0));
			var pole = MeshUtils.CreateCylinder(1 - tipHeight - 0.1f, poleRadius, 6, color);
			TransformMesh(pole, Matrix44.CreateTranslation(0, 0.1f, 0));
			var arrow = MeshUtils.Combine(tip, pole);
			TransformMesh(arrow, matrix);
			return arrow;
		}

		static void TransformMesh(Mesh<VertexPositionColor> mesh, Matrix44 matrix)
		{
			MeshUtils.TransformVertices(mesh, (ref VertexPositionColor v) => v.Position = matrix.TransformVector(v.Position));
		}
	}
}