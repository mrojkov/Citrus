using System;
using System.Diagnostics;
using System.IO;

namespace Launcher
{
	public class AssembliesInstaller
	{
		public void Start (string [] assemblies, string executablePath)
		{
#if WIN
			var applicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
#elif MAC
			var applicationBase = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "../../../");
#endif
			foreach (var assembly in assemblies) {
				Console.WriteLine($"Installing assembly \"{assembly}\"..");

				var sourceFile = new FileInfo(assembly);
				var destinationPath = Path.Combine(applicationBase, sourceFile.Name);
				var destinationFile = new FileInfo(destinationPath);

				destinationFile.Delete();
				sourceFile.CopyTo(destinationPath);

				Console.WriteLine($"Installed!");
			}

			var process = new Process {
				StartInfo = {
				FileName = executablePath
			}
			};
			process.Start();
		}
	}
}
