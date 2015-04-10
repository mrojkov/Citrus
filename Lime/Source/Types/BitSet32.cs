using System;

using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public struct BitSet32 : IEquatable<BitSet32>
	{
		public static readonly BitSet32 Empty = new BitSet32(0);
		public static readonly BitSet32 Full = new BitSet32(uint.MaxValue);

		[ProtoMember(1)]
		public uint Value;

		public BitSet32(uint value)
		{
			Value = value;
		}

		public bool this[int bit]
		{
			get { return (Value & 1U << bit) != 0; }
			set
			{
				if (value) {
					Value |= 1U << bit;
				} else {
					Value &= ~(1U << bit);
				}
			}
		}

		public bool All() { return Value == uint.MaxValue; }
		public bool Any() { return Value != 0; }

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return Value == ((BitSet32)obj).Value;
		}

		public bool Equals(BitSet32 other)
		{
			return Value == other.Value;
		}

		public static bool operator ==(BitSet32 lhs, BitSet32 rhs)
		{
			return lhs.Value == rhs.Value;
		}

		public static bool operator !=(BitSet32 lhs, BitSet32 rhs)
		{
			return lhs.Value != rhs.Value;
		}

		public override string ToString()
		{
			return Convert.ToString(Value, 2);
		}
	}
}