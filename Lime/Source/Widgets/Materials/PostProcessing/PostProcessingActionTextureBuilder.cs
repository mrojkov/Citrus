namespace Lime
{
	internal class PostProcessingActionTextureBuilder : PostProcessingAction
	{
		public override bool Enabled => true;

		public override void Do()
		{
			RenderObject.ProcessedViewport = Viewport.Default;
			RenderObject.TextureSize = RenderObject.Size;
			RenderObject.CurrentBufferSize = (Vector2)RenderObject.SourceTextureBuffer.Size;
			RenderObject.ViewportSize = (Size)(RenderObject.TextureSize * RenderObject.SourceTextureScaling);
			RenderObject.ProcessedTexture = RenderObject.SourceTextureBuffer.Texture;
			RenderObject.ProcessedUV1 = (Vector2)RenderObject.ViewportSize / RenderObject.CurrentBufferSize;
			if (!RenderObject.SourceTextureBuffer.IsDirty) {
				return;
			}

			RenderObject.PrepareOffscreenRendering(RenderObject.Size);
			RenderObject.SourceTextureBuffer.Texture.SetAsRenderTarget();
			try {
				Renderer.Viewport = new Viewport(0, 0, RenderObject.ViewportSize.Width, RenderObject.ViewportSize.Height);
				Renderer.Clear(Color4.Zero);
				Renderer.Transform2 = RenderObject.LocalToWorldTransform.CalcInversed();
				RenderObject.Objects.Render();
			} finally {
				RenderObject.SourceTextureBuffer.Texture.RestoreRenderTarget();
				RenderObject.FinalizeOffscreenRendering();
			}
			RenderObject.SourceTextureBuffer.IsDirty = false;
			RenderObject.MarkBuffersAsDirty = true;
		}
	}
}
