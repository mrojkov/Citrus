using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Yuzu;

namespace YuzuTest
{

	public class SampleBase
	{
		[YuzuRequired(1)]
		public int FBase;
	}

	public class SampleDerivedA : SampleBase
	{
		[YuzuRequired(2)]
		public int FA;
	}

	public class SampleDerivedB : SampleBase
	{
		[YuzuRequired(2)]
		public int FB;
	}

	public class Empty
	{
	}

	public class Sample1
	{
		[YuzuRequired(1)]
		public int X;
		[YuzuOptional(2)]
		public string Y = "zzz";
	}

	public class Sample2
	{
		[YuzuRequired(1)]
		public int X { get; set; }
		[YuzuOptional(2)]
		public string Y { get; set; }
	}

	public class Sample3
	{
		[YuzuRequired(1)]
		public Sample1 S1 { get; set; }
		[YuzuOptional(2)]
		public int F;
		[YuzuOptional(3)]
		public Sample2 S2;
	}

	public class SampleMethodOrder
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

	public class SampleList
	{
		[YuzuRequired(1)]
		public List<string> E;
	}

	public class SampleTree
	{
		[YuzuRequired(1)]
		public int Value;
		[YuzuRequired(2)]
		public List<SampleTree> Children;
	}


	[TestClass]
	public class TestMain
	{

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
			jd.FromString(v2, result);
			Assert.AreEqual(v1.X, v2.X);
			Assert.AreEqual(v1.Y, v2.Y);

			jd.FromString(v2, "{\"X\":999}");
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
			jd.FromString(v2, result);
			Assert.AreEqual(v1.X, v2.X);
			Assert.AreEqual(v1.Y, v2.Y);

			jd.FromString(v2, "{\"X\":999}");
			Assert.AreEqual(999, v2.X);
			Assert.AreEqual(v1.Y, v2.Y);
		}

		[TestMethod]
		public void TestJsonNested()
		{
			var js = new JsonSerializer();

			var v = new Sample3 {
				S1 = new Sample1 { X = 345, Y = "test" },
				F = 222,
				S2 = new Sample2 { X = 346, Y = "test1" },
			};
			js.JsonOptions.Indent = "";

			var result = js.ToString(v);
			Assert.AreEqual(
				"{\n\"S1\":" +
				"{\n\"X\":345,\n\"Y\":\"test\"\n},\n" +
				"\"F\":222,\n" +
				"\"S2\":" +
				"{\n\"X\":346,\n\"Y\":\"test1\"\n}\n" +
				"}",
				result);

			var jd = new JsonDeserializer();
			var w = new Sample3();
			jd.FromString(w, result);
			Assert.AreEqual(v.S1.X, w.S1.X);
			Assert.AreEqual(v.S1.Y, w.S1.Y);
			Assert.AreEqual(v.F, w.F);
			Assert.AreEqual(v.S2.X, w.S2.X);
			Assert.AreEqual(v.S2.Y, w.S2.Y);
		}

