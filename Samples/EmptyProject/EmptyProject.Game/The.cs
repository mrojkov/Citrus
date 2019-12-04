using System.Threading;
using EmptyProject.Application;
using EmptyProject.Dialogs;
using Lime;

namespace EmptyProject
{
	public static class The
	{
		public static Application.Application App => Application.Application.Instance;
		public static WindowWidget World => App.World;
		public static IWindow Window => World.Window;
		public static SoundManager SoundManager => SoundManager.Instance;
		public static AppData AppData => AppData.Instance;
		public static Profile Profile => Profile.Instance;
		public static DialogManager DialogManager => DialogManager.Instance;
		public static Logger Log => Logger.Instance;
		public static Persistence Persistence => persistence.Value;
		private static readonly ThreadLocal<Persistence> persistence = new ThreadLocal<Persistence>(() => new Persistence());
	}
}
