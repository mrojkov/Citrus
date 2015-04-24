using System;

namespace Lime
{
	public class SoftKeyboard
	{
		public bool Visible { get; internal set; }
		/// <summary>
		/// The height of the keyboard. Zero, until the keyboard is shown for the first time, and vary during app lifetime.
		/// </summary>
		public float Height { get; internal set; }
		public event Action Hidden;

		public void Show(bool show, string text)
		{
#if iOS || ANDROID
			GameView.Instance.ShowSoftKeyboard(show, text);
#endif
		}

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

