using System;

using ProtoBuf;

namespace Lime
{
	/// <summary>
	/// 32-битный массив битовых флагов
	/// </summary>
	[ProtoContract]
	public struct BitSet32 : IEquatable<BitSet32>
	{
		/// <summary>
		/// Возвращает 0x00000000 (все биты установлены в 0)
		/// </summary>
		public static readonly BitSet32 Empty = new BitSet32(0);

		/// <summary>
		/// Возвращает 0xffffffff (все биты установлены в 1)
		/// </summary>
		public static readonly BitSet32 Full = new BitSet32(uint.MaxValue);

		/// <summary>
		/// Текущеее значение
		/// </summary>
		[ProtoMember(1)]
		public uint Value;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="value">Текущеее значение</param>
		public BitSet32(uint value)
		{
			Value = value;
		}

		/// <summary>
		/// Возвращает или задает указанный бит
		/// </summary>
		/// <param name="bit">Номер бита (0 - 31)</param>
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
		/// Возвращает true, если все биты 1
		/// </summary>
		public bool All() { return Value == uint.MaxValue; }

		/// <summary>
		/// Возвращает true, если какой-нибудь из битов 1
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

		public override string ToString()
		{
			return Convert.ToString(Value, 2);
		}
	}
}