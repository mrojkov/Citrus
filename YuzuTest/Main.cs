using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;

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

	[ProtoContract]
	public class Sample1
	{
		[YuzuRequired(1)]
		[ProtoMember(1)]
		public int X;

		[YuzuOptional(2)][YuzuDefault("ttt")]
		[ProtoMember(2)]
		public string Y = "zzz";
	}

	public class Sample2
	{
		[YuzuRequired(1)]
		public int X { get; set; }

		[YuzuOptional(2)][YuzuSerializeIf("SaveYIf")]
		public string Y { get; set; }

		public bool SaveYIf()
		{
			return X.ToString() != Y;
		}
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

	public enum SampleEnum { E1, E2, E3 };

	public class Sample4
	{
		[YuzuOptional(1)]
		public SampleEnum E;
	}

	public class SampleBool
	{
		[YuzuRequired(1)]
		public bool B;
	}

	public class SampleFloat
	{
		[YuzuRequired(1)]
		public float F;
		[YuzuRequired(2)]
		public double D;
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

	[ProtoContract]
	public class SampleList
	{
		[YuzuRequired(1)]
		[ProtoMember(1)]
		public List<string> E;
	}

	public class SampleArray
	{
		[YuzuRequired(1)]
		public string[] A;
	}

	public class SampleTree
	{
		[YuzuRequired(1)]
		public int Value;
		[YuzuRequired(2)]
		public List<SampleTree> Children;
	}

	public class SampleClassList
	{
		[YuzuRequired(1)]
		public List<SampleBase> E;
	}

	public class SampleDict
	{
		[YuzuRequired(1)]
		public int Value;
		[YuzuOptional(2)]
		public Dictionary<string, SampleDict> Children;
	}

	public class SampleMatrix
	{
		[YuzuRequired(1)]
		public List<List<int>> M;
	}

	[YuzuCompact]
	public struct SamplePoint
	{
		[YuzuRequired(1)]
		public int X;
		[YuzuRequired(2)]
		public int Y;
	}

	public class SampleRect
	{
		[YuzuRequired(1)]
		public SamplePoint A;
		[YuzuRequired(2)]
		public SamplePoint B;
	}

	[TestClass]
	public class TestMain
	{


		private byte[] ProtoBufNetToBytes(object obj)
		{
			var ms = new MemoryStream();
			ProtoBuf.Serializer.Serialize(ms, obj);
			var result = ms.GetBuffer();
			Array.Resize(ref result, (int)ms.Length);
			return result;
		}

		[TestMethod]
		public void TestProtobufSimple()
		{
			var v1 = new Sample1 { X = 150, Y = "test" };
			var ps = new ProtobufSerializer();
			var result = ps.ToBytes(v1);
			CollectionAssert.AreEqual(new byte[] {
				0x08, 0x96, 0x01, 0x12, 0x04, (byte)'t', (byte)'e', (byte)'s', (byte)'t' }, result);
			CollectionAssert.AreEqual(result, ProtoBufNetToBytes(v1));
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
		public void TestProtobufFloat()
		{
			var v1 = new SampleFloat { F = 1e-20f, D = -3.1415E+100 };
			var ps = new ProtobufSerializer();
			var result = ps.ToBytes(v1);
			CollectionAssert.AreEqual(new byte[] {
				0x09, 0x00, 0x00, 0x00, 0x00, 0xA1, 0x9C, 0xC7, 0x3B,
				0x11, 0xCA, 0xDC, 0x09, 0x3E, 0xBE, 0xB9, 0xCC, 0xD4 }, result);
			var v2 = new SampleFloat();
			(new ProtobufDeserializer()).FromBytes(v2, result);
			Assert.AreEqual(v1.F, v2.F);
			Assert.AreEqual(v1.D, v2.D);
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
			using (jd.GenWriter = new StreamWriter(new FileStream(@"..\..\Generated.cs", FileMode.Create))) {
				jd.GenerateHeader("YuzuTest");
				jd.Generate<Sample1>();
				jd.Generate<Sample2>();
				jd.Generate<Sample3>();
				jd.JsonOptions.EnumAsString = true;
				jd.Generate<Sample4>();
				jd.Generate<SampleBool>();
				jd.Generate<SampleList>();
				jd.Generate<SampleDict>();
				jd.JsonOptions.ArrayLengthPrefix = true;
				jd.Generate<SampleArray>();
				jd.Generate<SampleBase>();
				jd.Generate<SampleDerivedA>();
				jd.Generate<SampleDerivedB>();
				jd.Generate<SampleMatrix>();
				jd.Generate<SamplePoint>();
				jd.Generate<SampleRect>();
				jd.Options.ClassNames = true;
				jd.Generate<SampleClassList>();
				jd.GenerateFooter();
			}
		}

	}
}
