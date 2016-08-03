using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Yuzu;
using Yuzu.Binary;

namespace YuzuTest.Binary
{
	[TestClass]
	public class TestBinary
	{
		private string VarintXS(long value)
		{
			string result = "";
			do {
				byte b = (byte)(value & 0x7f);
				value >>= 7;
				if (value != 0) {
					b |= 0x80;
				}
				result += b.ToString("X2") + " ";
			} while (value != 0);
			return result;
		}

		private string XS(IEnumerable<byte> bytes)
		{
			return String.Join(" ", bytes.Select(b => b.ToString("X2")));
		}

		private IEnumerable<byte> SX(string s)
		{
			return s.Split(' ').Select(p => Byte.Parse(p, NumberStyles.AllowHexSpecifier));
		}

		private string XS(string s)
		{
			return VarintXS(s.Length) + XS(s.ToCharArray().Select(ch => (byte)ch));
		}

		private string XS(params string[] s)
		{
			return String.Join(" ", s.Select(XS));
		}

		[TestMethod]
		public void TestXS()
		{
			Assert.AreEqual("01 FF", XS(new byte[] { 1, 255 }));
			Assert.AreEqual("02 41 42", XS("AB"));
		}

		[TestMethod]
		public void TestSimple()
		{
			var bs = new BinarySerializer();
			bs.Options.AllowEmptyTypes = true;
			Assert.AreEqual("01 00 " + XS("YuzuTest.Empty") + " 00 00 00 00", XS(bs.ToBytes(new Empty())));

			var v1 = new Sample1 { X = 345, Y = "test" };

			bs.ClearClassIds();
			var result = bs.ToBytes(v1);
			Assert.AreEqual("01 00 " + XS("YuzuTest.Sample1") + " 02 00 " + XS("X", "Y") +
				" 01 00 59 01 00 00 02 00 " + XS("test") + " 00 00", XS(result));
			Sample1 v2 = new Sample1();

			var bd = new BinaryDeserializer();
			bd.FromBytes(v2, result);
			Assert.AreEqual(v1.X, v2.X);
			Assert.AreEqual(v1.Y, v2.Y);

			bd.FromBytes(v2, new byte[] { 01, 00, 01, 00, 0xE7, 03, 00, 00, 00, 00 });
			Assert.AreEqual(999, v2.X);
			Assert.AreEqual(v1.Y, v2.Y);

			v1.X = int.MaxValue;
			bd.FromBytes(v2, bs.ToBytes(v1));
			Assert.AreEqual(v1.X, v2.X);

			v1.X = int.MaxValue;
			bd.FromBytes(v2, bs.ToBytes(v1));
			Assert.AreEqual(v1.X, v2.X);
		}

		[TestMethod]
		public void TestLong()
		{
			var bs = new BinarySerializer();
			var v1 = new SampleLong { S = -1L << 33, U = 1UL << 33 };

			var result = bs.ToBytes(v1);
			Assert.AreEqual(
				"01 00 " + XS("YuzuTest.SampleLong") + " 02 00 " + XS("S", "U") +
				" 01 00 00 00 00 00 FE FF FF FF 02 00 00 00 00 00 02 00 00 00 00 00", XS(result));

			var v2 = new SampleLong();
			var bd = new BinaryDeserializer();
			bd.FromBytes(v2, result);
			Assert.AreEqual(v1.S, v2.S);
			Assert.AreEqual(v1.U, v2.U);

			v1.S = long.MinValue;
			v1.U = ulong.MaxValue;
			bd.FromBytes(v2, bs.ToBytes(v1));
			Assert.AreEqual(v1.S, v2.S);
			Assert.AreEqual(v1.U, v2.U);
		}

