using System;
using Lime;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class Image : Widget, IImageCombinerArg
	{
		bool imageCombinerArg;

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

		public override void PreloadTextures()
		{
			Texture.GetHandle();
		}

		public override void Update(int delta)
		{
			base.Update(delta);
			imageCombinerArg = false;
		}

		public override void LateUpdate(int delta)
		{
		}

		public override void Render(float interpolation)
		{
			Renderer.Blending = globalBlending;
			Renderer.Transform1 = globalMatrix;
			Renderer.DrawSprite(Texture, globalColor, Vector2.Zero, Size, UV0, UV1);
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (globallyVisible && !imageCombinerArg) {
				chain.Add(this, Layer);
			}
		}

		ITexture IImageCombinerArg.GetTexture()
		{
			return Texture;
		}

		void IImageCombinerArg.BypassRendering()
		{
			imageCombinerArg = true;
		}

		public override bool HitTest(Vector2 point)
		{
			if (globallyVisible && !imageCombinerArg) {
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
