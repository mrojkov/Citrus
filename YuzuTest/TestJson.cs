using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Yuzu;

namespace YuzuTest
{
	[TestClass]
	public class TestJson
	{
		[TestMethod]
		public void TestSimple()
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
		public void TestSimpleProps()
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
		public void TestNested()
		{
			var js = new JsonSerializer();

			var v = new Sample3 {
				S1 = new Sample1 { X = 345, Y = "test" },
				F = 222,
				S2 = new Sample2 { X = -346, Y = "test1" },
			};
			js.JsonOptions.Indent = "";

			var result = js.ToString(v);
			Assert.AreEqual(
				"{\n\"S1\":" +
				"{\n\"X\":345,\n\"Y\":\"test\"\n},\n" +
				"\"F\":222,\n" +
				"\"S2\":" +
				"{\n\"X\":-346,\n\"Y\":\"test1\"\n}\n" +
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
		public void TestGenerated()
		{
			const string str =
				"{\n\"S1\":" +
				"{\n\"X\":345,\n\"Y\":\"test\"\n},\n" +
				"\"F\":222,\n" +
				"\"S2\":" +
				"{\n\"X\":-346,\n\"Y\":\"test1\"\n}\n" +
				"}";

			var jd = new Sample3_JsonDeserializer();
			var w = (Sample3)jd.FromString(str);
			Assert.AreEqual(345, w.S1.X);
			Assert.AreEqual("test", w.S1.Y);
			Assert.AreEqual(222, w.F);
			Assert.AreEqual(-346, w.S2.X);
			Assert.AreEqual("test1", w.S2.Y);

			var jdg = new JsonDeserializerGenerator();

			var w1 = new Sample1();
			jdg.FromString(w1, "{\"X\":88}");
			Assert.IsInstanceOfType(w1, typeof(Sample1));
			Assert.AreEqual(88, w1.X);

			jdg.Options.ClassNames = true;
			var w2 = jdg.FromString("{\"class\":\"YuzuTest.Sample1\",\"X\":99}");
			Assert.IsInstanceOfType(w2, typeof(Sample1));
			Assert.AreEqual(99, ((Sample1)w2).X);
		}

		[TestMethod]
		public void TestEnum()
		{
			var js = new JsonSerializer();

			var v = new Sample4 { E = SampleEnum.E3 };
			js.JsonOptions.Indent = "";

			var result1 = js.ToString(v);
			Assert.AreEqual("{\n\"E\":2\n}", result1);

			js.JsonOptions.EnumAsString = true;
			var result2 = js.ToString(v);
			Assert.AreEqual("{\n\"E\":\"E3\"\n}", result2);

			var jd = new JsonDeserializer();
			var w = new Sample4();
			jd.FromString(w, result1);
			Assert.AreEqual(SampleEnum.E3, w.E);

			w.E = SampleEnum.E1;
			jd.JsonOptions.EnumAsString = true;
			jd.FromString(w, result2);
			Assert.AreEqual(SampleEnum.E3, w.E);

			w = (Sample4)Sample4_JsonDeserializer.Instance.FromString(result2);
			Assert.AreEqual(SampleEnum.E3, w.E);
		}

		[TestMethod]
		public void TestBool()
		{
			var js = new JsonSerializer();

			var v = new SampleBool { B = true };
			js.JsonOptions.Indent = "";

			var result1 = js.ToString(v);
			Assert.AreEqual("{\n\"B\":true\n}", result1);

			var jd = new JsonDeserializer();
			var w = new SampleBool();
			jd.FromString(w, result1);
			Assert.AreEqual(true, w.B);

			w = (SampleBool)SampleBool_JsonDeserializer.Instance.FromString(result1);
			Assert.AreEqual(true, w.B);
		}

		[TestMethod]
		public void TestFloat()
		{
			var js = new JsonSerializer();

			var v = new SampleFloat { F = 1e-20f, D = -3.1415e100d };
			js.JsonOptions.Indent = "";

			var result1 = js.ToString(v);
			Assert.AreEqual("{\n\"F\":1E-20,\n\"D\":-3.1415E+100\n}", result1);

			var w = new SampleFloat();
			var jd = new JsonDeserializer();
			jd.FromString(w, result1);
			Assert.AreEqual(v.F, w.F);
			Assert.AreEqual(v.D, w.D);
		}

		[TestMethod]
		public void TestMemberOrder()
		{
			var js = new JsonSerializer();
			js.JsonOptions.Indent = "";
			var result = js.ToString(new SampleMethodOrder());
			Assert.AreEqual("{\n\"F1\":0,\n\"P1\":0,\n\"F2\":0,\n\"P2\":0\n}", result);
		}

		[TestMethod]
		public void TestClassNames()
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
		public void TestList()
		{
			var js = new JsonSerializer();
			js.JsonOptions.Indent = "";
			var jd = new JsonDeserializer();

			var v0 = new SampleList { E = new List<string> { "a", "b", "c" } };
			var result0 = js.ToString(v0);
			Assert.AreEqual("{\n\"E\":[\n\"a\",\n\"b\",\n\"c\"\n]\n}", result0);
			var w0 = new SampleList();
			jd.FromString(w0, result0);
			CollectionAssert.AreEqual(v0.E, w0.E);

			var v1 = new SampleTree { Value = 11, Children = new List<SampleTree>() };
			Assert.AreEqual("{\n\"Value\":11,\n\"Children\":[]\n}", js.ToString(v1));
			var w1 = new SampleTree();
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
					}
				}
			};
			var result2 = js.ToString(v2);
			Assert.AreEqual(
				"{\n\"Value\":11,\n\"Children\":[\n" +
				"{\n\"Value\":12,\n\"Children\":[]\n},\n" +
				"{\n\"Value\":13,\n\"Children\":null\n}\n" +
				"]\n}",
				result2);
			SampleTree w2 = new SampleTree();
			jd.FromString(w2, result2);
			Assert.AreEqual(v2.Value, w2.Value);
			Assert.AreEqual(v2.Children.Count, w2.Children.Count);
		}

