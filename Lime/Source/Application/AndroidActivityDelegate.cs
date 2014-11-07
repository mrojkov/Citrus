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
	public class ActivityDelegate
	{
		public static ActivityDelegate Instance { get; private set; }

		public event Action<Activity, Bundle> Created;
		public event Action<Activity> Started;
		public event Action<Activity> Resumed;
		public event Action<Activity> Paused;
		public event Action<Activity> Stopped;
		public event Action<Activity> Destroying;
		public event Action LowMemory;

		public Activity Activity { get; private set; }

		public ActivityDelegate()
		{
			Instance = this;
		}

		public virtual void OnCreate(Activity activity, Bundle bundle)
		{
			Activity = activity;
			if (Created != null) {
				Created(Activity, bundle);
			}
		}

		public virtual void OnStart()
		{
			if (Started != null) {
				Started(Activity);
			}
		}

		public virtual void OnResume()
		{
			if (Resumed != null) {
				Resumed(Activity);
			}
		}

		public virtual void OnPause()
		{
			if (Paused != null) {
				Paused(Activity);
			}
		}

		public virtual void OnStop()
		{
			if (Stopped != null) {
				Stopped(Activity);
			}
		}

		public virtual void OnDestroy()
		{
			if (Destroying != null) {
				Destroying(Activity);
			}
			Activity = null;
		}

		public virtual void OnLowMemory()
		{
			if (LowMemory != null) {
				LowMemory();
			}
		}
	}

	public class DefaultActivityDelegate : ActivityDelegate
	{
		public GameView GameView { get; private set; }
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
			GameView = new GameView(activity);
			ContentView = new RelativeLayout(activity.ApplicationContext);
			ContentView.AddView(GameView);
			activity.SetContentView(ContentView);
			GameView.Load += (object sender, EventArgs e) => {
				if (!applicationCreated) {
					applicationCreated = true;
					application.OnCreate();
				}
				GameView.Run();
			};
			base.OnCreate(activity, bundle);
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
			base.OnResume();
		}

		public override void OnLowMemory()
		{
			Logger.Write("Memory warning, texture memory: {0}mb", CommonTexture.TotalMemoryUsedMb);
			TexturePool.Instance.DiscardUnusedTextures(2);
			System.GC.Collect();
			base.OnLowMemory();
		}
	}
}
#endif