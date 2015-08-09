using ProtoBuf;
using System;

namespace Lime
{
	/// <summary>
	/// Представляет среднее значение и дисперсию. Используется для создания хаотичных значений
	/// </summary>
	[System.Diagnostics.DebuggerStepThrough]
	[ProtoContract]
	public struct NumericRange : IEquatable<NumericRange>
	{
		/// <summary>
		/// Среднее значение
		/// </summary>
		[ProtoMember(1)]
		public float Median;

		/// <summary>
		/// Отклонение от среднего значения
		/// </summary>
		[ProtoMember(2)]
		public float Dispersion;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="median">Среднее значение</param>
		/// <param name="variation">Отклонение от среднего значения</param>
		public NumericRange(float median, float variation)
		{
			Median = median;
			Dispersion = variation;
		}

		/// <summary>
		/// Возвращает случайное число, полученное с учетом среднего значения и дисперсии
		/// Результат не может быть меньше среднего значения
		/// </summary>
		public float NormalRandomNumber()
		{
			return Mathf.NormalRandom(Median, Dispersion);
		}

		public float NormalRandomNumber(System.Random rng)
		{
			return rng.NormalRandom(Median, Dispersion);
		}

		/// <summary>
		/// Возвращает случайное число, полученное с учетом среднего значения и дисперсии
		/// </summary>
		public float UniformRandomNumber()
		{
			return Mathf.UniformRandom(Median, Dispersion);
		}

		public float UniformRandomNumber(System.Random rng)
		{
			return rng.UniformRandom(Median, Dispersion);
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
