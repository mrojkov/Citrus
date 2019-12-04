using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime.Text
{
	class TextRenderer
	{
		private readonly List<Word> words = new List<Word>();
		private readonly List<Word> fittedWords = new List<Word>();
		public readonly List<TextStyle> Styles = new List<TextStyle>();
		private readonly List<string> texts = new List<string>();

		private float scaleFactor = 1.0f;
		private readonly TextOverflowMode overflowMode;
		private readonly bool wordSplitAllowed;

		private class Word
		{
			public int TextIndex;
			public int Style;
			public int Start;
			public int Length;
			public bool ForceLineBreak; // Line break come from text. It applies to the beginning of the word.
			public bool IsTagBegin;
			// Following fields are mutable:
			public float X;
			public float Width;
			public bool LineBreak; // Effective line break after the text formatting.
			public bool IsNbsp;
			public Word Clone() { return (Word)MemberwiseClone(); }
		};

		public TextRenderer(TextOverflowMode overflowMode, bool wordSplitAllowed)
		{
			this.overflowMode = overflowMode;
			this.wordSplitAllowed = wordSplitAllowed;
		}

		private int AddText(string text)
		{
			int i = texts.IndexOf(text);
			if (i >= 0)
				return i;
			texts.Add(text);
			return texts.Count - 1;
		}

		public void AddFragment(string text, int style, bool isNbsp)
		{
			var word = new Word {
				TextIndex = AddText(text),
				Style = style,
				Start = 0,
				Length = text.Length,
				X = 0,
				Width = 0,
				ForceLineBreak = false,
				IsTagBegin = !isNbsp,
				IsNbsp = isNbsp,
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
				word.Start = start;
				word.Length = curr - start;
				word.ForceLineBreak = lineBreak;
				words.Add(word);
				word = word.Clone();
				word.IsTagBegin = false;
				start = curr;
			}
		}

		public void AddStyle(TextStyle style)
		{
			Styles.Add(style);
		}

		public bool HasStyle(TextStyle style)
		{
			return Styles.Contains(style);
		}

		private bool IsBullet(Word word)
		{
			return
				word.IsTagBegin &&
				Styles[word.Style].ImageUsage == TextStyle.ImageUsageEnum.Bullet &&
				Styles[word.Style].ImageTexture != null &&
				Styles[word.Style].ImageSize.X != 0 &&
				Styles[word.Style].ImageSize.Y != 0;
		}

		private bool IsOverlay(Word word)
		{
			return
				Styles[word.Style].ImageUsage == TextStyle.ImageUsageEnum.Overlay &&
				Styles[word.Style].ImageTexture != null &&
				Styles[word.Style].ImageSize.X != 0 &&
				Styles[word.Style].ImageSize.Y != 0;
		}

		private char GetLastChar(string s)
		{
			return s.Length > 0 ? s[s.Length - 1] : char.MinValue;
		}

		float CalcWordWidth(Word word)
		{
			var style = Styles[word.Style];
			Vector2 size = style.Font.MeasureTextLine(
				texts[word.TextIndex], ScaleSize(style.Size), word.Start, word.Length, style.LetterSpacing + style.Font.Spacing);
			return size.X + (IsBullet(word) ? ScaleSize(style.ImageSize.X) : 0);
		}

		private float ScaleSize(float size)
		{
			return (size * scaleFactor).Floor();
		}

		public void Render(SpriteList[] spriteLists, Vector2 area, HAlignment hAlign, VAlignment vAlign, int maxCharacters = -1)
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
			int c = 0;
			foreach (int count in lines) {
				// Calculate height and width of line in pixels.
				float maxHeight = 0;
				float totalWidth = 0;
				for (int j = 0; j < count; j++) {
					var word = fittedWords[b + j];
					var style = Styles[word.Style];
					maxHeight = Math.Max(maxHeight, ScaleSize(style.Size + style.SpaceAfter));
					if (word.IsTagBegin) {
						maxHeight = Math.Max(maxHeight, ScaleSize(style.ImageSize.Y + style.SpaceAfter));
					}
					var ch = texts[word.TextIndex].Length > 0 ? texts[word.TextIndex][word.Start] : 0;
					if (!(j == count - 1 && (ch == ' ' || ch == '\n') && !IsBullet(word))) {
						totalWidth += word.Width;
					}
				}
				// Calculate offset for horizontal alignment.
				var offset = new Vector2();
				if (hAlign == HAlignment.Right)
					offset.X = area.X - totalWidth;
				else if (hAlign == HAlignment.Center)
					offset.X = ((area.X - totalWidth) * 0.5f).Round();
				// Calculate offset for vertical alignment.
				if (vAlign == VAlignment.Bottom)
					offset.Y = area.Y - totalHeight;
				else if (vAlign == VAlignment.Center)
					offset.Y = ((area.Y - totalHeight) * 0.5f).Round();
				// Draw string.
				for (int j = 0; j < count; j++) {
					var word = fittedWords[b + j];
					var t = texts[word.TextIndex];
					TextStyle style = Styles[word.Style];
					Vector2 position = new Vector2(word.X, y) + offset;
					if (IsBullet(word)) {
						if (maxCharacters >= 0 && c >= maxCharacters) {
							break;
						}
						var sz = style.ImageSize * scaleFactor;
						spriteLists[word.Style].Add(
							style.ImageTexture, Color4.White, position + new Vector2(0, (maxHeight - sz.Y) * 0.5f),
							sz, Vector2.Zero, Vector2.One, tag: word.Style);
						position.X += sz.X;
						c++;
					}
					var yOffset = new Vector2(0, (maxHeight - ScaleSize(style.Size)) * 0.5f);
					var font = style.Font;
					if (style.CastShadow) {
						for (int k = 0; k < (style.Bold ? 2 : 1); k++) {
							Renderer.DrawTextLine(
								font, position + style.ShadowOffset + yOffset, t, style.ShadowColor, ScaleSize(style.Size),
								word.Start, word.Length, font.Spacing + style.LetterSpacing, spriteLists[word.Style], tag: word.Style
							);
						}
					}
					int wordLength = word.Length;
					if (maxCharacters >= 0) {
						if (c >= maxCharacters) {
							break;
						}
						wordLength = wordLength.Min(maxCharacters - c);
					}
					for (int k = 0; k < (style.Bold ? 2 : 1); k++) {
						Renderer.DrawTextLine(
							font, position + yOffset, t, style.TextColor, ScaleSize(style.Size),
							word.Start, wordLength, font.Spacing + style.LetterSpacing, spriteLists[word.Style], tag: word.Style
						);
					}
					c += wordLength;
				}
				// Draw overlays
				for (int j = 0; j < count; j++) {
					var word = fittedWords[b + j];
					TextStyle style = Styles[word.Style];
					if (IsOverlay(word)) {
						int k = j + 1;
						for (; k < count; k++) {
							if (fittedWords[b + k].IsTagBegin)
								break;
						}
						k -= 1;
						var font = Styles[word.Style].Font;
						var fontHeight = Styles[word.Style].Size;
						var fontChar = font.CharSource.Get(texts[word.TextIndex][word.Start], fontHeight);
						if (fontChar == FontChar.Null) {
							continue;
						}
						float scale = fontChar.Height != 0.0f ? fontHeight / fontChar.Height : 0.0f;
						Vector2 lt = new Vector2(word.X + (word.X > 0 ? scale * Styles[word.Style].LetterSpacing : 0.0f), y) + offset;
						Vector2 rb = new Vector2(fittedWords[b + k].X + fittedWords[b + k].Width, y) + offset;
						float yOffset = (maxHeight - ScaleSize(style.ImageSize.Y)) * 0.5f;
						spriteLists[word.Style].Add(style.ImageTexture, Color4.White, lt + new Vector2(0, yOffset),
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

		public int CalcNumCharacters(Vector2 area)
		{
			List<int> lines;
			float totalHeight;
			float longestLineWidth;
			PrepareWordsAndLines(area.X, area.Y, out lines, out totalHeight, out longestLineWidth);

			int result = 0;
			int b = 0;
			foreach (int count in lines) {
				for (int j = 0; j < count; j++) {
					var word = fittedWords[b + j];
					TextStyle style = Styles[word.Style];
					if (IsBullet(word) && style.ImageTexture != null) {
						result++;
					}
					result += word.Length;
				}
				// buz: с оверлеями пока лень разбираться
				b += count;
			}
			return result;
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
			foreach (var word in fittedWords) {
				var style = Styles[word.Style];
				if (word.LineBreak && c > 0) {
					if (overflowMode == TextOverflowMode.Ellipsis && totalHeight + lineHeight > maxHeight) {
						ClipLastLineWithEllipsis(lines, fittedWords, maxWidth);
						c = 0;
						break;
					} else {
						totalHeight += lineHeight;
						lines.Add(c);
						lineHeight = 0;
						c = 0;
					}
				}
				lineHeight = Math.Max(lineHeight, ScaleSize(style.Size + style.SpaceAfter));
				if (word.IsTagBegin) {
					lineHeight = Math.Max(lineHeight, ScaleSize(style.ImageSize.Y + style.SpaceAfter));
				}
				c++;
			}
			if (c > 0) {
				if (overflowMode == TextOverflowMode.Ellipsis && totalHeight + lineHeight > maxHeight) {
					ClipLastLineWithEllipsis(lines, fittedWords, maxWidth);
				} else {
					totalHeight += lineHeight;
					lines.Add(c);
				}
			}
		}

		private void ClipLastLineWithEllipsis(List<int> lines, List<Word> fittedWords, float maxWidth)
		{
			int firstWordInLastLineIndex = 0;
			for (int i = 0; i < lines.Count - 1; i++) {
				firstWordInLastLineIndex += lines[i];
			}
			int lastWordInLastLine = firstWordInLastLineIndex + lines[lines.Count - 1] - 1;
			while (true) {
				var word = fittedWords[lastWordInLastLine];
				var style = Styles[word.Style];
				var font = style.Font;
				var t = texts[word.TextIndex];
				float dotsWidth = font.MeasureTextLine("...", ScaleSize(style.Size), style.LetterSpacing + font.Spacing).X;
				if (
					lastWordInLastLine > firstWordInLastLineIndex
					&& (
						word.X + font.MeasureTextLine(
							word.Length > 0 ? t.Substring(word.Start, 1) : string.Empty, ScaleSize(style.Size), style.LetterSpacing + font.Spacing).X + dotsWidth > maxWidth
						|| (word.Length == 1 && t[word.Start] == ' ')
					)
				) {
					lastWordInLastLine -= 1;
					lines[lines.Count - 1] -= 1;
				} else {
					break;
				}
			}
			ClipWordWithEllipsis(fittedWords[lastWordInLastLine], maxWidth);
		}

		private void PositionWordsHorizontally(float maxWidth, out float longestLineWidth)
		{
			fittedWords.Clear();
			fittedWords.AddRange(words);
			longestLineWidth = 0;
			float x = 0;
			for (int i = 0; i < fittedWords.Count; i++) {
				var word = fittedWords[i];
				word = word.Clone();
				word.LineBreak = word.ForceLineBreak;
				if (word.LineBreak) {
					x = 0;
				}
				word.Width = CalcWordWidth(word);
				var isLongerThanWidth = x + word.Width > maxWidth;
				var t = texts[word.TextIndex];
				var isTextOrBullet = (t.Length > 0 && t[word.Start] > ' ') || IsBullet(word);
				if (isLongerThanWidth && isTextOrBullet && (wordSplitAllowed || t.HasJapaneseSymbols(word.Start, word.Length))) {
					var fittedCharsCount = CalcFittedCharactersCount(word, maxWidth - x);
					if (fittedCharsCount > 0) {
						var wordEnd = word.Start + fittedCharsCount;
						Toolbox.AdjustLineBreakPosition(t, ref wordEnd);
						if (wordEnd > word.Start)
							fittedCharsCount = wordEnd - word.Start;
						var newWord = word.Clone();
						newWord.IsTagBegin = false;
						newWord.Start = word.Start + fittedCharsCount;
						newWord.Length = word.Length - fittedCharsCount;
						newWord.Width = CalcWordWidth(newWord);
						newWord.ForceLineBreak = true;
						word.Length = fittedCharsCount;
						word.Width = CalcWordWidth(word);
						word.X = x;
						fittedWords.Insert(i + 1, newWord);
						goto skip_default_placement;
					}
				}

				if (isLongerThanWidth && isTextOrBullet && x > 0 && !fittedWords[i - 1].IsNbsp) {
					var isWordContinue =
						word.TextIndex > 0 &&
						word.Start == 0 &&
						t.Length > 0 &&
						t[word.Start] > ' ' &&
						(GetLastChar(texts[fittedWords[i - 1].TextIndex]) > ' ' || IsBullet(fittedWords[i - 1]));
					if (isWordContinue) {
						var prev = fittedWords[i - 1];
						prev.X = 0;
						prev.LineBreak = true;
						word.X = prev.Width;
						x = word.X + word.Width;
						goto skip_default_placement;
					}
					word.X = 0;
					word.LineBreak = true;
					word.Width = CalcWordWidth(word);
					x = word.Width;
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
				if (isTextOrBullet) { // buz: при автопереносе на концах строк остаются пробелы, они не должны влиять на значение длины строки
					longestLineWidth = Math.Max(longestLineWidth, word.X + word.Width);
				}
				fittedWords[i] = word;
			}
		}

		private int CalcFittedCharactersCount(Word word, float maxWidth)
		{
			int min = 0;
			int max = word.Length;
			int mid = 0;
			bool isLineLonger = false;
			var style = Styles[word.Style];
			var font = style.Font;
			var t = texts[word.TextIndex];
			do {
				mid = min + ((max - min) / 2);
				var w = font.MeasureTextLine(t, ScaleSize(style.Size), word.Start, mid, style.LetterSpacing + font.Spacing).X + (IsBullet(word) ? style.ImageSize.X : 0);
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

		private void ClipWordWithEllipsis(Word word, float maxWidth)
		{
			var style = Styles[word.Style];
			float dotsWidth = style.Font.MeasureTextLine("...", ScaleSize(style.Size), style.LetterSpacing + style.Font.Spacing).X;
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
