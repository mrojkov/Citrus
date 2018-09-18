using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;

namespace Lime.Tests
{
	internal abstract class OrderConstraint<T> : Constraint
	{
		private T expected;
		private IList<T> list;

		protected OrderConstraint(T expected)
		{
			this.expected = expected;
		}

		public OrderConstraint<T> In(IList<T> list)
		{
			if (this.list != null) {
				throw new InvalidOperationException("In modifier must appear only once in a constraint expression");
			}
			if (list == null) {
				throw new ArgumentNullException("list");
			}
			this.list = list;
			return this;
		}

		public override ConstraintResult ApplyTo<TActual>(TActual actual)
		{
			if (!(actual is T)) {
				throw new ArgumentException("Actual value should be of the same type as expected value");
			}
			if (list == null) {
				throw new InvalidOperationException("In modifier must be called in this constraint expression");
			}
			var actualValue = (T)(object)actual;
			if (!list.Contains(expected) || !list.Contains(actualValue)) {
				throw new ArgumentException("Provided list doesn't contain expected or actual value");
			}
			var isSuccess = IsSuccess(list, actualValue, expected);
			return new OrderConstraintResult<T>(this, list, actualValue, expected, isSuccess);
		}

		protected abstract bool IsSuccess(IList<T> list, T actual, T expected);
		public abstract string StringRepresentation();
	}
}
