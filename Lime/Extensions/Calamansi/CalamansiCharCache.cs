using System.Collections.Generic;
using Lime;

namespace Calamansi
{
	public class CalamansiCharCache : CharCache
	{
		public FontRenderer FontRenderer
		{
			get => fontRenderer;
			set => fontRenderer = value;
		}

		public List<ITexture> Textures
		{
			get => textures;
			set => textures = value;
		}

		public int FontHeight
		{
			get => fontHeight;
			set => fontHeight = value;
		}

		public Size TextureSize { get; set; } = new Size(2048, 2048);

		protected override Size CalcTextureSize() => TextureSize;

		public CalamansiCharCache(int fontHeight, FontRenderer fontRenderer, List<ITexture> textures) :
			base(fontHeight, fontRenderer, textures) { }
	}
}
