using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

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
			var doc = new XmlDocument();
			doc.Load(projectFileName);
			using (new DirectoryChanger(Path.GetDirectoryName(projectFileName))) {
				ExcludeMissingItems(doc);
				IncludeNewItems(doc);
			}
			doc.Save(projectFileName);
			Console.WriteLine(string.Format("Synchronized project: {0}", projectFileName));
		}

		private static void IncludeNewItems(XmlDocument doc)
		{
			var itemGroups = doc["Project"].EnumerateElements("ItemGroup").ToArray();
			// It is assumed that the second <ItemGroup> tag contains compile items
			var compileItems = itemGroups[1];
			foreach (var file in new FileEnumerator(".").Enumerate(".cs")) {
				var path = ToWindowsSlashes(file.Path);
				if (!HasCompileItem(doc, path)) {
					if (IsItemShouldBeAdded(path)) {
						var item = doc.CreateElement("Compile", doc["Project"].NamespaceURI);
						var include = item.Attributes.Append(doc.CreateAttribute("Include"));
						include.Value = path;
						compileItems.AppendChild(item);
						Console.WriteLine("Added a new file: " + file.Path);
					}
				}
			}
		}

		private static bool HasCompileItem(XmlDocument doc, string path)
		{
			var itemGroups = doc["Project"].EnumerateElements("ItemGroup");
			foreach (var group in itemGroups) {
				foreach (var item in group.EnumerateElements("Compile")) {
					var itemPath = item.Attributes["Include"].Value;
					if (itemPath.ToLower() == path.ToLower()) {
						return true;
					}
				}
			}
			return false;
		}

		private static void ExcludeMissingItems(XmlDocument doc)
		{
			var itemGroups = doc["Project"].EnumerateElements("ItemGroup");
			foreach (var group in itemGroups) {
				ExcludeMissingItemsFromGroup(group);
			}
		}

		private static void ExcludeMissingItemsFromGroup(XmlNode group)
		{
			foreach (var item in group.EnumerateElements("Compile")) {
				var path = item.Attributes["Include"].Value;
				if (!File.Exists(ToUnixSlashes(path))) {
					group.RemoveChild(item);
					Console.WriteLine("Removed a missing file: " + path);
				}
			}
		}

		static string ToWindowsSlashes(string path)
		{
			return path.Replace('/', '\\');
		}

		static string ToUnixSlashes(string path)
		{
			return path.Replace('\\', '/');
		}

		private static IEnumerable<XmlNode> EnumerateElements(this XmlNode parent, string tag)
		{
			var items = parent.Cast<XmlNode>().ToArray();
			foreach (var item in items) {
				if (item.Name == tag) {
					yield return item;
				}
			}
		}

		private static bool IsItemShouldBeAdded(string filePath)
		{
			if (filePath.StartsWith("packages")) {
				return false;
			}
			// Ignore .cs files related to sub-projects 
			var dir = ToUnixSlashes(filePath);
			while (true) {
				dir = Path.GetDirectoryName(dir);
				if (string.IsNullOrEmpty(dir)) {
					break;
				}
				if (Directory.EnumerateFiles(dir, "*.csproj").Count() > 0) {
					return false;
				}
			}
			return true;
		}
	}
}
