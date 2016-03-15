using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
#if OPENGL
#if !MAC && !MONOMAC
using OpenTK.Graphics.ES20;
#else
using OpenTK.Graphics.OpenGL;
#endif
#endif

namespace Lime
{
	[ProtoContract]
	public class ModelViewport : Widget
	{
		private ModelCamera camera;
		private RenderChain chain = new RenderChain();
		private float frame;
		private List<ModelSubmesh> submeshes;
		private List<ModelMesh> modelMeshes;
		private bool zSortEnabled;
		public bool ZSortEnabled {
			get { return zSortEnabled; }
			set
			{
				if (value) {
					if (!zSortEnabled) {
						submeshes = new List<ModelSubmesh>();
						modelMeshes = new List<ModelMesh>();
					}
				} else {
					submeshes = null;
					modelMeshes = null;
				}
				zSortEnabled = value;
			}
		}
		private MeshCameraDistanceComparer meshCameraDistanceComparer = new MeshCameraDistanceComparer();

		public ModelCamera Camera
		{
			get { return camera; }
			set
			{
				camera = value;
				meshCameraDistanceComparer.Camera = camera;
				AdjustCameraAspectRatio();
			}
		}

		[ProtoMember(1)]
		public float Frame
		{
			get { return frame; }
			set
			{
				frame = value;
				RefreshChildren();
			}
		}

		protected override void OnSizeChanged(Vector2 sizeDelta)
		{
			base.OnSizeChanged(sizeDelta);
			AdjustCameraAspectRatio();
		}

		private void AdjustCameraAspectRatio()
		{
			if (camera != null) {
				camera.AspectRatio = Width / Height;
			}
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (!GloballyVisible) {
				return;
			}
			chain.Add(this, Layer);
		}

		private void RefreshChildren()
		{
			var time = (int)(Frame * 1000 / 16);
			foreach (var n in Nodes) {
				n.AnimationTime = time;
			}
		}

		internal class MeshCameraDistanceComparer : IComparer<ModelSubmesh>
		{
			public ModelCamera Camera;

			public int Compare(ModelSubmesh a, ModelSubmesh b)
			{
				float da = (a.Center * a.ModelMesh.GlobalTransform * Camera.View).Z;
				float db = (b.Center * b.ModelMesh.GlobalTransform * Camera.View).Z;
				if (da == db) {
					return 0;
				} else if (da > db) {
					return 1;
				} else {
					return -1;
				}
			}
		}

		public override void Render()
		{
			foreach (var node in Nodes) {
				node.AddToRenderChain(chain);
			}
			var oldZTestEnabled = Renderer.ZTestEnabled;
			var oldZWriteEnabled = Renderer.ZWriteEnabled;
			Renderer.Flush();
			Renderer.PushProjectionMatrix();
			Renderer.Projection = TransformProjection(Renderer.Projection);
#if OPENGL
			GL.Enable(EnableCap.CullFace);
#elif UNITY
			MaterialFactory.ThreeDimensionalRendering = true;
#endif
			if (ZSortEnabled) {
				for (int i = 0; i <= chain.MaxUsedLayer; i++) {
					Node node = chain.Layers[i];
					submeshes.Clear();
					modelMeshes.Clear();
					while (node != null) {
						node.PerformHitTest();
						var mm = node as ModelMesh;
						if (!mm.SkipRender) {
							modelMeshes.Add(mm);
							foreach (var sm in mm.Submeshes) {
								submeshes.Add(sm);
							}
						}
						node = node.NextToRender;
					}
					submeshes.Sort(meshCameraDistanceComparer);
					foreach (var mm in modelMeshes) {
						mm.PrepareToRender();
					}
					foreach (var sm in submeshes) {
						sm.Render();
					}
				}
				chain.Clear();
			} else {
				chain.RenderAndClear();
			}
#if OPENGL
			GL.Disable(EnableCap.CullFace);
#elif UNITY
			MaterialFactory.ThreeDimensionalRendering = false;
#endif
			Renderer.PopProjectionMatrix();
			Renderer.ZTestEnabled = oldZTestEnabled;
			Renderer.ZWriteEnabled = oldZWriteEnabled;
		}

		private Matrix44 TransformProjection(Matrix44 orthoProjection)
		{
			orthoProjection.M33 = 1; // Discard Z normalization, since it comes from the camera projection matrix
			orthoProjection.M43 = 0;
			return Camera.ViewProjection *
				// Transform from <-1, 1> normalized coordinates to the widget space
				Matrix44.CreateScale(new Vector3(Width / 2, -Height / 2, 1)) *
				Matrix44.CreateTranslation(new Vector3(Width / 2, Height / 2, 0)) *
				(Matrix44)LocalToWorldTransform * orthoProjection;
		}
	}
}