using System;
using System.Collections.Generic;

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

	public class SampleBool
	{
		[YuzuRequired]
		public bool B;
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