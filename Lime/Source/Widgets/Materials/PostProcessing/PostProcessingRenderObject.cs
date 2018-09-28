namespace Lime
{
	internal class PostProcessingRenderObject : RenderObject
	{
		private bool wasOffscreenRenderingPrepared;

		internal ITexture ProcessedTexture;
		internal Viewport ProcessedViewport;
		internal Vector2 ProcessedUV1;
		internal Size ViewportSize;
		internal Vector2 TextureSize;
		public Vector2 CurrentBufferSize;

		public readonly RenderObjectList Objects = new RenderObjectList();
		public PostProcessingAction[] PostProcessingActions;
		public IMaterial Material;
		public Matrix32 LocalToWorldTransform;
		public Vector2 Position;
		public Vector2 Size;
		public Color4 Color;
		public Vector2 UV0;
		public Vector2 UV1;
		public PostProcessingPresenter.DebugViewMode DebugViewMode;
		public bool MarkBuffersAsDirty;
		public PostProcessingAction.Buffer SourceTextureBuffer;
		public float SourceTextureScaling;
		public PostProcessingAction.Buffer FirstTemporaryBuffer;
		public PostProcessingAction.Buffer SecondTemporaryBuffer;
		public PostProcessingActionHSL.Buffer HSLBuffer;
		public HSLMaterial HSLMaterial;
		public bool HSLEnabled;
		public Vector3 HSL;
		public PostProcessingActionBlur.Buffer BlurBuffer;
		public BlurMaterial BlurMaterial;
		public bool BlurEnabled;
		public float BlurRadius;
		public float BlurTextureScaling;
		public float BlurAlphaCorrection;
		public Color4 BlurBackgroundColor;
		public PostProcessingActionBloom.Buffer BloomBuffer;
		public BloomMaterial BloomMaterial;
		public bool BloomEnabled;
		public float BloomStrength;
		public float BloomBrightThreshold;
		public Vector3 BloomGammaCorrection;
		public float BloomTextureScaling;
		public Color4 BloomColor;
		public PostProcessingActionNoise.Buffer NoiseBuffer;
		public bool NoiseEnabled;
		public float NoiseStrength;
		public ITexture NoiseTexture;
		public SoftLightMaterial SoftLightMaterial;
		public bool OverallImpactEnabled;
		public Color4 OverallImpactColor;
		public VignetteMaterial VignetteMaterial;
		public Texture2D TransparentTexture;
		public bool VignetteEnabled;
		public float VignetteRadius;
		public float VignetteSoftness;
		public Vector2 VignetteScale;
		public Vector2 VignettePivot;
		public Color4 VignetteColor;
		public IMaterial DefaultMaterial;
		public IMaterial BlendingAddMaterial;

		public bool IsNotDebugViewMode => DebugViewMode == PostProcessingPresenter.DebugViewMode.None;

		protected override void OnRelease()
		{
			ProcessedTexture = null;
			Objects.Clear();
			Material = null;
			SourceTextureBuffer = null;
			FirstTemporaryBuffer = null;
			SecondTemporaryBuffer = null;
			HSLBuffer = null;
			HSLMaterial = null;
			BlurBuffer = null;
			BlurMaterial = null;
			BloomBuffer = null;
			BloomMaterial = null;
			NoiseBuffer = null;
			NoiseTexture = null;
			SoftLightMaterial = null;
			VignetteMaterial = null;
			TransparentTexture = null;
			DefaultMaterial = null;
			BlendingAddMaterial = null;
		}

		public override void Render()
		{
			wasOffscreenRenderingPrepared = false;
			try {
				if (!IsNotDebugViewMode) {
					MarkBuffersAsDirty = true;
				}
				foreach (var action in PostProcessingActions) {
					action.RenderObject = this;
					if (MarkBuffersAsDirty) {
						action.TextureBuffer?.MarkAsDirty();
					}
					if (action.Enabled) {
						action.Do();
						if (action.TextureBuffer != null) {
							action.TextureBuffer.WasApplied = true;
						}
					} else if (action.TextureBuffer?.WasApplied ?? false) {
						action.TextureBuffer.WasApplied = false;
						action.TextureBuffer.MarkAsDirty();
						MarkBuffersAsDirty = true;
					}
					action.RenderObject = null;
				}
			} finally {
				PostProcessingActions = null;
				FinalizeOffscreenRendering();
			}
		}

		internal void RenderToTexture(ITexture renderTargetTexture, ITexture sourceTexture, IMaterial material, Color4 color, Color4 backgroundColor, Size? customViewportSize = null, Vector2? customUV1 = null)
		{
			var vs = customViewportSize ?? ViewportSize;
			var uv1 = customUV1 ?? ProcessedUV1;
			if (ProcessedViewport.Width != vs.Width || ProcessedViewport.Height != vs.Height) {
				Renderer.Viewport = ProcessedViewport = new Viewport(0, 0, vs.Width, vs.Height);
			}
			renderTargetTexture.SetAsRenderTarget();
			try {
				Renderer.Clear(backgroundColor);
				Renderer.DrawSprite(sourceTexture, null, material, color, Vector2.Zero, TextureSize, Vector2.Zero, uv1, Vector2.Zero, Vector2.Zero);
			} finally {
				renderTargetTexture.RestoreRenderTarget();
			}
		}

		internal void RenderTexture(ITexture texture, IMaterial customMaterial = null, Vector2? customUV1 = null)
		{
			var uv1 = customUV1 ?? ProcessedUV1;
			Renderer.Transform1 = LocalToWorldTransform;
			Renderer.DrawSprite(texture, null, customMaterial ?? Material, Color, Position, Size, UV0 * uv1, UV1 * uv1, Vector2.Zero, Vector2.Zero);
		}

		internal void PrepareOffscreenRendering(Vector2 orthogonalProjection)
		{
			if (wasOffscreenRenderingPrepared) {
				return;
			}
			Renderer.PushState(
				RenderState.Viewport |
				RenderState.World |
				RenderState.View |
				RenderState.Projection |
				RenderState.DepthState |
				RenderState.ScissorState |
				RenderState.CullMode |
				RenderState.Transform2
			);
			wasOffscreenRenderingPrepared = true;
			Renderer.ScissorState = ScissorState.ScissorDisabled;
			Renderer.World = Renderer.View = Matrix44.Identity;
			Renderer.SetOrthogonalProjection(Vector2.Zero, orthogonalProjection);
			Renderer.DepthState = DepthState.DepthDisabled;
			Renderer.CullMode = CullMode.None;
			Renderer.Transform2 = Matrix32.Identity;
			Renderer.Transform1 = Matrix32.Identity;
		}

		internal void FinalizeOffscreenRendering()
		{
			if (!wasOffscreenRenderingPrepared) {
				return;
			}
			Renderer.PopState();
			wasOffscreenRenderingPrepared = false;
		}
	}
}
