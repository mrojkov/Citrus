using System;

namespace Lime
{
	internal class PostProcessingActionTextureRender : PostProcessingAction
	{
		public override bool Enabled => true;

		public override void Do()
		{
			RenderObject.FinalizeOffscreenRendering();

			ITexture texture;
			switch (RenderObject.DebugViewMode) {
				case PostProcessingPresenter.DebugViewMode.None:
					texture = RenderObject.ProcessedTexture;
					break;
				case PostProcessingPresenter.DebugViewMode.Original:
					texture = RenderObject.SourceTextureBuffer?.Texture;
					break;
				case PostProcessingPresenter.DebugViewMode.Bloom:
					texture = RenderObject.BloomBuffer?.Texture;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			if (texture != null) {
				RenderObject.RenderTexture(texture);
			}
		}
	}
}
