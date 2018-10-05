using System;

namespace Lime
{
	public class PostProcessingPresenter : IPresenter
	{
		private readonly PostProcessingAction[] postProcessingActions;
		private readonly RenderChain renderChain = new RenderChain();
		private readonly IMaterial alphaDiffuseMaterial = WidgetMaterial.GetInstance(Blending.Alpha, ShaderId.Diffuse, 1);
		private readonly IMaterial addDiffuseMaterial = WidgetMaterial.GetInstance(Blending.Add, ShaderId.Diffuse, 1);
		private readonly IMaterial opaqueDiffuseMaterial = WidgetMaterial.GetInstance(Blending.Opaque, ShaderId.Diffuse, 1);
		private readonly Texture2D transparentTexture;
		private IMaterial material;
		private Blending blending;
		private ShaderId shader;
		private bool opaque;
		private PostProcessingAction.Buffer sourceTextureBuffer;
		private PostProcessingAction.Buffer firstTemporaryBuffer;
		private PostProcessingAction.Buffer secondTemporaryBuffer;
		private PostProcessingActionHSL.Buffer hslBuffer;
		private PostProcessingActionBlur.Buffer blurBuffer;
		private PostProcessingActionBloom.Buffer bloomBuffer;
		private PostProcessingActionNoise.Buffer noiseBuffer;

		public PostProcessingPresenter()
		{
			postProcessingActions = new PostProcessingAction[] {
				new PostProcessingActionTextureBuilder(),
				new PostProcessingActionOverallImpact(),
				new PostProcessingActionHSL(),
				new PostProcessingActionBlur(),
				new PostProcessingActionBloom(),
				new PostProcessingActionNoise(),
				new PostProcessingActionTextureRender(),
				new PostProcessingActionVignette()
			};
			transparentTexture = new Texture2D();
			transparentTexture.LoadImage(new[] { Color4.Zero }, 1, 1);
		}

