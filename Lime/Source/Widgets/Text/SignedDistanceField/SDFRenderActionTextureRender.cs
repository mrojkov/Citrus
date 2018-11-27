using System;

namespace Lime
{
	internal class SDFRenderActionTextureRender : SDFRenderAction
	{
		public override bool EnabledCheck(SDFRenderObject ro) => true;

		public override void Do(SDFRenderObject ro)
		{
			ro.FinalizeOffscreenRendering();
			ro.RenderTexture(ro.ProcessedTexture);
		}
	}
}