		[TestMethod]
		public void TestSmallTypes()
		{
			var bs = new BinarySerializer();
			var v1 = new SampleSmallTypes { Ch = 'A', Sh = -2000, USh = 2001, B = 198, Sb = -109 };

			var result = bs.ToBytes(v1);
			Assert.AreEqual(
				"01 00 " + XS("YuzuTest.SampleSmallTypes") + " 05 00 " + XS("B", "Ch", "Sb", "Sh", "USh") +
				" 01 00 C6 02 00 41 03 00 93 04 00 30 F8 05 00 D1 07 00 00", XS(result));

			var v2 = new SampleSmallTypes();
			var bd = new BinaryDeserializer();
			bd.FromBytes(v2, result);
			Assert.AreEqual(v1.Ch, v2.Ch);
			Assert.AreEqual(v1.USh, v2.USh);
			Assert.AreEqual(v1.Sh, v2.Sh);
			Assert.AreEqual(v1.B, v2.B);
			Assert.AreEqual(v1.Sb, v2.Sb);

			bd.FromBytes(v2, new byte[] {
				01, 00, 01, 00, 255, 02, 00, 65 + 25, 03, 00, 256 - 128, 04, 00, 00, 128, 05, 00, 255, 127, 00, 00 });
			Assert.AreEqual('Z', v2.Ch);
			Assert.AreEqual(32767, v2.USh);
			Assert.AreEqual(-32768, v2.Sh);
			Assert.AreEqual(255, v2.B);
			Assert.AreEqual(-128, v2.Sb);
		}

		[TestMethod]
		public void TestNested()
		{
			var bs = new BinarySerializer();

			var v = new Sample3 {
				S1 = new Sample1 { X = 345, Y = "test" },
				F = 222,
				S2 = new Sample2 { X = -346, Y = "test1" },
			};

			var result = bs.ToBytes(v);
			Assert.AreEqual(
				"01 00 " + XS("YuzuTest.Sample3") + " 03 00 " + XS("S1", "F", "S2") +
				" 01 00 02 00 " + XS("YuzuTest.Sample1") + " 02 00 " + XS("X", "Y") +
				" 01 00 59 01 00 00 02 00 " + XS("test") + " 00 00 " +
				"02 00 DE 00 00 00 " +
				"03 00 03 00 " + XS("YuzuTest.Sample2") + " 02 00 " + XS("X", "Y") +
				" 01 00 A6 FE FF FF 02 00 " + XS("test1") + " 00 00 00 00",
				XS(result));

			var bd = new BinaryDeserializer();
			var w = new Sample3();
			bd.FromBytes(w, result);
			Assert.AreEqual(v.S1.X, w.S1.X);
			Assert.AreEqual(v.S1.Y, w.S1.Y);
			Assert.AreEqual(v.F, w.F);
			Assert.AreEqual(v.S2.X, w.S2.X);
			Assert.AreEqual(v.S2.Y, w.S2.Y);
		}

		[TestMethod]
		public void TestEnum()
		{
			var bs = new BinarySerializer();

			var v = new Sample4 { E = SampleEnum.E3 };

			var result1 = bs.ToBytes(v);
			Assert.AreEqual(
				"01 00 " + XS("YuzuTest.Sample4") + " 01 00 " + XS("E") + " 01 00 02 00 00 00 00 00",
				XS(result1));


			var bd = new BinaryDeserializer();
			var w = new Sample4();
			bd.FromBytes(w, result1);
			Assert.AreEqual(SampleEnum.E3, w.E);
		}

		[TestMethod]
		public void TestBool()
		{
			var bs = new BinarySerializer();

			var v = new SampleBool { B = true };

			var result1 = bs.ToBytes(v);
			Assert.AreEqual(
				"01 00 " + XS("YuzuTest.SampleBool") + " 01 00 " + XS("B") + " 01 00 01 00 00",
				XS(result1));

			var bd = new BinaryDeserializer();
			var w = new SampleBool();
			bd.FromBytes(w, result1);
			Assert.AreEqual(true, w.B);
		}

		[TestMethod]
		public void TestFloat()
		{
			var bs = new BinarySerializer();

			var v = new SampleFloat { F = 1e-20f, D = -3.1415e100d };

			var result1 = bs.ToBytes(v);
			Assert.AreEqual(
				"01 00 " + XS("YuzuTest.SampleFloat") + " 02 00 " + XS("F", "D") +
				" 01 00 08 E5 3C 1E 02 00 CA DC 09 3E BE B9 CC D4 00 00",
				XS(result1));

			var w = new SampleFloat();
			var bd = new BinaryDeserializer();
			bd.FromBytes(w, result1);
			Assert.AreEqual(v.F, w.F);
			Assert.AreEqual(v.D, w.D);
		}

