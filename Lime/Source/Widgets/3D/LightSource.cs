using Yuzu;

namespace Lime
{
	public class LightSource : Node3D
	{
		private const int TextureSize = 1024;

		public ITexture ShadowMap
		{ get { return depthBufferRenderer?.Texture ?? null; } }

		public Matrix44 ViewProjection
		{ get { return lightViewProjection; } }

		[YuzuMember]
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
		public float Intensity
		{ get; set; } = 1f;

		[YuzuMember]
		public bool ShadowMapping
		{
			get { return shadowMappingEnabled; }
			set
			{
				if (shadowMappingEnabled == value) {
					return;
				}

				shadowMappingEnabled = value;
			}
		}

		[YuzuMember]
		public float ShadowMapSize
		{
			get { return shadowMapSize; }
			set
			{
				if (shadowMapSize != value) {
					shadowMapSize = value;
					recalcViewProjection = true;
				}
			}
		}

		[YuzuMember]
		public float ShadowMapZNear
		{
			get { return shadowMapZNear; }
			set
			{
				if (shadowMapZNear != value) {
					shadowMapZNear = value;
					recalcViewProjection = true;
				}
			}
		}

		[TangerineInspect]
		public float ShadowMapZFar
		{
			get { return shadowMapZFar; }
			set
			{
				if (shadowMapZFar != value) {
					shadowMapZFar = value;
					recalcViewProjection = true;
				}
			}
		}

		private float shadowMapSize = 15;
		private float shadowMapZNear = -15;
		private float shadowMapZFar = 15;

		private DepthBufferRenderer depthBufferRenderer;
		private Matrix44 lightView;
		private Matrix44 lightProjection;
		private Matrix44 lightViewProjection;
		private WindowRect lightViewport = new WindowRect() {
			Width = TextureSize,
			Height = TextureSize
		};

		private bool shadowMappingEnabled;
		private bool recalcViewProjection = true;

		public override void Update(float delta)
		{
			if (shadowMappingEnabled) {
				if (depthBufferRenderer == null) {
					depthBufferRenderer = new DepthBufferRenderer(viewport, TextureSize);
				}

				if (recalcViewProjection) {
					lightProjection = Matrix44.CreateOrthographic(shadowMapSize, shadowMapSize, shadowMapZNear, shadowMapZFar);
					lightView = Matrix44.CreateLookAt(Position.Normalized * shadowMapSize / 2f, Vector3.Zero, Vector3.UnitY);
					lightViewProjection = lightView * lightProjection;

					recalcViewProjection = false;
				}

				depthBufferRenderer.Render(lightViewProjection, lightViewport);
			}
		}
	}
}
