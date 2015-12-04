#if WIN || MAC
using System;
using System.Collections.Generic;

namespace Lime
{
	public class DynamicFont : IFont
	{
		public string About { get { return string.Empty; } }

		public IFontCharSource Chars { get; private set; }

		public List<ITexture> Textures { get; private set; }

		public DynamicFont(byte[] fontData)
		{
			Textures = new List<ITexture>();
			Chars = new DynamicFontCharSource(fontData, Textures);
		}

		public void Dispose()
		{
			if (Chars != null) {
				Chars.Dispose();
			}
			foreach (var texture in Textures) {
				if (texture != null) {
					texture.Discard();
				}
			}
		}

		public void ClearCache()
		{
			(Chars as DynamicFontCharSource).ClearCache();
		}
	}

	internal class DynamicFontCharSource : IFontCharSource
	{
		private FontRenderer fontRenderer;
		private List<ITexture> textures;
		private Dictionary<int, CharCache> charCaches = new Dictionary<int, CharCache>();
		
		public DynamicFontCharSource(byte[] fontData, List<ITexture> textures)
		{
			fontRenderer = new FontRenderer(fontData);
			this.textures = textures;
		}

		public FontChar Get(char code, float heightHint)
		{
			var roundedHeight = heightHint.Round();
			CharCache charChache;
			if (!charCaches.TryGetValue(roundedHeight, out charChache)) {
				charCaches[roundedHeight] = charChache = new CharCache(roundedHeight, fontRenderer, textures);
			}
			return charChache.Get(code);
		}

		public void ClearCache()
		{
			charCaches.Clear();
		}

		public void Dispose()
		{
			if (fontRenderer != null) {
				fontRenderer.Dispose();
				fontRenderer = null;
			}
		}

		class CharCache
		{
			// Use inheritance instead of composition in a sake of performance
			class DynamicTexture : Texture2D
			{
				public readonly Color4[] Data;
				public bool Invalidated;

				public DynamicTexture(Size size)
				{
					ImageSize = SurfaceSize = size;
					Data = new Color4[size.Width * size.Height];
				}

				public override uint GetHandle()
				{
					if (Invalidated) {
						LoadImage(Data, ImageSize.Width, ImageSize.Height, generateMips: false);
						Invalidated = false;
					}
					return base.GetHandle();
				}
			}

			private FontRenderer fontRenderer;
			private DynamicTexture texture;
			private IntVector2 position;
			private List<ITexture> textures;
			private int textureIndex;
			private int fontHeight;

			public FontChar[][] CharMap = new FontChar[256][];

			public CharCache(int fontHeight, FontRenderer fontRenderer, List<ITexture> textures)
			{
				this.fontHeight = fontHeight;
				this.textures = textures;
				this.fontRenderer = fontRenderer;
			}

			public FontChar Get(char code)
			{
				byte hb = (byte)(code >> 8);
				byte lb = (byte)(code & 255);
				if (CharMap[hb] == null) {
					CharMap[hb] = new FontChar[256];
				}
				var c = CharMap[hb][lb];
				if (c != null) {
					return c;
				}
				c = CreateFontChar(code);
				if (c != null) {
					CharMap[hb][lb] = c;
					return c;
				}
				return FontCharCollection.TranslateKnownMissingChars(ref code) ?
					Get(code) : (CharMap[hb][lb] = FontChar.Null);
			}

			private FontChar CreateFontChar(char code)
			{
				var glyph = fontRenderer.Render(code, fontHeight);
				if (glyph == null)
					return null;
				if (texture == null) {
					CreateNewFontTexture();
				}
				// We add 1px left and right padding to each char on texture and also to the UV
				// so that chars will be blurred correctly after stretching or drawing to float position.
				// And we copensate this padding thourgh ACWidth, so that text will take the same space.
				const int paddingSum = 2;

				if (position.X + glyph.Width + paddingSum + 1>= texture.ImageSize.Width) {
					position.X = 0;
					position.Y += fontHeight + 1;
				}
				if (position.Y + fontHeight + 1 >= texture.ImageSize.Height) {
					CreateNewFontTexture();
					position = IntVector2.Zero;
				}
				DrawGlyphToTexture(glyph, paddingSum);
				var fontChar = new FontChar {
					Char = code,
					UV0 = (Vector2)position / (Vector2)texture.ImageSize,
					UV1 = ((Vector2)position + new Vector2(glyph.Width + paddingSum, fontHeight)) / (Vector2)texture.ImageSize,
					ACWidths = new Vector2(glyph.ACWidths.X - paddingSum / 2, glyph.ACWidths.Y - paddingSum / 2),
					Width = glyph.Width + paddingSum,
					Height = fontHeight,
					KerningPairs = glyph.KerningPairs,
					TextureIndex = textureIndex
				};
				position.X += glyph.Width + paddingSum + 1;
				return fontChar;
			}

			private void CreateNewFontTexture()
			{
				texture = new DynamicTexture(CalcTextureSize());
				textureIndex = textures.Count;
				textures.Add(texture);
			}

			private Size CalcTextureSize()
			{
				const int glyphsPerTexture = 30;
				var glyphMaxArea = fontHeight * (fontHeight / 2);
				var size =
					CalcUpperPowerOfTwo((int)Math.Sqrt(glyphMaxArea * glyphsPerTexture))
					.Clamp(64, 1024);
				return new Size(size, size);
			}

			private int CalcUpperPowerOfTwo(int x)
			{
				x--;
				x |= (x >> 1);
				x |= (x >> 2);
				x |= (x >> 4);
				x |= (x >> 8);
				x |= (x >> 16);
				return (x + 1);
			}

			private void DrawGlyphToTexture(FontRenderer.Glyph glyph, int padding)
			{
				var srcPixels = glyph.Pixels;
				if (srcPixels == null)
					return; // Invisible glyph
				var si = 0;
				var color = Color4.White;
				var dstPixels = texture.Data;
				for (int i = 0; i < glyph.Height; i++) {
					int di = (position.Y + glyph.VerticalOffset + i) * texture.ImageSize.Width + position.X;
					di += padding / 2;
					for (int j = glyph.Width; j > 0; j--) {
						color.A = srcPixels[si++];
						dstPixels[di++] = color;
					}
					di += padding / 2;
				}
				texture.Invalidated = true;
			}
		}
	}
}
#endif
