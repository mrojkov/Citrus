using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lime;

namespace EmptyProject
{
	static class The
	{
		public static World World { get { return World.Instance; } }
		public static Application Application { get { return Application.Instance; } }
		public static ApplicationData ApplicationData { get { return Application.Instance.Data; } }
		public static GameProgress Progress { get { return Application.Instance.Data.Progress; } }
		public static SoundManager SoundManager { get { return SoundManager.Instance; } }
	}
}
