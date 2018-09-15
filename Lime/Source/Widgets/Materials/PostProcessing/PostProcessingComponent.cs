using Yuzu;

namespace Lime
{
	// TODO: Attribute to exclude components with custom presenters and/or custom render chain builders
	[TangerineRegisterComponent]
	[AllowedComponentOwnerTypes(typeof(Widget))]
	public class PostProcessingComponent : NodeComponent
	{
		private const float MinimumTextureScaling = 0.01f;
		private const float MaximumTextureScaling = 1f;
		private const float MaximumHue = 1f;
		private const float MaximumSaturation = 1f;
		private const float MaximumLightness = 1f;
		private const float MaximumBlurRadius = 10f;
		private const float MaximumGammaCorrection = 10f;

		internal HSLMaterial HSLMaterial { get; private set; } = new HSLMaterial();
		internal BlurMaterial BlurMaterial { get; private set; } = new BlurMaterial();
		internal BloomMaterial BloomMaterial { get; private set; } = new BloomMaterial();

		// TODO: Solve promblem of storing and restoring savedPresenter&savedRenderChainBuilder
		private PostProcessingPresenter presenter = new PostProcessingPresenter();
		private PostProcessingRenderChainBuilder renderChainBuilder = new PostProcessingRenderChainBuilder();
		private Vector3 hsl = new Vector3(0, 1, 1);
		private float blurRadius = 1f;
		private float blurTextureScaling = 1f;
		private float blurAlphaCorrection = 1f;
		private float bloomStrength = 1f;
		private float bloomBrightThreshold = 1f;
		private Vector3 bloomGammaCorrection = Vector3.One;
		private float bloomTextureScaling = 0.5f;

		[TangerineGroup("1. Debug view")]
		[TangerineInspect]
		public PostProcessingPresenter.DebugViewMode DebugViewMode { get; set; } = PostProcessingPresenter.DebugViewMode.None;

		[YuzuMember]
		[TangerineGroup("2. HSL")]
		public bool HSLEnabled { get; set; }

		[YuzuMember]
		[TangerineGroup("2. HSL")]
		public float Hue
		{
			get => hsl.X * 360f;
			set => hsl.X = Mathf.Clamp(value / 360f, 0f, MaximumHue);
		}

		[YuzuMember]
		[TangerineGroup("2. HSL")]
		public float Saturation
		{
			get => hsl.Y * 100f;
			set => hsl.Y = Mathf.Clamp(value * 0.01f, 0f, MaximumSaturation);
		}

		[YuzuMember]
		[TangerineGroup("2. HSL")]
		public float Lightness
		{
			get => hsl.Z * 100f;
			set => hsl.Z = Mathf.Clamp(value * 0.01f, 0f, MaximumLightness);
		}

		public Vector3 HSL => hsl;

		[YuzuMember]
		[TangerineGroup("3. Blur effect")]
		public bool BlurEnabled { get; set; }

		[YuzuMember]
		[TangerineGroup("3. Blur effect")]
		public float BlurRadius
		{
			get => blurRadius;
			set => blurRadius = Mathf.Clamp(value, 0f, MaximumBlurRadius);
		}

		[YuzuMember]
		[TangerineGroup("3. Blur effect")]
		public float BlurTextureScaling
		{
			get => blurTextureScaling * 100f;
			set => blurTextureScaling = Mathf.Clamp(value * 0.01f, MinimumTextureScaling, MaximumTextureScaling);
		}

		[YuzuMember]
		[TangerineGroup("3. Blur effect")]
		public float BlurAlphaCorrection
		{
			get => blurAlphaCorrection;
			set => blurAlphaCorrection = Mathf.Clamp(value, 1f, MaximumGammaCorrection);
		}

		[YuzuMember]
		[TangerineGroup("3. Blur effect")]
		public Color4 BlurBackgroundColor { get; set; } = new Color4(127, 127, 127, 0);

		[YuzuMember]
		[TangerineGroup("4. Bloom effect")]
		public bool BloomEnabled { get; set; }

		[YuzuMember]
		[TangerineGroup("4. Bloom effect")]
		public float BloomStrength
		{
			get => bloomStrength;
			set => bloomStrength = Mathf.Clamp(value, 0f, MaximumBlurRadius);
		}

		[YuzuMember]
		[TangerineGroup("4. Bloom effect")]
		public float BloomBrightThreshold
		{
			get => bloomBrightThreshold * 100f;
			set => bloomBrightThreshold = Mathf.Clamp(value * 0.01f, 0f, 1f);
		}

		[YuzuMember]
		[TangerineGroup("4. Bloom effect")]
		public Vector3 BloomGammaCorrection
		{
			get => bloomGammaCorrection;
			set => bloomGammaCorrection = new Vector3(
				Mathf.Clamp(value.X, 0f, MaximumGammaCorrection),
				Mathf.Clamp(value.Y, 0f, MaximumGammaCorrection),
				Mathf.Clamp(value.Z, 0f, MaximumGammaCorrection)
			);
		}

		[YuzuMember]
		[TangerineGroup("4. Bloom effect")]
		public float BloomTextureScaling
		{
			get => bloomTextureScaling * 100f;
			set => bloomTextureScaling = Mathf.Clamp(value * 0.01f, MinimumTextureScaling, MaximumTextureScaling);
		}

		[YuzuMember]
		[TangerineGroup("4. Bloom effect")]
		public Color4 BloomColor { get; set; } = Color4.White;

		[YuzuMember]
		[TangerineGroup("5. Overall impact")]
		public bool OverallImpactEnabled { get; set; }

		[YuzuMember]
		[TangerineGroup("5. Overall impact")]
		public Color4 OverallImpactColor { get; set; } = Color4.White;

		public void GetOwnerRenderObjects(RenderChain renderChain, RenderObjectList roObjects)
		{
			Owner.RenderChainBuilder = Owner;
			Owner.Presenter = DefaultPresenter.Instance;
			Owner.AddToRenderChain(renderChain);
			renderChain.GetRenderObjects(roObjects);
			Owner.RenderChainBuilder = renderChainBuilder;
			Owner.Presenter = presenter;
		}

		protected override void OnOwnerChanged(Node oldOwner)
		{
			base.OnOwnerChanged(oldOwner);
			if (oldOwner != null) {
				oldOwner.Presenter = DefaultPresenter.Instance;
				oldOwner.RenderChainBuilder = oldOwner;
				renderChainBuilder.Owner = null;
			}
			if (Owner != null) {
				Owner.Presenter = presenter;
				Owner.RenderChainBuilder = renderChainBuilder;
				renderChainBuilder.Owner = Owner.AsWidget;
			}
		}

		public override NodeComponent Clone()
		{
			var clone = (PostProcessingComponent)base.Clone();
			clone.presenter = (PostProcessingPresenter)presenter.Clone();
			clone.renderChainBuilder = (PostProcessingRenderChainBuilder)renderChainBuilder.Clone(null);
			clone.BlurMaterial = (BlurMaterial)BlurMaterial.Clone();
			clone.BloomMaterial = (BloomMaterial)BloomMaterial.Clone();
			return clone;
		}
	}
}
