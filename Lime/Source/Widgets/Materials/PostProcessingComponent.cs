using Yuzu;

namespace Lime
{
	[TangerineRegisterComponent]
	[AllowedComponentOwnerTypes(typeof(Image))]
	public class PostProcessingComponent : PresenterComponent<PostProcessingPresenter>
	{
		private const float MinimumTextureScaling = 0.01f;
		private const float MaximumTextureScaling = 1f;
		private const float MaximumBlurRadius = 10f;
		private const float MaximumAlphaCorrection = 10f;

		internal BlurMaterial BlurMaterial { get; private set; } = new BlurMaterial();
		internal BloomMaterial BloomMaterial { get; private set; } = new BloomMaterial();

		private float blurRadius = 1f;
		private float blurTextureScaling = 1f;
		private float blurAlphaCorrection = 1f;
		private float bloomStrength = 1f;
		private float bloomBrightThreshold = 1f;
		private float bloomAlphaCorrection = 1f;
		private float bloomTextureScaling = 0.0625f;

		[YuzuMember]
		[TangerineGroup("Blur effect")]
		public float BlurRadius
		{
			get => blurRadius;
			set => blurRadius = Mathf.Clamp(value, 0f, MaximumBlurRadius);
		}

		[YuzuMember]
		[TangerineGroup("Blur effect")]
		public float BlurTextureScaling
		{
			get => blurTextureScaling;
			set => blurTextureScaling = Mathf.Clamp(value, MinimumTextureScaling, MaximumTextureScaling);
		}

		[YuzuMember]
		[TangerineGroup("Blur effect")]
		public float BlurAlphaCorrection
		{
			get => blurAlphaCorrection;
			set => blurAlphaCorrection = Mathf.Clamp(value, 1f, MaximumAlphaCorrection);
		}

		[YuzuMember]
		[TangerineGroup("Blur effect")]
		public Color4 BlurBackgroundColor { get; set; } = new Color4(127, 127, 127, 0);

		[YuzuMember]
		[TangerineGroup("Bloom effect")]
		public bool BloomEnabled { get; set; }

		[YuzuMember]
		[TangerineGroup("Bloom effect")]
		public float BloomStrength
		{
			get => bloomStrength;
			set => bloomStrength = Mathf.Clamp(value, 0f, MaximumBlurRadius);
		}

		[YuzuMember]
		[TangerineGroup("Bloom effect")]
		public float BloomBrightThreshold
		{
			get => bloomBrightThreshold;
			set => bloomBrightThreshold = Mathf.Clamp(value, 0f, 1f);
		}

		[YuzuMember]
		[TangerineGroup("Bloom effect")]
		public float BloomAlphaCorrection
		{
			get => bloomAlphaCorrection;
			set => bloomAlphaCorrection = Mathf.Clamp(value, 1f, MaximumAlphaCorrection);
		}

		[YuzuMember]
		[TangerineGroup("Bloom effect")]
		public float BloomTextureScaling
		{
			get => bloomTextureScaling;
			set => bloomTextureScaling = Mathf.Clamp(value, MinimumTextureScaling, MaximumTextureScaling);
		}

		public PostProcessingComponent()
		{
			CustomPresenter.Component = this;
		}

		public override NodeComponent Clone()
		{
			var component = (PostProcessingComponent)base.Clone();
			component.BlurMaterial = (BlurMaterial)BlurMaterial.Clone();
			component.BloomMaterial = (BloomMaterial)BloomMaterial.Clone();
			component.CustomPresenter.Component = this;
			return component;
		}
	}

	public class PostProcessingPresenter : IPresenter
	{
		private bool wasOffscreenRenderingPrepared;
		private ScissorState savedScissorState;
		private Viewport savedViewport;
		private Matrix44 savedWorld;
		private Matrix44 savedView;
		private Matrix44 savedProj;
		private DepthState savedDepthState;
		private CullMode savedCullMode;
		private Matrix32 savedTransform2;

		private ITexture processedTexture;
		private Viewport processedViewport;
		private Vector2 processedUV1;
		private Size viewportSize;
		private Vector2 originalSize;
		private IMaterial material;
		private Blending blending;
		private ShaderId shader;

		private RenderTexture verticalBlurTexture;
		private RenderTexture gaussBlurTexture;
		private float renderedBlurRadius = float.NaN;
		private float renderedBlurTextureScaling = float.NaN;
		private float renderedBlurAlphaCorrection = float.NaN;
		private Color4 renderedBlurBackgroundColor = Color4.Zero;

