namespace Lime.Tests
{
	internal class Is: NUnit.Framework.Is
	{
		public static Vector2EqualConstraint EqualTo(Vector2 vector)
		{
			return new Vector2EqualConstraint(vector);
		}
	}
}