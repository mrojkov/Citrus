using System;
using Lime;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class Image : Widget, IImageCombinerArg
	{
		bool skipRender;
		bool requestSkipRender;

		[ProtoMember(1)]
		public ITexture Texture { get; set; }

		[ProtoMember(2)]
		public Vector2 UV0 { get; set; }

		[ProtoMember(3)]
		public Vector2 UV1 { get; set; }

		public Image()
		{
			UV0 = Vector2.Zero;
			UV1 = Vector2.One;
			Texture = new SerializableTexture();
		}

		public Image(ITexture texture)
		{
			UV0 = Vector2.Zero;
			UV1 = Vector2.One;
			Texture = texture;
			Size = (Vector2)texture.ImageSize;
		}

		public Image(string texturePath)
		{
			UV0 = Vector2.Zero;
			UV1 = Vector2.One;
			Texture = new SerializableTexture(texturePath);
			Size = (Vector2)Texture.ImageSize;
		}

		public override Vector2 CalcContentSize()
		{
			return (Vector2)Texture.ImageSize;
		}

		public override void PreloadTextures()
		{
			Texture.GetHandle();
		}

		public override void Render()
		{
			Renderer.Blending = globalBlending;
			Renderer.Transform1 = globalMatrix;
			Renderer.DrawSprite(Texture, globalColor, Vector2.Zero, Size, UV0, UV1);
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (globallyVisible && !skipRender) {
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

		public override void Update(int delta)
		{
			skipRender = requestSkipRender;
			requestSkipRender = false;
			base.Update(delta);
		}

		public override bool HitTest(Vector2 point)
		{
			if (globallyVisible && !skipRender) {
				if (HitTestMethod == HitTestMethod.Contents) {
					Vector2 pt = GlobalMatrix.CalcInversed().TransformVector(point);
					Vector2 sz = Size;
					if (sz.X < 0) {
						pt.X = -pt.X;
						sz.X = -sz.X;
					}
					if (sz.Y < 0) {
						pt.Y = -pt.Y;
						sz.Y = -sz.Y;
					}
					if (pt.X >= 0 && pt.Y >= 0 && pt.X < sz.X && pt.Y < sz.Y) {
						int u = (int)(Texture.ImageSize.Width * (pt.X / sz.X));
						int v = (int)(Texture.ImageSize.Height * (pt.Y / sz.Y));
						return !Texture.IsTransparentPixel(u, v);
					} else
						return false;
				} else
					return base.HitTest(point);
			}
			return false;
		}
	}
}