		[TestMethod]
		public void TestJsonGenerated()
		{
			const string str =
				"{\n\"S1\":" +
				"{\n\"X\":345,\n\"Y\":\"test\"\n},\n" +
				"\"F\":222,\n" +
				"\"S2\":" +
				"{\n\"X\":346,\n\"Y\":\"test1\"\n}\n" +
				"}";

			var jd = new Sample3_JsonDeserializer();
			var w = (Sample3)jd.FromString(str);
			Assert.AreEqual(345, w.S1.X);
			Assert.AreEqual("test", w.S1.Y);
			Assert.AreEqual(222, w.F);
			Assert.AreEqual(346, w.S2.X);
			Assert.AreEqual("test1", w.S2.Y);
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
		public void TestJsonClassNames()
		{
			var js = new JsonSerializer();
			js.JsonOptions.Indent = "";
			js.Options.ClassNames = true;
			Assert.AreEqual(
				"{\n\"class\":\"YuzuTest.SampleBase\",\n\"FBase\":0\n}", js.ToString(new SampleBase()));
			Assert.AreEqual(
				"{\n\"class\":\"YuzuTest.SampleDerivedA\",\n\"FBase\":0,\n\"FA\":0\n}",
				js.ToString(new SampleDerivedA()));

			var jd = new JsonDeserializer();
			jd.Options.ClassNames = true;
			var v = jd.FromString(
				"{\n\"class\":\"YuzuTest.SampleDerivedB\",\n\"FBase\":3,\n\"FB\":7\n}");
			Assert.IsInstanceOfType(v, typeof(SampleDerivedB));
			var b = (SampleDerivedB)v;
			Assert.AreEqual(3, b.FBase);
			Assert.AreEqual(7, b.FB);
		}

		[TestMethod]
		public void TestJsonList()
		{
			var js = new JsonSerializer();
			js.JsonOptions.Indent = "";
			var jd = new JsonDeserializer();

			var v0 = new SampleList { E = new List<string> { "a", "b", "c" } };
			var result0 = js.ToString(v0);
			Assert.AreEqual("{\n\"E\":[\n\"a\",\n\"b\",\n\"c\"\n]\n}", result0);
			SampleList w0 = new SampleList();
			jd.FromString(w0, result0);
			CollectionAssert.AreEqual(v0.E, w0.E);

			var v1 = new SampleTree { Value = 11, Children = new List<SampleTree>() };
			Assert.AreEqual("{\n\"Value\":11,\n\"Children\":[]\n}", js.ToString(v1));
			SampleTree w1 = new SampleTree();
			jd.FromString(w1, js.ToString(v1));
			Assert.AreEqual(0, w1.Children.Count);

			var v2 = new SampleTree {
				Value = 11,
				Children = new List<SampleTree> {
					new SampleTree {
						Value = 12,
						Children = new List<SampleTree>(),
					},
					new SampleTree {
						Value = 13,
						Children = new List<SampleTree>(),
					}
				}
			};
			var result2 = js.ToString(v2);
			Assert.AreEqual(
				"{\n\"Value\":11,\n\"Children\":[\n" +
				"{\n\"Value\":12,\n\"Children\":[]\n},\n" +
				"{\n\"Value\":13,\n\"Children\":[]\n}\n" +
				"]\n}",
				result2);
			SampleTree w2 = new SampleTree();
			jd.FromString(w2, result2);
			Assert.AreEqual(v2.Value, w2.Value);
			Assert.AreEqual(v2.Children.Count, w2.Children.Count);
		}

		[TestMethod]
		public void TestJsonLongList()
		{
			var list1 = new SampleList { E = new List<string>() };
			for (int i = 0; i < 100000; ++i)
				list1.E.Add(i.ToString());

			var js = new JsonSerializer();
			var result1 = js.ToString(list1);
			Assert.IsTrue(result1 != "");
			var result2 = js.ToString(list1);
			Assert.IsTrue(result1 == result2);

			var list2 = new SampleList();
			var jd = new JsonDeserializer();
			jd.FromString(list2, result1);
			Assert.AreEqual(list1.E.Count, list2.E.Count);

			var jdg = new SampleList_JsonDeserializer();
			var list3 = (SampleList)jdg.FromString(result1);
			Assert.AreEqual(list1.E.Count, list3.E.Count);
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
			//(new TestMain()).TestJsonMemberOrder();
			var jd = JsonDeserializerGenerator.Instance;
			//jd.Options.ClassNames = true;
			using (jd.GenWriter = new StreamWriter(new FileStream(@"..\..\Sample.cs", FileMode.Create))) {
				jd.GenerateHeader("YuzuTest");
				jd.Generate<Sample1>();
				jd.Generate<Sample2>();
				jd.Generate<Sample3>();
				jd.Generate<SampleList>();
				jd.GenerateFooter();
			}
		}
	}
}
