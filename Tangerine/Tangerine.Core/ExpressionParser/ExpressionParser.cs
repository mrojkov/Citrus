using Lime;
using System;
using System.Collections.Generic;

namespace Tangerine.Core.ExpressionParser
{
	public static class Parser
	{
		private enum Operator
		{
			Add, Substract, Multiply, Divide, Negate, LParenthesis
		}

		private static readonly Dictionary<TokenType, Operator> BinaryOperators = new Dictionary<TokenType, Operator> {
			{ TokenType.Add, Operator.Add },
			{ TokenType.Substract, Operator.Substract },
			{ TokenType.Multiply, Operator.Multiply },
			{ TokenType.Divide, Operator.Divide },
		};

		private static readonly Dictionary<Operator, int> Precendence = new Dictionary<Operator, int> {
			{ Operator.Add, 1 },
			{ Operator.Substract, 1 },
			{ Operator.Multiply, 2 },
			{ Operator.Divide, 2 },
			{ Operator.Negate, 4 }
		};

		public static bool TryParse(string input, out double output)
		{
			try {
				output = Parse(input);
				return true;
			} catch {
				output = default;
				return false;
			}
		}

		public static double Parse(string input)
		{
			var output = new Stack<double>();
			var operators = new Stack<Operator>();
			TokenType? prev = null;
			foreach (var token in Tokenizer.Tokenize(input)) {
				if (token == null) {
					throw new System.Exception("Expression syntax error");
				}
				switch (token.Type) {
					case TokenType.Number:
						output.Push(token.Value);
						break;
					case TokenType.LParenthesis:
						operators.Push(Operator.LParenthesis);
						break;
					case TokenType.RParenthesis:
						while (operators.Peek() != Operator.LParenthesis) {
							ExecuteFirst(output, operators);
						}
						operators.Pop();
						break;
					case TokenType.Add:
					case TokenType.Substract:
						if (prev == TokenType.Number || prev == TokenType.RParenthesis) {
							goto case TokenType.Divide;
						}
						if (token.Type == TokenType.Substract) {
							operators.Push(Operator.Negate);
						}
						break;
					case TokenType.Divide:
					case TokenType.Multiply:
						var oper = BinaryOperators[token.Type];
						int precendence = Precendence[oper];
						while (
							operators.Count > 0 &&
							operators.Peek() != Operator.LParenthesis &&
							precendence <= Precendence[operators.Peek()]
						) {
							ExecuteFirst(output, operators);
						}
						operators.Push(oper);
						break;
				}
				prev = token.Type;
			}
			while (operators.Count > 0) {
				ExecuteFirst(output, operators);
			}
			if (output.Count != 1) {
				throw new System.Exception("Expression syntax error");
			}
			return output.Pop();
		}

		private static void ExecuteFirst(Stack<double> output, Stack<Operator> operators)
		{
			var op = operators.Pop();
			if (op == Operator.Negate) {
				output.Push(-output.Pop());
				return;
			}
			double right = output.Pop();
			double left = output.Pop();
			output.Push(Execute(op, left, right));
		}

		private static double Execute(Operator operation, double left, double right)
		{
			switch (operation) {
				case Operator.Add:
					return left + right;
				case Operator.Substract:
					return left - right;
				case Operator.Multiply:
					return left * right;
				case Operator.Divide:
					if (Math.Abs(right) < Mathf.ZeroTolerance) {
						throw new ArithmeticException();
					}
					return left / right;
				default:
					throw new System.Exception("Expression syntax error");
			}
		}
	}
}
