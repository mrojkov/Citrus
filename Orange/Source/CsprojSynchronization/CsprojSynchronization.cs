using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Build.Evaluation;

namespace Orange
{
	public static class CsprojSynchronization
	{
		public static void SynchronizeAll()
		{
			var gameProj = The.Workspace.GetGameCsprojFilePath();
			SynchronizeProject(gameProj);

			var mainProj = The.Workspace.GetMainCsprojFilePath();
			SynchronizeProject(mainProj);
		}

		public static void SynchronizeProject(string projectFileName)
		{
			var projectCollection = new ProjectCollection();
			var project = projectCollection.LoadProject(projectFileName);
			using (new DirectoryChanger(Path.GetDirectoryName(projectFileName))) {
				ExcludeMissingItems(project);
				IncludeNewItems(project);
			}
			project.Save();
			Console.WriteLine("Synchronized project: {0}", projectFileName);
		}

		private static void IncludeNewItems(Project project)
		{
			var items = project.GetItems("Compile");
			foreach (var file in new FileEnumerator(".").Enumerate(".cs")) {
				var path = file.Path.Replace('/', '\\');
				if (items.Count(item => item.EvaluatedInclude.ToLower() == path.ToLower()) == 0) {
					if (IsItemShouldBeAdded(path)) {
						project.AddItem("Compile", path);
						Console.WriteLine("Added a new file: " + file.Path);
					}
				}
			}
		}

		private static void ExcludeMissingItems(Project project)
		{
			var items = project.GetItems("Compile");
			foreach (var item in items.ToArray()) {
				if (!File.Exists(item.EvaluatedInclude)) {
					project.RemoveItem(item);
					Console.WriteLine("Removed a missing file: " + item.EvaluatedInclude);
				}
			}
		}

		private static bool IsItemShouldBeAdded(string filePath)
		{
			if (filePath.StartsWith("packages")) {
				return false;
			}
			return true;
		}
	}
}
