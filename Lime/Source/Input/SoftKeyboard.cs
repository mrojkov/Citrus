using System;

namespace Lime
{
	/// <summary>
	/// Програмная клавиатура (для мобильных устройств)
	/// </summary>
	public interface ISoftKeyboard
	{
		/// <summary>
		/// Возвращает true, если в данный момент клавиатура видима
		/// </summary>
		bool Visible { get; }

		/// <summary>
		/// Высота клавиатуры. Значение устанавливается, когда клавиатура показана первый раз (до этого 0). Значение может меняться
		/// </summary>
		float Height { get; }

		/// <summary>
		/// Возвращает true, если програмная клавиатура подерживается текущей платформой
		/// </summary>
		bool Supported { get; }

		/// <summary>
		/// Occurs when the keyboard shown on the screen.
		/// </summary>
		event Action Shown;

		/// <summary>
		/// Генерируется, когда клавиатура исчезла
		/// </summary>
		event Action Hidden;

		/// <summary>
		/// Показывает или прячет клавиатуру
		/// </summary>
		/// <param name="show">true, чтобы показать; false, чтобы спрятать</param>
		/// <param name="text">Набранный текст в поле ввода</param>
		void Show(bool show, string text);

		/// <summary>
		/// Изменяет текст в поле ввода
		/// </summary>
		void ChangeText(string text);
	}

	internal class DummySoftKeyboard : ISoftKeyboard
	{
		public bool Visible { get { return false; } }
		public float Height { get { return 0; } }
		public event Action Shown;
		public event Action Hidden;
		public void Show(bool show, string text) { }
		public void ChangeText(string text) { }
		public bool Supported { get { return false; } }
	}
}

