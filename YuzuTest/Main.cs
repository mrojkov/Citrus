using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Yuzu;

namespace YuzuTest
{
	[TestClass]
	public class TestMain
	{
		private class Sample1
		{
			public int X;
			public string Y;
		}

		[TestMethod]
		public void TestMethod1()
		{
			var v1 = new Sample1 { X = 345, Y = "test" };
			var js = new JsonSerializer();
			js.JsonOptions.Indent = "";
			var result = js.SerializeToStringUTF8(v1);
			Assert.AreEqual("{\n\"X\":345,\n\"Y\":\"test\"\n}", result);
			Sample1 v2 = new Sample1();
			(new JsonDeserializer()).DeserializeFromStringUTF8(v2, result);
			Assert.AreEqual(v1.X, v2.X);
			Assert.AreEqual(v1.Y, v2.Y);
		}

		public static void Main()
		{
		}
	}
}
