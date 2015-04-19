using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Lime.Text
{

	class TextRenderer
	{
		private readonly List<Fragment> words = new List<Fragment>();
		private readonly List<TextStyle> styles = new List<TextStyle>();
		private readonly List<string> texts = new List<string>();

		private float scaleFactor = 1.0f;
		private readonly TextOverflowMode overflowMode;
		private readonly bool wordSplitAllowed;

		private class Fragment
		{
			public int TextIndex;
			public int Style;
			public int Start;
			public int Length;
			public float X;
			public float Width;
			public bool LineBreak; // Line break before the fragment
			public bool IsTagBegin;
			public Fragment Clone() { return (Fragment)MemberwiseClone(); }
		};

		public TextRenderer(TextOverflowMode overflowMode, bool wordSplitAllowed)
		{
			this.overflowMode = overflowMode;
			this.wordSplitAllowed = wordSplitAllowed;
		}

		public int AddText(string text)
		{
			int i = texts.IndexOf(text);
			if (i >= 0)
				return i;
			texts.Add(String.Intern(text));
			return texts.Count - 1;
		}

		public void AddFragment(string text, int style)
		{
			var word = new Fragment {
				TextIndex = AddText(text),
				Style = style,
				Start = 0,
				Length = text.Length,
				X = 0,
				Width = 0,
				LineBreak = false,
				IsTagBegin = true,
			};
			int length = word.Length;
			if (length == 0) {
				words.Add(word);
				return;
			}
			int curr = word.Start;
			int start = word.Start;
			var t = texts[word.TextIndex];
			while (curr < length) {
				bool lineBreak = false;
				if (t[curr] <= ' ') {
					if (t[curr] == '\n') {
						lineBreak = true;
					}
					curr++;
				}
				else {
					while (curr < length && t[curr] > ' ') {
						curr++;
					}
				}
				word = word.Clone();
				word.Start = start;
				word.Length = curr - start;
				word.LineBreak = lineBreak;
				words.Add(word);
				word.IsTagBegin = false;
				start = curr;
			}
		}

		public void AddStyle(TextStyle style)
		{
			styles.Add(style);
		}

		public bool HasStyle(TextStyle style)
		{
			return styles.Contains(style);
		}

		float CalcWordWidth(Fragment word)
		{
			TextStyle style = styles[word.Style];
			var font = style.Font.Instance;
			float bullet = 0;
			if (word.IsTagBegin && style.ImageUsage == TextStyle.ImageUsageEnum.Bullet)
				bullet = style.ImageSize.X * scaleFactor;
			var t = texts[word.TextIndex];
			if (word.Length == 1) {
				var c = font.Chars[t[word.Start]];
				if (c == FontChar.Null) {
					return 0;
				}
				float fontScale = (style.Size * scaleFactor) / c.Height;
				float width = bullet + (c.ACWidths.X + c.ACWidths.Y + c.Width) * fontScale;
				return width;
			} else {
				Vector2 size = Renderer.MeasureTextLine(font, t, style.Size * scaleFactor, word.Start, word.Length);
				size.X += bullet;
				return size.X;
			}
		}

		public void Render(SpriteList spriteList, Vector2 area, HAlignment hAlign, VAlignment vAlign)
		{
			if (overflowMode == TextOverflowMode.Minify) {
				FitTextInsideArea(area);
			}
			List<int> lines;
			float totalHeight;
			float longestLineWidth;
			PrepareWordsAndLines(area.X, area.Y, out lines, out totalHeight, out longestLineWidth);
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
					var t = texts[word.TextIndex];
					TextStyle style = styles[word.Style];
					Vector2 position = new Vector2(word.X, y) + offset;
					if (
						word.IsTagBegin && style.ImageUsage == TextStyle.ImageUsageEnum.Bullet &&
						!String.IsNullOrEmpty(style.ImageTexture.SerializationPath)
					) {
						var sz = style.ImageSize * scaleFactor;
						spriteList.Add(
							style.ImageTexture, Color4.White, position + new Vector2(0, (maxHeight - sz.Y) * 0.5f),
							sz, Vector2.Zero, Vector2.One, tag: word.Style);
						position.X += sz.X;
					}
					var yOffset = new Vector2(0, (maxHeight - style.Size * scaleFactor) * 0.5f);
					var font = style.Font.Instance;
					if (style.CastShadow) {
						for (int k = 0; k < (style.Bold ? 2 : 1); k++) {
							Renderer.DrawTextLine(
								font, position + style.ShadowOffset + yOffset, t, style.ShadowColor, style.Size * scaleFactor,
								word.Start, word.Length, spriteList, tag: word.Style);
						}
					}
					for (int k = 0; k < (style.Bold ? 2 : 1); k++) {
						Renderer.DrawTextLine(
							font, position + yOffset, t, style.TextColor, style.Size * scaleFactor,
							word.Start, word.Length, spriteList, tag: word.Style);
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
							Vector2.Zero, Vector2.One, tag: word.Style);
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
			List<int> lines;
			float totalHeight;
			float longestLineWidth;
			PrepareWordsAndLines(maxWidth, maxHeight, out lines, out totalHeight, out longestLineWidth);
			var extent = new Vector2(longestLineWidth, totalHeight);
			return extent;
		}

		private void PrepareWordsAndLines(
			float maxWidth, float maxHeight, out List<int> lines, out float totalHeight, out float longestLineWidth)
		{
			PositionWordsHorizontally(maxWidth, out longestLineWidth);

			// Calculate word count for every string.
			lines = new List<int>();
			totalHeight = 0;
			float lineHeight = 0;
			int c = 0;
			foreach (Fragment word in words) {
				TextStyle style = styles[word.Style];
				if (word.LineBreak && c > 0) {
					if (overflowMode == TextOverflowMode.Ellipsis && totalHeight + lineHeight > maxHeight) {
						ClipLastLineWithEllipsis(lines, maxWidth);
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
					ClipLastLineWithEllipsis(lines, maxWidth);
				} else {
					totalHeight += lineHeight;
					lines.Add(c);
				}
			}
		}

		private void ClipLastLineWithEllipsis(List<int> lines, float maxWidth)
		{
			int firstWordInLastLineIndex = 0;
			for (int i = 0; i < lines.Count - 1; i++) {
				firstWordInLastLineIndex += lines[i];
			}
			int lastWordInLastLine = firstWordInLastLineIndex + lines[lines.Count - 1] - 1;
			while (true) {
				var word = words[lastWordInLastLine];
				var style = styles[word.Style];
				var font = style.Font.Instance;
				var t = texts[word.TextIndex];
				float dotsWidth = Renderer.MeasureTextLine(font, "...", style.Size * scaleFactor).X;
				if (
					lastWordInLastLine > firstWordInLastLineIndex 
					&& (
						word.X + Renderer.MeasureTextLine(
							font, t.Substring(word.Start, 1), style.Size * scaleFactor).X + dotsWidth > maxWidth
						|| (word.Length == 1 && t[word.Start] == ' ')
					)
				) {
					lastWordInLastLine -= 1;
					lines[lines.Count - 1] -= 1;
				} else {
					break;
				}
			}
			ClipWordWithEllipsis(words[lastWordInLastLine], maxWidth);
		}

		private void PositionWordsHorizontally(float maxWidth, out float longestLineWidth)
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
				var t = texts[word.TextIndex];
				var isText = t[word.Start] > ' ';
				if (isLongerThanWidth && isText && (wordSplitAllowed || t.HasJapaneseSymbols(word.Start, word.Length))) {
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
						ClipWordWithEllipsis(word, maxWidth);
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
			var style = styles[word.Style];
			var font = style.Font.Instance;
			var t = texts[word.TextIndex];
			do {
				mid = min + ((max - min) / 2);
				var w = Renderer.MeasureTextLine(font, t, style.Size * scaleFactor, word.Start, mid).X;
				isLineLonger = word.X + w > maxWidth;
				if (isLineLonger) {
					max = mid;
				} else {
					min = mid;
				}
			}
			while (min < max && !(!isLineLonger && ((max - min) / 2) == 0));

			return mid;
		}

		private void ClipWordWithEllipsis(Fragment word, float maxWidth)
		{
			var style = styles[word.Style];
			float dotsWidth = Renderer.MeasureTextLine(style.Font.Instance, "...", style.Size * scaleFactor).X;
			while (word.Length > 1 && word.X + word.Width + dotsWidth > maxWidth) {
				word.Length--;
				word.Width = CalcWordWidth(word);
			}
			var t = texts[word.TextIndex];
			var newText = t.Substring(word.Start, word.Length) + "...";
			word.TextIndex = AddText(newText);
			word.Start = 0;
			word.Length = newText.Length;
		}

	}
}
