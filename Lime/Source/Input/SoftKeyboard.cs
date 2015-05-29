using System;

namespace Lime
{
	/// <summary>
	/// Програмная клавиатура (для мобильных устройств)
	/// </summary>
	public class SoftKeyboard
	{
		/// <summary>
		/// Возвращает true, если в данный момент клавиатура видима
		/// </summary>
		public bool Visible { get; internal set; }

		/// <summary>
		/// Высота клавиатуры. Значение устанавливается, когда клавиатура показана первый раз (до этого 0). Значение может меняться
		/// </summary>
		public float Height { get; internal set; }

		/// <summary>
		/// Генерируется, когда клавиатура исчезла
		/// </summary>
		public event Action Hidden;

		/// <summary>
		/// Показывает или прячет клавиатуру
		/// </summary>
		/// <param name="show">true, чтобы показать; false, чтобы спрятать</param>
		/// <param name="text">Набранный текст в поле ввода</param>
		public void Show(bool show, string text)
		{
#if iOS || ANDROID
			GameView.Instance.ShowSoftKeyboard(show, text);
#endif
		}

		/// <summary>
		/// Изменяет текст в поле ввода
		/// </summary>
		public void ChangeText(string text)
		{
#if iOS || ANDROID
			GameView.Instance.ChangeSoftKeyboardText(text);
#endif
		}

		internal void RaiseHidden()
		{
			if (Hidden != null)
				Hidden();
		}

		/// <summary>
		/// Возвращает true, если програмная клавиатура подерживается текущей платформой
		/// </summary>
		public bool Supported
		{
			get {
#if iOS || ANDROID
				return true;
#else
				return false;
#endif
			}
		}
	}
}

