using System;
using Lime;
using ProtoBuf;

namespace Lime
{
	/// <summary>
	/// Виджет, содержащий в себе изображение
	/// </summary>
	[ProtoContract]
	public class Image : Widget, IImageCombinerArg
	{
		bool skipRender;
		bool requestSkipRender;

		public Matrix32 UVTransform {
			get {
				return new Matrix32(new Vector2(UV1.X - UV0.X, 0), new Vector2(0, UV1.Y - UV0.Y), UV0);
			}
		}

		/// <summary>
		/// Изображение, отображаемое этим виджетом
		/// </summary>
		[ProtoMember(1)]
		public override sealed ITexture Texture { get; set; }

		/// <summary>
		/// Текстурная координата левого верхнего угла текстуры
		/// </summary>
		[ProtoMember(2)]
		public Vector2 UV0 { get; set; }

		/// <summary>
		/// Текстурная координата правого нижнего угла текстуры
		/// </summary>
		[ProtoMember(3)]
		public Vector2 UV1 { get; set; }

		public Image()
		{
			UV0 = Vector2.Zero;
			UV1 = Vector2.One;
			HitTestMethod = HitTestMethod.Contents;
			Texture = new SerializableTexture();
		}

		public Image(ITexture texture)
		{
			UV0 = Vector2.Zero;
			UV1 = Vector2.One;
			Texture = texture;
			HitTestMethod = HitTestMethod.Contents;
			Size = (Vector2)texture.ImageSize;
		}

		public Image(string texturePath)
		{
			UV0 = Vector2.Zero;
			UV1 = Vector2.One;
			Texture = new SerializableTexture(texturePath);
			Size = (Vector2)Texture.ImageSize;
		}

		/// <summary>
		/// Возвращает размер текстуры
		/// </summary>
		public override Vector2 CalcContentSize()
		{
			return (Vector2)Texture.ImageSize;
		}

		public override void Render()
		{
			Renderer.Blending = GlobalBlending;
			Renderer.Shader = GlobalShader;
			Renderer.Transform1 = LocalToWorldTransform;
			Renderer.DrawSprite(Texture, GlobalColor, Vector2.Zero, Size, UV0, UV1);
		}

		/// <summary>
		/// Добавляет виджет и все его дочерние виджеты в очередь отрисовки
		/// </summary>
		public override void AddToRenderChain(RenderChain chain)
		{
			if (GloballyVisible && !skipRender) {
				chain.Add(this, Layer);
			}
		}

		ITexture IImageCombinerArg.GetTexture()
		{
			return Texture;
		}

		void IImageCombinerArg.SkipRender()
		{
			requestSkipRender = true;
		}

		protected override void SelfUpdate(float delta)
		{
			skipRender = requestSkipRender;
			requestSkipRender = false;
		}

		protected override bool SelfHitTest(Vector2 point)
		{
			if (!GloballyVisible || skipRender || !InsideClipRect(point)) {
				return false;
			}
			if (HitTestMethod != HitTestMethod.Contents) {
				return base.SelfHitTest(point);
			}
			Vector2 localPoint = LocalToWorldTransform.CalcInversed().TransformVector(point);
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
	}
}
