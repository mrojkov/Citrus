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
			Console.WriteLine("Synchronized project: {0}", projectFileName);
		}

		private static void IncludeNewItems(XmlDocument doc)
		{
			var itemGroups = doc["Project"].EnumerateElements("ItemGroup").ToArray();
			// It is assumed that the second <ItemGroup> tag contains compile items
			var compileItems = itemGroups[1];
			foreach (var file in new FileEnumerator(".").Enumerate(".cs")) {
				var path = file.Path.Replace('/', '\\');
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
			bool done = false;
			while (!done) {
				done = true;
				foreach (var item in group.EnumerateElements("Compile")) {
					var path = item.Attributes["Include"].Value;
					if (!File.Exists(path)) {
						group.RemoveChild(item);
						Console.WriteLine("Removed a missing file: " + path);
						done = false;
					}
				}
			}
		}

		private static IEnumerable<XmlNode> EnumerateElements(this XmlNode parent, string tag)
		{
			foreach (var item in parent.Cast<XmlNode>()) {
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
			return true;
		}
	}
}
