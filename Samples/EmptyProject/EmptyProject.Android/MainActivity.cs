using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace EmptyProject.Android
{
	// the ConfigurationChanges flags set here keep the EGL context
	// from being destroyed whenever the device is rotated or the
	// keyboard is shown (highly recommended for all GL apps)
	[Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize,
		Theme = "@android:style/Theme.Black.NoTitleBar.Fullscreen",
		MainLauncher = true)]
	public class MainActivity : Activity
	{
		static MainActivity()
		{
			System.Threading.ThreadPool.SetMinThreads(4, 8);
			new Lime.ActivityDelegate(() => {
				Lime.Application.Initialize(
					new Lime.ApplicationOptions
					{
						DecodeAudioInSeparateThread = false,
					}
				);
				EmptyProject.Application.Application.Initialize();
			});
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);
			Lime.ActivityDelegate.Instance.OnActivityResult(requestCode, resultCode, data);
		}

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			Lime.ActivityDelegate.Instance.OnCreate(this, bundle);
		}

		protected override void OnStart()
		{
			base.OnStart();
			Lime.ActivityDelegate.Instance.OnStart();
		}

		protected override void OnPause()
		{
			base.OnPause();
			Lime.ActivityDelegate.Instance.OnPause();
		}

		protected override void OnResume()
		{
			base.OnResume();
			Lime.ActivityDelegate.Instance.OnResume();
		}

		public override void OnLowMemory()
		{
			base.OnLowMemory();
			Lime.ActivityDelegate.Instance.OnLowMemory();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			Lime.ActivityDelegate.Instance.OnDestroy();
		}

		protected override void OnStop()
		{
			base.OnStop();
			Lime.ActivityDelegate.Instance.OnStop();
		}

		public override void OnBackPressed()
		{
			if (!Lime.ActivityDelegate.Instance.OnBackPressed())
			{
				base.OnBackPressed();
			}
		}
	}
}
