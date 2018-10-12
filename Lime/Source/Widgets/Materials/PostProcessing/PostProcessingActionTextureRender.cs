using System;

namespace Lime
{
	internal class PostProcessingActionTextureRender : PostProcessingAction
	{
		public override bool EnabledCheck(PostProcessingRenderObject ro) => true;

		public override void Do(PostProcessingRenderObject ro)
		{
			ro.FinalizeOffscreenRendering();

			ITexture texture;
			switch (ro.DebugViewMode) {
				case PostProcessingPresenter.DebugViewMode.None:
					texture = ro.ProcessedTexture;
					break;
				case PostProcessingPresenter.DebugViewMode.Original:
					texture = ro.SourceTextureBuffer?.Texture;
					break;
				case PostProcessingPresenter.DebugViewMode.Bloom:
					texture = ro.BloomBuffer?.Texture;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			if (texture != null) {
				ro.RenderTexture(texture);
			}
		}
	}
}