		[TestMethod]
		public void TestDictionary()
		{
			var js = new JsonSerializer();
			js.JsonOptions.Indent = "";
			var jd = new JsonDeserializer();

			var v0 = new SampleDict {
				Value = 3, Children = new Dictionary<string, SampleDict> {
				{ "a", new SampleDict { Value = 5, Children = new Dictionary<string, SampleDict>() } },
				{ "b", new SampleDict { Value = 7 } },
			}
			};
			var result0 = js.ToString(v0);
			Assert.AreEqual(
				"{\n\"Value\":3,\n\"Children\":{\n" +
				"\"a\":{\n\"Value\":5,\n\"Children\":{}\n},\n" +
				"\"b\":{\n\"Value\":7,\n\"Children\":null\n}\n" +
				"}\n}", result0);

			var w0 = new SampleDict();
			jd.FromString(w0, result0);
			Assert.AreEqual(v0.Value, w0.Value);
			Assert.AreEqual(v0.Children.Count, w0.Children.Count);
			Assert.AreEqual(v0.Children["a"].Value, w0.Children["a"].Value);

			var w1 = (SampleDict)SampleDict_JsonDeserializer.Instance.FromString(result0);
			Assert.AreEqual(v0.Value, w1.Value);
			Assert.AreEqual(v0.Children.Count, w1.Children.Count);
			Assert.AreEqual(v0.Children["a"].Value, w1.Children["a"].Value);
		}

		[TestMethod]
		public void TestArray()
		{
			var js = new JsonSerializer();
			js.JsonOptions.Indent = "";
			var jd = new JsonDeserializer();

			var v0 = new SampleArray { A = new string[] { "a", "b", "c" } };
			var result0 = js.ToString(v0);
			Assert.AreEqual("{\n\"A\":[\n\"a\",\n\"b\",\n\"c\"\n]\n}", result0);
			var w0 = new SampleArray();
			jd.FromString(w0, result0);
			CollectionAssert.AreEqual(v0.A, w0.A);

			// Generated deserializer uses array prefix.
			var w1 = (SampleArray)SampleArray_JsonDeserializer.Instance.FromString(
				"{\n\"A\":[\n3,\n\"a\",\n\"b\",\n\"c\"\n]\n}");
			CollectionAssert.AreEqual(v0.A, w1.A);
		}

		[TestMethod]
		public void TestClassList()
		{
			var js = new JsonSerializer();
			js.Options.ClassNames = true;
			var jd = new JsonDeserializer();
			jd.Options.ClassNames = true;

			var v = new SampleClassList {
				E = new List<SampleBase> {
					new SampleDerivedA(),
					new SampleDerivedB { FB = 9 },
					new SampleDerivedB { FB = 8 },
				}
			};

			var result = js.ToString(v);
			var w = (SampleClassList)jd.FromString(result);

			Assert.AreEqual(3, w.E.Count);
			Assert.IsInstanceOfType(w.E[0], typeof(SampleDerivedA));
			Assert.IsInstanceOfType(w.E[1], typeof(SampleDerivedB));
			Assert.AreEqual(9, ((SampleDerivedB)w.E[1]).FB);
			Assert.IsInstanceOfType(w.E[2], typeof(SampleDerivedB));
			Assert.AreEqual(8, ((SampleDerivedB)w.E[2]).FB);

			w = (SampleClassList)SampleClassList_JsonDeserializer.Instance.FromString(result);
			Assert.AreEqual(3, w.E.Count);
			Assert.IsInstanceOfType(w.E[0], typeof(SampleDerivedA));
			Assert.IsInstanceOfType(w.E[1], typeof(SampleDerivedB));
			Assert.AreEqual(9, ((SampleDerivedB)w.E[1]).FB);
			Assert.IsInstanceOfType(w.E[2], typeof(SampleDerivedB));
			Assert.AreEqual(8, ((SampleDerivedB)w.E[2]).FB);
		}

