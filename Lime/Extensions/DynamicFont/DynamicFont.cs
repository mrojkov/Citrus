using System;
using System.Collections.Generic;

namespace Lime
{
	/// <summary>
	/// Dynamic font used in combination with freetype supported fonts.
	/// Dynamic font doesn't contain all the chars from the file, it rasterize them on demand,
	/// caching into a texture. Also when we demand the same char with new height, this char will
	/// be rasterized again with new height and cached too, so that we don't need to scale them.
	/// </summary>
	public class DynamicFont : IFont
	{
		/// <summary>
		/// Creates dynamic font by file data supported by freetype.
		/// </summary>
		/// <param name="fontData">Font file data.</param>
		public DynamicFont(byte[] fontData)
		{
			Chars = new DynamicFontCharSource(fontData);
		}

		/// <summary>
		/// Creates dynamic font by file name of freetype supported font.
		/// File should be located in assets bundle under Fonts/ folder.
		/// </summary>
		/// <param name="fontName">Font file name.</param>
		public DynamicFont(string fontName)
			: this(AssetBundle.Current.ReadFile("Fonts/" + fontName))
		{
		}

		/// <summary>
		/// Legacy interface property
		/// </summary>
		public string About
		{
			get { return string.Empty; }
		}

		/// <summary>
		/// Collection of <see cref="FontChar"/> available in this font.
		/// <see cref="DynamicFont"/> implements <see cref="DynamicFontCharSource"/>.
		/// </summary>
		[CLSCompliant(false)]
		public IFontCharSource Chars
		{
			get; private set;
		}

		/// <summary>
		/// Removes information about cached chars. Forces to re-render chars.
		/// </summary>
		public void ClearCache()
		{
			(Chars as DynamicFontCharSource).ClearCache();
		}

		public void SetFontHeightResolver(Func<int, int> fontHeightResolver)
		{
			((DynamicFontCharSource)Chars).SetFontHeightResolver(fontHeightResolver);
		}

		public bool RoundCoordinates { get; set; } = true;

		public void Dispose()
		{
			if (Chars != null) {
				Chars.Dispose();
			}
		}

		/// <summary>
		/// Collection of chars for DynamicFont. Collection is auto-filled by <see cref="Get(char, float)"/> method.
		/// </summary>
		private class DynamicFontCharSource : IFontCharSource
		{
			private readonly Dictionary<int, CharCache> charCaches = new Dictionary<int, CharCache>();
			private readonly FontRenderer fontRenderer;
			private readonly List<ITexture> textures = new List<ITexture>();

			public DynamicFontCharSource(byte[] fontData)
			{
				fontRenderer = new FontRenderer(fontData);
			}

			/// <summary>
			/// At first this method looks if there is <see cref="CharCache"/> for a given height.
			/// If not - the method creates such <see cref="CharCache"/>.
			/// The rest of the logic is on <see cref="CharCache"/>.
			/// </summary>
			/// <param name="code">Char to obtain.</param>
			/// <param name="heightHint">Desired height. It will be rounded to int to reduce number of caches.</param>
			/// <returns></returns>
			public FontChar Get(char code, float heightHint)
			{
				var roundedHeight = (heightHint * Window.Current.PixelScale).Round();
				CharCache charChache;
				if (!charCaches.TryGetValue(roundedHeight, out charChache)) {
					charCaches[roundedHeight] = charChache = new CharCache(roundedHeight, fontRenderer, textures);
				}
				var c = charChache.Get(code);
				if (c.Texture == null) {
					c.Texture = textures[c.TextureIndex];
				}
				return c;
			}

			/// <summary>
			/// Checks if a given char is available for rendering
			/// </summary>
			/// <param name="code">Char to check</param>
			public bool Contains(char code)
			{
				return fontRenderer.ContainsGlyph(code);
			}

			public void SetFontHeightResolver(Func<int, int> fontHeightResolver)
			{
				fontRenderer.SetFontHeightResolver(fontHeightResolver);
			}

			public void ClearCache()
			{
				charCaches.Clear();
			}

			public void Dispose()
			{
				if (fontRenderer != null) {
					fontRenderer.Dispose();
				}
				foreach (var texture in textures) {
					if (texture != null) {
						texture.Discard();
					}
				}
				textures.Clear();
			}
		}
	}
}
