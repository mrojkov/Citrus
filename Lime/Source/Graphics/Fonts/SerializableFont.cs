using System;
using System.Collections.Generic;
using System.Linq;
using Yuzu;

namespace Lime
{
	public class SerializableFont: IFont
	{
		private IFont font;

		[YuzuMember]
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				if (name == value)
					return;
				name = value;
				font = null;
			}
		}

		public string About {
			get
			{
				if (font == null)
					font = FontPool.Instance[Name];
				return font.About;
			}
		}

		public IFontCharSource Chars {
			get
			{
				if (font == null)
					font = FontPool.Instance[Name];
				return font.Chars;
			}
		}

		public bool RoundCoordinates
		{
			get
			{
				if (font == null)
					font = FontPool.Instance[Name];
				return font.RoundCoordinates;
			}
		}

		private string name;

		public SerializableFont()
		{
			Name = "";
		}

		public SerializableFont(string name)
		{
			Name = name;
		}

		public void ClearCache()
		{
			if (font == null)
				font = FontPool.Instance[Name];
			font.ClearCache();
		}

		public void Dispose()
		{
			font = null;
		}
	}

	public class FontPool
	{
		public const string DefaultFontDirectory = "Fonts/";
		public const string DefaultFontName = "Default";
		public IFont Null = new Font();
		private Dictionary<string, IFont> fonts = new Dictionary<string, IFont>();

		static readonly FontPool instance = new FontPool();
		public static FontPool Instance { get { return instance; } }

		public Func<string, string> FontNameChanger;

		public IFont DefaultFont { get { return this[null]; } }

		public void AddFont(string name, IFont font)
		{
			fonts[name] = font;
		}

		public IFont this[string name]
		{
			get
			{
				if (FontNameChanger != null)
					name = FontNameChanger(name);
				if (string.IsNullOrEmpty(name))
					name = DefaultFontName;
				IFont font;
				if (fonts.TryGetValue(name, out font))
					return font;
				if (!AssetBundle.Initialized || !TryFindOrUpdateBundleFonts(name, out var updatedName)) {
					return Null;
				}
				font = Serialization.ReadObject<Font>(updatedName);
				fonts[updatedName] = font;
				return font;
			}
		}

		public void Clear(bool preserveDefaultFont = false)
		{
			var defaultFont = this[DefaultFontName];
			foreach (var font in fonts.Values) {
				if (font == defaultFont && preserveDefaultFont) {
					continue;
				}
				font.Dispose();
			}
			fonts.Clear();
			if (preserveDefaultFont) {
				fonts.Add(DefaultFontName, defaultFont);
			}
		}

		public void ClearCache()
		{
			foreach (var font in fonts.Values) {
				font.ClearCache();
			}
		}

		private bool TryFindOrUpdateBundleFonts(string name, out string updatedName)
		{
			var fontPaths = AssetBundle.Current.EnumerateFiles(DefaultFontDirectory).Where(i => i.EndsWith(".fnt") || i.EndsWith(".tft"));
			if (fontPaths.Contains(name)) {
				updatedName = name;
				return true;
			}
			// Look through all the paths to find one containing font with the same name.
			foreach (var path in fontPaths) {
				if (System.IO.Path.GetFileNameWithoutExtension(path) == name) {
					updatedName = path;
					return true;
				}
			}
			updatedName = null;
			return false;
		}
	}
}
