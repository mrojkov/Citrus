#if ANDROID
using System;
using Android.App;
using Android.OS;
using Android.Content.PM;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

namespace Lime
{
	public class MainActivity : Activity
	{
		public static MainActivity Instance { get; private set; }
		public GameView GameView { get; private set; }
		public RelativeLayout ContentView { get; private set; }
		private InputMethodManager imm;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			AudioSystem.Initialize();
			GameView = new GameView(this);
			ContentView = new RelativeLayout(ApplicationContext);
			ContentView.AddView(GameView);
			SetContentView(ContentView);
			imm = (InputMethodManager)ApplicationContext.GetSystemService(Android.Content.Context.InputMethodService);
			Instance = this;
		}

		protected override void OnPause()
		{
			base.OnPause();
			Lime.Application.Instance.Active = false;
			Lime.Application.Instance.OnDeactivate();
			AudioSystem.Active = false;
			GameView.Pause();
			GameView.ClearFocus();
		}

		protected override void OnResume()
		{
			AudioSystem.Active = true;
			Lime.Application.Instance.Active = true;
			Lime.Application.Instance.OnActivate();
			GameView.Resume();
			base.OnResume();
			if (!GameView.IsFocused) {
				GameView.RequestFocus();
			}
		}
			
		public void ShowOnscreenKeyboard(bool show, string text)
		{
			if (show) {
				imm.ShowSoftInput(GameView, 0);
			} else {
				imm.HideSoftInputFromWindow(GameView.WindowToken, 0);
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