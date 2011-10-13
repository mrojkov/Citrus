using System;
using Lime;
using ProtoBuf;

namespace Lemon
{
    [ProtoContract]
	public class Image : Widget, IImageCombinerArg
	{
        [ProtoMember(1)]
		public PersistentTexture Texture { get; set; }

        [ProtoMember(2)]
		public Vector2 UV0 { get; set; }

        [ProtoMember(3)]
		public Vector2 UV1 { get; set; }

		public Image ()
		{
			UV0 = Vector2.Zero;
			UV1 = Vector2.One;
			Texture = new PersistentTexture ();
		}

		public override void Update (int delta)
		{
			base.Update (delta);
			imageCombinerArg = false;
		}

		public override void Render ()
		{
			if (imageCombinerArg)
				return;
			Renderer.Instance.WorldMatrix = WorldMatrix;
			Renderer.Instance.Blending = WorldBlending;
			Renderer.Instance.DrawSprite (Texture, WorldColor, Vector2.Zero, Size, UV0, UV1); 
		}

		bool imageCombinerArg;

		ITexture IImageCombinerArg.GetTexture ()
		{
			return Texture;
		}

		void IImageCombinerArg.BypassRendering ()
		{
			imageCombinerArg = true;
		}

		public override bool HitTest (Vector2 point)
		{
			if (WorldShown && !imageCombinerArg) {
				if (HitTestMethod == HitTestMethod.Contents) {
					Vector2 pt = WorldMatrix.CalcInversed ().TransformVector (point);
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
						return !Texture.IsTransparentPixel (u, v);
					} else
						return false;
				} else
					return base.HitTest (point);
			}
			return false;
		}
	}
}
