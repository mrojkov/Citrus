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
		private const float MaximumGammaCorrection = 10f;

		internal BlurMaterial BlurMaterial { get; private set; } = new BlurMaterial();
		internal BloomMaterial BloomMaterial { get; private set; } = new BloomMaterial();

		private float blurRadius = 1f;
		private float blurTextureScaling = 1f;
		private float blurAlphaCorrection = 1f;
		private float bloomStrength = 1f;
		private float bloomBrightThreshold = 1f;
		private Vector3 bloomGammaCorrection = Vector3.One;
		private float bloomTextureScaling = 0.5f;

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
			set => blurAlphaCorrection = Mathf.Clamp(value, 1f, MaximumGammaCorrection);
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
		public Vector3 BloomGammaCorrection
		{
			get => bloomGammaCorrection;
			set => bloomGammaCorrection = new Vector3(
				Mathf.Clamp(value.X, 0f, MaximumGammaCorrection),
				Mathf.Clamp(value.Y, 0f, MaximumGammaCorrection),
				Mathf.Clamp(value.Y, 0f, MaximumGammaCorrection)
			);
		}

		[YuzuMember]
		[TangerineGroup("Bloom effect")]
		public float BloomTextureScaling
		{
			get => bloomTextureScaling;
			set => bloomTextureScaling = Mathf.Clamp(value, MinimumTextureScaling, MaximumTextureScaling);
		}

		public override NodeComponent Clone()
		{
			var component = (PostProcessingComponent)base.Clone();
			component.BlurMaterial = (BlurMaterial)BlurMaterial.Clone();
			component.BloomMaterial = (BloomMaterial)BloomMaterial.Clone();
			return component;
		}
	}
}
