using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using OpenTK.Graphics.ES20;

namespace Lime
{
	[ProtoContract]
	public class ModelViewport : Widget
	{
		private RenderChain chain = new RenderChain();
		private float frame;

		public ModelCamera Camera { get; set; }

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

		public override void AddToRenderChain(RenderChain chain)
		{
			if (!GloballyVisible) {
				return;
			}
			chain.Add(this, Layer);
		}

		private void RefreshChildren()
		{
			foreach (var n in Nodes) {
				var time = (int)(Frame * 1000 / 16);
				if (n.AnimationTime != time) {
					n.AnimationTime = time;
				}
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
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.CullFace);
			chain.RenderAndClear();
			GL.Disable(EnableCap.DepthTest);
			GL.Disable(EnableCap.CullFace);
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