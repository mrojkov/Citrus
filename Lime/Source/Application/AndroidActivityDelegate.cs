#if ANDROID
using System;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Content.PM;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Android.Content;

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
		internal Input Input { get; private set; }
		
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
			Input = new Input();
		}

		public void OnCreate(Activity activity, Bundle bundle)
		{
			Debug.Write("Activity.OnCreate");
			Activity = activity;
			if (GameView == null) {
				GameView = new GameView(Activity, Input);
				GameView.Resize += (s, e) => {
					// Initialize the application on Resize (not Load) event,
					// because we may need a valid screen resolution
					if (gameInitializer != null) {
						gameInitializer();
						gameInitializer = null;
					}
				};
			} else {
				RemoveGameViewFromParent();
			}
			ContentView = new RelativeLayout(activity.ApplicationContext);
			ContentView.AddView(GameView);
			Activity.SetContentView(ContentView);
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
			GameView.Resume();
			if (!GameView.IsFocused) {
				GameView.RequestFocus();
			}
			GameView.Run();
			AccelerometerListener.StartListening(Input);
			if (Resumed != null) {
				Resumed(Activity);
			}
		}

		public void OnPause()
		{
			AudioSystem.Active = false;
			GameView.Pause();
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
			Activity = null;
		}

		public void OnLowMemory()
		{
			Logger.Write("Memory warning, texture memory: {0}mb", CommonTexture.TotalMemoryUsedMb);
			TexturePool.Instance.DiscardTexturesUnderPressure();
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
	}
<<<<<<< .working

	public class DefaultActivityDelegate : ActivityDelegate
	{
		public static GameView GameView { get; private set; }
		public RelativeLayout ContentView { get; private set; }
		private bool applicationCreated;
		private Application application;

		public DefaultActivityDelegate(Application app)
		{
			this.application = app;
			AudioSystem.Initialize();
		}

		public override void OnCreate(Activity activity, Bundle bundle)
		{
			if (GameView == null) {
				GameView = new GameView(activity);
				GameView.OnCreate();
			}
			Debug.Write("Activity.OnCreate");
			RemoveGameViewFromParent();
			ContentView = new RelativeLayout(activity.ApplicationContext);
			ContentView.AddView(GameView);
			activity.SetContentView(ContentView);
			GameView.Resize += (object sender, EventArgs e) => {
				// Initialize the application on Resize (not Load) event,
				// because we may need a valid screen resolution
				if (!applicationCreated) {
					applicationCreated = true;
					application.OnCreate();
				}
			};
			base.OnCreate(activity, bundle);
		}

		private void RemoveGameViewFromParent()
		{
			if (GameView.Instance.Parent != null) {
				(GameView.Instance.Parent as RelativeLayout).RemoveView(GameView);
			}
		}

		public override void OnPause()
		{
			application.Active = false;
			application.OnDeactivate();
			AudioSystem.Active = false;
			GameView.Pause();
			GameView.ClearFocus();
			GameView.ShowSoftKeyboard(false, null);
			base.OnPause();
		}

		public override void OnResume()
		{
			AudioSystem.Active = true;
			application.Active = true;
			application.OnActivate();
			GameView.Resume();
			if (!GameView.IsFocused) {
				GameView.RequestFocus();
			}
			GameView.Run();
			base.OnResume();
		}

		public override void OnLowMemory()
		{
			Logger.Write("Memory warning, texture memory: {0}mb", CommonTexture.TotalMemoryUsedMb);
			TexturePool.Instance.DiscardTexturesUnderPressure();
			System.GC.Collect();
			base.OnLowMemory();
		}

#if __ANDROID_14__
		public override void OnTrimMemory(TrimMemory level)
		{
			Logger.Write("Memory warning, texture memory: {0}mb", CommonTexture.TotalMemoryUsedMb);
			System.GC.Collect();
			base.OnTrimMemory(level);
		}
#endif
	}

||||||| .merge-left.r226789

	public class DefaultActivityDelegate : ActivityDelegate
	{
		public static GameView GameView { get; private set; }
		public RelativeLayout ContentView { get; private set; }
		private bool applicationCreated;
		private Application application;

		public DefaultActivityDelegate(Application app)
		{
			this.application = app;
			AudioSystem.Initialize();
		}

		public override void OnCreate(Activity activity, Bundle bundle)
		{
			if (GameView == null) {
				GameView = new GameView(activity);
				GameView.OnCreate();
			}
			Debug.Write("Activity.OnCreate");
			RemoveGameViewFromParent();
			ContentView = new RelativeLayout(activity.ApplicationContext);
			ContentView.AddView(GameView);
			activity.SetContentView(ContentView);
			GameView.Resize += (object sender, EventArgs e) => {
				// Initialize the application on Resize (not Load) event,
				// because we may need a valid screen resolution
				if (!applicationCreated) {
					applicationCreated = true;
					application.OnCreate();
				}
			};
			base.OnCreate(activity, bundle);
		}

		private void RemoveGameViewFromParent()
		{
			if (GameView.Instance.Parent != null) {
				(GameView.Instance.Parent as RelativeLayout).RemoveView(GameView);
			}
		}

		public override void OnPause()
		{
			application.Active = false;
			application.OnDeactivate();
			AudioSystem.Active = false;
			GameView.Pause();
			GameView.ClearFocus();
			base.OnPause();
		}

		public override void OnResume()
		{
			AudioSystem.Active = true;
			application.Active = true;
			application.OnActivate();
			GameView.Resume();
			if (!GameView.IsFocused) {
				GameView.RequestFocus();
			}
			GameView.Run();
			base.OnResume();
		}

		public override void OnLowMemory()
		{
			Logger.Write("Memory warning, texture memory: {0}mb", CommonTexture.TotalMemoryUsedMb);
			TexturePool.Instance.DiscardTexturesUnderPressure();
			System.GC.Collect();
			base.OnLowMemory();
		}

#if __ANDROID_14__
		public override void OnTrimMemory(TrimMemory level)
		{
			Logger.Write("Memory warning, texture memory: {0}mb", CommonTexture.TotalMemoryUsedMb);
			System.GC.Collect();
			base.OnTrimMemory(level);
		}
#endif
	}

=======
>>>>>>> .merge-right.r228510
}
#endif