		[TestMethod]
		public void TestMemberOrder()
		{
			var bs = new BinarySerializer();
			var result = bs.ToBytes(new SampleMethodOrder());
			Assert.AreEqual(
				"01 00 " + XS("YuzuTest.SampleMethodOrder") + " 04 00 " + XS("F1", "P1", "F2", "P2") +
				" 01 00 00 00 00 00 02 00 00 00 00 00 03 00 00 00 00 00 04 00 00 00 00 00 00 00",
				XS(result));
		}

		[TestMethod]
		public void TestClassNames()
		{
			var bs = new BinarySerializer();
			Assert.AreEqual(
				"01 00 " + XS("YuzuTest.SampleBase") + " 01 00 " + XS("FBase") +
				" 01 00 00 00 00 00 00 00",
				XS(bs.ToBytes(new SampleBase())));
			Assert.AreEqual(
				"02 00 " + XS("YuzuTest.SampleDerivedA") + " 02 00 " + XS("FBase", "FA") +
				" 01 00 00 00 00 00 02 00 00 00 00 00 00 00",
				XS(bs.ToBytes(new SampleDerivedA())));

			var bd = new BinaryDeserializer();
			var v = bd.FromBytes(SX(
				"02 00 " + XS("YuzuTest.SampleDerivedB") + " 02 00 " + XS("FBase", "FB") +
				" 01 00 03 00 00 00 02 00 07 00 00 00 00 00").ToArray());
			Assert.IsInstanceOfType(v, typeof(SampleDerivedB));
			var b = (SampleDerivedB)v;
			Assert.AreEqual(3, b.FBase);
			Assert.AreEqual(7, b.FB);
		}

		[TestMethod]
		public void TestList()
		{
			var bs = new BinarySerializer();
			var bd = new BinaryDeserializer();

			var v0 = new SampleList { E = new List<string> { "a", "b", "c" } };
			var result0 = bs.ToBytes(v0);
			Assert.AreEqual(
				"01 00 " + XS("YuzuTest.SampleList") + " 01 00 " + XS("E") +
				" 01 00 03 00 00 00 " + XS("a", "b", "c") + " 00 00",
				XS(result0));
			var w0 = new SampleList();
			bd.FromBytes(w0, result0);
			CollectionAssert.AreEqual(v0.E, w0.E);

			var v1 = new SampleTree { Value = 11, Children = new List<SampleTree>() };
			var result1 = bs.ToBytes(v1);
			Assert.AreEqual(
				"02 00 " + XS("YuzuTest.SampleTree") + " 02 00 " + XS("Value", "Children") +
				" 01 00 0B 00 00 00 02 00 00 00 00 00 00 00",
				XS(result1));
			Assert.AreEqual("02 00 01 00 0B 00 00 00 02 00 00 00 00 00 00 00", XS(bs.ToBytes(v1)));
			var w1 = new SampleTree();
			bd.FromBytes(w1, result1);
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
			var result2 = bs.ToBytes(v2);
			Assert.AreEqual(
				"02 00 01 00 0B 00 00 00 02 00 02 00 00 00 " +
				"02 00 01 00 0C 00 00 00 02 00 00 00 00 00 00 00 " +
				"02 00 01 00 0D 00 00 00 02 00 FF FF FF FF 00 00 00 00",
				XS(result2));
			SampleTree w2 = new SampleTree();
			bd.FromBytes(w2, result2);
			Assert.AreEqual(v2.Value, w2.Value);
			Assert.AreEqual(v2.Children.Count, w2.Children.Count);
			Assert.AreEqual(v2.Children[0].Value, w2.Children[0].Value);
			Assert.AreEqual(v2.Children[1].Children, w2.Children[1].Children);
		}

