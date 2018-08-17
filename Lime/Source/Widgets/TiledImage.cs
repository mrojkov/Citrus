using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yuzu;

namespace Lime
{
	[TangerineRegisterNode(Order = 31)]
	[TangerineVisualHintGroup("/All/Nodes/Images")]
	public class TiledImage : Widget
	{
		private bool skipRender;
		private ITexture texture;
		private IMaterial material;

		[YuzuMember]
		[YuzuSerializeIf(nameof(IsNotRenderTexture))]
		[TangerineKeyframeColor(15)]
		public override sealed ITexture Texture
		{
			get { return texture; }
			set {
				if (texture != value) {
					texture = value;
					DiscardMaterial();
					Window.Current?.Invalidate();
				}
			}
		}

		protected override void DiscardMaterial()
		{
			material = null;
		}

		[YuzuMember]
		[TangerineKeyframeColor(16)]
		public Vector2 TileRatio { get; set; }

		[YuzuMember]
		[TangerineKeyframeColor(17)]
		public Vector2 TileOffset { get; set; }

		[YuzuMember]
		[TangerineKeyframeColor(18)]
		public bool TileRounding { get; set; }

		public IMaterial CustomMaterial { get; set; }

		public TiledImage()
		{
			Presenter = DefaultPresenter.Instance;
			TileOffset = Vector2.Zero;
			TileRatio = Vector2.One;
			HitTestMethod = HitTestMethod.Contents;
			var texture = new SerializableTexture();
			Texture = texture;
		}

		public TiledImage(ITexture texture)
		{
			Presenter = DefaultPresenter.Instance;
			TileOffset = Vector2.Zero;
			TileRatio = Vector2.One;
			Texture = texture;
			HitTestMethod = HitTestMethod.Contents;
			Size = (Vector2)texture.ImageSize;
		}

		public TiledImage(string texturePath)
			: this(new SerializableTexture(texturePath))
		{
		}

		public override Vector2 CalcContentSize()
		{
			return (Vector2)Texture.ImageSize;
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (GloballyVisible && !skipRender && ClipRegionTest(chain.ClipRegion)) {
				AddSelfToRenderChain(chain, Layer);
			}
			skipRender = false;
		}

		internal protected override bool PartialHitTestByContents(ref HitTestArgs args)
		{
			Vector2 localPoint = LocalToWorldTransform.CalcInversed().TransformVector(args.Point);
			Vector2 size = Size;
			if (size.X < 0) {
				localPoint.X = -localPoint.X;
				size.X = -size.X;
			}
			if (size.Y < 0) {
				localPoint.Y = -localPoint.Y;
				size.Y = -size.Y;
			}
			if (localPoint.X >= 0 && localPoint.Y >= 0 && localPoint.X < size.X && localPoint.Y < size.Y) {
				int u = (int)(Texture.ImageSize.Width * (localPoint.X / size.X));
				int v = (int)(Texture.ImageSize.Height * (localPoint.Y / size.Y));
				return !Texture.IsTransparentPixel(u, v);
			} else {
				return false;
			}
		}

		public override void Render()
		{
			var blending = GlobalBlending;
			var shader = GlobalShader;
			if (material == null) {
				material = WidgetMaterial.GetInstance(blending, shader, 1);
			}
			Renderer.Transform1 = LocalToWorldTransform;
			var UV1 = new Vector2 {
				X = Size.X / texture.ImageSize.Width * TileRatio.X + TileOffset.X,
				Y = Size.Y / texture.ImageSize.Height * TileRatio.Y + TileOffset.Y
			};
			if (TileRounding) {
				UV1.X = (float)Math.Truncate(UV1.X);
				UV1.Y = (float)Math.Truncate(UV1.Y);
			}
			Renderer.DrawSprite(texture, null, CustomMaterial ?? material, GlobalColor, ContentPosition, ContentSize, TileOffset, UV1, Vector2.Zero, Vector2.Zero);
		}

		public bool IsNotRenderTexture()
		{
			return !(texture is RenderTexture);
		}
	}
}
