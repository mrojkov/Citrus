using ProtoBuf;

namespace Lime
{
	/// <summary>
	/// Способ интерполяции между текущим и следующим ключевым кадром
	/// </summary>
	public enum KeyFunction
	{
		/// <summary>
		/// Линейная интерполяция
		/// </summary>
		Linear,

		/// <summary>
		/// Значение не интерполируется, оно всегда одинаковое, пока не будет достигнут следующий ключевой кадр
		/// </summary>
		Steep,

		/// <summary>
		/// Интерполяция по сплайну. Создает эффект плавного изменения значения
		/// </summary>
		Spline,

		/// <summary>
		/// Аналогично интерполяции по сплайну, по при зацикленной анимации дает чуть более плавную анимацию
		/// из-за того, меньшего сглаживания
		/// </summary>
		ClosedSpline
	}

	/// <summary>
	/// Интерфейс ключевого кадры
	/// </summary>
	public interface IKeyframe
	{
		/// <summary>
		/// Номер кадра, на котором установлен ключевой кадр
		/// </summary>
		int Frame { get; set; }

		/// <summary>
		/// Способ интерполяции между текущим и следующим ключевым кадром
		/// </summary>
		KeyFunction Function { get; set; }

		/// <summary>
		/// Значение свойства
		/// </summary>
		object Value { get; set; }

		IKeyframe Clone();
	}

	/// <summary>
	/// Ключевые кадры (ключи) используются для анимации свойств объектов.
	/// Ключевой кадр устанавливается на определенном кадре и хранит значение свойства.
	/// При анимации ищутся 2 рядом стоящих ключевых кадра и выбирается значение между ними в зависимости от текущего кадра
	/// </summary>
	/// <typeparam name="T">Тип свойства, анимируемый ключевым кадром</typeparam>
	[ProtoContract]
	public class Keyframe<T> : IKeyframe
	{
		/// <summary>
		/// Номер кадра, на котором установлен ключевой кадр
		/// </summary>
		[ProtoMember(1)]
		public int Frame { get; set; }

		/// <summary>
		/// Способ интерполяции между текущим и следующим ключевым кадром
		/// </summary>
		[ProtoMember(2)]
		public KeyFunction Function { get; set; }

		/// <summary>
		/// Значение свойства
		/// </summary>
		[ProtoMember(3)]
		public T Value;

		object IKeyframe.Value
		{
			get { return (object)this.Value; }
			set { this.Value = (T)value; }
		}

		public Keyframe() { }

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="frame">Номер кадра, на котором установлен ключевой кадр</param>
		/// <param name="value">Значение свойства</param>
		/// <param name="function">Способ интерполяции между текущим и следующим ключевым кадром</param>
		public Keyframe(int frame, T value, KeyFunction function)
		{			
			this.Frame = frame;
			this.Value = value;
			this.Function = function;
		}

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="frame">Номер кадра, на котором установлен ключевой кадр</param>
		/// <param name="value">Значение свойства</param>
		public Keyframe(int frame, T value)
		{
			this.Frame = frame;
			this.Value = value;
		}

		/// <summary>
		/// Создает клон ключевого кадра
		/// </summary>
		public Keyframe<T> Clone()
		{
			return new Keyframe<T>() {
				Frame = Frame,
				Function = Function,
				Value = Value
			};
		}

		IKeyframe IKeyframe.Clone()
		{
			return Clone();
		}
	}
}
