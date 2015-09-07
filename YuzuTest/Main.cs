using System;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Yuzu;

namespace YuzuTest
{
	[TestClass]
	public class TestMain
	{

		private class Empty
		{
		}

		private class Sample1
		{
			[YuzuRequired(1)]
			public int X;
			[YuzuOptional(2)]
			public string Y = "zzz";
		}

		private class Sample2
		{
			[YuzuRequired(1)]
			public int X { get; set; }
			[YuzuOptional(2)]
			public string Y { get; set; }
		}

		private class SampleMethodOrder
		{
			[YuzuRequired(4)]
			public int P2 { get; set; }
			[YuzuRequired(2)]
			public int P1 { get; set; }
			public int F_no;
			[YuzuRequired(1)]
			public int F1;
			[YuzuRequired(3)]
			public int F2;
			public int Func() { return 0; }
		}

		[TestMethod]
		public void TestJsonSimple()
		{
			var js = new JsonSerializer();
			Assert.AreEqual("{\n}", js.ToString(new Empty()));

			var v1 = new Sample1 { X = 345, Y = "test" };
			js.JsonOptions.Indent = "";

			var result = js.ToString(v1);
			Assert.AreEqual("{\n\"X\":345,\n\"Y\":\"test\"\n}", result);
			Sample1 v2 = new Sample1();

			var jd = new JsonDeserializer();
			jd.FromStringUTF8(v2, result);
			Assert.AreEqual(v1.X, v2.X);
			Assert.AreEqual(v1.Y, v2.Y);

			jd.FromStringUTF8(v2, "{\"X\":999}");
			Assert.AreEqual(999, v2.X);
			Assert.AreEqual(v1.Y, v2.Y);
		}

		[TestMethod]
		public void TestJsonSimpleProps()
		{
			var js = new JsonSerializer();

			var v1 = new Sample1 { X = 345, Y = "test" };
			js.JsonOptions.Indent = "";

			var result = js.ToString(v1);
			Assert.AreEqual("{\n\"X\":345,\n\"Y\":\"test\"\n}", result);
			Sample1 v2 = new Sample1();

			var jd = new JsonDeserializer();
			jd.FromStringUTF8(v2, result);
			Assert.AreEqual(v1.X, v2.X);
			Assert.AreEqual(v1.Y, v2.Y);

			jd.FromStringUTF8(v2, "{\"X\":999}");
			Assert.AreEqual(999, v2.X);
			Assert.AreEqual(v1.Y, v2.Y);
		}

		[TestMethod]
		public void TestJsonMemberOrder()
		{
			var js = new JsonSerializer();
			js.JsonOptions.Indent = "";
			var result = js.ToString(new SampleMethodOrder());
			Assert.AreEqual("{\n\"F1\":0,\n\"P1\":0,\n\"F2\":0,\n\"P2\":0\n}", result);
		}

		[TestMethod]
		public void TestProtobufSimple()
		{
			var v1 = new Sample1 { X = 150, Y = "test" };
			var ps = new ProtobufSerializer();
			var result = ps.ToBytes(v1);
			CollectionAssert.AreEqual(new byte[] {
				0x08, 0x96, 0x01, 0x12, 0x04, (byte)'t', (byte)'e', (byte)'s', (byte)'t' }, result);
			var v2 = new Sample1();
			(new ProtobufDeserializer()).FromBytes(v2, result);
			Assert.AreEqual(v1.X, v2.X);
			Assert.AreEqual(v1.Y, v2.Y);
		}

		[TestMethod]
		public void TestProtobufSimpleProps()
		{
			var v1 = new Sample2 { X = 150, Y = "test" };
			var ps = new ProtobufSerializer();
			var result = ps.ToBytes(v1);
			CollectionAssert.AreEqual(new byte[] {
				0x08, 0x96, 0x01, 0x12, 0x04, (byte)'t', (byte)'e', (byte)'s', (byte)'t' }, result);
			var v2 = new Sample2();
			(new ProtobufDeserializer()).FromBytes(v2, result);
			Assert.AreEqual(v1.X, v2.X);
			Assert.AreEqual(v1.Y, v2.Y);
		}

		[TestMethod]
		public void TestCodeAssignSimple()
		{
			var v1 = new Sample1 { X = 150, Y = "test" };
			var cs = new CodeAssignSerializer();
			var result1 = cs.ToString(v1);
			Assert.AreEqual("void Init(Sample1 obj) {\n\tobj.X = 150;\n\tobj.Y = \"test\";\n}\n", result1);

			var v2 = new Sample2 { X = 150, Y = "test" };
			var result2 = cs.ToString(v2);
			Assert.AreEqual("void Init(Sample2 obj) {\n\tobj.X = 150;\n\tobj.Y = \"test\";\n}\n", result2);
		}

		public static void Main()
		{
			JsonDeserializerGenerator.Instance.Generate<Sample1>();
		}
	}
}
