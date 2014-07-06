using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class TextStyle : Node
	{
		public static TextStyle Default = new TextStyle();

		[ProtoContract]
		public enum ImageUsageEnum
		{
			[ProtoEnum]
			Bullet,
			[ProtoEnum]
			Overlay
		}

		[ProtoMember(1)]
		public SerializableTexture ImageTexture { get; set; }
		
		[ProtoMember(2)]
		public Vector2 ImageSize { get; set; }
		
		[ProtoMember(3)]
		public ImageUsageEnum ImageUsage { get; set; }
		
		[ProtoMember(4)]
		public SerializableFont Font { get; set; }
		
		[ProtoMember(5)]
		public float Size { get; set; }
		
		[ProtoMember(6)]
		public float SpaceAfter { get; set; }
		
		[ProtoMember(7)]
		public bool Bold { get; set; }
		
		[ProtoMember(8)]
		public bool CastShadow { get; set; }
		
		[ProtoMember(9)]
		public Vector2 ShadowOffset { get; set; }

		[ProtoMember(10)]
		public Color4 TextColor { get; set; }

		[ProtoMember(11)]
		public Color4 ShadowColor { get; set; }

		public TextStyle()
		{
			Size = 15;
			TextColor = Color4.White;
			ShadowColor = Color4.Black;
			ShadowOffset = Vector2.One;
			Font = new SerializableFont();
			ImageTexture = new SerializableTexture();
		}
	}

	[ProtoContract]
	public class RichText : Widget
	{
		private TextParser parser = new TextParser();
		private string text;
		private HAlignment hAlignment;
		private VAlignment vAlignment;
		private SpriteList spriteList;

		[ProtoMember(1)]
		public override string Text
		{
			get { return text; }
			set { SetText(value); }
		}

		[ProtoMember(2)]
		public HAlignment HAlignment 
		{ 
			get { return hAlignment; } 
			set { SetHAlignment(value); } 
		}
		
		[ProtoMember(3)]
		public VAlignment VAlignment 
		{ 
			get { return vAlignment; } 
			set { SetVAlignment(value); } 
		}

		public RichText()
		{
			// CachedRendering = true;
		}

		private string errorMessage;

		public string ErrorMessage
		{ 
			get 
			{
				ParseText();
				return errorMessage;
			}
		}

		protected override void OnSizeChanged(Vector2 sizeDelta)
		{
			Invalidate();
		}

		public override void Render()
		{
			if (spriteList == null) {
				var renderer = PrepareRenderer();
				spriteList = new SpriteList();
				renderer.Render(spriteList, Size, HAlignment, VAlignment);
			}
			Renderer.Transform1 = LocalToWorldTransform;
			Renderer.Blending = GlobalBlending;
			Renderer.Shader = GlobalShader;
			spriteList.Render(GlobalColor);
		}

		public Vector2 MeasureText()
		{
			var renderer = PrepareRenderer();
			return renderer.MeasureText(Size.X);
		}

		private TextRenderer PrepareRenderer()
		{
			ParseText();
			var renderer = new TextRenderer();
			// Setup default style(take first one from node list or TextStyle.Default).
			TextStyle defaultStyle = null;
			if (Nodes.Count > 0) {
				defaultStyle = Nodes[0] as TextStyle;
			}
			renderer.AddStyle(defaultStyle ?? TextStyle.Default);
			// Fill up style list.
			foreach (var styleName in parser.Styles) {
				var style = Nodes.TryFind(styleName) as TextStyle;
				renderer.AddStyle(style ?? TextStyle.Default);
			}
			// Add text fragments.
			foreach (var frag in parser.Fragments) {
				// Warning! Using style + 1, because -1 is a default style.
				renderer.AddFragment(frag.Text, frag.Style + 1);
			}
			return renderer;
		}

		private void SetHAlignment(Lime.HAlignment value)
		{
			if (value == hAlignment) {
				return;
			}
			hAlignment = value;
			Invalidate();
		}

		private void SetVAlignment(Lime.VAlignment value)
		{
			if (value == vAlignment) {
				return;
			}
			vAlignment = value;
			Invalidate();
		}

		private void SetText(string value)
		{
			if (value == text) {
				return;
			}
			Invalidate();
			text = value;
		}

		private void ParseText()
		{
			if (parser != null) {
				return;
			}
			var localizedText = Localization.GetString(text);
			parser = new TextParser(localizedText);
			errorMessage = parser.ErrorMessage;
			if (errorMessage != null) {
				parser = new TextParser("Error: " + errorMessage);
			}
		}

		public void Invalidate()
		{
			InvalidateRenderCache();
			if (spriteList != null) {
				spriteList.Dispose();
				spriteList = null;
			}
			parser = null;
		}

		/// Call on user-supplied parts of text.
		public static string Escape(string text)
		{
			return text.
				Replace("&amp;", "&amp;amp;").Replace("&lt;", "&amp;lt;").Replace("&gt;", "&amp;&gt").
				Replace("<", "&lt;").Replace(">", "&gt;");
		}
	}

	class TextParser
	{
		public struct Fragment
		{
			public int Style;
			public string Text;
		}

		readonly string text;
		readonly Stack<string> tagStack = new Stack<string>();

		int pos = 0;
		int currentStyle = -1;
		public string ErrorMessage;
		public List<string> Styles = new List<string>();
		public List<Fragment> Fragments = new List<Fragment>();

		public TextParser(string text = null)
		{
			text = text ?? "";
			this.text = text;
			while (pos < text.Length) {
				if (text[pos] == '<') {
					ParseTag();
				} else {
					ParseText();
				}
			}
			if (tagStack.Count > 0) {
				ErrorMessage = String.Format("Unmatched tag '&lt;{0}&gt;'", tagStack.Peek());
			}
		}

		void ParseText()
		{
			int p = pos;
			while (pos < text.Length) {
				if (text[pos] == '<') {
					break;
				} else if (text[pos] == '>') {
					ErrorMessage = "Unexpected '&gt;'";
					pos = text.Length;
					return;
				} else {
					pos++;
				}
			}
			if (p != pos) {
				ProcessTextBlock (text.Substring(p, pos - p));
			}
		}

		void ParseTag()
		{
			bool isOpeningTag = true;
			bool isClosedTag = false;
			int p = ++pos;
			while (pos < text.Length) {
				if (text[pos] == '/') {
					if (p == pos) {
						isOpeningTag = false;
						pos++;
						p++;
					} else if (pos + 1 < text.Length && text[pos + 1] == '>' && isOpeningTag) {
						pos++;
						isClosedTag = true;
					} else {
						ErrorMessage = "Unexpected '/'";
						pos = text.Length;
						return;
					}
				} else if (text[pos] == '>') {
					if (isClosedTag) {
						string tag = text.Substring(p, pos - p - 1);
						ProcessOpeningTag(tag);
						ProcessTextBlock ("");
						ProcessClosingTag(tag);
					} else if (isOpeningTag) {
						string tag = text.Substring(p, pos - p);
						if (!ProcessOpeningTag(tag)) {
							pos = text.Length;
							return;
						}
					} else {
						string tag = text.Substring(p, pos - p);
						if (!ProcessClosingTag(tag)) {
							pos = text.Length;
							return;
						}
					}
					pos++;
					return;
				} else {
					pos++;
				}
			}
			ErrorMessage = "Unclosed tag";
			pos = text.Length;
		}

		bool ProcessOpeningTag(string tag)
		{
			tagStack.Push(tag);
			SetStyle(tag);
			return true;
		}

		bool ProcessClosingTag(string tag)
		{
			if (tagStack.Count == 0 || tagStack.Peek() != tag) {
				ErrorMessage = String.Format("Unexpected closing tag '&lt;/{0}&gt;'", tag);
				return false;
			}
			tagStack.Pop();
			SetStyle(tagStack.Count == 0 ? null : tagStack.Peek());
			return true;
		}

		void ProcessTextBlock(string text)
		{
			Fragments.Add(new Fragment { Style = currentStyle, Text = UnescapeTaggedString(text) });
		}

		string UnescapeTaggedString(string text)
		{
			text = text.Replace("&lt;", "<");
			text = text.Replace("&gt;", ">");
			text = text.Replace("&amp;", "&");
			return text;
		}

		void SetStyle(string styleName)
		{
			if (styleName != null) {
				for (int i = 0; i < Styles.Count; i++) {
					if (Styles[i] == styleName) {
						currentStyle = i;
						return;
					}
				}
			} else {
				currentStyle = -1;
				return;
			}
			currentStyle = Styles.Count;
			Styles.Add(styleName);
		}
	}

	class TextRenderer
	{
		readonly List<Fragment> fragments = new List<Fragment>();
		readonly List<TextStyle> styles = new List<TextStyle>();
		readonly List<SerializableFont> fonts = new List<SerializableFont>();

		struct Fragment
		{
			public string Text;
			public int Style;
			public int Start;
			public int Length;
			public float X;
			public float Width;
			public bool LineBreak; // Line break before the fragment
			public bool IsTagBegin;
		};

		public void AddFragment(string text, int style)
		{
			fragments.Add(new Fragment { 
				Text = text, 
				Start = 0, 
				Length = text.Length, 
				Style = style, 
				IsTagBegin = false, 
				LineBreak = false, 
				X = 0, 
				Width = 0 
			});
		}

		public void AddStyle(TextStyle style)
		{
			styles.Add(style);
			fonts.Add(style.Font);
		}

		float CalcWordWidth(Fragment word)
		{
			Font font = fonts[word.Style].Instance;
			TextStyle style = styles[word.Style];
			float bullet = 0;
			if (word.IsTagBegin && style.ImageUsage == TextStyle.ImageUsageEnum.Bullet)
				bullet = style.ImageSize.X;
			if (word.Length == 1) {
				var c = font.Chars[word.Text[word.Start]];
				if (c == FontChar.Null) {
					return 0;
				}
				float fontScale = style.Size / c.Height;
				float width = bullet + (c.ACWidths.X + c.ACWidths.Y + c.Width) * fontScale;
				return width;
			} else {
				Vector2 size = Renderer.MeasureTextLine(font, word.Text, style.Size, word.Start, word.Length);
				size.X += bullet;
				return size.X;
			}
		}

		public void Render(SpriteList spriteList, Vector2 area, HAlignment hAlign, VAlignment vAlign)
		{
			List<Fragment> words;
			List<int> lines;
			float totalHeight;
			float longestLineWidth;
			PrepareWordsAndLines(area.X, out words, out lines, out totalHeight, out longestLineWidth);
			// Draw all lines.
			int b = 0;
			float y = 0;
			foreach (int count in lines) {
				// Calculate height and width of line in pixels.
				float maxHeight = 0;
				float totalWidth = 0;
				for (int j = 0; j < count; j++) {
					Fragment word = words[b + j];
					TextStyle style = styles[word.Style];
					maxHeight = Math.Max(maxHeight, style.Size + style.SpaceAfter);
					if (word.IsTagBegin) {
						maxHeight = Math.Max(maxHeight, style.ImageSize.Y + style.SpaceAfter);
					}
					totalWidth += word.Width;
				}
				// Calculate offset for horizontal alignment.
				var offset = new Vector2();
				if (hAlign == HAlignment.Right)
					offset.X = area.X - totalWidth;
				else if (hAlign == HAlignment.Center)
					offset.X = (area.X - totalWidth) * 0.5f;
				// Calculate offset for vertical alignment.
				if (vAlign == VAlignment.Bottom)
					offset.Y = area.Y - totalHeight;
				else if (vAlign == VAlignment.Center)
					offset.Y = (area.Y - totalHeight) * 0.5f;
				// Draw string.
				for (int j = 0; j < count; j++) {
					Fragment word = words[b + j];
					TextStyle style = styles[word.Style];
					Vector2 yOffset;
					Vector2 position = new Vector2(word.X, y) + offset;
					Font font = fonts[word.Style].Instance;
					if (word.IsTagBegin && style.ImageUsage == TextStyle.ImageUsageEnum.Bullet) {
						yOffset = new Vector2(0, (maxHeight - style.ImageSize.Y) * 0.5f);
						if (style.ImageTexture.SerializationPath != null) {
							spriteList.Add(style.ImageTexture, Color4.White, position + yOffset, style.ImageSize, Vector2.Zero, Vector2.One);
							// Renderer.DrawSprite(SpriteList, style.ImageTexture, color, position + yOffset, style.ImageSize, Vector2.Zero, Vector2.One);
						}
						position.X += style.ImageSize.X;
					}
					yOffset = new Vector2(0, (maxHeight - style.Size) * 0.5f);
					if (style.CastShadow) {
						for (int k = 0; k < (style.Bold ? 2 : 1); k++) {
							Renderer.DrawTextLine(spriteList, font, position + style.ShadowOffset + yOffset, word.Text, style.ShadowColor, style.Size, word.Start, word.Length);
						}
					}
					for (int k = 0; k < (style.Bold ? 2 : 1); k++) {
						Renderer.DrawTextLine(spriteList, font, position + yOffset, word.Text, style.TextColor, style.Size, word.Start, word.Length);
					}
				}
				// Draw overlays
				for (int j = 0; j < count; j++) {
					var word = words[b + j];
					TextStyle style = styles[word.Style];
					if (style.ImageUsage == TextStyle.ImageUsageEnum.Overlay) {
						int k = j + 1;
						for (; k < count; k++) {
							if (words[b + k].IsTagBegin)
								break;
						}
						k -= 1;
						Vector2 lt = new Vector2(words[b + j].X, y) + offset;
						Vector2 rb = new Vector2(words[b + k].X + words[b + k].Width, y) + offset;
						float yOffset = (maxHeight - style.ImageSize.Y) * 0.5f;
						spriteList.Add(style.ImageTexture, Color4.White, lt + new Vector2(0, yOffset),
							rb - lt + new Vector2(0, style.ImageSize.Y),
							Vector2.Zero, Vector2.One);
						j = k;
					}
				}
				y += maxHeight;
				b += count;
			}
		}

		public Vector2 MeasureText(float maxWidth)
		{
			List<Fragment> words;
			List<int> lines;
			float totalHeight;
			float longestLineWidth;
			PrepareWordsAndLines(maxWidth, out words, out lines, out totalHeight, out longestLineWidth);
			var extent = new Vector2(longestLineWidth, totalHeight);
			return extent;
		}

		private void PrepareWordsAndLines(float maxWidth, out List<Fragment> words, out List<int> lines, out float totalHeight, out float longestLineWidth)
		{
			words = new List<Fragment>();
			// Split whole text into words. Every whitespace, linebreak, etc. consider to be separate word.
			foreach (var fragment in fragments) {
				SplitFragmentIntoWords(words, fragment);
			}
			// Calculate words sizes and insert additional spaces to fit by width.
			longestLineWidth = 0;
			float x = 0;
			for (int i = 0; i < words.Count; i++) {
				Fragment word = words[i];
				word.Width = CalcWordWidth(word);
				if (word.LineBreak) {
					x = 0;
				}
				if (x > 0 && x + word.Width > maxWidth && word.Text[word.Start] > ' ') {
					x = word.Width;
					word.X = 0;
					word.LineBreak = true;
				} else {
					word.X = x;
					x += word.Width;
				}
				if (word.X + word.Width > longestLineWidth) {
					longestLineWidth = word.X + word.Width;
				}
				words[i] = word;
			}
			// Calculate word count for every string.
			lines = new List<int>();
			totalHeight = 0;
			float lineHeight = 0;
			int c = 0;
			foreach (Fragment word in words) {
				TextStyle style = styles[word.Style];
				if (word.LineBreak && c > 0) {
					totalHeight += lineHeight;
					lineHeight = 0;
					lines.Add(c);
					c = 0;
				}
				lineHeight = Math.Max(lineHeight, style.Size + style.SpaceAfter);
				if (word.IsTagBegin) {
					lineHeight = Math.Max(lineHeight, style.ImageSize.Y + style.SpaceAfter);
				}
				c++;
			}
			if (c > 0) {
				lines.Add(c);
				totalHeight += lineHeight;
			}
		}

		private static void SplitFragmentIntoWords(List<Fragment> words, Fragment fragment)
		{
			var word = fragment;
			int curr = word.Start;
			int start = word.Start;
			int length = word.Length;
			if (length == 0) {
				word.IsTagBegin = true;
				words.Add(word);
			} else {
				bool isTagBegin = true;
				while (curr < length) {
					bool lineBreak = false;
					if (word.Text[curr] <= ' ') {
						if (word.Text[curr] == '\n') {
							lineBreak = true;
						}
						curr++;
					} else {
						while (curr < length && word.Text[curr] > ' ') {
							curr++;
						}
					}
					word.Start = start;
					word.Length = curr - start;
					word.LineBreak = lineBreak;
					word.IsTagBegin = isTagBegin;
					words.Add(word);
					start = curr;
					isTagBegin = false;
				}
			}
		}
	}
}
