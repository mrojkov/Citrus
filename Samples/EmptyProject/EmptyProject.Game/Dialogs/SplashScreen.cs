using System;

namespace EmptyProject.Dialogs
{
	public class SplashScreen : Dialog<Scenes.Splash>
	{
		public SplashScreen(Action onClosed = null)
		{
			Scene.RunAnimationStart();
			Root.AnimationStopped += () => {
				new ScreenCrossfade(() => {
					CloseImmediately();
					onClosed.SafeInvoke();
				});
			};
		}
	}
}
