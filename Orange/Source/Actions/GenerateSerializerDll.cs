using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orange
{
	static partial class Actions
	{
		[MenuItem("Generate Serializer.dll")]
		public static void GenerateSerializerDllAction()
		{
			The.MainWindow.Execute(() => {
				GenerateSerializerDll();
			});
		}

		public static bool GenerateSerializerDll()
		{
			AssetCooker.BuildForActivePlatform();
			if (!BuildGame(Orange.TargetPlatform.Desktop)) {
				return false;
			}
			var builder = new SolutionBuilder(TargetPlatform.Desktop);
			int exitCode = builder.Run("--GenerateSerializationAssembly");
			if (exitCode != 0) {
				Console.WriteLine("Application terminated with exit code {0}", exitCode);
				return false;
			}
			string app = builder.GetApplicationPath();
			string dir = System.IO.Path.GetDirectoryName(app);
			string assembly = System.IO.Path.Combine(dir, "Serializer.dll");
			if (!System.IO.File.Exists(assembly)) {
				Console.WriteLine("{0} doesn't exist", assembly);
				Console.WriteLine(@"Ensure your Application.cs contains following code:
	public static void Main(string[] args)
	{
		if (Array.IndexOf(args, ""--GenerateSerializationAssembly"") >= 0) {
			Lime.Environment.GenerateSerializationAssembly(""Serializer"");
			return;
		}");
				return false;
			}
			var destination = System.IO.Path.Combine(The.Workspace.ProjectDirectory, "Serializer.dll");
			if (System.IO.File.Exists(destination)) {
				System.IO.File.Delete(destination);
			}
			System.IO.File.Move(assembly, destination);
			Console.Write("Serialization assembly saved to '{0}'\n", destination);
			return true;
		}
	}
}
