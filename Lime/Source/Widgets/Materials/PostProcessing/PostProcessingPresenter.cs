using System;

namespace Lime
{
	public class PostProcessingPresenter : IPresenter
	{
		private readonly RenderChain renderChain = new RenderChain();
		private readonly IMaterial blendingDefaultMaterial = WidgetMaterial.GetInstance(Blending.Inherited, ShaderId.Inherited, 1);
		private readonly IMaterial blendingAddMaterial = WidgetMaterial.GetInstance(Blending.Add, ShaderId.Inherited, 1);
		private IMaterial material;
		private Blending blending;
		private ShaderId shader;
		private SourceTextureBuffer sourceTextureBuffer;
		private BlurBuffer blurBuffer;
		private BloomBuffer bloomBuffer;

		public RenderObject GetRenderObject(Node node)
		{
			var component = node.Components.Get<PostProcessingComponent>();
			if (component == null) {
				throw new InvalidOperationException();
			}

			const int MaxBufferSize = 2048;
			const float BufferReserve = 1.2f;
			var widget = (Widget)node;
			var asImage = widget as Image;
			var ro = RenderObjectPool<PostProcessingRenderObject>.Acquire();
			Size bufferSize;
			Size bufferSizeWithReserve;
			if (asImage != null) {
				bufferSize = bufferSizeWithReserve = asImage.Texture.ImageSize;
			} else {
				bufferSize = (Size)widget.Size;
				bufferSize = new Size(Math.Min(bufferSize.Width, MaxBufferSize), Math.Min(bufferSize.Height, MaxBufferSize));
				bufferSizeWithReserve = (Size)(widget.Size * BufferReserve);
				bufferSizeWithReserve = new Size(Math.Min(bufferSizeWithReserve.Width, MaxBufferSize), Math.Min(bufferSizeWithReserve.Height, MaxBufferSize));

				component.GetOwnerRenderObjects(renderChain, ro.Objects);
				renderChain.Clear();
			}

			// TODO: Buffers pool
			// TODO: Recreate buffers when image.Texture was changed
			if (asImage == null && (sourceTextureBuffer == null || sourceTextureBuffer.Size.Width < bufferSize.Width || sourceTextureBuffer.Size.Height < bufferSize.Height)) {
				sourceTextureBuffer = new SourceTextureBuffer(bufferSizeWithReserve);
			}
			if (blurBuffer == null || blurBuffer.Size.Width < bufferSize.Width || blurBuffer.Size.Height < bufferSize.Height) {
				blurBuffer = new BlurBuffer(bufferSizeWithReserve);
			}
			if (bloomBuffer == null || bloomBuffer.Size.Width < bufferSize.Width || bloomBuffer.Size.Height < bufferSize.Height) {
				bloomBuffer = new BloomBuffer(bufferSizeWithReserve);
			}

			ro.Texture = asImage?.Texture;
			ro.Material = asImage != null ? GetImageMaterial(asImage) : blendingDefaultMaterial;
			ro.LocalToWorldTransform = widget.LocalToWorldTransform;
			ro.Position = asImage?.ContentPosition ?? Vector2.Zero;
			ro.Size = asImage?.ContentSize ?? widget.Size;
			ro.Color = widget.GlobalColor;
			ro.UV0 = asImage?.UV0 ?? Vector2.Zero;
			ro.UV1 = asImage?.UV1 ?? Vector2.One;
			ro.DebugViewMode = component.DebugViewMode;
			ro.SourceTextureBuffer = sourceTextureBuffer;
			ro.BlurBuffer = blurBuffer;
			ro.BlurMaterial = component.BlurMaterial;
			ro.BlurRadius = component.BlurRadius;
			ro.BlurTextureScaling = component.BlurTextureScaling;
			ro.BlurAlphaCorrection = component.BlurAlphaCorrection;
			ro.BlurBackgroundColor = component.BlurBackgroundColor;
			ro.BloomBuffer = bloomBuffer;
			ro.BloomMaterial = component.BloomMaterial;
			ro.BloomEnabled = component.BloomEnabled;
			ro.BloomStrength = component.BloomStrength;
			ro.BloomBrightThreshold = component.BloomBrightThreshold;
			ro.BloomGammaCorrection = component.BloomGammaCorrection;
			ro.BloomTextureScaling = component.BloomTextureScaling;
			ro.OverallImpactEnabled = component.OverallImpactEnabled;
			ro.OverallImpactColor = component.OverallImpactColor;
			ro.BlendingDefaultMaterial = blendingDefaultMaterial;
			ro.BlendingAddMaterial = blendingAddMaterial;
			return ro;
		}

