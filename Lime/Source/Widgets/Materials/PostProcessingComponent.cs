using Yuzu;

namespace Lime
{
	[TangerineRegisterComponent]
	[AllowedComponentOwnerTypes(typeof(Image))]
	public class PostProcessingComponent : PresenterComponent<PostProcessingPresenter>
	{
		private BlurMaterial blurMaterial;

		[YuzuMember]
		[TangerineGroup("Blur effect")]
		public float BlurRadius
		{
			get => blurMaterial.Radius;
			set => blurMaterial.Radius = value;
		}

		[YuzuMember]
		[TangerineGroup("Blur effect")]
		public float BlurTextureScaling
		{
			get => blurMaterial.TextureScaling;
			set => blurMaterial.TextureScaling = value;
		}

		[YuzuMember]
		[TangerineGroup("Blur effect")]
		public Color4 BlurBackgroundColor
		{
			get => blurMaterial.BackgroundColor;
			set => blurMaterial.BackgroundColor = value;
		}

		public PostProcessingComponent()
		{
			blurMaterial = new BlurMaterial();
			CustomPresenter.BlurMaterial = blurMaterial;
		}

		public override NodeComponent Clone()
		{
			var component = (PostProcessingComponent)base.Clone();
			component.blurMaterial = new BlurMaterial();
			component.CustomPresenter.BlurMaterial = blurMaterial;
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
		private Vector2 processedUV;
		private Size viewportSize;
		private Vector2 originalSize;
		private IMaterial material;
		private Blending blending;
		private ShaderId shader;

		private RenderTexture verticalBlurTexture;
		private RenderTexture gaussBlurTexture;
		private float renderedBlurRadius = float.NaN;
		private float renderedBlurTextureScaling = float.NaN;
		private Color4 renderedBlurBackgroundColor = Color4.Zero;

		public BlurMaterial BlurMaterial { get; set; }

		public void Render(Node node)
		{
			var image = (Image)node;
			if (image.Width <= 0 || image.Height <= 0) {
				return;
			}

			processedTexture = image.Texture;
			processedUV = Vector2.One;
			viewportSize = processedTexture.ImageSize;
			originalSize = (Vector2)viewportSize;
			ApplyBlur();
			FinalizeOffscreenRendering();
			RenderFinalImage(image);
		}

		private void ApplyBlur()
		{
			if (Mathf.Abs(BlurMaterial.Radius) <= Mathf.ZeroTolerance) {
				return;
			}
			viewportSize = (Size)((Vector2)viewportSize * BlurMaterial.TextureScaling);
			if (
				renderedBlurRadius == BlurMaterial.Radius &&
				renderedBlurTextureScaling == BlurMaterial.TextureScaling &&
				renderedBlurBackgroundColor == BlurMaterial.BackgroundColor
			) {
				processedTexture = gaussBlurTexture;
				processedUV = (Vector2)viewportSize / originalSize;
				return;
			}

			var textureSize = (Size)originalSize;
			if (gaussBlurTexture == null || gaussBlurTexture.ImageSize != textureSize) {
				verticalBlurTexture = new RenderTexture(textureSize.Width, textureSize.Height);
				gaussBlurTexture = new RenderTexture(textureSize.Width, textureSize.Height);
			}
			PrepareOffscreenRendering(originalSize);
			BlurMaterial.Resolution = (Vector2)viewportSize;
			BlurMaterial.Dir = Vector2.Down;
			RenderToTexture(verticalBlurTexture, processedTexture, BlurMaterial, Color4.White, BlurMaterial.BackgroundColor);
			processedUV = (Vector2)viewportSize / originalSize;
			BlurMaterial.Dir = Vector2.Right;
			RenderToTexture(gaussBlurTexture, verticalBlurTexture, BlurMaterial, Color4.White, BlurMaterial.BackgroundColor);

			processedTexture = gaussBlurTexture;
			renderedBlurRadius = BlurMaterial.Radius;
			renderedBlurTextureScaling = BlurMaterial.TextureScaling;
			renderedBlurBackgroundColor = BlurMaterial.BackgroundColor;
		}

		private void RenderToTexture(ITexture renderTargetTexture, ITexture sourceTexture, IMaterial material, Color4 color, Color4 backgroundColor)
		{
			if (processedViewport.Width != viewportSize.Width || processedViewport.Height != viewportSize.Height) {
				Renderer.Viewport = processedViewport = new Viewport(0, 0, viewportSize.Width, viewportSize.Height);
			}
			renderTargetTexture.SetAsRenderTarget();
			Renderer.Clear(backgroundColor);
			Renderer.DrawSprite(sourceTexture, null, material, color, Vector2.Zero, originalSize, Vector2.Zero, processedUV, Vector2.Zero, Vector2.Zero);
			renderTargetTexture.RestoreRenderTarget();
		}

		private void RenderFinalImage(Image image)
		{
			if (material == null || blending != image.GlobalBlending || shader != image.GlobalShader) {
				blending = image.GlobalBlending;
				shader = image.GlobalShader;
				material = WidgetMaterial.GetInstance(blending, shader, 1);
			}
			Renderer.Transform1 = image.LocalToWorldTransform;
			Renderer.DrawSprite(
				processedTexture,
				null,
				image.CustomMaterial ?? material,
				image.GlobalColor,
				image.ContentPosition,
				image.ContentSize,
				image.UV0 * processedUV,
				image.UV1 * processedUV,
				Vector2.Zero,
				Vector2.Zero
			);
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

		public IPresenter Clone() => new PostProcessingPresenter();
	}
}
