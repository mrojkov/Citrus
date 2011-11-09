using System;
using ProtoBuf;

namespace Lime
{
	[System.Diagnostics.DebuggerStepThrough]
	[ProtoContract]
	public struct NumericRange : IEquatable<NumericRange>
	{
		[ProtoMember (1)]
		public float Median;

		[ProtoMember (2)]
		public float Variation;

		public NumericRange (float median, float variation)
		{
			Median = median;
			Variation = variation;
		}

		public float NormalRandomNumber ()
		{
			float x = 0;
			for (int i = 0; i < 12; ++i)
				x += Utils.Random ();
			x -= 6;
			return Median + x * Variation;
		}

		public float UniformRandomNumber ()
		{
			return Median + (Utils.Random () - 0.5f) * Variation;
		}

		bool IEquatable<NumericRange>.Equals (NumericRange rhs)
		{
			return Median == rhs.Median && Variation == rhs.Variation;
		}

		public override string ToString ()
		{
			return String.Format ("{0}, {1}", Median, Variation);
		}
	}
}
