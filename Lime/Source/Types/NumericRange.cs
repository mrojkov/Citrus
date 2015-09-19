using ProtoBuf;
using System;

namespace Lime
{
	/// <summary>
	/// Representation of numeric range of numbers around median.
	/// </summary>
	[System.Diagnostics.DebuggerStepThrough]
	[ProtoContract]
	public struct NumericRange : IEquatable<NumericRange>
	{
		[ProtoMember(1)]
		public float Median;

		/// <summary>
		/// Variation of numbers around median.
		/// </summary>
		[ProtoMember(2)]
		public float Dispersion;

		public NumericRange(float median, float variation)
		{
			Median = median;
			Dispersion = variation;
		}

		// BUG (in code or in documentation): Result can be greater than median.
		/// <summary>
		/// Returns random number from this range. Result is always lesser than median.
		/// </summary>
		public float NormalRandomNumber()
		{
			return Mathf.NormalRandom(Median, Dispersion);
		}

		public float NormalRandomNumber(Random rng)
		{
			return rng.NormalRandom(Median, Dispersion);
		}

		/// <summary>
		/// Returns random number from this range.
		/// </summary>
		public float UniformRandomNumber()
		{
			return Mathf.UniformRandom(Median, Dispersion);
		}

		public float UniformRandomNumber(Random rng)
		{
			return rng.UniformRandom(Median, Dispersion);
		}

		public bool Equals(NumericRange rhs)
		{
			return Median == rhs.Median && Dispersion == rhs.Dispersion;
		}

		public override string ToString()
		{
			return string.Format("{0}, {1}", Median, Dispersion);
		}
	}
}
