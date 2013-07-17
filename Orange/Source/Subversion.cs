using System;

namespace Orange
{
	public class Subversion
	{
		private static string svn;

		static Subversion()
		{
#if WIN
			svn = "svn";
#else
			Svn = "/opt/subversion/bin";
			if (!File.PathExist(Svn)) {
				Console.WriteLine(string.Format("WARNING: '{0}' not found", Svn));
			}
#endif
		}

		public static void Update(string path)
		{
			SvnCommand("update " + path);
		}

		public static void Commit(string path, string message)
		{
			SvnCommand("commit " + path);
		}

		private static void SvnCommand(string args)
		{
			if (Toolbox.StartProcess(svn, args) != 0) {
				throw new Lime.Exception("SVN error");
			}
		}
	}
}

