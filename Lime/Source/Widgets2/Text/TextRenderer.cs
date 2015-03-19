using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Lime.Widgets2.Text
{

	class TextRenderer
	{
		readonly List<Fragment> fragments = new List<Fragment>();
		readonly List<TextStyle> styles = new List<TextStyle>();
		readonly List<SerializableFont> fonts = new List<SerializableFont>();

		private float scaleFactor = 1.0f;
		private TextOverflowMode overflowMode;
		private bool wordSplitAllowed;

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

		public TextRenderer(TextOverflowMode overflowMode, bool wordSplitAllowed)
		{
			this.overflowMode = overflowMode;
			this.wordSplitAllowed = wordSplitAllowed;
		}

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
				bullet = style.ImageSize.X * scaleFactor;
			if (word.Length == 1) {
				var c = font.Chars[word.Text[word.Start]];
				if (c == FontChar.Null) {
					return 0;
				}
				float fontScale = (style.Size * scaleFactor) / c.Height;
				float width = bullet + (c.ACWidths.X + c.ACWidths.Y + c.Width) * fontScale;
				return width;
			} else {
				Vector2 size = Renderer.MeasureTextLine(font, word.Text, style.Size * scaleFactor, word.Start, word.Length);
				size.X += bullet;
				return size.X;
			}
		}

		public void Render(SpriteList spriteList, Vector2 area, HAlignment hAlign, VAlignment vAlign)
		{
			if (overflowMode == TextOverflowMode.Minify) {
				FitTextInsideArea(area);
			}
			List<Fragment> words;
			List<int> lines;
			float totalHeight;
			float longestLineWidth;
			PrepareWordsAndLines(area.X, area.Y, out words, out lines, out totalHeight, out longestLineWidth);
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
					maxHeight = Math.Max(maxHeight, (style.Size + style.SpaceAfter) * scaleFactor);
					if (word.IsTagBegin) {
						maxHeight = Math.Max(maxHeight, (style.ImageSize.Y + style.SpaceAfter) * scaleFactor);
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
						yOffset = new Vector2(0, (maxHeight - style.ImageSize.Y * scaleFactor) * 0.5f);
						if (style.ImageTexture.SerializationPath != null) {
							spriteList.Add(style.ImageTexture, Color4.White, position + yOffset, style.ImageSize * scaleFactor, Vector2.Zero, Vector2.One, word.Style);
							// Renderer.DrawSprite(SpriteList, style.ImageTexture, color, position + yOffset, style.ImageSize, Vector2.Zero, Vector2.One);
						}
						position.X += style.ImageSize.X * scaleFactor;
					}
					yOffset = new Vector2(0, (maxHeight - style.Size * scaleFactor) * 0.5f);
					if (style.CastShadow) {
						for (int k = 0; k < (style.Bold ? 2 : 1); k++) {
							Renderer.DrawTextLine(font, position + style.ShadowOffset + yOffset, word.Text, style.ShadowColor, style.Size * scaleFactor, word.Start, word.Length, spriteList);
						}
					}
					for (int k = 0; k < (style.Bold ? 2 : 1); k++) {
						Renderer.DrawTextLine(font, position + yOffset, word.Text, style.TextColor, style.Size * scaleFactor, word.Start, word.Length, spriteList);
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
						float yOffset = (maxHeight - style.ImageSize.Y * scaleFactor) * 0.5f;
						spriteList.Add(style.ImageTexture, Color4.White, lt + new Vector2(0, yOffset),
							rb - lt + new Vector2(0, style.ImageSize.Y),
							Vector2.Zero, Vector2.One, word.Style);
						j = k;
					}
				}
				y += maxHeight;
				b += count;
			}
		}

		public void FitTextInsideArea(Vector2 size)
		{
			var minScale = 0.0f;
			var maxScale = 1.0f;
			scaleFactor = maxScale;
			float bestScaleFactor = minScale;
			while (maxScale - minScale >= 0.1f) {
				var textSize = MeasureText(size.X, size.Y);
				var fit = (textSize.X <= size.X && textSize.Y <= size.Y);
				if (fit) {
					minScale = scaleFactor;
					bestScaleFactor = Mathf.Max(bestScaleFactor, scaleFactor);
				} else {
					maxScale = scaleFactor;
				}
				scaleFactor = (minScale + maxScale) / 2;
			}
			scaleFactor = bestScaleFactor;
		}

		public Vector2 MeasureText(float maxWidth, float maxHeight)
		{
			List<Fragment> words;
			List<int> lines;
			float totalHeight;
			float longestLineWidth;
			PrepareWordsAndLines(maxWidth, maxHeight, out words, out lines, out totalHeight, out longestLineWidth);
			var extent = new Vector2(longestLineWidth, totalHeight);
			return extent;
		}

		private void PrepareWordsAndLines(float maxWidth, float maxHeight, out List<Fragment> words, out List<int> lines, out float totalHeight, out float longestLineWidth)
		{
			words = GetWords();
			PositionWordsHorizontally(maxWidth, words, out longestLineWidth);

			// Calculate word count for every string.
			lines = new List<int>();
			totalHeight = 0;
			float lineHeight = 0;
			int c = 0;
			foreach (Fragment word in words) {
				TextStyle style = styles[word.Style];
				if (word.LineBreak && c > 0) {
					if (overflowMode == TextOverflowMode.Ellipsis && totalHeight + lineHeight > maxHeight) {
						ClipLastLineWithEllipsis(words, lines, maxWidth);
						c = 0;
						break;
					} else {
						totalHeight += lineHeight;
						lines.Add(c);
						lineHeight = 0;
						c = 0;
					}
				}
				lineHeight = Math.Max(lineHeight, (style.Size + style.SpaceAfter) * scaleFactor);
				if (word.IsTagBegin) {
					lineHeight = Math.Max(lineHeight, (style.ImageSize.Y + style.SpaceAfter) * scaleFactor);
				}
				c++;
			}
			if (c > 0) {
				if (overflowMode == TextOverflowMode.Ellipsis && totalHeight + lineHeight > maxHeight) {
					ClipLastLineWithEllipsis(words, lines, maxWidth);
				} else {
					totalHeight += lineHeight;
					lines.Add(c);
				}
			}
		}

		private void ClipLastLineWithEllipsis(List<Fragment> words, List<int> lines, float maxWidth)
		{
			int firstWordInLastLineIndex = 0;
			for (int i = 0; i < lines.Count - 1; i++) {
				firstWordInLastLineIndex += lines[i];
			}
			int lastWordInLastLine = firstWordInLastLineIndex + lines[lines.Count - 1] - 1;
			while (true) {
				var word = words[lastWordInLastLine];
				var font = fonts[word.Style].Instance;
				var style = styles[word.Style];
				float dotsWidth = Renderer.MeasureTextLine(font, "...", style.Size * scaleFactor).X;
				if (
					lastWordInLastLine > firstWordInLastLineIndex 
					&& (
						word.X + Renderer.MeasureTextLine(font, word.Text.Substring(word.Start, 1), style.Size * scaleFactor).X + dotsWidth > maxWidth
						|| (word.Length == 1 && word.Text[word.Start] == ' ')
					)
				) {
					lastWordInLastLine -= 1;
					lines[lines.Count - 1] -= 1;
				} else {
					break;
				}
			}
			words[lastWordInLastLine] = ClipWordWithEllipsis(words[lastWordInLastLine], maxWidth);
		}

		private void PositionWordsHorizontally(float maxWidth, List<Fragment> words, out float longestLineWidth)
		{
			longestLineWidth = 0;
			float x = 0;
			for (int i = 0; i < words.Count; i++) {
				Fragment word = words[i];
				word.Width = CalcWordWidth(word);
				if (word.LineBreak) {
					x = 0;
				}
				var isLongerThanWidth = x + word.Width > maxWidth;
				var isText = word.Text[word.Start] > ' ';
				if (isLongerThanWidth && isText && (wordSplitAllowed || word.Text.HasJapaneseSymbols(word.Start, word.Length))) {
					var fittedCharsCount = CalcFittedCharactersCount(word, maxWidth - x);
					if (fittedCharsCount > 0) {
						var newWord = word;
						newWord.Start = word.Start + fittedCharsCount;
						newWord.Length = word.Length - fittedCharsCount;
						newWord.Width = CalcWordWidth(newWord);
						newWord.LineBreak = true;
						word.Length = fittedCharsCount;
						word.Width = CalcWordWidth(word);
						word.X = x;
						x += word.Width;
						words.Insert(i + 1, newWord);
						goto skip_default_placement;
					}
				}

				if (isLongerThanWidth && isText && x > 0) {
					x = word.Width;
					word.X = 0;
					word.LineBreak = true;
				} else {
					word.X = x;
					x += word.Width;
				}
			skip_default_placement:

				if (overflowMode == TextOverflowMode.Ellipsis) {
					if (word.X == 0 && word.Width > maxWidth) {
						word = ClipWordWithEllipsis(word, maxWidth);
					}
				}
				words[i] = word;
				if (isText) { // buz: при автопереносе на концах строк остаются пробелы, они не должны влиять на значение длины строки
					longestLineWidth = Math.Max(longestLineWidth, word.X + word.Width);
				}
			}
		}

		private int CalcFittedCharactersCount(Fragment word, float maxWidth)
		{
			int min = 0;
			int max = word.Length;
			int mid = 0;
			bool isLineLonger = false;
			var font = fonts[word.Style].Instance;
			var style = styles[word.Style];
			do {
				mid = min + ((max - min) / 2);
				isLineLonger = word.X + Renderer.MeasureTextLine(font, word.Text, style.Size * scaleFactor, word.Start, mid).X > maxWidth;
				if (isLineLonger) {
					max = mid;
				} else {
					min = mid;
				}
			}
			while (min < max && !(!isLineLonger && ((max - min) / 2) == 0));

			return mid;
		}

		private Fragment ClipWordWithEllipsis(Fragment word, float maxWidth)
		{
			var font = fonts[word.Style].Instance;
			var style = styles[word.Style];
			float dotsWidth = Renderer.MeasureTextLine(font, "...", style.Size * scaleFactor).X;
			while (word.Length > 1 && word.X + word.Width + dotsWidth > maxWidth) {
				word.Length -= 1;
				word.Width = CalcWordWidth(word);
			}
			word.Text = word.Text.Substring(word.Start, word.Length) + "...";
			word.Start = 0;
			word.Length = word.Text.Length;
			return word;
		}

		private List<Fragment> GetWords()
		{
			var words = new List<Fragment>();
			foreach (var fragment in fragments) {
				SplitFragmentIntoWords(words, fragment);
			}
			return words;
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
