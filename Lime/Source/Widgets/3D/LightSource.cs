using Yuzu;

namespace Lime
{
	public class LightSource : Node3D
	{
		private const int shadowMapSize = 2048;

		public ITexture ShadowMap
		{ get { return depthBufferRenderer?.Texture ?? null; } }

		[YuzuMember]
		public float Intensity
		{ get; set; } = 1f;

		[TangerineInspect]
		public bool ShadowMapping
		{
			get { return shadowMappingEnabled; }
			set
			{
				if (shadowMappingEnabled == value) {
					return;
				}

				shadowMappingEnabled = value;

				if (shadowMappingEnabled) {
					depthBufferRenderer = new DepthBufferRenderer(viewport, shadowMapSize);
				}
				else {
					depthBufferRenderer = null;
				}
			}
		}

		[TangerineInspect]
		public float ShadowMapSize
		{ get; set; } = 35;

		[TangerineInspect]
		public float ShadowMapZNear
		{ get; set; } = 1;

		[TangerineInspect]
		public float ShadowMapZFar
		{ get; set; } = 5;


		private bool shadowMappingEnabled;
		private DepthBufferRenderer depthBufferRenderer;

		public override void Update(float delta)
		{
			if (shadowMappingEnabled) {
				var lightViewport = new WindowRect() { Width = shadowMapSize, Height = shadowMapSize };
				var lightProjection = Matrix44.CreateOrthographic(ShadowMapSize, ShadowMapSize, ShadowMapZNear, ShadowMapZFar);
				var lightView = Matrix44.CreateLookAt(Position.Normalized * ShadowMapSize / 2f, Vector3.Zero, Vector3.UnitY);
				var lightViewProjection = lightView * lightProjection;

				depthBufferRenderer.Render(lightViewProjection, lightViewport);
			}
		}
	}
}
