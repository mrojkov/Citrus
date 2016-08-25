using System;
using System.Collections;
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
		public ushort USh;
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
		[YuzuRequired]
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

		public override bool Equals(object obj)
		{
			return ((SamplePoint)obj).X == X && ((SamplePoint)obj).Y == Y;
		}
	}

	public class SampleRect
	{
		[YuzuRequired]
		public SamplePoint A;
		[YuzuRequired]
		public SamplePoint B;
	}

	public class SampleDefault
	{
		[YuzuMember]
		public int A = 3;

		[YuzuMember]
		public string B = "default";

		[YuzuMember]
		public SamplePoint P;

		public SampleDefault()
		{
			P = new SamplePoint { X = 7, Y = 2 };
		}
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
	public class SampleWithNullFieldCompact
	{
		[YuzuRequired]
		public Sample1 N;
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
		public string Name { get; set; }

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

	public interface ISample
	{
		int X { get; set; }
	}

	public interface ISampleMember
	{
		[YuzuMember]
		int X { get; set; }
	}

	public abstract class SampleMemberAbstract
	{
		[YuzuMember]
		int X = 72;
	}

	public class SampleMemberI : ISampleMember
	{
		public int X { get; set; }
		public SampleMemberI() { X = 71; }
	}

	public class SampleInterfaced : ISample
	{
		[YuzuRequired]
		public int X { get; set; }
	}

	public class SampleInterfacedGeneric<T> : ISample
	{
		[YuzuRequired]
		public int X { get; set; }

		[YuzuRequired]
		public T G;
	}

	public class SampleInterfaceField
	{
		[YuzuRequired]
		public ISample I { get; set; }
	}

	public interface ISampleField
	{
		[YuzuRequired]
		int X { get; set; }
	}

	public class SampleInterfacedField : ISampleField
	{
		public int X { get; set; }
	}

	public class SampleInterfacedFieldDup : ISampleField
	{
		[YuzuOptional]
		public int X { get; set; }
	}

	public abstract class SampleAbstract { }

	public class SampleConcrete: SampleAbstract
	{
		[YuzuRequired]
		public int XX;
	}

	public class SampleCollection<T> : ICollection<T>
	{
		private List<T> impl = new List<T>();
		public int Count { get { return impl.Count; } }
		public bool IsReadOnly { get { return false; } }
		public void Add(T item) { impl.Add(item); }
		public void Clear() { impl.Clear(); }
		public bool Contains(T item) { return impl.Contains(item); }
		public void CopyTo(T[] array, int arrayIndex) { impl.CopyTo(array, arrayIndex); }
		public bool Remove(T item) { return impl.Remove(item); }
		IEnumerator<T> IEnumerable<T>.GetEnumerator() { return impl.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return impl.GetEnumerator(); }
	}

	public class SampleExplicitCollection<T> : ICollection<T>
	{
		private List<T> impl = new List<T>();
		int ICollection<T>.Count { get { return impl.Count; } }
		bool ICollection<T>.IsReadOnly { get { return false; } }
		void ICollection<T>.Add(T item) { impl.Add(item); }
		void ICollection<T>.Clear() { impl.Clear(); }
		bool ICollection<T>.Contains(T item) { return impl.Contains(item); }
		void ICollection<T>.CopyTo(T[] array, int arrayIndex) { impl.CopyTo(array, arrayIndex); }
		bool ICollection<T>.Remove(T item) { return impl.Remove(item); }
		IEnumerator<T> IEnumerable<T>.GetEnumerator() { return impl.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return impl.GetEnumerator(); }
	}

	public class SampleCollectionWithField<T> : SampleCollection<T>
	{
		[YuzuRequired]
		public int X;
	}

	public class SampleConcreteCollection : SampleCollection<int> { }

	public class SampleWithCollection
	{
		[YuzuRequired]
		public SampleCollection<ISample> A = new SampleCollection<ISample>();
		[YuzuRequired]
		public SampleCollection<int> B = new SampleCollection<int>();
	}

	public class SampleAfter
	{
		[YuzuRequired]
		public string X;
		[YuzuAfterDeserialization]
		public void After() { X += "1"; }
	}

	public class SampleAfter2: SampleAfter
	{
		[YuzuAfterDeserialization]
		public void After2() { X += "2"; }
		[YuzuAfterDeserialization]
		public void After3() { X += "3"; }
	}

	public class SampleMerge
	{
		private Dictionary<int, int> di = new Dictionary<int,int>();
		private List<int> li = new List<int>();
		[YuzuRequired]
		public Dictionary<int, int> DI { get { return di; } }
		[YuzuRequired]
		public List<int> LI { get { return li; } }
		[YuzuOptional, YuzuMerge]
		public Sample1 M;
	}

	public class SampleNested
	{
		public enum NestedEnum { One, Two };
		public class NestedClass
		{
			[YuzuOptional]
			public int Z;
		}
		[YuzuRequired]
		public NestedEnum E;
		[YuzuRequired]
		public NestedClass C;
		[YuzuMember]
		public NestedEnum[] Z = null;
	}

	public class Bad1
	{
		[YuzuRequired]
		[YuzuOptional]
		public int F;
	}

	public class Bad2
	{
		[YuzuRequired("привет")]
		public int F;
	}

	public class Bad3
	{
		[YuzuRequired("q")]
		public int F;
		[YuzuRequired("q")]
		public int G;
	}

	public class BadMerge1
	{
		[YuzuRequired]
		public int F { get { return 1; } }
	}

	public class BadMerge2
	{
		[YuzuRequired, YuzuMerge]
		public int F;
	}

	public static class XAssert
	{
		public static void Throws<TExpectedException>(Action exceptionThrower, string expectedExceptionMessage = "")
			where TExpectedException : Exception
		{
			try {
				exceptionThrower();
			}
			catch (TExpectedException ex) {
				StringAssert.Contains(ex.Message, expectedExceptionMessage, "Bad exception message");
				return;
			}
			Assert.Fail("Expected exception:<{0}>. Actual exception: none.", typeof(TExpectedException).Name);
		}
	}

}

namespace YuzuTest2
{
	public class SampleNamespace
	{
		[YuzuRequired]
		public YuzuTest.SampleBase B;
	}
}
