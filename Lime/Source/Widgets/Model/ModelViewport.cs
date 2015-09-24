using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
#if OPENGL
#if !MAC
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

		public ModelCamera Camera
		{
			get { return camera; }
			set
			{
				camera = value;
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

		public override void Render()
		{
			foreach (var node in Nodes) {
				node.AddToRenderChain(chain);
			}
			Renderer.Flush();
			Renderer.PushProjectionMatrix();
			Renderer.Projection = TransformProjection(Renderer.Projection);
#if OPENGL
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.CullFace);
#elif UNITY
			MaterialFactory.ThreeDimensionalRendering = true;
#endif
			chain.RenderAndClear();
#if OPENGL
			GL.Disable(EnableCap.DepthTest);
			GL.Disable(EnableCap.CullFace);
#elif UNITY
			MaterialFactory.ThreeDimensionalRendering = false;
#endif
			Renderer.PopProjectionMatrix();
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