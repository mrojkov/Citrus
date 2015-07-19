using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Yuzu;

namespace YuzuTest
{
	[TestClass]
	public class TestMain
	{
		[TestMethod]
		public void TestMethod1()
		{
			var yuzu = new Yuzu.Yuzu();
			Assert.IsTrue(true);
		}

		public static void Main()
		{
			
		}
	}
}
