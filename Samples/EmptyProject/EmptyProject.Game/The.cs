using EmptyProject.Application;
using Lime;

namespace EmptyProject
{
	public static class The
	{
		public static Logger Log { get { return Logger.StaticLogger; } }
		public static WindowWidget World { get { return Application.Application.World; } }
		public static IWindow Window { get { return World.Window; } }
		public static AppData AppData { get { return AppData.Instance; } }
		public static SoundManager SoundManager { get { return SoundManager.Instance; } }
		public static Profile Profile { get { return Profile.Instance; } }
		public static Application.Application App { get { return Application.Application.Instance; } }
	}
}