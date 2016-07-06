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
			js.Options.AllowEmptyTypes = true;
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

			v1.X = int.MaxValue;
			jd.FromString(v2, js.ToString(v1));
			Assert.AreEqual(v1.X, v2.X);

			v1.X = int.MinValue;
			jd.FromString(v2, js.ToString(v1));
			Assert.AreEqual(v1.X, v2.X);
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
		public void TestLong()
		{
			var js = new JsonSerializer();
			var v1 = new SampleLong { S = -1L << 33, U = 1UL << 33 };

			js.JsonOptions.Indent = "";
			var result = js.ToString(v1);
			Assert.AreEqual("{\n\"S\":-8589934592,\n\"U\":8589934592\n}", result);

			var v2 = new SampleLong();
			var jd = new JsonDeserializer();
			jd.FromString(v2, result);
			Assert.AreEqual(v1.S, v2.S);
			Assert.AreEqual(v1.U, v2.U);

			js.JsonOptions.Int64AsString = true;
			var result1 = js.ToString(v1);
			Assert.AreEqual("{\n\"S\":\"-8589934592\",\n\"U\":\"8589934592\"\n}", result1);
			var jd1 = new JsonDeserializer();
			jd1.JsonOptions.Int64AsString = true;
			jd1.FromString(v2, result1);
			Assert.AreEqual(v1.S, v2.S);
			Assert.AreEqual(v1.U, v2.U);

			js.JsonOptions.Int64AsString = false;
			v1.S = long.MinValue;
			v1.U = ulong.MaxValue;
			jd.FromString(v2, js.ToString(v1));
			Assert.AreEqual(v1.S, v2.S);
			Assert.AreEqual(v1.U, v2.U);
		}

		[TestMethod]
		public void TestSmallTypes()
		{
			var js = new JsonSerializer();
			var v1 = new SampleSmallTypes { Ch = 'A', Sh = -2000, USh = 2001, B = 198, Sb = -109 };

			js.JsonOptions.Indent = "";
			var result = js.ToString(v1);
			Assert.AreEqual("{\n\"B\":198,\n\"Ch\":\"A\",\n\"Sb\":-109,\n\"Sh\":-2000,\n\"USh\":2001\n}", result);

			var v2 = new SampleSmallTypes();
			var jd = new JsonDeserializer();
			jd.FromString(v2, result);
			Assert.AreEqual(v1.Ch, v2.Ch);
			Assert.AreEqual(v1.USh, v2.USh);
			Assert.AreEqual(v1.Sh, v2.Sh);
			Assert.AreEqual(v1.B, v2.B);
			Assert.AreEqual(v1.Sb, v2.Sb);

			XAssert.Throws<YuzuException>(() => jd.FromString(v2, result.Replace("A", "ABC")), "ABC");
			XAssert.Throws<OverflowException>(() => jd.FromString(v2, result.Replace("198", "298")));
			XAssert.Throws<OverflowException>(() => jd.FromString(v2, result.Replace("109", "209")));
			XAssert.Throws<OverflowException>(() => jd.FromString(v2, result.Replace("2000", "40000")));
			XAssert.Throws<OverflowException>(() => jd.FromString(v2, result.Replace("2001", "200000")));

			jd.FromString(v2, "{\n\"B\":255,\n\"Ch\":\"Z\",\n\"Sb\":-128,\n\"Sh\":-32768,\n\"USh\":32767\n}");
			Assert.AreEqual('Z', v2.Ch);
			Assert.AreEqual(32767, v2.USh);
			Assert.AreEqual(-32768, v2.Sh);
			Assert.AreEqual(255, v2.B);
			Assert.AreEqual(-128, v2.Sb);
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
		public void TestDictionaryKeys()
		{
			var js = new JsonSerializer();
			js.JsonOptions.Indent = "";
			js.JsonOptions.FieldSeparator = "";

			var v0 = new SampleDictKeys {
				I =  new Dictionary<int, int> { { 5, 7 } },
				E =  new Dictionary<SampleEnum, int> { { SampleEnum.E2, 8 } },
				K =  new Dictionary<SampleKey, int> { { new SampleKey { V = 3 }, 9 } },
			};
			var result0 = js.ToString(v0);
			Assert.AreEqual(
				"{" +
				"\"E\":{\"E2\":8}," +
				"\"I\":{\"5\":7}," +
				"\"K\":{\"3!\":9}" +
				"}", result0);

			JsonDeserializer.RegisterKeyParser(
				typeof(SampleKey),
				s => new SampleKey { V = int.Parse(s.Substring(0, s.Length - 1)) });

			var jd = new JsonDeserializer();
			var w = new SampleDictKeys();
			jd.FromString(w, result0);
			Assert.AreEqual(1, w.I.Count);
			Assert.AreEqual(7, w.I[5]);
			Assert.AreEqual(1, w.E.Count);
			Assert.AreEqual(8, w.E[SampleEnum.E2]);
			Assert.AreEqual(1, w.K.Count);
			Assert.AreEqual(9, w.K[new SampleKey { V = 3 }]);

			w = (SampleDictKeys)SampleDictKeys_JsonDeserializer.Instance.FromString(result0);
			Assert.AreEqual(1, w.I.Count);
			Assert.AreEqual(7, w.I[5]);
			Assert.AreEqual(1, w.E.Count);
			Assert.AreEqual(8, w.E[SampleEnum.E2]);
			Assert.AreEqual(1, w.K.Count);
			Assert.AreEqual(9, w.K[new SampleKey { V = 3 }]);
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
		public void TestAlias()
		{
			var js = new JsonSerializer();
			js.JsonOptions.Indent = "";
			js.JsonOptions.FieldSeparator = "";
			js.Options.TagMode = TagMode.Aliases;

			var v = new SampleTree { Value = 9 };
			var result = js.ToString(v);
			Assert.AreEqual("{\"a\":9,\"b\":null}", result);

			js.Options.TagMode = TagMode.Names;
			var result1 = js.ToString(v);
			Assert.AreEqual("{\"Value\":9,\"Children\":null}", result1);

			js.Options.TagMode = TagMode.Ids;
			var result2 = js.ToString(v);
			Assert.AreEqual("{\"AAAB\":9,\"AAAC\":null}", result2);

			var prev = IdGenerator.GetNextId();
			for (int i = 0; i < 2 * 52 - 5; ++i) {
				var next = IdGenerator.GetNextId();
				Assert.IsTrue(String.CompareOrdinal(prev, next) < 0);
				prev = next;
			}
			Assert.AreEqual("AABz", IdGenerator.GetNextId());

			var jd = new JsonDeserializer();
			jd.Options.TagMode = TagMode.Aliases;
			var w = new SampleTree();
			jd.FromString(w, result);
			Assert.AreEqual(9, w.Value);
			Assert.AreEqual(null, w.Children);
		}

		[TestMethod]
		public void TestObject()
		{
			var jd = new JsonDeserializer();
			var w = new SampleObj();
			jd.FromString(w, "{ \"F\": 123.4 }");
			Assert.AreEqual(123.4, w.F);
			jd.FromString(w, "{ \"F\": [1,2,3] }");
			CollectionAssert.AreEqual(new object[] { 1.0, 2.0, 3.0 }, (List<object>)w.F);
			jd.FromString(w, "{ \"F\": {\"a\":\"1\", \"b\": \"2\"} }");
			CollectionAssert.AreEqual(
				new Dictionary<string, object>() { { "a", "1" }, { "b", "2" } },
				(Dictionary<string, object>)w.F);
		}

		[TestMethod]
		public void TestNewFields()
		{
			var jd = new JsonDeserializer();
			jd.Options.TagMode = TagMode.Aliases;
			jd.Options.IgnoreNewFields = true;

			var w = new SampleTree();
			jd.FromString(w, "{\"a\":9,\"a1\":[],\"b\":null}");
			Assert.AreEqual(9, w.Value);

			jd.FromString(w, "{\"a\":10, \"a1\":[], \"b\":null, \"x\":null}");
			Assert.AreEqual(10, w.Value);

			jd.FromString(w, "{\"a\":11, \"a1\":[], \"x\":null}");
			Assert.AreEqual(11, w.Value);
		}

		[TestMethod]
		public void TestSpaces()
		{
			var jd = new JsonDeserializer();
			var w = new SampleList();
			jd.FromString(w, "{   \t\t\n\n\n\r \"E\":   \t\t\n\n[  \n\t\n\t]    }");
			Assert.AreEqual(0, w.E.Count);
		}

		[TestMethod]
		public void TestEscape()
		{
			var js = new JsonSerializer();
			js.JsonOptions.Indent = "";
			js.JsonOptions.FieldSeparator = "";

			var s = "\"/{\u0001}\n\t\"\"";
			var v = new Sample1 { Y = s };
			var result = js.ToString(v);
			Assert.AreEqual("{\"X\":0,\"Y\":\"\\\"\\/{\\u0001}\\n\\t\\\"\\\"\"}", result);

			var w = new Sample1();
			var jd = new JsonDeserializer();
			jd.FromString(w, result);
			Assert.AreEqual(s, w.Y);

			v.Y = result;
			var result1 = js.ToString(v);
			jd.FromString(w, result1);
			Assert.AreEqual(result, w.Y);

			v.Y = "привет";
			var result2 = js.ToString(v);
			Assert.AreEqual("{\"X\":0,\"Y\":\"привет\"}", result2);

			jd.FromString(w, result2);
			Assert.AreEqual(v.Y, w.Y);
			jd.FromString(w, "{\"X\":0,\"Y\":\"\u043F\u0440\u0438\u0432\u0435\u0442\"}");
			Assert.AreEqual(v.Y, w.Y);
		}

		[TestMethod]
		public void TestDate()
		{
			var js = new JsonSerializer();
			js.JsonOptions.Indent = "";
			js.JsonOptions.FieldSeparator = " ";

			var v1 = new SampleDate { D = new DateTime(2011, 3, 25), T = TimeSpan.FromMinutes(5) };
			var result1 = js.ToString(v1);
			Assert.AreEqual("{ \"D\":\"2011-03-25T00:00:00.0000000\", \"T\":\"00:05:00\" }", result1);
			js.JsonOptions.DateFormat = @"yyyy";
			Assert.AreEqual("{ \"D\":\"2011\", \"T\":\"00:05:00\" }", js.ToString(v1));

			var w1 = new SampleDate();
			(new JsonDeserializer()).FromString(w1, result1);
			Assert.AreEqual(v1.D, w1.D);
			Assert.AreEqual(v1.T, w1.T);

			w1 = (SampleDate)SampleDate_JsonDeserializer.Instance.FromString(result1);
			Assert.AreEqual(v1.D, w1.D);
			Assert.AreEqual(v1.T, w1.T);
		}

		[TestMethod]
		public void TestDelegate()
		{
			var js = new JsonSerializer();
			var v1 = new SampleSelfDelegate { x = 77 };
			js.ToString(v1);
			Assert.AreEqual(1, 1);
		}

		[TestMethod]
		public void TestNullField()
		{
			var js = new JsonSerializer();
			var sample = new SampleWithNullField();
			var s = js.ToString(sample);
			Assert.AreEqual("{\n\t\"About\":null\n}", s);
			var jd = new JsonDeserializer();
			var w = new SampleWithNullField { About = "zzz" };
			jd.FromString(w, s);
			Assert.AreEqual(sample.About, w.About);
		}

		[TestMethod]
		public void TestErrors()
		{
			var js = new JsonSerializer();
			XAssert.Throws<YuzuException>(() => js.ToString(new Empty()), "Empty");

			var jd = new JsonDeserializer();
			var w = new Sample1();
			XAssert.Throws<YuzuException>(() => jd.FromString(w, "{}"));
			XAssert.Throws<YuzuException>(() => jd.FromString(w, "{ \"X\" }"), ":");
			XAssert.Throws<YuzuException>(() => jd.FromString(w, "nn"), "u");
			XAssert.Throws<YuzuException>(() => jd.FromString(w, "{ \"X\":1, \"Y\": \"\\z\" }"), "z");
			XAssert.Throws<YuzuException>(() => jd.FromString(w, "{ \"X\":1, \"Y\": \"\\uQ\" }"), "Q");
			XAssert.Throws<YuzuException>(() => jd.FromString(new SampleBool(), "{ \"B\": 1 }"), "1");
			XAssert.Throws<YuzuException>(() => jd.FromString(w, "{ ,}"), ",");
			XAssert.Throws<YuzuException>(() => jd.FromString(w, "{ \"Y\": \"q\" }"), "'X'");
			XAssert.Throws<YuzuException>(() => jd.FromString(w, "[]"), "'Sample1'");
			jd.Options.ClassNames = true;
			XAssert.Throws<YuzuException>(() => jd.FromString("{\"X\":0}"), "class");
			XAssert.Throws<YuzuException>(() => jd.FromString(w, "{ \"class\": \"Q\" }"), "'Q'");
			XAssert.Throws<YuzuException>(() => jd.FromString(new SamplePoint(), "[ \"QQ\" ]"), "'QQ'");
			jd.Options.ClassNames = false;
			XAssert.Throws<YuzuException>(() => jd.FromString(""), "unspecified");
			XAssert.Throws<System.IO.EndOfStreamException>(() => jd.FromString(w, "{ \"X\": 1"));
		}

		[TestMethod]
		public void TestDeclarationErrors()
		{
			var js = new JsonSerializer();
			js.Options.TagMode = TagMode.Aliases;
			XAssert.Throws<YuzuException>(() => js.ToString(new Bad1()), "F");
			XAssert.Throws<YuzuException>(() => js.ToString(new Bad2()), "F");
			XAssert.Throws<YuzuException>(() => js.ToString(new Bad3()), "G");
		}
	}
}
