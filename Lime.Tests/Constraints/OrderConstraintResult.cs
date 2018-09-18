using System.Collections.Generic;
using NUnit.Framework.Constraints;

namespace Lime.Tests
{
	internal class OrderConstraintResult<T>: ConstraintResult
	{
		private IList<T> list;
		private T actual;
		private T expected;
		private string orderStringRepresentation;

		public OrderConstraintResult(OrderConstraint<T> constraint, IList<T> list, T actualValue, T expectedValue, bool isSuccess)
			: base(constraint, actualValue, isSuccess)
		{
			this.list = list;
			actual = actualValue;
			expected = expectedValue;
			orderStringRepresentation = constraint.StringRepresentation();
		}

		public override void WriteActualValueTo(MessageWriter writer)
		{
			writer.Write(actual.ToString());
		}

		public override void WriteMessageTo(MessageWriter writer)
		{
			writer.WriteLine("Actual value: {0}", actual);
			writer.WriteLine("is not {0}", orderStringRepresentation);
			writer.WriteLine("expected value: {0}", expected);
			writer.WriteLine("in list: {");
			foreach (var item in list) {
				writer.WriteLine("	{0}", item);
			}
			writer.WriteLine("}");
		}
	}
}
