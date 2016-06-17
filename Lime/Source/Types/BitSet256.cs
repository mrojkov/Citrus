using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Lime
{
	/// <summary>
	/// Representation of 256-bit array of bit flags.
	/// </summary>
	[ProtoContract]
	public struct BitSet256
	{
		[ProtoMember(1)]
		public uint Nubble0;

		[ProtoMember(2)]
		public uint Nubble1;

		[ProtoMember(3)]
		public uint Nubble2;

		[ProtoMember(4)]
		public uint Nubble3;

		[ProtoMember(5)]
		public uint Nubble4;

		[ProtoMember(6)]
		public uint Nubble5;

		[ProtoMember(7)]
		public uint Nubble6;

		[ProtoMember(8)]
		public uint Nubble7;

		public int Count { get { return 256; } }

		public static readonly BitSet256 Empty = new BitSet256();
		public static readonly BitSet256 Full = FromRange(0, 255);

		public static BitSet256 FromRange(int min, int max)
		{
			var result = new BitSet256();
			for (int i = min; i <= max; i++) {
				result[i] = true;
			}
			return result;
		}

		public bool this[int index]
		{
			get { return (GetNubble(index / 32) & 1U << index % 32) != 0; }
			set
			{
				if (value) {
					SetNubble(index / 32, GetNubble(index / 32) | 1U << index % 32);
				} else {
					SetNubble(index / 32, GetNubble(index / 32) & ~(1U << index % 32));
				}
			}
		}

		public static BitSet256 operator - (BitSet256 bitset, int index)
		{
			bitset[index] = false;
			return bitset;
		}

		public static BitSet256 operator + (BitSet256 bitset, int index)
		{
			bitset[index] = true;
			return bitset;
		}

		private uint GetNubble(int index)
		{
			switch (index) {
				case 0: return Nubble0;
				case 1: return Nubble1;
				case 2: return Nubble2;
				case 3: return Nubble3;
				case 4: return Nubble4;
				case 5: return Nubble5;
				case 6: return Nubble6;
				case 7: return Nubble7;
				default: throw new IndexOutOfRangeException();
			}
		}

		private void SetNubble(int index, uint value)
		{
			switch (index) {
				case 0: Nubble0 = value; break;
				case 1: Nubble1 = value; break;
				case 2: Nubble2 = value; break;
				case 3: Nubble3 = value; break;
				case 4: Nubble4 = value; break;
				case 5: Nubble5 = value; break;
				case 6: Nubble6 = value; break;
				case 7: Nubble7 = value; break;
			}
		}

		public static BitSet256 operator | (BitSet256 lhs, BitSet256 rhs)
		{
			lhs.Nubble0 |= rhs.Nubble0;
			lhs.Nubble1 |= rhs.Nubble1;
			lhs.Nubble2 |= rhs.Nubble2;
			lhs.Nubble3 |= rhs.Nubble3;
			lhs.Nubble4 |= rhs.Nubble4;
			lhs.Nubble5 |= rhs.Nubble5;
			lhs.Nubble6 |= rhs.Nubble6;
			lhs.Nubble7 |= rhs.Nubble7;
			return lhs;
		}

		public static BitSet256 operator & (BitSet256 lhs, BitSet256 rhs)
		{
			lhs.Nubble0 &= rhs.Nubble0;
			lhs.Nubble1 &= rhs.Nubble1;
			lhs.Nubble2 &= rhs.Nubble2;
			lhs.Nubble3 &= rhs.Nubble3;
			lhs.Nubble4 &= rhs.Nubble4;
			lhs.Nubble5 &= rhs.Nubble5;
			lhs.Nubble6 &= rhs.Nubble6;
			lhs.Nubble7 &= rhs.Nubble7;
			return lhs;
		}

		public static BitSet256 operator ^ (BitSet256 lhs, BitSet256 rhs)
		{
			lhs.Nubble0 ^= rhs.Nubble0;
			lhs.Nubble1 ^= rhs.Nubble1;
			lhs.Nubble2 ^= rhs.Nubble2;
			lhs.Nubble3 ^= rhs.Nubble3;
			lhs.Nubble4 ^= rhs.Nubble4;
			lhs.Nubble5 ^= rhs.Nubble5;
			lhs.Nubble6 ^= rhs.Nubble6;
			lhs.Nubble7 ^= rhs.Nubble7;
			return lhs;
		}

		public static BitSet256 operator ~ (BitSet256 value)
		{
			value.Nubble0 = ~value.Nubble0;
			value.Nubble1 = ~value.Nubble1;
			value.Nubble2 = ~value.Nubble2;
			value.Nubble3 = ~value.Nubble3;
			value.Nubble4 = ~value.Nubble4;
			value.Nubble5 = ~value.Nubble5;
			value.Nubble6 = ~value.Nubble6;
			value.Nubble7 = ~value.Nubble7;
			return value;
		}
	}
}