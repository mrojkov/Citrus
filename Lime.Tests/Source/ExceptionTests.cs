using NUnit.Framework;

namespace Lime.Tests.Source
{
	[TestFixture]
	public class ExceptionTests
	{
		[Test]
		public void ExceptionTest()
		{
			var exceptionMessage = "Sample text";
			var e = Assert.Catch<Exception>(() => { throw new Exception(exceptionMessage); });
			Assert.That(e.Message, Is.EqualTo(exceptionMessage));
			exceptionMessage = "Some text with variables: {0}, {1}, {2}";
			var variables = new object[] { 1, true, "test" };
			e = Assert.Catch<Exception>(() => { throw new Exception(exceptionMessage, variables); });
			Assert.That(e.Message, Is.EqualTo(string.Format(exceptionMessage, variables)));
		}
	}
}
