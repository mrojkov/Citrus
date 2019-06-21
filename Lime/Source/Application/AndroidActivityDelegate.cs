#if ANDROID
using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Widget;

namespace Lime
{
	public class ActivityDelegate
	{
		public class BackButtonEventArgs
		{
			public bool Handled;
		}

		public static ActivityDelegate Instance { get; private set; }

		private Action gameInitializer;
		internal Input Input { get; } = Application.Input;

		public Activity Activity { get; private set; }
		public GameView GameView { get; private set; }
		public RelativeLayout ContentView { get; private set; }

		public delegate void BackButtonDelegate(BackButtonEventArgs args);
		public delegate void ActivityResultDelegate(int requestCode, Result resultCode, Intent data);

		public event Action<Activity, Bundle> Created;
		public event Action<Activity> Started;
		public event Action<Activity> Resumed;
		public event Action<Activity> Paused;
		public event Action<Activity> Stopped;
		public event Action<Activity> Destroying;
#if __ANDROID_14__
		public event Action<TrimMemory> TrimmingMemory;
#endif
		public event Action LowMemory;

		public event Action<Android.Content.Res.Configuration> ConfigurationChanged;
		public event BackButtonDelegate BackPressed;
		public event ActivityResultDelegate ActivityResult;

		public ActivityDelegate(Action gameInitializer)
		{
			Instance = this;
			this.gameInitializer = gameInitializer;
		}

		public delegate void RequestPermissionsResultHandler(
			int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults);

		public event RequestPermissionsResultHandler RequestPermissionsResult;

		public void OnCreate(Activity activity, Bundle bundle)
		{
			Debug.Write("Activity.OnCreate");
			Activity = activity;
			if (GameView == null) {
				GameView = new GameView(Activity, Input);
			} else {
				RemoveGameViewFromParent();
			}
			ContentView = new RelativeLayout(activity.ApplicationContext);
			ContentView.AddView(GameView);
			Activity.SetContentView(ContentView);
			RestrictSupportedOrientationsWith(Application.SupportedDeviceOrientations);
			Application.SupportedDeviceOrientationsChanged += RestrictSupportedOrientationsWith;
			if (gameInitializer != null) {
				gameInitializer();
				gameInitializer = null;
			}
			if (Created != null) {
				Created(Activity, bundle);
			}
		}

		private void RemoveGameViewFromParent()
		{
			if (GameView.Parent != null) {
				(GameView.Parent as RelativeLayout).RemoveView(GameView);
			}
		}

		public void OnStart()
		{
			if (Started != null) {
				Started(Activity);
			}
			AccelerometerListener.StartListening(Input);
		}

		public void OnResume()
		{
			AudioSystem.Active = true;
			if (!GameView.IsFocused) {
				GameView.RequestFocus();
			}
			AccelerometerListener.StartListening(Input);
			if (Resumed != null) {
				Resumed(Activity);
			}
		}

		public void OnPause()
		{
			AudioSystem.Active = false;
			GameView.ClearFocus();
			if (Paused != null) {
				Paused(Activity);
			}
			AccelerometerListener.StopListening();
		}

		public void OnStop()
		{
			if (Stopped != null) {
				Stopped(Activity);
			}
			AccelerometerListener.StopListening();
		}

		public void OnDestroy()
		{
			if (Destroying != null) {
				Destroying(Activity);
			}
			AccelerometerListener.StopListening();
			Application.SupportedDeviceOrientationsChanged -= RestrictSupportedOrientationsWith;
			Activity = null;
		}

		public void OnLowMemory()
		{
			Logger.Write("Memory warning, texture memory: {0}mb", CommonTexture.TotalMemoryUsedMb);
			Window.Current.InvokeOnRendering(() => {
				TexturePool.Instance.DiscardTexturesUnderPressure();
			});
			System.GC.Collect();
			if (LowMemory != null) {
				LowMemory();
			}
		}

#if __ANDROID_14__
		public void OnTrimMemory(TrimMemory level)
		{
			Logger.Write("Memory warning, texture memory: {0}mb", CommonTexture.TotalMemoryUsedMb);
			System.GC.Collect();
			if (TrimmingMemory != null) {
				TrimmingMemory(level);
			}
		}
#endif

		public bool OnBackPressed()
		{
			var args = new BackButtonEventArgs();
			if (BackPressed != null) {
				BackPressed(args);
			}
			return args.Handled;
		}

		public void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
		{
			if (ConfigurationChanged != null) {
				ConfigurationChanged(newConfig);
			}
		}

		public void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			if (ActivityResult != null) {
				ActivityResult(requestCode, resultCode, data);
			}
		}

		public void OnRequestPermissionsResult(
			int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
		{
			if (RequestPermissionsResult != null) {
				RequestPermissionsResult(requestCode, permissions, grantResults);
			}
		}

		private void RestrictSupportedOrientationsWith(DeviceOrientation orientation)
		{
			Activity.RequestedOrientation = GetScreenOrientation(orientation);
		}

		private static ScreenOrientation GetScreenOrientation(DeviceOrientation orientation)
		{
			switch (orientation) {
				case DeviceOrientation.LandscapeLeft:
					return ScreenOrientation.Landscape;
				case DeviceOrientation.LandscapeRight:
					return ScreenOrientation.ReverseLandscape;
				case DeviceOrientation.AllLandscapes:
					return ScreenOrientation.UserLandscape;
				case DeviceOrientation.Portrait:
					return ScreenOrientation.Portrait;
				case DeviceOrientation.PortraitUpsideDown:
					return ScreenOrientation.ReversePortrait;
				case DeviceOrientation.AllPortraits:
					return ScreenOrientation.UserPortrait;
				default:
					return ScreenOrientation.FullUser;
			}
		}
	}
}
#endif
