using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public static class CommandLineArgs
	{
		public static readonly bool MaximizedWindow = CheckFlag("--Maximized");
		public static readonly bool OpenGL = CheckFlag("--OpenGL");
		public static readonly bool Limit25FPS = CheckFlag("--Limit25FPS");
		public static readonly bool FullscreenMode = CheckFlag("--Fullscreen");
		public static readonly bool SimulateSlowExternalStorage = CheckFlag("--SimulateSlowExternalStorage");
		public static readonly bool NoAudio = CheckFlag("--NoAudio");
		public static readonly bool NoMusic = CheckFlag("--NoMusic");

		public static string[] Get()
		{
			return System.Environment.GetCommandLineArgs();
		}

		public static bool CheckFlag(string name)
		{
			return Array.IndexOf(Get(), name) >= 0;
		}
	}
}
