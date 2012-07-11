using System;
using ProtoBuf;

namespace Lime
{
	[System.Diagnostics.DebuggerStepThrough]
	[ProtoContract]
	public struct NumericRange : IEquatable<NumericRange>
	{
		[ProtoMember(1)]
		public float Median;

		[ProtoMember(2)]
		public float Dispersion;

		public NumericRange(float median, float variation)
		{
			Median = median;
			Dispersion = variation;
		}

		public float NormalRandomNumber()
		{
			return Mathf.NormalRandom(Median, Dispersion);
		}

		public float UniformRandomNumber()
		{
			return Mathf.UniformRandom(Median, Dispersion);
		}

		bool IEquatable<NumericRange>.Equals(NumericRange rhs)
		{
			return Median == rhs.Median && Dispersion == rhs.Dispersion;
		}

		public override string ToString()
		{
			return String.Format("{0}, {1}", Median, Dispersion);
		}
	}
}
