#if ANDROID
using System;
using Android.Views.InputMethods;

namespace Lime
{
	public class AndroidSoftKeyboard : ISoftKeyboard
	{
		private readonly GameView gameView;
		private readonly InputMethodManager imm;
		private float height;
		private bool prevKeyboardVisible;

		public AndroidSoftKeyboard(GameView gameView)
		{
			this.gameView = gameView;
			imm = (InputMethodManager)gameView.Context.GetSystemService(Android.Content.Context.InputMethodService);
		}

		public bool Visible
		{
			get { return Height > 0; }
		}

		public float Height 
		{ 
			get { return height / Window.Current.PixelScale; }
			internal set 
			{
				height = value;
				OnHeightChanged();
			}
		}

		public bool Supported
		{
			get { return true; }
		}

		public event Action Shown;
		public event Action Hidden;

		public void Show(bool show, SoftKeyboardType type)
		{
			gameView.SoftKeyboardType = type;
			if (show) {
				gameView.Focusable = true;
				gameView.FocusableInTouchMode = true;
				gameView.RequestFocus();
				imm.ShowSoftInput(gameView, ShowFlags.Forced);
			} else {
				gameView.Focusable = false;
				gameView.FocusableInTouchMode = false;
				imm.HideSoftInputFromWindow(gameView.WindowToken, 0);
				// HACK: This will reset UI flags back to values defined in OnWindowFocusChanged
				ActivityDelegate.Instance.Activity?.OnWindowFocusChanged(true);
			}
		}

		void OnHeightChanged()
		{
			if (Visible != prevKeyboardVisible) {
				var handler = Visible ? Shown : Hidden;
				if (handler != null) {
					handler();
				}
			}
			prevKeyboardVisible = Visible;
		}
	}
}
#endif