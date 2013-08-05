using System;
using System.IO;

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
			svn = "/opt/subversion/bin/svn";
			if (!File.Exists(svn)) {
				Console.WriteLine(string.Format("WARNING: '{0}' not found. Visit http://www.wandisco.com/subversion/download#osx\n", svn));
			}
#endif
		}

		public static void Update(string path)
		{
			SvnCommand("update " + path);
		}

		public static void Commit(string path, string message)
		{
			SvnCommand(string.Format("commit {0} -m \"{1}\"", path, message));
		}

		private static void SvnCommand(string args)
		{
			if (Process.Start(svn, args) != 0) {
				throw new Lime.Exception("SVN error");
			}
		}
	}
}

