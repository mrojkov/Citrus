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
		private PostProcessingActionColorCorrection.Buffer colorCorrectionBuffer;
		private PostProcessingActionBlur.Buffer blurBuffer;
		private PostProcessingActionBloom.Buffer bloomBuffer;
		private PostProcessingActionDistortion.Buffer distortionBuffer;
		private PostProcessingActionSharpen.Buffer sharpenBuffer;
		private PostProcessingActionNoise.Buffer noiseBuffer;
		private PostProcessingActionFXAA.Buffer fxaaBuffer;

		public PostProcessingPresenter()
		{
			postProcessingActions = new PostProcessingAction[] {
				new PostProcessingActionTextureBuilder(),
				new PostProcessingActionOverallImpact(),
				new PostProcessingActionColorCorrection(),
				new PostProcessingActionBlur(),
				new PostProcessingActionBloom(),
				new PostProcessingActionSharpen(),
				new PostProcessingActionDistortion(),
				new PostProcessingActionFXAA(),
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
			try {
				component.GetOwnerRenderObjects(renderChain, ro.Objects);
			} finally {
				renderChain.Clear();
			}

			var bufferSize = component.TextureSizeLimit;
			var widget = (Widget)node;
			var sourceTextureScaling = Mathf.Min(bufferSize.Width / widget.Width, bufferSize.Height / widget.Height);
			if (sourceTextureBuffer?.Size != bufferSize) {
				sourceTextureBuffer = new PostProcessingAction.Buffer(bufferSize);
			}
			if (component.HSLEnabled && colorCorrectionBuffer?.Size != bufferSize) {
				colorCorrectionBuffer = new PostProcessingActionColorCorrection.Buffer(bufferSize);
			}
			if (component.BlurEnabled && blurBuffer?.Size != bufferSize) {
				blurBuffer = new PostProcessingActionBlur.Buffer(bufferSize);
			}
			if (component.BloomEnabled && bloomBuffer?.Size != bufferSize) {
				bloomBuffer = new PostProcessingActionBloom.Buffer(bufferSize);
			}
			if (component.DistortionEnabled && distortionBuffer?.Size != bufferSize) {
				distortionBuffer = new PostProcessingActionDistortion.Buffer(bufferSize);
			}
			if (component.SharpenEnabled && sharpenBuffer?.Size != bufferSize) {
				sharpenBuffer = new PostProcessingActionSharpen.Buffer(bufferSize);
			}
			if (component.NoiseEnabled && noiseBuffer?.Size != bufferSize) {
				noiseBuffer = new PostProcessingActionNoise.Buffer(bufferSize);
			}
			if (component.FXAAEnabled && fxaaBuffer?.Size != bufferSize) {
				fxaaBuffer = new PostProcessingActionFXAA.Buffer(bufferSize);
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
			ro.ColorCorrectionBuffer = colorCorrectionBuffer;
			ro.ColorCorrectionMaterial = component.ColorCorrectionMaterial;
			ro.HSLEnabled = component.HSLEnabled;
			ro.HSL = new Vector3(component.HSL.X * (1f / 360f), component.HSL.Y * 0.01f + 1f, component.HSL.Z * 0.01f + 1f);
			ro.Brightness = component.Brightness * 0.01f;
			ro.Contrast = component.Contrast * 0.01f + 1f;
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
			ro.DistortionBuffer = distortionBuffer;
			ro.DistortionMaterial = component.DistortionMaterial;
			ro.DistortionEnabled = component.DistortionEnabled;
			ro.DistortionBarrelPincushion = component.DistortionBarrelPincushion * 0.01f;
			ro.DistortionChromaticAberration = component.DistortionChromaticAberration * 0.01f;
			ro.DistortionRed = component.DistortionRed * 0.01f;
			ro.DistortionGreen = component.DistortionGreen * 0.01f;
			ro.DistortionBlue = component.DistortionBlue * 0.01f;
			ro.SharpenBuffer = sharpenBuffer;
			ro.SharpenMaterial = component.SharpenMaterial;
			ro.SharpenEnabled = component.SharpenEnabled;
			ro.SharpenStrength = component.SharpenStrength;
			ro.SharpenLimit = component.SharpenLimit;
			ro.SharpenStep = component.SharpenStep;
			ro.NoiseBuffer = noiseBuffer;
			ro.NoiseMaterial = component.NoiseMaterial;
			ro.NoiseEnabled = component.NoiseEnabled && component.NoiseTexture != null && !component.NoiseTexture.IsStubTexture;
			ro.NoiseBrightThreshold = component.NoiseBrightThreshold * 0.01f;
			ro.NoiseDarkThreshold = component.NoiseDarkThreshold * 0.01f;
			ro.NoiseSoftLight = component.NoiseSoftLight * 0.01f;
			ro.NoiseOffset = component.NoiseOffset;
			ro.NoiseScale = component.NoiseScale;
			ro.NoiseTexture = component.NoiseTexture;
			ro.FXAABuffer = fxaaBuffer;
			ro.FXAAMaterial = component.FXAAMaterial;
			ro.FXAAEnabled = component.FXAAEnabled;
			ro.FXAALumaTreshold = component.FXAALumaTreshold;
			ro.FXAAMulReduce = component.FXAAMulReduce;
			ro.FXAAMinReduce = component.FXAAMinReduce;
			ro.FXAAMaxSpan = component.FXAAMaxSpan;
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
			if (component.CustomMaterial != null) {
				return component.CustomMaterial;
			}
			if (material != null && blending == widget.GlobalBlending && shader == widget.GlobalShader && opaque == component.OpagueRendering) {
				return material;
			}
			blending = widget.GlobalBlending;
			shader = widget.GlobalShader;
			opaque = component.OpagueRendering;
			var isOpaqueRendering = opaque && blending == Blending.Inherited;
			return material = WidgetMaterial.GetInstance(!isOpaqueRendering ? blending : Blending.Opaque, shader, 1);
		}

		public bool PartialHitTest(Node node, ref HitTestArgs args)
		{
			var widget = (Widget)node;
			if (!widget.BoundingRectHitTest(args.Point)) {
				return false;
			}
			var savedLayer = renderChain.CurrentLayer;
			try {
				renderChain.CurrentLayer = widget.Layer;
				for (var child = widget.FirstChild; child != null; child = child.NextSibling) {
					child.RenderChainBuilder?.AddToRenderChain(renderChain);
				}
				return renderChain.HitTest(ref args) || SelfPartialHitTest(widget, ref args);
			} finally {
				renderChain.Clear();
				renderChain.CurrentLayer = savedLayer;
			}
		}

		public bool SelfPartialHitTest(Widget widget, ref HitTestArgs args)
		{
			Node targetNode;
			for (targetNode = widget; targetNode != null; targetNode = targetNode.Parent) {
				var method = targetNode.AsWidget?.HitTestMethod ?? HitTestMethod.Contents;
				if (method == HitTestMethod.Skip || targetNode != widget && method == HitTestMethod.BoundingRect) {
					return false;
				}
				if (targetNode.HitTestTarget) {
					break;
				}
			}
			if (targetNode == null) {
				return false;
			}
			if (
				widget.HitTestMethod == HitTestMethod.BoundingRect && widget.BoundingRectHitTest(args.Point) ||
				widget.HitTestMethod == HitTestMethod.Contents && widget.PartialHitTestByContents(ref args)
			) {
				args.Node = targetNode;
				return true;
			}
			return false;
		}

		public enum DebugViewMode
		{
			None,
			Original,
			Bloom
		}
	}
}
