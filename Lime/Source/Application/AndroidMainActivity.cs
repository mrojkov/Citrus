#if ANDROID
using System;
using Android.App;
using Android.OS;
using Android.Content.PM;
using Android.Views;

namespace Lime
{
	public class MainActivity : Activity
	{
		public static MainActivity Instance { get; private set; }
		GameView view;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate(bundle);
			AudioSystem.Initialize();
			view = new GameView(this);
			SetContentView(view);
			Instance = this;
		}

		protected override void OnPause()
		{
			base.OnPause();
			Lime.Application.Instance.Active = false;
			Lime.Application.Instance.OnDeactivate();
			AudioSystem.Active = false;
			view.Pause();
		}

		protected override void OnResume()
		{
			view.Resume();
			AudioSystem.Active = true;
			Lime.Application.Instance.Active = true;
			Lime.Application.Instance.OnActivate();
			base.OnResume();
		}
	}
}
#endif