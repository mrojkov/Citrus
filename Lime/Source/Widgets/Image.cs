using Yuzu;

namespace Lime
{
	[TangerineRegisterNode(Order = 2)]
	[TangerineVisualHintGroup("/All/Nodes/Images")]
	public class Image : Widget, IImageCombinerArg
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
			set
			{
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
		public Vector2 UV0 { get; set; }

		[YuzuMember]
		[TangerineKeyframeColor(17)]
		public Vector2 UV1 { get; set; }

		public IMaterial CustomMaterial { get; set; }

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

		public override void AddToRenderChain(RenderChain chain)
		{
			if (GloballyVisible && !skipRender && ClipRegionTest(chain.ClipRegion)) {
				AddSelfToRenderChain(chain, Layer);
			}
			skipRender = false;
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
				float u = UV0.X + (UV1.X - UV0.X) * (localPoint.X / size.X);
				float v = UV0.Y + (UV1.Y - UV0.Y) * (localPoint.Y / size.Y);
				int tu = (int)(Texture.ImageSize.Width * u);
				int tv = (int)(Texture.ImageSize.Height * v);
				return !Texture.IsTransparentPixel(tu, tv);
			} else {
				return false;
			}
		}

		public bool IsNotRenderTexture()
		{
			return !(texture is RenderTexture);
		}

		protected internal virtual Lime.RenderObject GetRenderObject<TRenderObject>() where TRenderObject : RenderObject, new()
		{
			var blending = GlobalBlending;
			var shader = GlobalShader;
			if (material == null) {
				material = WidgetMaterial.GetInstance(blending, shader, 1);
			}
			var ro = RenderObjectPool<TRenderObject>.Acquire();
			ro.Texture = Texture;
			ro.Material = CustomMaterial ?? material;
			ro.UV0 = UV0;
			ro.UV1 = UV1;
			ro.LocalToWorldTransform = LocalToWorldTransform;
			ro.Position = ContentPosition;
			ro.Size = ContentSize;
			ro.Color = GlobalColor;
			return ro;
		}

		protected internal override Lime.RenderObject GetRenderObject()
		{
			return GetRenderObject<RenderObject>();
		}

		protected internal class RenderObject : Lime.RenderObject
		{
			public ITexture Texture;
			public IMaterial Material;
			public Color4 Color;
			public Vector2 Position;
			public Vector2 Size;
			public Vector2 UV0;
			public Vector2 UV1;
			public Matrix32 LocalToWorldTransform;

			public override void Render()
			{
				Renderer.Transform1 = LocalToWorldTransform;
				Renderer.DrawSprite(Texture, null, Material, Color, Position, Size, UV0, UV1, Vector2.Zero, Vector2.Zero);
			}

			protected override void OnRelease()
			{
				Texture = null;
				Material = null;
			}
		}
	}
}
