using System;

namespace Lime
{
	public static class GraphicsExtensions
	{
		public static int GetElementSize(this IndexFormat indexFormat)
		{
			switch (indexFormat) {
				case IndexFormat.Index16Bits:
					return 2;
				default:
					throw new NotSupportedException();
			}
		}

		public static FrontFace Invert(this FrontFace value)
		{
			switch (value) {
				case FrontFace.CW:
					return FrontFace.CCW;
				case FrontFace.CCW:
					return FrontFace.CW;
				default:
					throw new NotSupportedException();
			}
		}

		public static BlendState GetBlendState(this Blending blending, bool alphaPremul = false)
		{
			var bs = BlendState.Default;
			switch (blending) {
				case Blending.Opaque:
					bs.SrcBlend = Blend.One;
					bs.DstBlend = Blend.Zero;
					break;
				case Blending.PremultipliedAlpha:
					bs.SrcBlend = Blend.One;
					bs.DstBlend = Blend.InverseSrcAlpha;
					break;
				case Blending.None:
				case Blending.Inherited:
				case Blending.Alpha:
					bs.SrcBlend = alphaPremul ? Blend.One : Blend.SrcAlpha;
					bs.DstBlend = Blend.InverseSrcAlpha;
					break;
				case Blending.LcdTextFirstPass:
					bs.SrcBlend = Blend.Zero;
					bs.DstBlend = Blend.InverseSrcColor;
					break;
				case Blending.LcdTextSecondPass:
					bs.SrcBlend = Blend.SrcAlpha;
					bs.DstBlend = Blend.One;
					break;
				case Blending.Add:
				case Blending.Glow:
					bs.SrcBlend = alphaPremul ? Blend.One : Blend.SrcAlpha;
					bs.DstBlend = Blend.One;
					break;
				case Blending.Burn:
				case Blending.Darken:
					bs.SrcBlend = Blend.DstColor;
					bs.DstBlend = Blend.InverseSrcAlpha;
					break;
				case Blending.Modulate:
					bs.SrcBlend = Blend.DstColor;
					bs.DstBlend = Blend.Zero;
					break;
			}
			if (PlatformRenderer.OffscreenRendering) {
				bs.AlphaSrcBlend = Blend.InverseDstAlpha;
				bs.AlphaDstBlend = Blend.One;
			}
			return bs;
		}
	}
}