		[TestMethod]
		public void TestCollection()
		{
			var bs = new BinarySerializer();
			var bd = new BinaryDeserializer();

			var v0 = new SampleWithCollection();
			v0.A.Add(new SampleInterfaced { X = 9 });
			v0.B.Add(7);
			v0.B.Add(6);
			var result0 = bs.ToBytes(v0);
			Assert.AreEqual(
				"01 00 " + XS("YuzuTest.SampleWithCollection") + " 02 00 " + XS("A", "B") +
				" 01 00 01 00 00 00 02 00 " + XS("YuzuTest.SampleInterfaced") + " 01 00 " + XS("X") +
				" 01 00 09 00 00 00 00 00" +
				" 02 00 02 00 00 00 07 00 00 00 06 00 00 00 00 00",
				XS(result0));

			var w0 = new SampleWithCollection();
			bd.FromBytes(w0, result0);
			Assert.AreEqual(1, w0.A.Count);
			Assert.IsInstanceOfType(w0.A.First(), typeof(SampleInterfaced));
			Assert.AreEqual(9, w0.A.First().X);
			CollectionAssert.AreEqual(new int[] { 7, 6 }, w0.B.ToList());

			var v2 = new SampleConcreteCollection { 2, 5, 4 };
			var result1 = bs.ToBytes(v2);
			Assert.AreEqual("03 00 00 00 02 00 00 00 05 00 00 00 04 00 00 00", XS(result1));
			SampleConcreteCollection w2 = new SampleConcreteCollection();
			bd.FromBytes(w2, result1);
			CollectionAssert.AreEqual(v2.ToList(), w2.ToList());
		}

		[TestMethod]
		public void TestTopLevelList()
		{
			var bs = new BinarySerializer();
			var bd = new BinaryDeserializer();

			var v0 = new List<string> { "a", "b", "c" };
			var result0 = bs.ToBytes(v0);
			Assert.AreEqual("03 00 00 00 " + XS("a", "b", "c"), XS(result0));

			var w0 = new List<string>();
			bd.FromBytes(w0, result0);
			CollectionAssert.AreEqual(v0, w0);
			bd.FromBytes(w0, new byte[] { 0, 0, 0, 0 });
			CollectionAssert.AreEqual(v0, w0);
			bd.FromBytes(w0, result0);
			CollectionAssert.AreEqual(new List<string> { "a", "b", "c", "a", "b", "c" }, w0);
		}

