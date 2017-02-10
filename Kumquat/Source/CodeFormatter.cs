using System;
using System.Collections.Generic;
using System.Linq;

namespace Kumquat
{
	public class CodeFormatter
	{
		private readonly List<string> lines;

		public string Code { get { return GetCode(); } }
		public string FormattedCode
		{
			get
			{
				FullFormat();
				return GetCode();
			}
		}

		public CodeFormatter(string code)
		{
			lines = code
			        .Split('\n')
					.ToList();
		}

		public void FullFormat()
		{
			TrimLines();
			RemoveRedundantBlankLines();
			AddIndents();
		}

		public void TrimLines()
		{
			for (var i = 0; i < lines.Count; i++) {
				lines[i] = lines[i].Trim();
			}
		}

		public void RemoveRedundantBlankLines()
		{
			var isLastLineEmpty = true;
			for (var i = 0; i < lines.Count; i++) {
				var isLineEmpty = string.IsNullOrEmpty(lines[i]);
				if (isLineEmpty && isLastLineEmpty) {
					lines.RemoveAt(i);
				}

				isLastLineEmpty = isLineEmpty;
			}

			for (var i = 1; i < lines.Count; i++) {
				var indentOff = MatchesCountInARow(lines[i], "}");
				if (indentOff == 0 || !string.IsNullOrEmpty(lines[i - 1])) {
					continue;
				}

				lines.RemoveAt(i - 1);
				++i;
			}

			if (lines.Count > 0 && string.IsNullOrEmpty(lines[lines.Count - 1])) {
				lines.RemoveAt(lines.Count - 1);
			}
		}

		public void AddIndents()
		{
			var indent = 0;
			for (var i = 0; i < lines.Count; i++) {
				var indentOff = MatchesCountInARow(lines[i], "}");
				var tabsCount = indent - indentOff;
				var tabs = "";
				for (var j = 0; j < tabsCount; j++) {
					tabs += '\t';
				}
				indent += MatchesCount(lines[i], "{") - MatchesCount(lines[i], "}");

				if (tabsCount > 0 && lines[i].Length != 0) {
					lines[i] = tabs + lines[i];
				}
			}
		}

		private string GetCode()
		{
			return lines.Aggregate("", (current, line) => current + (line + '\n'));
		}

		private static int MatchesCount(string source, string substring)
		{
			if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(substring)) {
				return 0;
			}

			var count = 0;
			var i = 0;
			while ((i = source.IndexOf(substring, i, StringComparison.InvariantCulture)) != -1) {
				i += substring.Length;
				++count;
			}
			return count;
		}

		private static int MatchesCountInARow(string source, string substring, int startIndex = 0)
		{
			if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(substring)) {
				return 0;
			}

			var count = 0;
			var i = startIndex;
			do {
				if (char.IsWhiteSpace(source[i])) {
					++i;
					continue;
				}

				var index = source.IndexOf(substring, i, StringComparison.InvariantCulture);
				if (index == -1 || index != i) {
					break;
				}

				i = index + substring.Length;
				++count;
			} while (i < source.Length);

			return count;
		}
	}
}
