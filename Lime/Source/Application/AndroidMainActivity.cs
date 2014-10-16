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
	/// <summary>
	/// This class is useful when we can't directly inherit the app activity 
	/// from Lime.MainActivity
	/// </summary>
	public class MainActivityImplementation
	{
		Activity activity;
		public GameView GameView { get; private set; }
		public RelativeLayout ContentView { get; private set; }

		public MainActivityImplementation(Activity activity)
		{
			this.activity = activity;
		}

		public void OnCreate(Bundle bundle)
		{
			AudioSystem.Initialize();
			GameView = new GameView(activity);
			ContentView = new RelativeLayout(activity.ApplicationContext);
			ContentView.AddView(GameView);
			activity.SetContentView(ContentView);
		}

		public void OnPause()
		{
			Lime.Application.Instance.Active = false;
			Lime.Application.Instance.OnDeactivate();
			AudioSystem.Active = false;
			GameView.Pause();
			GameView.ClearFocus();
		}

		public void OnResume()
		{
			AudioSystem.Active = true;
			Lime.Application.Instance.Active = true;
			Lime.Application.Instance.OnActivate();
			GameView.Resume();
			if (!GameView.IsFocused) {
				GameView.RequestFocus();
			}
		}

		public void OnDestroy()
		{
			Lime.Application.Instance.OnTerminate();
		}

		public void OnLowMemory()
		{
			Logger.Write("Memory warning, texture memory: {0}mb", CommonTexture.TotalMemoryUsedMb);
			Lime.TexturePool.Instance.DiscardUnusedTextures(2);
			System.GC.Collect();
		}
	}

	public class MainActivity : Activity
	{
		public MainActivityImplementation Implementation { get; private set; }
		// In case if the app's main activity isn't inherited from this class
		// this property should be set manually
		public static Activity Instance { get; set; }

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			Instance = this;
			Implementation = new MainActivityImplementation(this);
			Implementation.OnCreate(bundle);
		}

		protected override void OnPause()
		{
			base.OnPause();
			Implementation.OnPause();
		}

		protected override void OnResume()
		{
			base.OnResume();
			Implementation.OnResume();
		}

		public override void OnLowMemory()
		{
			Implementation.OnLowMemory();
			base.OnLowMemory();
		}
	}
}
#endif