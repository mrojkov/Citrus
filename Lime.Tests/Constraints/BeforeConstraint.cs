using System.Collections.Generic;

namespace Lime.Tests
{
	internal class BeforeConstraint<T> : OrderConstraint<T>
	{
		public BeforeConstraint(T expected) : base(expected)
		{ }

		protected override bool IsSuccess(IList<T> list, T actual, T expected)
		{
			return list.IndexOf(actual) < list.IndexOf(expected);
		}

		public override string StringRepresentation()
		{
			return "before";
		}
	}
}