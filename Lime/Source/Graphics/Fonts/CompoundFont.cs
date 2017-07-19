using System.Collections.Generic;

namespace Lime
{
	/// <summary>
	/// Font class which combines different fonts into one. You can use collection of <see cref="IFont"/>
	/// in order of priority to use (if first font doesn't contain <see cref="FontChar"/> it will look for it
	/// in the next font and so on).
	/// </summary>
	public class CompoundFont : IFont
	{
		private readonly CompoundFontCharSource chars;
		private readonly List<IFont> fonts = new List<IFont>();

		public CompoundFont()
		{
			chars = new CompoundFontCharSource(fonts);
		}

		public CompoundFont(IEnumerable<IFont> fonts) : this()
		{
			this.fonts.AddRange(fonts);
		}

		public CompoundFont(params IFont[] fonts)
			: this((IEnumerable<IFont>)fonts)
		{
		}

		/// <summary>
		/// Legacy interface property
		/// </summary>
		public string About
		{
			get { return string.Empty; }
		}

		public IFontCharSource Chars
		{
			get { return chars; }
		}

		public void ClearCache()
		{
			chars.ClearCache();
		}

		public bool RoundCoordinates { get; } = true;

		public void Dispose()
		{
			chars.Dispose();
		}

		private class CompoundFontCharSource : IFontCharSource
		{
			private readonly List<IFont> fonts;

			public CompoundFontCharSource(List<IFont> fonts)
			{
				this.fonts = fonts;
			}

			public FontChar Get(char code, float heightHint)
			{
				foreach (var font in fonts) {
					var c = font.Chars.Get(code, heightHint);
					if (c != FontChar.Null) {
						return c;
					}
				}
				return FontChar.Null;
			}

			public bool Contains(char code)
			{
				foreach (var font in fonts) {
					if (font.Chars.Contains(code)) {
						return true;
					}
				}
				return false;
			}

			public void ClearCache()
			{
				foreach (var font in fonts) {
					font.ClearCache();
				}
			}

			public void Dispose()
			{
				foreach (var font in fonts) {
					font.Dispose();
				}
				fonts.Clear();
			}
		}
	}
}
