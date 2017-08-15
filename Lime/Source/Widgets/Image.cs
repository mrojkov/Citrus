using Yuzu;

namespace Lime
{
	public class Image : Widget, IImageCombinerArg
	{
		bool skipRender;
		private ITexture texture;

		[YuzuMember]
		public override sealed ITexture Texture
		{
			get { return texture; }
			set
			{
				if (texture != value) {
					texture = value;
					Window.Current?.Invalidate();
				}
			}
		}

		[YuzuMember]
		public Vector2 UV0 { get; set; }

		[YuzuMember]
		public Vector2 UV1 { get; set; }

		public Image()
		{
			Presenter = DefaultPresenter.Instance;
			UV1 = Vector2.One;
			HitTestMethod = HitTestMethod.Contents;
			Texture = new SerializableTexture();
		}

		public Image(ITexture texture)
		{
			Presenter = DefaultPresenter.Instance;
			UV1 = Vector2.One;
			Texture = texture;
			HitTestMethod = HitTestMethod.Contents;
			Size = (Vector2)texture.ImageSize;
		}

		public Image(string texturePath)
			: this(new SerializableTexture(texturePath))
		{
		}

		public override Vector2 CalcContentSize()
		{
			return (Vector2)Texture.ImageSize;
		}

		internal protected override void AddToRenderChain(RenderChain chain)
		{
			if (GloballyVisible && !skipRender) {
				AddSelfToRenderChain(chain);
			}
			skipRender = false;
		}

		public override void Dispose()
		{
			if (Texture != null) {
				Texture.Dispose();
			}
			base.Dispose();
		}

		ITexture IImageCombinerArg.GetTexture()
		{
			return Texture;
		}

		void IImageCombinerArg.SkipRender()
		{
			skipRender = true;
		}

		Matrix32 IImageCombinerArg.UVTransform
		{
			get { return new Matrix32(new Vector2(UV1.X - UV0.X, 0), new Vector2(0, UV1.Y - UV0.Y), UV0); }
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
			PrepareRendererState();
			Renderer.DrawSprite(Texture, GlobalColor, ContentPosition, ContentSize, UV0, UV1);
		}
	}
}