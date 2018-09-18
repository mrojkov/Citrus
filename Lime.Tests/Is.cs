namespace Lime.Tests
{
	internal class Is: NUnit.Framework.Is
	{
		public static Vector2EqualConstraint EqualTo(Vector2 vector)
		{
			return new Vector2EqualConstraint(vector);
		}

		public static AfterConstraint<T> After<T>(T value)
		{
			return new AfterConstraint<T>(value);
		}

		public static BeforeConstraint<T> Before<T>(T value)
		{
			return new BeforeConstraint<T>(value);
		}
	}
}