		public RenderObject GetRenderObject(Node node)
		{
			var component = node.Components.Get<PostProcessingComponent>();
			if (component == null) {
				throw new InvalidOperationException();
			}

			var ro = RenderObjectPool<PostProcessingRenderObject>.Acquire();
			component.GetOwnerRenderObjects(renderChain, ro.Objects);
			renderChain.Clear();

			var bufferSize = component.TextureSizeLimit;
			var widget = (Widget)node;
			var sourceTextureScaling = Mathf.Min(bufferSize.Width / widget.Width, bufferSize.Height / widget.Height);
			if (sourceTextureBuffer?.Size != bufferSize) {
				sourceTextureBuffer = new PostProcessingAction.Buffer(bufferSize);
			}
			if (component.HSLEnabled && hslBuffer?.Size != bufferSize) {
				hslBuffer = new PostProcessingActionHSL.Buffer(bufferSize);
			}
			if (component.BlurEnabled && blurBuffer?.Size != bufferSize) {
				blurBuffer = new PostProcessingActionBlur.Buffer(bufferSize);
			}
			if (component.BloomEnabled && bloomBuffer?.Size != bufferSize) {
				bloomBuffer = new PostProcessingActionBloom.Buffer(bufferSize);
			}
			if (component.NoiseEnabled && noiseBuffer?.Size != bufferSize) {
				noiseBuffer = new PostProcessingActionNoise.Buffer(bufferSize);
			}
			if ((component.BlurEnabled || component.BloomEnabled) && firstTemporaryBuffer?.Size != bufferSize) {
				firstTemporaryBuffer = new PostProcessingAction.Buffer(bufferSize);
				secondTemporaryBuffer = new PostProcessingAction.Buffer(bufferSize);
			}
			if (component.RequiredRefreshSource) {
				sourceTextureBuffer?.MarkAsDirty();
				component.RequiredRefreshSource = false;
			}

			ro.PostProcessingActions = postProcessingActions;
			ro.Material = GetMaterial(widget, component);
			ro.LocalToWorldTransform = widget.LocalToWorldTransform;
			ro.Position = widget.ContentPosition;
			ro.Size = widget.ContentSize;
			ro.Color = widget.GlobalColor;
			// TODO: Custom UV?
			ro.UV0 = Vector2.Zero;
			ro.UV1 = Vector2.One;
			ro.DebugViewMode = component.DebugViewMode;
			ro.MarkBuffersAsDirty = false;
			ro.SourceTextureBuffer = sourceTextureBuffer;
			ro.SourceTextureScaling = sourceTextureScaling;
			ro.OpagueRendering = component.OpagueRendering;
			ro.FirstTemporaryBuffer = firstTemporaryBuffer;
			ro.SecondTemporaryBuffer = secondTemporaryBuffer;
			ro.HSLBuffer = hslBuffer;
			ro.HSLMaterial = component.HSLMaterial;
			ro.HSLEnabled = component.HSLEnabled;
			ro.HSL = new Vector3(component.HSL.X * (1f / 360f), component.HSL.Y * 0.01f + 1f, component.HSL.Z * 0.01f + 1f);
			ro.BlurBuffer = blurBuffer;
			ro.BlurMaterial = component.BlurMaterial;
			ro.BlurEnabled = component.BlurEnabled;
			ro.BlurRadius = component.BlurRadius;
			ro.BlurShader = component.BlurShader;
			ro.BlurTextureScaling = component.BlurTextureScaling * 0.01f;
			ro.BlurAlphaCorrection = component.BlurAlphaCorrection;
			ro.BlurBackgroundColor = component.BlurBackgroundColor;
			ro.BloomBuffer = bloomBuffer;
			ro.BloomMaterial = component.BloomMaterial;
			ro.BloomEnabled = component.BloomEnabled;
			ro.BloomStrength = component.BloomStrength;
			ro.BloomShaderId = component.BloomShaderId;
			ro.BloomBrightThreshold = component.BloomBrightThreshold * 0.01f;
			ro.BloomGammaCorrection = component.BloomGammaCorrection;
			ro.BloomTextureScaling = component.BloomTextureScaling * 0.01f;
			ro.BloomColor = component.BloomColor;
			ro.NoiseBuffer = noiseBuffer;
			ro.NoiseEnabled = component.NoiseEnabled && component.NoiseTexture != null && !component.NoiseTexture.IsStubTexture;
			ro.NoiseBrightThreshold = component.NoiseBrightThreshold * 0.01f;
			ro.NoiseDarkThreshold = component.NoiseDarkThreshold * 0.01f;
			ro.NoiseSoftLight = component.NoiseSoftLight * 0.01f;
			ro.NoiseOffset = component.NoiseOffset;
			ro.NoiseScale = component.NoiseScale;
			ro.NoiseTexture = component.NoiseTexture;
			ro.NoiseMaterial = component.NoiseMaterial;
			ro.OverallImpactEnabled = component.OverallImpactEnabled;
			ro.OverallImpactColor = component.OverallImpactColor;
			ro.VignetteMaterial = component.VignetteMaterial;
			ro.TransparentTexture = transparentTexture;
			ro.VignetteEnabled = component.VignetteEnabled;
			ro.VignetteRadius = component.VignetteRadius * 0.01f;
			ro.VignetteSoftness = component.VignetteSoftness * 0.01f;
			ro.VignetteScale = component.VignetteScale;
			ro.VignettePivot = component.VignettePivot;
			ro.VignetteColor = component.VignetteColor;
			ro.AlphaDiffuseMaterial = alphaDiffuseMaterial;
			ro.AddDiffuseMaterial = addDiffuseMaterial;
			ro.OpaqueDiffuseMaterial = opaqueDiffuseMaterial;
			return ro;
		}

		private IMaterial GetMaterial(Widget widget, PostProcessingComponent component)
		{
			if (material != null && blending == widget.GlobalBlending && shader == widget.GlobalShader && opaque == component.OpagueRendering) {
				return material;
			}
			blending = widget.GlobalBlending;
			shader = widget.GlobalShader;
			opaque = component.OpagueRendering;
			var isOpaqueRendering = opaque && blending == Blending.Inherited;
			return material = WidgetMaterial.GetInstance(!isOpaqueRendering ? blending : Blending.Opaque, shader, 1);
		}

		// TODO: Fix HitTest of child nodes
		public bool PartialHitTest(Node node, ref HitTestArgs args) => DefaultPresenter.Instance.PartialHitTest(node, ref args);

		public IPresenter Clone() => new PostProcessingPresenter();

		public enum DebugViewMode
		{
			None,
			Original,
			Bloom
		}
	}
}
