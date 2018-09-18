using System;

using Yuzu;

namespace Lime
{
	/// <summary>
	/// Representation of 32-bit array of bit flags.
	/// </summary>
	[YuzuCompact]
	public struct BitSet32 : IEquatable<BitSet32>
	{
		/// <summary>
		/// Returns 0x00000000 (all bits are 0).
		/// </summary>
		public static readonly BitSet32 Empty = new BitSet32(0);

		/// <summary>
		/// Returns 0xffffffff (all bits are 1).
		/// </summary>
		public static readonly BitSet32 Full = new BitSet32(uint.MaxValue);

		/// <summary>
		/// Uint that is used for array representation.
		/// </summary>
		[YuzuMember("0")]
		public uint Value;

		public int Count { get { return 32; } }

		public BitSet32(uint value)
		{
			Value = value;
		}

		/// <param name="bit">Bit index (0 - 31).</param>
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

		/// <summary>
		/// Returns true if all bits are 1.
		/// </summary>
		public bool All() { return Value == uint.MaxValue; }

		/// <summary>
		/// Returns true if any bit is 1.
		/// </summary>
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

		/// <summary>
		/// Returns the <see cref="string"/> representation of this <see cref="BitSet32"/>
		/// as sequence of "0" and "1" (from 32 bit to 1 bit). Zeroes on the left are trimmed.
		/// </summary>
		public override string ToString()
		{
			return Convert.ToString(Value, 2);
		}
	}
}