		[TestMethod]
		public void TestMatrix()
		{
			var src = "{\"M\":[[1,2,3],[4,5],[6],[]]}";
			var v = new SampleMatrix();
			(new JsonDeserializer()).FromString(v, src);
			Assert.AreEqual(4, v.M.Count);
			CollectionAssert.AreEqual(new int[] { 1, 2, 3 }, v.M[0]);
			CollectionAssert.AreEqual(new int[] { 4, 5 }, v.M[1]);
			CollectionAssert.AreEqual(new int[] { 6 }, v.M[2]);
			Assert.AreEqual(0, v.M[3].Count);

			v = (SampleMatrix)SampleMatrix_JsonDeserializer.Instance.FromString(src);
			Assert.AreEqual(4, v.M.Count);
			CollectionAssert.AreEqual(new int[] { 1, 2, 3 }, v.M[0]);
			CollectionAssert.AreEqual(new int[] { 4, 5 }, v.M[1]);
			CollectionAssert.AreEqual(new int[] { 6 }, v.M[2]);
			Assert.AreEqual(0, v.M[3].Count);

			var js = new JsonSerializer();
			js.JsonOptions.FieldSeparator = "";
			js.JsonOptions.Indent = "";
			Assert.AreEqual(src, js.ToString(v));
		}

		private void CheckSampleRect(SampleRect expected, SampleRect actual)
		{
			Assert.AreEqual(expected.A.X, actual.A.X);
			Assert.AreEqual(expected.A.Y, actual.A.Y);
			Assert.AreEqual(expected.B.X, actual.B.X);
			Assert.AreEqual(expected.B.Y, actual.B.Y);
		}

		[TestMethod]
		public void TestStruct()
		{
			var v = new SampleRect {
				A = new SamplePoint { X = 33, Y = 44 },
				B = new SamplePoint { X = 55, Y = 66 },
			};
			var js = new JsonSerializer();
			js.JsonOptions.Indent = "";
			js.JsonOptions.FieldSeparator = " ";
			js.JsonOptions.IgnoreCompact = true;
			var result = js.ToString(v);
			Assert.AreEqual("{ \"A\":{ \"X\":33, \"Y\":44 }, \"B\":{ \"X\":55, \"Y\":66 } }", result);

			var jd = new JsonDeserializer();
			var w = new SampleRect();
			jd.FromString(w, result);
			CheckSampleRect(v, w);

			w = (SampleRect)SampleRect_JsonDeserializer.Instance.FromString(result);
			CheckSampleRect(v, w);

			var p = (SamplePoint)(new JsonDeserializerGenerator()).
				FromString(new SamplePoint(), "{ \"X\":34, \"Y\":45 }");
			Assert.AreEqual(34, p.X);
			Assert.AreEqual(45, p.Y);
		}

		[TestMethod]
		public void TestCompact()
		{
			var v = new SampleRect {
				A = new SamplePoint { X = 33, Y = 44 },
				B = new SamplePoint { X = 55, Y = 66 },
			};
			var js = new JsonSerializer();
			js.JsonOptions.Indent = "";
			js.JsonOptions.FieldSeparator = " ";
			var result = js.ToString(v);
			Assert.AreEqual("{ \"A\":[ 33, 44 ], \"B\":[ 55, 66 ] }", result);

			var jd = new JsonDeserializer();
			var w = new SampleRect();
			jd.FromString(w, result);
			CheckSampleRect(v, w);

			w = (SampleRect)SampleRect_JsonDeserializer.Instance.FromString(result);
			CheckSampleRect(v, w);
		}

		[TestMethod]
		public void TestDefault()
		{
			var js = new JsonSerializer();
			js.JsonOptions.Indent = "";
			js.JsonOptions.FieldSeparator = " ";

			var v1 = new Sample1 { X = 6, Y = "ttt" };
			var result1 = js.ToString(v1);
			Assert.AreEqual("{ \"X\":6 }", result1);
			var w1 = (Sample1)Sample1_JsonDeserializer.Instance.FromString(result1);
			Assert.AreEqual(6, w1.X);
			Assert.AreEqual("zzz", w1.Y);

			var v2 = new Sample2 { X = 5, Y = "5" };
			var result2 = js.ToString(v2);
			Assert.AreEqual("{ \"X\":5 }", result2);
		}

		[TestMethod]
		public void TestErrors()
		{
			var jd = new JsonDeserializer();
			var w = new Sample1();
			XAssert.Throws<YuzuException>(() => jd.FromString(w, "{}"));
			XAssert.Throws<YuzuException>(() => jd.FromString(w, "{ \"X\" }"));
			XAssert.Throws<YuzuException>(() => jd.FromString(w, "{ \"Y\": \"x\" }"));
			XAssert.Throws<YuzuException>(() => jd.FromString(w, "{ \"X\": 1"));
		}
	}
}
