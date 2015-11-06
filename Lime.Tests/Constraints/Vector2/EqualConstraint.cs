using NUnit.Framework.Constraints;
using System;

namespace Lime.Tests
{
	public class Vector2EqualConstraint : EqualConstraint
	{
		private Vector2 expected;
		private Vector2Tolerance tolerance;

		private Vector2 ActualVector
		{
			get { return (Vector2) actual; }
		}

		public Vector2EqualConstraint(Vector2 expected): base(expected)
		{
			this.expected = expected;
		}

		public Vector2EqualConstraint Within(Vector2 vector)
		{
			base.Within(vector);
			if (tolerance != null)
				throw new InvalidOperationException("Within modifier may appear only once in a constraint expression");
			tolerance = new Vector2Tolerance(vector);
			return this;
		}

		public override bool Matches(object actual)
		{
			this.actual = actual;
			var vector = (Vector2) actual;
			if (tolerance == null) {
				return vector == expected;
			}
			return vector.X.InRange(expected.X - tolerance.Amount.X, expected.X + tolerance.Amount.X) &&
					vector.Y.InRange(expected.Y - tolerance.Amount.Y, expected.Y + tolerance.Amount.Y);
		}

		public override void WriteActualValueTo(MessageWriter writer)
		{
			writer.Write(ActualVector);
		}
	}
}