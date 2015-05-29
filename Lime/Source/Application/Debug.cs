using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	/// <summary>
	/// Предоставляет отладочные методы
	/// </summary>
	public static class Debug
	{
		/// <summary>
		/// Остановить выполнение программы, когда будет нажата кнопка (виджет типа Button) (аналогично брейкпоинту)
		/// </summary>
		public static bool BreakOnButtonClick { get; set; }
		
		/// <summary>
		/// Выводит сообщение в лог
		/// </summary>
		public static void Write(string message)
		{
			Logger.Write(message);
		}

		/// <summary>
		/// Выводит сообщение в лог
		/// </summary>
		/// <param name="value">Будет преобразовано в строку</param>
		public static void Write(object value)
		{
			Write(value.ToString());
		}

		/// <summary>
		/// Выводит сообщение в лог
		/// </summary>
		public static void Write(string msg, params object[] args)
		{
			Logger.Write(msg, args);
		}
	}
}
