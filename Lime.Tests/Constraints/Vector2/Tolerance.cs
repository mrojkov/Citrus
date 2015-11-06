using NUnit.Framework.Constraints;

namespace Lime.Tests
{
	public class Vector2Tolerance : Tolerance
	{
		public Vector2 Amount { get; }

		public Vector2Tolerance(Vector2 amount) 
			: base(amount)
		{
			Amount = amount;
		}
	}
}