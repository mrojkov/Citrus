namespace Lime
{
	internal class PostProcessingActionTextureBuilder : PostProcessingAction
	{
		public override bool EnabledCheck(PostProcessingRenderObject ro) => true;

		public override void Do(PostProcessingRenderObject ro)
		{
			ro.ProcessedViewport = Viewport.Default;
			ro.TextureSize = ro.Size;
			ro.CurrentBufferSize = (Vector2)ro.SourceTextureBuffer.Size;
			ro.ViewportSize = (Size)(ro.TextureSize * ro.SourceTextureScaling);
			ro.ProcessedTexture = ro.SourceTextureBuffer.Texture;
			ro.ProcessedUV1 = (Vector2)ro.ViewportSize / ro.CurrentBufferSize;
			if (!ro.SourceTextureBuffer.IsDirty) {
				return;
			}

			ro.PrepareOffscreenRendering(ro.Size);
			ro.SourceTextureBuffer.Texture.SetAsRenderTarget();
			try {
				Renderer.Viewport = new Viewport(0, 0, ro.ViewportSize.Width, ro.ViewportSize.Height);
				Renderer.Clear(Color4.Zero);
				Renderer.Transform2 = ro.LocalToWorldTransform.CalcInversed();
				ro.Objects.Render();
			} finally {
				ro.SourceTextureBuffer.Texture.RestoreRenderTarget();
				ro.FinalizeOffscreenRendering();
			}
			ro.SourceTextureBuffer.IsDirty = false;
			ro.MarkBuffersAsDirty = true;
		}
	}
}
