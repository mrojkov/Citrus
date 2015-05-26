using System;
using System.Text;
using ProtoBuf;

namespace Lime
{
	/// <summary>
	/// Типы маркеров анимации
	/// </summary>
	[ProtoContract]
	public enum MarkerAction
	{
		/// <summary>
		/// Начало анимации
		/// </summary>
		[ProtoEnum]
		Play,

		/// <summary>
		/// Конец анимации
		/// </summary>
		[ProtoEnum]
		Stop,

		/// <summary>
		/// Переход на другой маркер
		/// </summary>
		[ProtoEnum]
		Jump,

		/// <summary>
		/// Уничтожение объекта
		/// </summary>
		[ProtoEnum]
		Destroy
	}

	/// <summary>
	/// Маркер анимации
	/// </summary>
	[ProtoContract]
	public class Marker
	{
		/// <summary>
		/// Название маркера
		/// </summary>
		[ProtoMember(1)]
		public string Id { get; set; }

		/// <summary>
		/// Номер кадра, на который установлен маркер
		/// </summary>
		[ProtoMember(2)]
		public int Frame { get; set; }

		/// <summary>
		/// Номер кадра, переведенный в миллисекунды
		/// </summary>
		public int Time { get { return AnimationUtils.FramesToMsecs(Frame); } }

		/// <summary>
		/// Тип маркеры
		/// </summary>
		[ProtoMember(3)]
		public MarkerAction Action { get; set; }

		/// <summary>
		/// Id маркера, на который будет осуществлен переход (только если Action == MarkerAction.Jump)
		/// </summary>
		[ProtoMember(4)]
		public string JumpTo { get; set; }

		/// <summary>
		/// Произвольное действие, назначаемое пользователем. Выполнится, когда будет достигнут этот маркер
		/// </summary>
		public Action CustomAction { get; set; }

		internal Marker Clone()
		{
			return (Marker)MemberwiseClone();
		}

		public override string ToString()
		{
			return string.Format("{1} '{0}'", Id, Action);
		}
	}
}
