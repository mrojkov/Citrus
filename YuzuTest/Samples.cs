using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;

using Yuzu;

namespace YuzuTest
{
	public class SampleBase
	{
		[YuzuRequired("0_FBase")]
		public int FBase;
	}

	public class SampleDerivedA : SampleBase
	{
		[YuzuRequired]
		public int FA;
	}

	public class SampleDerivedB : SampleBase
	{
		[YuzuRequired]
		public int FB;
	}

	public class Empty
	{
	}

	[ProtoContract]
	public class Sample1
	{
		[YuzuRequired]
		[ProtoMember(1)]
		public int X;

		[YuzuOptional]
		[YuzuDefault("ttt")]
		[ProtoMember(2)]
		public string Y = "zzz";
	}

	public class Sample2
	{
		[YuzuRequired]
		public int X { get; set; }

		[YuzuOptional]
		[YuzuSerializeIf("SaveYIf")]
		public string Y { get; set; }

		public bool SaveYIf()
		{
			return X.ToString() != Y;
		}
	}

	public class Sample3
	{
		[YuzuRequired]
		public Sample1 S1 { get; set; }
		[YuzuOptional("S11")]
		public int F;
		[YuzuOptional]
		public Sample2 S2;
	}

	public enum SampleEnum { E1, E2, E3 };

	public class Sample4
	{
		[YuzuOptional]
		public SampleEnum E;
	}

	public class SampleLong
	{
		[YuzuRequired]
		public long S;
		[YuzuRequired]
		public ulong U;
	}

	public class SampleBool
	{
		[YuzuRequired]
		public bool B;
	}

	public class SampleSmallTypes
	{
		[YuzuRequired]
		public char Ch;
		[YuzuRequired]
		public short Sh;
		[YuzuRequired]
		public short USh;
		[YuzuRequired]
		public byte B;
		[YuzuRequired]
		public sbyte Sb;
	}

	public class SampleFloat
	{
		[YuzuRequired("1")]
		public float F;
		[YuzuRequired("2")]
		public double D;
	}

	public class SampleMethodOrder
	{
		[YuzuRequired("4")]
		public int P2 { get; set; }
		[YuzuRequired("2")]
		public int P1 { get; set; }
		public int F_no;
		[YuzuRequired("1")]
		public int F1;
		[YuzuRequired("3")]
		public int F2;
		public int Func() { return 0; }
	}

	[ProtoContract]
	public class SampleList
	{
		[YuzuRequired]
		[ProtoMember(1)]
		public List<string> E;
	}

	public class SampleArray
	{
		[YuzuRequired]
		public string[] A;
	}

	public class SampleTree
	{
		[YuzuRequired("a")]
		public int Value;
		[YuzuOptional("b")]
		public List<SampleTree> Children;
	}

	public class SampleClassList
	{
		[YuzuRequired]
		public List<SampleBase> E;
	}

	public class SampleDict
	{
		[YuzuRequired("a")]
		public int Value;
		[YuzuOptional("b")]
		public Dictionary<string, SampleDict> Children;
	}

	public class SampleKey : IEquatable<SampleKey>
	{
		public int V;
		public override string ToString() { return V.ToString() + "!"; }
		public bool Equals(SampleKey other) { return V == other.V; }
		public override int GetHashCode() { return V; }
	}

	public class SampleDictKeys
	{
		[YuzuRequired]
		public Dictionary<int, int> I;
		[YuzuRequired]
		public Dictionary<SampleEnum, int> E;
		[YuzuRequired]
		public Dictionary<SampleKey, int> K;
	}

	public class SampleMatrix
	{
		[YuzuRequired]
		public List<List<int>> M;
	}

	[YuzuCompact]
	public struct SamplePoint
	{
		[YuzuRequired]
		public int X;
		[YuzuRequired]
		public int Y;
	}

	public class SampleRect
	{
		[YuzuRequired]
		public SamplePoint A;
		[YuzuRequired]
		public SamplePoint B;
	}

	public class SampleObj
	{
		[YuzuRequired]
		public object F;
	}

	public class SampleDate
	{
		[YuzuRequired]
		public DateTime D;
		[YuzuRequired]
		public TimeSpan T;
	}

	public class SampleWithNullField
	{
		[YuzuRequired]
		public string About = null;
	}

	[YuzuCompact]
	[ProtoContract]
	public class Color
	{
		[YuzuRequired]
		[ProtoMember(1)]
		public byte R;

		[YuzuRequired]
		[ProtoMember(2)]
		public byte G;

		[YuzuRequired]
		[ProtoMember(3)]
		public byte B;
	}

	[ProtoContract]
	public class SamplePerson
	{
		public static int Counter = 0;

		[YuzuRequired("1")]
		[ProtoMember(1)]
		public string Name;

		[YuzuRequired("2")]
		[ProtoMember(2)]
		public DateTime Birth;

		[YuzuRequired("3")]
		[ProtoMember(3)]
		public List<SamplePerson> Children;

		[YuzuRequired("4")]
		[ProtoMember(4)]
		public Color EyeColor;

		public SamplePerson() { }

		public SamplePerson(Random rnd, int depth)
		{
			Counter++;
			StringBuilder sb = new StringBuilder();
			var len = rnd.Next(1, 40);
			for (int i = 0; i < len; ++i)
				sb.Append((char)rnd.Next((int)'a', (int)'z' + 1));
			Name = sb.ToString();
			Birth = new DateTime(1999, rnd.Next(10) + 1, 13);
			var childCount = rnd.Next(28 / depth);
			Children = new List<SamplePerson>();
			for (int i = 0; i < childCount; ++i)
				Children.Add(new SamplePerson(rnd, depth + 1));
			EyeColor = new Color { R = (byte)rnd.Next(256), G = (byte)rnd.Next(256), B = (byte)rnd.Next(256) };
		}
	}

	public class SampleSelfDelegate
	{
		[YuzuRequired]
		public int x;
		[YuzuRequired]
		public Action<int> OnSomething;

		public void Handler1(int v) { x += v; }
		public void Handler2(int v) { x *= v; }
	}

	public class Bad1
	{
		[YuzuRequired]
		[YuzuOptional]
		public int F;
	};

	public class Bad2
	{
		[YuzuRequired("привет")]
		public int F;
	};

	public class Bad3
	{
		[YuzuRequired("q")]
		public int F;
		[YuzuRequired("q")]
		public int G;
	};

	public static class XAssert
	{
		public static void Throws<TExpectedException>(Action exceptionThrower, string expectedExceptionMessage = "")
			where TExpectedException : Exception
		{
			try {
				exceptionThrower();
				Assert.Fail("Expected exception:<{0}>. Actual exception: none.", typeof(TExpectedException).Name);
			}
			catch (AssertFailedException) {
				throw;
			}
			catch (TExpectedException ex) {
				StringAssert.Contains(ex.Message, expectedExceptionMessage, "Bad exception message");
			}
		}
	}

}