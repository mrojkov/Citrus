using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lime
{
	internal class SDFRenderActionMain : SDFRenderAction
	{
		public override bool EnabledCheck(SDFRenderObject ro) => true;
		public override SDFRenderAction.Buffer GetTextureBuffer(SDFRenderObject ro) => ro.SDFBuffer;

		public override void Do(SDFRenderObject ro)
		{
			ro.PrepareOffscreenRendering(ro.Size);
			ro.SDFMaterial.Contrast = ro.Contrast;
			ro.RenderToTexture(ro.SDFBuffer.Texture, ro.ProcessedTexture, ro.SDFMaterial, Color4.White, Color4.Zero);
			ro.CurrentBufferSize = (Vector2)ro.SDFBuffer.Size;
			ro.ProcessedUV1 = (Vector2)ro.ViewportSize / ro.CurrentBufferSize;

			ro.SDFBuffer.SetRenderParameters(ro);
			ro.MarkBuffersAsDirty = true;
			ro.ProcessedTexture = ro.SDFBuffer.Texture;
		}

		internal new class Buffer : SDFRenderAction.Buffer
		{
			private float contrast;

			public Buffer(Size size) : base(size) { }

			public bool EqualRenderParameters(SDFRenderObject ro) =>
				!IsDirty &&
				contrast == ro.Contrast;

			public void SetRenderParameters(SDFRenderObject ro)
			{
				IsDirty = false;
				contrast = ro.Contrast;
			}
		}
	}
}
