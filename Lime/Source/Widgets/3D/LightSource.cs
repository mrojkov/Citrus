using Yuzu;

namespace Lime
{
	// TODO: We need to tweak shadow quality settings
	public enum ShadowMapTextureQuality
	{
		Low,
		Medium,
		High,
		Ultra
	}

	public class LightSource : Node3D
	{
		public IntVector2 ShadowMapSize
		{ get { return lightViewport.Bounds.Size; } }

		public ITexture ShadowMap
		{ get { return depthBufferRenderer?.Texture ?? null; } }

		public Matrix44 ViewProjection
		{ get { return lightViewProjection; } }

		public Matrix44 View
		{ get { return lightView; } }

		[YuzuMember]
		[TangerineGroup("Shadow")]
		[TangerineKeyframeColor(23)]
		public ShadowMapTextureQuality ShadowMapQuality
		{
			get { return shadowMapQuality; }
			set
			{
				if (shadowMapQuality != value) {
					switch (value) {
						case ShadowMapTextureQuality.Low:
							depthTextureSize = 512;
							break;
						case ShadowMapTextureQuality.Medium:
							depthTextureSize = 1024;
							break;
						case ShadowMapTextureQuality.High:
							depthTextureSize = 1024;
							break;
						case ShadowMapTextureQuality.Ultra:
							depthTextureSize = 2048;
							break;
					}
					Viewport?.InvalidateMaterials();
					shadowMapQuality = value;
					depthBufferRenderer = null;
					recalcViewProjection = true;
					lightViewport = new Viewport(0, 0, depthTextureSize, depthTextureSize);
				}
			}
		}

		[YuzuMember]
		[TangerineKeyframeColor(24)]
		public new Vector3 Position
		{
			get { return base.Position; }
			set
			{
				base.Position = value;
				recalcViewProjection = true;
			}
		}

		[YuzuMember]
		[TangerineGroup("Light")]
		[TangerineKeyframeColor(25)]
		public float Intensity
		{
			get { return intensity; }
			set { intensity = Mathf.Clamp(value, 0.0f, 10.0f); }
		}

		[YuzuMember]
		[TangerineGroup("Light")]
		[TangerineKeyframeColor(26)]
		public float Strength
		{
			get { return strength; }
			set { strength = Mathf.Clamp(value, 0.0f, 1.0f); }
		}

		[YuzuMember]
		[TangerineGroup("Light")]
		[TangerineKeyframeColor(27)]
		public float Ambient
		{
			get { return ambient; }
			set { ambient = Mathf.Clamp(value, 0.0f, 1.0f); }
		}

		[YuzuMember]
		[TangerineGroup("Shadow")]
		[TangerineKeyframeColor(28)]
		public bool ShadowMappingEnabled
		{
			get { return shadowMappingEnabled; }
			set
			{
				if (shadowMappingEnabled != value) {
					shadowMappingEnabled = value;
					recalcViewProjection = true;
				}
			}
		}

		[YuzuMember]
		[TangerineGroup("Shadow")]
		[TangerineIgnoreIf(nameof(ShadowMappingDisabled))]
		public Color4 ShadowColor
		{
			get;
			set;
		} = Color4.Black;

		[YuzuMember]
		[TangerineGroup("Shadow")]
		[TangerineIgnoreIf(nameof(ShadowMappingDisabled))]
		public float ShadowMapProjectionSize
		{
			get { return shadowMapProjSize; }
			set
			{
				if (shadowMapProjSize != value) {
					shadowMapProjSize = value;
					recalcViewProjection = true;
				}
			}
		}

		[YuzuMember]
		[TangerineGroup("Shadow")]
		[TangerineIgnoreIf(nameof(ShadowMappingDisabled))]
		public float ShadowMapProjectionZNear
		{
			get { return shadowMapProjZNear; }
			set
			{
				if (shadowMapProjZNear != value) {
					shadowMapProjZNear = value;
					recalcViewProjection = true;
				}
			}
		}

		[YuzuMember]
		[TangerineGroup("Shadow")]
		[TangerineIgnoreIf(nameof(ShadowMappingDisabled))]
		public float ShadowMapProjectionZFar
		{
			get { return shadowMapProjZFar; }
			set
			{
				if (shadowMapProjZFar != value) {
					shadowMapProjZFar = value;
					recalcViewProjection = true;
				}
			}
		}

		private float shadowMapProjSize = 15;
		private float shadowMapProjZNear = -15;
		private float shadowMapProjZFar = 15;

		private ShadowMapTextureQuality shadowMapQuality = (ShadowMapTextureQuality)(-1);
		private int depthTextureSize;
		private DepthBufferRenderer depthBufferRenderer;
		private Matrix44 lightView;
		private Matrix44 lightProjection;
		private Matrix44 lightViewProjection;
		private Viewport lightViewport;
		private float intensity = 1.0f;
		private float ambient = 0.05f;
		private float strength = 1f;

		private bool shadowMappingEnabled = false;
		private bool recalcViewProjection = true;

		public LightSource()
		{
			ShadowMapQuality = ShadowMapTextureQuality.Medium;
		}

		public override void Update(float delta)
		{
			if (shadowMappingEnabled) {
				if (depthBufferRenderer == null) {
					depthBufferRenderer = new DepthBufferRenderer(Viewport, depthTextureSize);
				}

				if (recalcViewProjection) {
					lightProjection = Matrix44.CreateOrthographic(shadowMapProjSize, shadowMapProjSize, shadowMapProjZNear, shadowMapProjZFar);
					lightView = Matrix44.CreateLookAt(Position.Normalized * shadowMapProjSize / 2f, Vector3.Zero, Vector3.UnitY);
					lightViewProjection = lightView * lightProjection;

					recalcViewProjection = false;
				}

				depthBufferRenderer.Render(lightView, lightViewProjection, lightViewport);
			}
		}
		
		private bool ShadowMappingDisabled()
		{
			return !shadowMappingEnabled;
		}
	}
}