		private RenderTexture brightColoredRegionsTexture;
		private RenderTexture verticalBlurOfBloomTexture;
		private RenderTexture gaussBlurOfBloomTexture;
		private readonly IMaterial blendingAddMaterial = WidgetMaterial.GetInstance(Blending.Add, ShaderId.Inherited, 1);

		public PostProcessingComponent Component { get; internal set; }
		public BlurMaterial BlurMaterial => Component.BlurMaterial;
		public BloomMaterial BloomMaterial => Component.BloomMaterial;

		public void Render(Node node)
		{
			var image = (Image)node;
			if (image.Width <= 0 || image.Height <= 0) {
				return;
			}

			processedTexture = image.Texture;
			processedUV1 = Vector2.One;
			viewportSize = processedTexture.ImageSize;
			originalSize = (Vector2)viewportSize;
			ApplyBlur();
			PrepareBloom();
			FinalizeOffscreenRendering();

			RenderTexture(image, processedTexture);
			ApplyBloom(image);
		}

		private void ApplyBlur()
		{
			if (Mathf.Abs(Component.BlurRadius) <= Mathf.ZeroTolerance) {
				return;
			}
			viewportSize = (Size)((Vector2)viewportSize * Component.BlurTextureScaling);
			if (
				renderedBlurRadius == Component.BlurRadius &&
				renderedBlurTextureScaling == Component.BlurTextureScaling &&
				renderedBlurAlphaCorrection == Component.BlurAlphaCorrection &&
				renderedBlurBackgroundColor == Component.BlurBackgroundColor
			) {
				processedTexture = gaussBlurTexture;
				processedUV1 = (Vector2)viewportSize / originalSize;
				return;
			}

			var textureSize = (Size)originalSize;
			if (gaussBlurTexture == null || gaussBlurTexture.ImageSize != textureSize) {
				verticalBlurTexture = new RenderTexture(textureSize.Width, textureSize.Height);
				gaussBlurTexture = new RenderTexture(textureSize.Width, textureSize.Height);
			}
			PrepareOffscreenRendering(originalSize);
			BlurMaterial.Radius = Component.BlurRadius;
			BlurMaterial.Step = new Vector2(Component.BlurTextureScaling / viewportSize.Width, Component.BlurTextureScaling / viewportSize.Height);
			BlurMaterial.Dir = Vector2.Down;
			BlurMaterial.AlphaCorrection = Component.BlurAlphaCorrection;
			RenderToTexture(verticalBlurTexture, processedTexture, BlurMaterial, Color4.White, Component.BlurBackgroundColor);
			processedUV1 = (Vector2)viewportSize / originalSize;
			BlurMaterial.Dir = Vector2.Right;
			RenderToTexture(gaussBlurTexture, verticalBlurTexture, BlurMaterial, Color4.White, Component.BlurBackgroundColor);

			processedTexture = gaussBlurTexture;
			renderedBlurRadius = Component.BlurRadius;
			renderedBlurTextureScaling = Component.BlurTextureScaling;
			renderedBlurAlphaCorrection = Component.BlurAlphaCorrection;
			renderedBlurBackgroundColor = Component.BlurBackgroundColor;
		}

		private void PrepareBloom()
		{
			if (!Component.BloomEnabled) {
				return;
			}

			var bloomViewportSize = (Size)(originalSize * Component.BloomTextureScaling);
			var textureSize = (Size)originalSize;
			if (gaussBlurOfBloomTexture == null || gaussBlurOfBloomTexture.ImageSize != textureSize) {
				brightColoredRegionsTexture = new RenderTexture(textureSize.Width, textureSize.Height);
				verticalBlurOfBloomTexture = new RenderTexture(textureSize.Width, textureSize.Height);
				gaussBlurOfBloomTexture = new RenderTexture(textureSize.Width, textureSize.Height);
			}
			PrepareOffscreenRendering(originalSize);
			BloomMaterial.BrightThreshold = Component.BloomBrightThreshold;
			RenderToTexture(brightColoredRegionsTexture, processedTexture, BloomMaterial, Color4.White, Color4.Zero, bloomViewportSize);
			var bloomUV1 = (Vector2)bloomViewportSize / originalSize;
			BlurMaterial.Radius = Component.BloomStrength;
			BlurMaterial.Step = new Vector2(Component.BloomTextureScaling / bloomViewportSize.Width, Component.BloomTextureScaling / bloomViewportSize.Height);
			BlurMaterial.Dir = Vector2.Down;
			BlurMaterial.AlphaCorrection = Component.BloomAlphaCorrection;
			RenderToTexture(verticalBlurOfBloomTexture, brightColoredRegionsTexture, BlurMaterial, Color4.White, Color4.Zero, bloomViewportSize, bloomUV1);
			BlurMaterial.Dir = Vector2.Right;
			RenderToTexture(gaussBlurOfBloomTexture, verticalBlurOfBloomTexture, BlurMaterial, Color4.White, Color4.Zero, bloomViewportSize, bloomUV1);

			// TODO: Save temps
		}

