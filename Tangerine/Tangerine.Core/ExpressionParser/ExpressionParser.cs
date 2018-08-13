using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangerine.Core.ExpressionParser;

namespace Tangerine.Core.ExpressionParser
{
	public static class Parser
	{
		private static readonly Dictionary<TokenType, int> Precendence = new Dictionary<TokenType, int> {
			{ TokenType.Add, 1 },
			{ TokenType.Substract, 1 },
			{ TokenType.Multiply, 2 },
			{ TokenType.Divide, 2 },
		};

		public static double ParseAndCompute(string input)
		{
			var output = new Stack<double>();
			var operators = new Stack<TokenType>();
			foreach (var token in Tokenizer.Tokenize(input)) {
				if (token == null) {
					throw new Exception("Expression syntax error");
				}
				switch (token.Type) {
					case TokenType.Number:
						output.Push(token.Value);
						break;
					case TokenType.LParenthesis:
						operators.Push(TokenType.LParenthesis);
						break;
					case TokenType.RParenthesis:
						while (operators.Peek() != TokenType.LParenthesis) {
							var op = operators.Pop();
							double right = output.Pop();
							double left = output.Pop();
							output.Push(Execute(op, left, right));
						}
						operators.Pop();
						break;
					case TokenType.Add:
					case TokenType.Substract:
					case TokenType.Divide:
					case TokenType.Multiply:
						int precendence = Precendence[token.Type];
						while (
							operators.Count > 0 &&
							operators.Peek() != TokenType.LParenthesis &&
							precendence <= Precendence[operators.Peek()]
						) {
							var op = operators.Pop();
							double right = output.Pop();
							double left = output.Pop();
							output.Push(Execute(op, left, right));
						}
						operators.Push(token.Type);
						break;
				}
			}
			while (operators.Count > 0) {
				var op = operators.Pop();
				if (op == TokenType.LParenthesis) {
					throw new Exception("Expression syntax error");
				}
				double right = output.Pop();
				double left = output.Pop();
				output.Push(Execute(op, left, right));
			}
			if (output.Count != 1) {
				throw new Exception("Expression syntax error");
			}
			return output.Pop();
		}

		private static double Execute(TokenType operation, double left, double right)
		{
			switch (operation) {
				case TokenType.Add:
					return left + right;
				case TokenType.Substract:
					return left - right;
				case TokenType.Multiply:
					return left * right;
				case TokenType.Divide:
					return left / right;
				default:
					throw new Exception("Expression syntax error");
			}
		}
	}
}
