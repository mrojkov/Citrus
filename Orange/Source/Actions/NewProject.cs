using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using Lime;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Orange
{
	public static class NewProject
	{
		[Export(nameof(OrangePlugin.MenuItems))]
		[ExportMetadata("Label", "New Project")]
		[ExportMetadata("Priority", 3)]
		public static void NewProjectAction()
		{
			Application.InvokeOnMainThread(() => {
				var citrusPath = Toolbox.CalcCitrusDirectory();
				var dlg = new FileDialog {
					InitialDirectory = citrusPath,
					AllowedFileTypes = new string[] { "" },
					Mode = FileDialogMode.SelectFolder
				};
				if (dlg.RunModal()) {
					var projectName = Path.GetFileName(dlg.FileName);
					var newProjectApplicationName = String.Join(" ", Regex.Split(projectName, @"(?<!^)(?=[A-Z])"));
					Console.WriteLine($"New project name is \"{projectName}\"");
					HashSet<string> textfileExtensions = new HashSet<string> { ".cs", ".xml", ".csproj", ".sln", ".citproj", ".projitems", ".shproj", ".txt", ".plist", ".strings" };
					using (var dc = new DirectoryChanger($"{citrusPath}/Samples/EmptyProject/")) {
						var fe = new FileEnumerator(".");
						foreach (var f in fe.Enumerate()) {
							var targetPath = Path.Combine(dlg.FileName, f.Path.Replace("EmptyProject", projectName));
							Console.WriteLine($"Copying: {f.Path} => {targetPath}");
							Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
							System.IO.File.Copy(f.Path, targetPath);
							if (textfileExtensions.Contains(Path.GetExtension(targetPath).ToLower(CultureInfo.InvariantCulture))) {
								string text = File.ReadAllText(targetPath);
								text = text.Replace("EmptyProject", projectName);
								text = text.Replace("Empty Project", newProjectApplicationName);
								text = text.Replace("emptyproject", projectName.ToLower());
								var citrusUri = new Uri(citrusPath);
								var fileUri = new Uri(targetPath);
								var relativeUri = fileUri.MakeRelativeUri(citrusUri);
								text = text.Replace("..\\..\\..", relativeUri.ToString());
								File.WriteAllText(targetPath, text);
							}
						}
					}
				}
			});
		}
	}
}
