using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lime
{
	class SDFRenderObject : RenderObject
	{
		private bool wasOffscreenRenderingPrepared;

		internal ITexture ProcessedTexture;
		internal Viewport ProcessedViewport;
		internal Vector2 ProcessedUV1;
		internal Size ViewportSize;
		internal Vector2 TextureSize;
		public Vector2 CurrentBufferSize;

		public readonly RenderObjectList Objects = new RenderObjectList();
		public SDFRenderAction[] RenderActions;
		public IMaterial Material;
		public Matrix32 LocalToWorldTransform;
		public Vector2 Position;
		public Vector2 Size;
		public Color4 Color;
		public Vector2 UV0;
		public Vector2 UV1;
		public bool MarkBuffersAsDirty;
		public SDFRenderAction.Buffer SourceTextureBuffer;
		public float SourceTextureScaling;
		public bool OpagueRendering;

		public SDFRenderActionMain.Buffer SDFBuffer;
		public SignedDistanceFieldMaterial SDFMaterial;
		public float Contrast;

		protected override void OnRelease()
		{
			Objects.Clear();
			SourceTextureBuffer = null;
			Material = null;
			SDFBuffer = null;
			SDFMaterial = null;
		}

		public override void Render()
		{
			wasOffscreenRenderingPrepared = false;
			try {
				foreach (var action in RenderActions) {
					var buffer = action.GetTextureBuffer(this);
					if (MarkBuffersAsDirty) {
						buffer?.MarkAsDirty();
					}
					if (action.EnabledCheck(this)) {
						action.Do(this);
						if (buffer != null) {
							buffer.WasApplied = true;
						}
					} else if (buffer?.WasApplied ?? false) {
						buffer.WasApplied = false;
						buffer.MarkAsDirty();
						MarkBuffersAsDirty = true;
					}
				}
			} finally {
				FinalizeOffscreenRendering();
			}
		}

		internal void RenderToTexture(ITexture renderTargetTexture, ITexture sourceTexture, IMaterial material, Color4 color, Color4 backgroundColor, Size? customViewportSize = null, Vector2? customUV1 = null)
		{
			var vs = customViewportSize ?? ViewportSize;
			var uv1 = customUV1 ?? ProcessedUV1;
			if (ProcessedViewport.Width != vs.Width || ProcessedViewport.Height != vs.Height) {
				Renderer.Viewport = ProcessedViewport = new Viewport(0, 0, vs.Width, vs.Height);
			}
			renderTargetTexture.SetAsRenderTarget();
			try {
				Renderer.Clear(backgroundColor);
				Renderer.DrawSprite(sourceTexture, null, material, color, Vector2.Zero, TextureSize, Vector2.Zero, uv1, Vector2.Zero, Vector2.Zero);
			} finally {
				renderTargetTexture.RestoreRenderTarget();
			}
		}

		internal void RenderTexture(ITexture texture, IMaterial customMaterial = null, Vector2? customUV1 = null)
		{
			var uv1 = customUV1 ?? ProcessedUV1;
			Renderer.Transform1 = LocalToWorldTransform;
			Renderer.DrawSprite(texture, null, customMaterial ?? Material, Color, Position, Size, UV0 * uv1, UV1 * uv1, Vector2.Zero, Vector2.Zero);
		}

		internal void PrepareOffscreenRendering(Vector2 orthogonalProjection)
		{
			if (wasOffscreenRenderingPrepared) {
				return;
			}
			Renderer.PushState(
				RenderState.Viewport |
				RenderState.World |
				RenderState.View |
				RenderState.Projection |
				RenderState.DepthState |
				RenderState.ScissorState |
				RenderState.CullMode |
				RenderState.Transform2
			);
			wasOffscreenRenderingPrepared = true;
			Renderer.ScissorState = ScissorState.ScissorDisabled;
			Renderer.World = Renderer.View = Matrix44.Identity;
			Renderer.SetOrthogonalProjection(Vector2.Zero, orthogonalProjection);
			Renderer.DepthState = DepthState.DepthDisabled;
			Renderer.CullMode = CullMode.None;
			Renderer.Transform2 = Matrix32.Identity;
			Renderer.Transform1 = Matrix32.Identity;
		}

		internal void FinalizeOffscreenRendering()
		{
			if (!wasOffscreenRenderingPrepared) {
				return;
			}
			Renderer.PopState();
			wasOffscreenRenderingPrepared = false;
		}
	}
}
