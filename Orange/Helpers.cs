using System;
using System.Collections.Generic;
using System.IO;

namespace Orange
{
	public static class Helpers
	{
		public static bool IsPathHidden (string path)
		{
			if (path == ".") {
				// "." directory is always hidden.
				path = System.IO.Directory.GetCurrentDirectory ();
			}
			return (System.IO.File.GetAttributes (path) & FileAttributes.Hidden) != 0;
		}
		
		public static List<string> GetAllFiles (string directory, string mask)
		{
			List<string> result = new List<string> ();
			string[] files = Directory.GetFiles (directory, mask, SearchOption.AllDirectories);
			string skipPath = "";
			foreach (string path in files) {
				string dir = System.IO.Path.GetDirectoryName (path);
				if (IsPathHidden (dir)) {
					skipPath = dir;
					continue;
				}
				if (skipPath != "" && path.StartsWith (skipPath)) {
					continue;
				}
				if (!IsPathHidden (path)) {
					result.Add (path);
				}
			}
			return result;
		}

		public static List<string> GetAllDirectories (string directory, string mask)
		{
			List<string> result = new List<string> ();
			string[] directories = Directory.GetDirectories (directory, mask, SearchOption.AllDirectories);
			string skipPath = "";
			foreach (string path in directories) {
				if (IsPathHidden (path)) {
					skipPath = path;
					continue;
				}
				if (skipPath != "" && path.StartsWith (skipPath)) {
					continue;
				}
				result.Add (path);
			}
			return result;
		}

	}
}

