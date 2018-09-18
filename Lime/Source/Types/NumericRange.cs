using System;
using Yuzu;

namespace Lime
{
	/// <summary>
	/// Representation of numeric range of numbers around median.
	/// </summary>
	[System.Diagnostics.DebuggerStepThrough]
	public struct NumericRange : IEquatable<NumericRange>
	{
		[YuzuMember]
		public float Median;

		[YuzuMember]
		public float Dispersion;

		public NumericRange(float median, float dispersion)
		{
			Median = median;
			Dispersion = dispersion;
		}

		/// <summary>
		/// Returns random number from a normal distribution
		/// with given median and variance (dispersion)
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
		/// Returns random number from uniform distribution
		/// with given median and range (note that Dispersion here is not the actual
		/// variance of a distribution but half of the range).
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