		private IMaterial GetImageMaterial(Image image)
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

		// TODO: Fix HitTest of child nodes
		public bool PartialHitTest(Node node, ref HitTestArgs args) => DefaultPresenter.Instance.PartialHitTest(node, ref args);

		public IPresenter Clone() => new PostProcessingPresenter();

		public enum DebugViewMode
		{
			None,
			Bloom
		}

		internal class SourceTextureBuffer
		{
			private RenderTexture texture;

			public Size Size { get; }
			public RenderTexture Texture => texture ?? (texture = new RenderTexture(Size.Width, Size.Height));

			public SourceTextureBuffer(Size size)
			{
				Size = size;
			}
		}

		internal class BlurBuffer
		{
			private bool isDirty = true;
			private float radius = float.NaN;
			private float textureScaling = float.NaN;
			private float alphaCorrection = float.NaN;
			private Color4 backgroundColor = Color4.Zero;

			public RenderTexture FirstPassTexture { get; }
			public RenderTexture FinalTexture { get; }
			public Size Size { get; }

			public BlurBuffer(Size size)
			{
				Size = size;
				FirstPassTexture = new RenderTexture(size.Width, size.Height);
				FinalTexture = new RenderTexture(size.Width, size.Height);
			}

			public void MarkAsDirty() => isDirty = true;

			public bool EqualRenderParameters(float radius, float textureScaling, float alphaCorrection, Color4 backgroundColor) =>
				!isDirty && this.radius == radius && this.textureScaling == textureScaling && this.alphaCorrection == alphaCorrection && this.backgroundColor == backgroundColor;

			public void SetParameters(float radius, float textureScaling, float alphaCorrection, Color4 backgroundColor)
			{
				isDirty = false;
				this.radius = radius;
				this.textureScaling = textureScaling;
				this.alphaCorrection = alphaCorrection;
				this.backgroundColor = backgroundColor;
			}
		}

		internal class BloomBuffer
		{
			private bool isDirty = true;
			private float strength = float.NaN;
			private float brightThreshold = float.NaN;
			private Vector3 gammaCorrection = -Vector3.One;
			private float textureScaling = float.NaN;

			public RenderTexture BrightColorsTexture { get; }
			public RenderTexture FirstBlurPassTexture { get; }
			public RenderTexture FinalTexture { get; }
			public Size Size { get; }

			public BloomBuffer(Size size)
			{
				Size = size;
				BrightColorsTexture = new RenderTexture(size.Width, size.Height);
				FirstBlurPassTexture = new RenderTexture(size.Width, size.Height);
				FinalTexture = new RenderTexture(size.Width, size.Height);
			}

			public void MarkAsDirty() => isDirty = true;

			public bool EqualRenderParameters(float strength, float brightThreshold, Vector3 gammaCorrection, float textureScaling) =>
				!isDirty && this.strength == strength && this.brightThreshold == brightThreshold && this.gammaCorrection == gammaCorrection && this.textureScaling == textureScaling;

			public void SetParameters(float strength, float brightThreshold, Vector3 gammaCorrection, float textureScaling)
			{
				isDirty = false;
				this.strength = strength;
				this.brightThreshold = brightThreshold;
				this.gammaCorrection = gammaCorrection;
				this.textureScaling = textureScaling;
			}
		}
	}
}
