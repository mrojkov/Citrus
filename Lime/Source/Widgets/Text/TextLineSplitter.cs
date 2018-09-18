using System;
using System.Collections.Generic;

namespace Lime
{
	internal static class TextLineSplitter
	{
		public delegate bool MeasureTextLineWidthDelegate(string line, int start, int count);

		public static bool CarryLastWordToNextLine(List<string> strings, int line, bool isWordSplitAllowed, MeasureTextLineWidthDelegate measureHandler)
		{
			string lastWord;
			string lineWithoutLastWord;
			if (TrySplitLine(strings[line], isWordSplitAllowed, measureHandler, out lineWithoutLastWord, out lastWord)) {
				PushWordToLine(lastWord, strings, line + 1);
				strings[line] = lineWithoutLastWord;
				return true;
			} else {
				return false;
			}
		}

		private static bool TrySplitLine(string line, bool isWordSplitAllowed, MeasureTextLineWidthDelegate measureHandler, out string lineWithoutLastWord, out string lastWord)
		{
			return
				TryCutLastWord(line, out lineWithoutLastWord, out lastWord)
				|| (
					(isWordSplitAllowed || line.HasJapaneseSymbols())
					&& TryCutWordTail(line, measureHandler, out lineWithoutLastWord, out lastWord)
					);
		}

		private static bool TryCutLastWord(string text, out string lineWithoutLastWord, out string lastWord)
		{
			lineWithoutLastWord = null;
			lastWord = null;
			// Use Line Separator character as a soft break
			int lastSpaceAt = Math.Max(text.LastIndexOf(' '), text.LastIndexOf((char)8232));
			if (lastSpaceAt <= 0) {
				return false;
			}
			if (lastSpaceAt == text.Length - 1) {
				// Treat a space character as a word
				lastWord = text.Substring(lastSpaceAt);
				lineWithoutLastWord = text.Substring(0, lastSpaceAt);
			} else {
				lineWithoutLastWord = text.Substring(0, lastSpaceAt + 1);
				lastWord = text.Substring(lastSpaceAt + 1);
			}
			return true;
		}

		private static bool TryCutWordTail(string textLine, MeasureTextLineWidthDelegate measureHandler, out string currentLinePart, out string nextLinePart)
		{
			currentLinePart = null;
			nextLinePart = null;
			var cutFrom = CalcFittedCharactersCount(textLine, measureHandler);
			if (cutFrom > 0) {
				Toolbox.AdjustLineBreakPosition(textLine, ref cutFrom);
				nextLinePart = textLine.Substring(cutFrom);
				currentLinePart = textLine.Substring(0, cutFrom);
				return true;
			} else {
				return false;
			}
		}

		private static int CalcFittedCharactersCount(string textLine, MeasureTextLineWidthDelegate measureHandler)
		{
			int min = 0;
			int max = textLine.Length;
			int mid = 0;
			bool isLineLonger = false;

			do {
				mid = min + ((max - min) / 2);
				isLineLonger = !measureHandler(textLine, 0, mid);
				if (isLineLonger) {
					max = mid;
				} else {
					min = mid;
				}
			}
			while (min < max && !(!isLineLonger && ((max - min) / 2) == 0));

			return mid;
		}

		private static void PushWordToLine(string word, List<string> strings, int line)
		{
			if (line >= strings.Count || EndsWith(word, '\n')) {
				strings.Insert(line, word);
			} else {
				strings[line] = word + strings[line];
			}
		}

		private static bool EndsWith(string s, char c)
		{
			int i = s.Length;
			return i > 0 && s[i - 1] == c;
		}
	}
}