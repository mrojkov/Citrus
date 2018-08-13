using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tangerine.Core.ExpressionParser
{
	public enum TokenType
	{
		Add,
		Substract,
		Multiply,
		Divide,
		Number,
		LParenthesis,
		RParenthesis
	}

	public class Token
	{
		public TokenType Type { get; set; }
		public double Value { get; set; }
	}

	public static class Tokenizer
	{
		//TODO: autogenerate pattern based on a dictionary
		private const string Pattern = @"(?xn)
				(?<Number>(
					(\d+\.?\d*) |
					(\.?\d+) |
					(\d+\.?\d+)
				)([eE][-+]?\d+)?) |
				(?<Add>\+) |
				(?<Substract>-) |
				(?<Multiply>\*) |
				(?<Divide>\/) |
				(?<LParenthesis>\() |
				(?<RParenthesis>\))
		";

		private const string NonWhitespacePattern = @"\S";

		private static readonly Regex Regex = new Regex(Pattern, RegexOptions.Compiled);

		private static readonly Regex NonWhitespaceRegex = new Regex(NonWhitespacePattern, RegexOptions.Compiled);

		public static IEnumerable<Token> Tokenize(string input)
		{
			var position = 0;
			var names = Enum.GetNames(typeof(TokenType));
			while (position < input.Length) {
				var skip = NonWhitespaceRegex.Match(input, position);
				position = skip.Index;
				var match = Regex.Match(input, position);
				if (match.Index != position) {
					yield return null;
					yield break;
				}
				foreach (string name in names) {
					var group = match.Groups[name];
					if (group.Success && group.Index == position) {
						Enum.TryParse(name, out TokenType type);
						double value = default;
						if (type == TokenType.Number) {
							if (!double.TryParse(group.Value, out value)) {
								yield return null;
								yield break;
							}
						}
						yield return new Token {
							Type = type,
							Value = value
						};
						position = group.Index + group.Length;
						break;
					}
				}
			}
		}
	}
}