		private void ApplyBloom(Image image)
		{
			if (Component.BloomEnabled) {
				var bloomUV1 = originalSize * Component.BloomTextureScaling / originalSize;
				RenderTexture(image, gaussBlurOfBloomTexture, blendingAddMaterial, bloomUV1);
			}
		}

		private void RenderToTexture(ITexture renderTargetTexture, ITexture sourceTexture, IMaterial material, Color4 color, Color4 backgroundColor, Size? customViewportSize = null, Vector2? customUV1 = null)
		{
			var vs = customViewportSize ?? viewportSize;
			var uv1 = customUV1 ?? processedUV1;
			if (processedViewport.Width != vs.Width || processedViewport.Height != vs.Height) {
				Renderer.Viewport = processedViewport = new Viewport(0, 0, vs.Width, vs.Height);
			}
			renderTargetTexture.SetAsRenderTarget();
			Renderer.Clear(backgroundColor);
			Renderer.DrawSprite(sourceTexture, null, material, color, Vector2.Zero, originalSize, Vector2.Zero, uv1, Vector2.Zero, Vector2.Zero);
			renderTargetTexture.RestoreRenderTarget();
		}

		private void RenderTexture(Image image, ITexture texture, IMaterial customMaterial = null, Vector2? customUV1 = null)
		{
			var uv1 = customUV1 ?? processedUV1;
			Renderer.Transform1 = image.LocalToWorldTransform;
			Renderer.DrawSprite(
				texture,
				null,
				customMaterial ?? GetFinalImageMaterial(image),
				image.GlobalColor,
				image.ContentPosition,
				image.ContentSize,
				image.UV0 * uv1,
				image.UV1 * uv1,
				Vector2.Zero,
				Vector2.Zero
			);
		}

		private IMaterial GetFinalImageMaterial(Image image)
		{
			if (image.CustomMaterial != null) {
				return image.CustomMaterial;
			}
			if (material != null && blending == image.GlobalBlending && shader == image.GlobalShader) {
				return material;
			}
			blending = image.GlobalBlending;
			shader = image.GlobalShader;
			return material = WidgetMaterial.GetInstance(blending, shader, 1);
		}

		private void PrepareOffscreenRendering(Vector2 orthogonalProjection)
		{
			if (wasOffscreenRenderingPrepared) {
				return;
			}
			wasOffscreenRenderingPrepared = true;
			processedViewport = Viewport.Default;
			savedScissorState = Renderer.ScissorState;
			savedViewport = Renderer.Viewport;
			savedWorld = Renderer.World;
			savedView = Renderer.View;
			savedProj = Renderer.Projection;
			savedDepthState = Renderer.DepthState;
			savedCullMode = Renderer.CullMode;
			savedTransform2 = Renderer.Transform2;
			Renderer.ScissorState = ScissorState.ScissorDisabled;
			Renderer.World = Renderer.View = Matrix44.Identity;
			Renderer.SetOrthogonalProjection(Vector2.Zero, orthogonalProjection);
			Renderer.DepthState = DepthState.DepthDisabled;
			Renderer.CullMode = CullMode.None;
			Renderer.Transform2 = Matrix32.Identity;
			Renderer.Transform1 = Matrix32.Identity;
		}

		private void FinalizeOffscreenRendering()
		{
			if (!wasOffscreenRenderingPrepared) {
				return;
			}
			Renderer.Transform2 = savedTransform2;
			Renderer.Viewport = savedViewport;
			Renderer.World = savedWorld;
			Renderer.View = savedView;
			Renderer.Projection = savedProj;
			Renderer.ScissorState = savedScissorState;
			Renderer.DepthState = savedDepthState;
			Renderer.CullMode = savedCullMode;
			processedViewport = Viewport.Default;
			wasOffscreenRenderingPrepared = false;
		}

		public bool PartialHitTest(Node node, ref HitTestArgs args) => node.PartialHitTest(ref args);

		public IPresenter Clone()
		{
			var clone = new PostProcessingPresenter {
				Component = Component
			};
			return clone;
		}
	}
}
