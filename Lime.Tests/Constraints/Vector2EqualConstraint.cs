using NUnit.Framework.Constraints;

namespace Lime.Tests
{
	public class Vector2EqualConstraint : EqualConstraint
	{
		private Vector2 expected;

		public Vector2EqualConstraint(Vector2 expected): base(expected)
		{
			this.expected = expected;
		}

		public override ConstraintResult ApplyTo<TActual>(TActual actual)
		{
			if (!(actual is Vector2)) {
				return new EqualConstraintResult(this, actual, false);
			}
			var actualVector = (Vector2)(object)actual;
			var tolerance = Tolerance;
			var isSuccess =
				Numerics.AreEqual(expected.X, actualVector.X, ref tolerance) &&
				Numerics.AreEqual(expected.Y, actualVector.Y, ref tolerance);
			return new EqualConstraintResult(this, actual, isSuccess);
		}
	}
}