namespace EmptyProject.Dialogs
{
	public class SplashScreen : Dialog<Scenes.Splash>
	{
		public SplashScreen()
		{
			Scene.RunAnimationStart();
			Root.AnimationStopped += CrossfadeInto<MainMenu>;
		}
	}
}
