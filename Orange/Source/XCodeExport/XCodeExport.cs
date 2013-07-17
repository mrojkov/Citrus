using System;
using System.IO;

namespace Orange
{
	public static class XCodeExport
	{
		[MenuItem("Update XCode Project")]
		public static void XCodeExportAction()
		{
			AssetCooker.CookForActivePlatform();
			if (Actions.BuildGame()) {
				The.MainWindow.ScrollLogToEnd();
				var buffer = The.MainWindow.OutputPane.Buffer;
				string allText = buffer.StartIter.GetText(buffer.EndIter);
				foreach (var line in allText.Split('\n')) {
					if (line.Contains("/bin/mtouch")) {
						var mtouch = line;
						GenerateUnsignedBinary(mtouch);
						var appPath = GetGeneratedAppPath(mtouch);
						var dstPath = GetBigFishSVNDataPath();
						CopyContent(appPath, dstPath);
					}
				}
			}
		}
		
		private static void CopyContent(string appPath, string dstPath)
		{
			CopyFiles(appPath, dstPath, "*.dll");
			CopyFiles(appPath, dstPath, "*.exe");
			CopyFiles(appPath, dstPath, "*.png");
			CopyFiles(appPath, dstPath, "Data.iOS");
			CopyFiles(appPath, dstPath, "Levels.dat");
		}

		static void CopyFiles(string source, string destination, string searchPattern)
		{
			var files = new DirectoryInfo(source).GetFiles(searchPattern);
			foreach (var file in files) {
				var destFile = Path.Combine(destination, file.Name);
				Console.WriteLine("Writing " + destFile); 
				file.CopyTo(destFile, overwrite: true);
			}
		}


		private static string GetBigFishSVNDataPath()
		{
			return Path.Combine(GetBigFishSVNPath(), "ZZZ");
		}

		private static string GetBigFishSVNPath()
		{
			var p = Path.GetDirectoryName(The.Workspace.ProjectFile);
			p = Path.Combine(p, "BigFishSVN");
			return p;
		}
		
		private static string GetGeneratedAppPath(string mtouch)
		{
			string[] args = mtouch.Trim().Split(' ');
			var dev = Array.IndexOf(args, "-dev");
			var x = args[dev + 1];
			x = x.Substring(1, x.Length - 2);
			return x;
		}

		static void GenerateUnsignedBinary(string mtouch)
		{
			Console.WriteLine("======================================");
			Console.WriteLine("Generating unsigned application bundle");
			Console.WriteLine("======================================");
			mtouch = mtouch.TrimStart();
			var x = mtouch.IndexOf(' ');
			var app = mtouch.Substring(0, x);
			var args = mtouch.Substring(x + 1);
			Toolbox.StartProcess(app, args);
		}
	}
}