		[TestMethod]
		public void TestTopLevelDict()
		{
			var bs = new BinarySerializer();
			var bd = new BinaryDeserializer();

			var v0 = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
			var result0 = bs.ToBytes(v0);
			Assert.AreEqual("02 00 00 00 " + XS("a") + " 01 00 00 00 " + XS("b") + " 02 00 00 00", XS(result0));

			var w0 = new Dictionary<string, int>();
			bd.FromBytes(w0, result0);
			CollectionAssert.AreEqual(v0, w0);
			bd.FromBytes(w0, new byte[] { 0, 0, 0, 0 });
			CollectionAssert.AreEqual(v0, w0);
			bd.FromBytes(w0, SX("01 00 00 00 " + XS("c") + " 03 00 00 00").ToArray());
			CollectionAssert.AreEqual(
				new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } }, w0);
		}

		[TestMethod]
		public void TestDictionary()
		{
			var bs = new BinarySerializer();
			var bd = new BinaryDeserializer();

			var v0 = new SampleDict {
				Value = 3, Children = new Dictionary<string, SampleDict> {
				{ "a", new SampleDict { Value = 5, Children = new Dictionary<string, SampleDict>() } },
				{ "b", new SampleDict { Value = 7 } },
			}
			};
			var result0 = bs.ToBytes(v0);
			Assert.AreEqual(
				("01 00 " + XS("YuzuTest.SampleDict") + " 02 00 " + XS("Value", "Children") +
				" 01 00 03 00 00 00 02 00 02 00 00 00 " + XS("a") +
				" 01 00 01 00 05 00 00 00 02 00 00 00 00 00 00 00 " +
				XS("b") + " 01 00 01 00 07 00 00 00 02 00 FF FF FF FF 00 00 00 00"),
				XS(result0));

			var w0 = new SampleDict();
			bd.FromBytes(w0, result0);
			Assert.AreEqual(v0.Value, w0.Value);
			Assert.AreEqual(v0.Children.Count, w0.Children.Count);
			Assert.AreEqual(v0.Children["a"].Value, w0.Children["a"].Value);
		}

		[TestMethod]
		public void TestDictionaryKeys()
		{
			var bs = new BinarySerializer();

			var v0 = new SampleDictKeys {
				I = new Dictionary<int, int> { { 5, 7 } },
				E = new Dictionary<SampleEnum, int> { { SampleEnum.E2, 8 } },
				K = new Dictionary<SampleKey, int> { { new SampleKey { V = 3 }, 9 } },
			};
			var result0 = bs.ToBytes(v0);
			Assert.AreEqual(
				"01 00 " + XS("YuzuTest.SampleDictKeys") + " 03 00 " + XS("E", "I", "K") +
				" 01 00 01 00 00 00 01 00 00 00 08 00 00 00 " +
				"02 00 01 00 00 00 05 00 00 00 07 00 00 00 " +
				"03 00 01 00 00 00 02 00 " + XS("YuzuTest.SampleKey") + " 01 00 " + XS("V") +
				" 01 00 03 00 00 00 00 00 09 00 00 00 00 00", XS(result0));

			var bd = new BinaryDeserializer();
			var w = new SampleDictKeys();
			bd.FromBytes(w, result0);
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
			var bs = new BinarySerializer();
			var bd = new BinaryDeserializer();

			var v0 = new SampleArray { A = new string[] { "a", "b", "c" } };
			var result0 = bs.ToBytes(v0);
			Assert.AreEqual(
				"01 00 " + XS("YuzuTest.SampleArray") + " 01 00 " + XS("A") +
				" 01 00 03 00 00 00 " + XS("a", "b", "c") + " 00 00",
				XS(result0));
			var w0 = new SampleArray();
			bd.FromBytes(w0, result0);
			CollectionAssert.AreEqual(v0.A, w0.A);

			var v2 = new SampleArray();
			var result2 = bs.ToBytes(v2);
			Assert.AreEqual("01 00 01 00 FF FF FF FF 00 00", XS(result2));
			var w2 = new SampleArray();
			bd.FromBytes(w2, result2);
			CollectionAssert.AreEqual(v2.A, w2.A);
		}

		[TestMethod]
		public void TestClassList()
		{
			var bs = new BinarySerializer();
			var bd = new BinaryDeserializer();

			var v = new SampleClassList {
				E = new List<SampleBase> {
					new SampleDerivedA(),
					new SampleDerivedB { FB = 9 },
					new SampleDerivedB { FB = 8 },
				}
			};

			var result = bs.ToBytes(v);
			var w = (SampleClassList)bd.FromBytes(result);

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
			var src =
				"01 00 " + XS("YuzuTest.SampleMatrix") + " 01 00 " + XS("M") + " 01 00 " +
				"04 00 00 00 03 00 00 00 01 00 00 00 02 00 00 00 03 00 00 00 " +
				"02 00 00 00 04 00 00 00 05 00 00 00 " +
				"01 00 00 00 06 00 00 00 00 00 00 00 00 00";
			var v = new SampleMatrix();
			(new BinaryDeserializer()).FromBytes(v, SX(src).ToArray());
			Assert.AreEqual(4, v.M.Count);
			CollectionAssert.AreEqual(new int[] { 1, 2, 3 }, v.M[0]);
			CollectionAssert.AreEqual(new int[] { 4, 5 }, v.M[1]);
			CollectionAssert.AreEqual(new int[] { 6 }, v.M[2]);
			Assert.AreEqual(0, v.M[3].Count);

			var bs = new BinarySerializer();
			Assert.AreEqual(src, XS(bs.ToBytes(v)));
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
			var bs = new BinarySerializer();
			var result = bs.ToBytes(v);
			Assert.AreEqual(
				"01 00 " + XS("YuzuTest.SampleRect") + " 02 00 " + XS("A", "B") + 
			" 01 00 02 00 " + XS("YuzuTest.SamplePoint") + " 02 00 " + XS("X", "Y") +
			" 21 00 00 00 2C 00 00 00 " +
			"02 00 02 00 37 00 00 00 42 00 00 00 00 00",
			XS(result));

			var bd = new BinaryDeserializer();
			var w = new SampleRect();
			bd.FromBytes(w, result);
			CheckSampleRect(v, w);
		}

		[TestMethod]
		public void TestInterface()
		{
			var bs = new BinarySerializer();
			var v1 = new SampleInterfaceField { I = new SampleInterfaced { X = 34 } };
			var result1 = bs.ToBytes(v1);
			Assert.AreEqual(
				"01 00 " + XS("YuzuTest.SampleInterfaceField") + " 01 00 " + XS("I") +
				" 01 00 02 00 " + XS("YuzuTest.SampleInterfaced") + " 01 00 " + XS("X") +
				" 01 00 22 00 00 00 00 00 00 00",
				XS(result1));

			var w1 = new SampleInterfaceField();
			var bd = new BinaryDeserializer();
			bd.FromBytes(w1, result1);
			Assert.IsInstanceOfType(w1.I, typeof(SampleInterfaced));
			Assert.AreEqual(34, w1.I.X);

			var w1n = (SampleInterfaceField)bd.FromBytes(new byte[] { 01, 00, 01, 00, 00, 00, 00, 00 });
			Assert.AreEqual(null, w1n.I);

			var v2 = new List<ISample> { null, new SampleInterfaced { X = 37 } };
			var result2 = bs.ToBytes(v2);
			Assert.AreEqual("02 00 00 00 00 00 02 00 01 00 25 00 00 00 00 00", XS(result2));

			var w2 = new List<ISample>();
			bd.FromBytes(w2, result2);
			Assert.AreEqual(2, w2.Count);
			Assert.IsNull(w2[0]);
			Assert.AreEqual(37, w2[1].X);

			ISampleField v3 = new SampleInterfacedField { X = 41 };
			var result3 = bs.ToBytes(v3);
			Assert.AreEqual(
				"03 00 " + XS("YuzuTest.SampleInterfacedField") + " 01 00 " + XS("X") +
				" 01 00 29 00 00 00 00 00", XS(result3));
			var w3 = (ISampleField)bd.FromBytes(result3);
			Assert.AreEqual(41, w3.X);
		}

		[TestMethod]
		public void TestGeneric()
		{
			var bs = new BinarySerializer();
			var v1 = new SampleInterfaceField { I = new SampleInterfacedGeneric<string> { X = 35, G = "qq" } };
			var n = typeof(SampleInterfacedGeneric<string>).FullName;
			var result1 = bs.ToBytes(v1);
			Assert.AreEqual(
				"01 00 " + XS("YuzuTest.SampleInterfaceField") + " 01 00 " + XS("I") +
				" 01 00 02 00 " + XS(n) + " 02 00 " + XS("G", "X") +
				" 01 00 " + XS("qq") + " 02 00 23 00 00 00 00 00 00 00",
				XS(result1));
			var w1 = (SampleInterfaceField)(new BinaryDeserializer()).FromBytes(new SampleInterfaceField(), result1);
			Assert.AreEqual(w1.I.X, 35);
			Assert.AreEqual((w1.I as SampleInterfacedGeneric<string>).G, "qq");
		}

		[TestMethod]
		public void TestDefault()
		{
			var bs = new BinarySerializer();

			var v1 = new Sample1 { X = 6, Y = "ttt" };
			var result1 = bs.ToBytes(v1);
			Assert.AreEqual(
				"01 00 " + XS("YuzuTest.Sample1") + " 02 00 " + XS("X", "Y") +
				" 01 00 06 00 00 00 00 00", XS(result1));
			var w1 = (Sample1)(new BinaryDeserializer()).FromBytes(result1);
			Assert.AreEqual(6, w1.X);
			Assert.AreEqual("zzz", w1.Y);

			var v2 = new Sample2 { X = 5, Y = "5" };
			var result2 = bs.ToBytes(v2);
			Assert.AreEqual(
				"02 00 " + XS("YuzuTest.Sample2") + " 02 00 " + XS("X", "Y") +
				" 01 00 05 00 00 00 00 00", XS(result2));
		}

		[TestMethod]
		public void TestEscape()
		{
			var bs = new BinarySerializer();

			var s = "\"/{\u0001}\n\t\"\"";
			var v = new Sample1 { Y = s };
			var result = bs.ToBytes(v);
			Assert.AreEqual(
				"01 00 " + XS("YuzuTest.Sample1") + " 02 00 " + XS("X", "Y") +
				" 01 00 00 00 00 00 02 00 " + XS(s) + " 00 00",
				XS(result));

			var w = new Sample1();
			var bd = new BinaryDeserializer();
			bd.FromBytes(w, result);
			Assert.AreEqual(s, w.Y);

			v.Y = "привет";
			var result2 = bs.ToBytes(v);
			Assert.AreEqual(
				"01 00 01 00 00 00 00 00 02 00 0C " +
				XS(Encoding.UTF8.GetBytes("привет")) + " 00 00",
				XS(result2));
			bd.FromBytes(w, result2);
			Assert.AreEqual(v.Y, w.Y);
		}

		[TestMethod]
		public void TestDate()
		{
			var bs = new BinarySerializer();
			var bd = new BinaryDeserializer();

			var v1 = new SampleDate { D = new DateTime(2011, 3, 25), T = TimeSpan.FromMinutes(5) };
			var result1 = bs.ToBytes(v1);
			Assert.AreEqual(
				"01 00 " + XS("YuzuTest.SampleDate") + " 02 00 " + XS("D", "T") +
				" 01 00 00 00 F5 B7 96 B8 CD 08 02 00 00 5E D0 B2 00 00 00 00 00 00",
				XS(result1));

			var w1 = new SampleDate();
			bd.FromBytes(w1, result1);
			Assert.AreEqual(v1.D, w1.D);
			Assert.AreEqual(v1.T, w1.T);

			var v2 = new DateTime(2011, 3, 25, 1, 2, 3, DateTimeKind.Utc);
			var result2 = bs.ToBytes(v2);
			var w2 = bd.FromBytes<DateTime>(result2);
			Assert.AreEqual(v2, w2);
			Assert.AreEqual(v2.Kind, w2.Kind);

			var v3 = new DateTime(2011, 3, 25, 1, 2, 3, DateTimeKind.Local);
			var result3 = bs.ToBytes(v3);
			var w3 = bd.FromBytes<DateTime>(result3);
			Assert.AreEqual(v3, w3);
			Assert.AreEqual(v3.Kind, w3.Kind);
		}

		[TestMethod]
		public void TestDelegate()
		{
			var bs = new BinarySerializer();

			var v1 = new SampleSelfDelegate { x = 77 };
			v1.OnSomething = v1.Handler1;
			var result = bs.ToBytes(v1);
			Assert.AreEqual(
				"01 00 " + XS("YuzuTest.SampleSelfDelegate") + " 02 00 " + XS("OnSomething", "x") +
				" 01 00 " + XS("Handler1") + " 02 00 4D 00 00 00 00 00",
				XS(result));

			var w1 = new SampleSelfDelegate();
			var bd = new BinaryDeserializer();
			bd.FromBytes(w1, result);
			Assert.AreEqual(v1.x, w1.x);
			w1.OnSomething(10);
			Assert.AreEqual(87, w1.x);

			result[56]++; // Replace("Handler1", "Handler2")
			(new BinaryDeserializer()).FromBytes(w1, result);
			w1.OnSomething(10);
			Assert.AreEqual(770, w1.x);
		}

		[TestMethod]
		public void TestNullField()
		{
			var bs = new BinarySerializer();
			var sample = new SampleWithNullField();
			var result = bs.ToBytes(sample);
			Assert.AreEqual(
				"01 00 " + XS("YuzuTest.SampleWithNullField") + " 01 00 " + XS("About") +
				" 01 00 00 01 00 00",
				XS(result));
			var bd = new BinaryDeserializer();
			var w = new SampleWithNullField { About = "zzz" };
			bd.FromBytes(w, result);
			Assert.AreEqual(sample.About, w.About);

			bd.FromBytes(w, new byte[] { 01, 00, 01, 00, 00, 00, 00, 00 });
			Assert.AreEqual("", w.About);
		}

		[TestMethod]
		public void TestAfterDeserialization()
		{
			var bs = new BinarySerializer();
			var v0 = new SampleAfter { X = "m" };
			var result0 = bs.ToBytes(v0);
			Assert.AreEqual(
				"01 00 " + XS("YuzuTest.SampleAfter") + " 01 00 " + XS("X") +
				" 01 00 " + XS("m") + " 00 00",
				XS(result0));

			var bd = new BinaryDeserializer();
			var w0 = new SampleAfter();
			bd.FromBytes(w0, result0);
			Assert.AreEqual("m1", w0.X);

			var w1 = new SampleAfter2();
			bd.FromBytes(w1, bs.ToBytes(new SampleAfter2 { X = "m" }));
			Assert.AreEqual("m231", w1.X);
		}

		[TestMethod]
		public void TestMerge()
		{
			var bs = new BinarySerializer();

			var v1 = new SampleMerge();
			v1.LI.Add(33);
			v1.M = new Sample1 { X = 768, Y = "ttt" };

			var result1 = bs.ToBytes(v1);
			Assert.AreEqual(
				"01 00 " + XS("YuzuTest.SampleMerge") + " 02 00 " + XS("LI", "M") +
				" 01 00 01 00 00 00 21 00 00 00 02 00 02 00 " +
				XS("YuzuTest.Sample1") + " 02 00 " + XS("X", "Y") +
				" 01 00 00 03 00 00 00 00 00 00",
				XS(result1));

			var bd = new BinaryDeserializer();
			var w1 = new SampleMerge();
			w1.LI.Add(44);
			w1.M = new Sample1 { X = 999, Y = "qqq" };
			bd.FromBytes(w1, result1);
			CollectionAssert.AreEqual(new[] { 44, 33 }, w1.LI);
			Assert.AreEqual(768, w1.M.X);
			Assert.AreEqual("qqq", w1.M.Y);
		}

		[TestMethod]
		public void TestNamespaces()
		{
			var bs = new BinarySerializer();

			var v1 = new YuzuTest2.SampleNamespace { B = new SampleBase { FBase = 3 } };
			var result1 = bs.ToBytes(v1);
			Assert.AreEqual(
				"01 00 " + XS("YuzuTest2.SampleNamespace") + " 01 00 " + XS("B") +
				" 01 00 02 00 " + XS("YuzuTest.SampleBase") + " 01 00 " + XS("FBase") +
				" 01 00 03 00 00 00 00 00 00 00",
				XS(result1));

			var w1 = (new BinaryDeserializer()).FromBytes(result1);
			Assert.AreEqual(3, (w1 as YuzuTest2.SampleNamespace).B.FBase);
		}

		[TestMethod]
		public void TestNestedTypes()
		{
			var bs = new BinarySerializer();

			var v1 = new SampleNested { E = SampleNested.NestedEnum.One, C = new SampleNested.NestedClass() };
			var result1 = bs.ToBytes(v1);
			Assert.AreEqual(
				"01 00 " + XS("YuzuTest.SampleNested") + " 02 00 " + XS("C", "E") +
				" 01 00 02 00 " + XS("YuzuTest.SampleNested+NestedClass") + " 01 00 " + XS("Z") +
				" 01 00 00 00 00 00 00 00 " +
				"02 00 00 00 00 00 00 00",
				XS(result1));

			var w1 = (SampleNested)(new BinaryDeserializer()).FromBytes(result1);
			Assert.AreEqual(v1.E, w1.E);
			Assert.AreEqual(v1.C.Z, w1.C.Z);
		}

	}
}
