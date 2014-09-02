#if ANDROID
using System;
using Android.App;
using Android.OS;
using Android.Content.PM;
using Android.Views;
using Android.Views.InputMethods;

namespace Lime
{
	public class MainActivity : Activity
	{
		public static MainActivity Instance { get; private set; }
		GameView view;
		InputMethodManager imm;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate(bundle);
			AudioSystem.Initialize();
			view = new GameView(this);
			SetContentView(view);
			Instance = this;
			imm = (InputMethodManager)ApplicationContext.GetSystemService(Android.Content.Context.InputMethodService);
		}

		protected override void OnPause()
		{
			base.OnPause();
			Lime.Application.Instance.Active = false;
			Lime.Application.Instance.OnDeactivate();
			AudioSystem.Active = false;
			view.Pause();
			view.ClearFocus();
		}

		protected override void OnResume()
		{
			view.Resume();
			AudioSystem.Active = true;
			Lime.Application.Instance.Active = true;
			Lime.Application.Instance.OnActivate();
			base.OnResume();
			if (!view.IsFocused) {
				view.RequestFocus();
			}
		}
			
		public void ShowOnscreenKeyboard(bool show, string text)
		{
			if (show) {
				imm.ShowSoftInput(view, ShowFlags.Implicit);
			} else {
				imm.HideSoftInputFromWindow(view.WindowToken, 0);
			}
		}

		public void ChangeOnscreenKeyboardText(string text)
		{
		}

		public override void OnLowMemory()
		{
			Logger.Write("Memory warning, texture memory: {0}mb", CommonTexture.TotalMemoryUsedMb);
			Lime.TexturePool.Instance.DiscardUnusedTextures(2);
			System.GC.Collect();
			base.OnLowMemory();
		}
	}
}
#endif