using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Kumquat;

namespace Orange
{
	using LocDict = Dictionary<string, Lime.Frame>;

	public class KumquatCooker
	{
		private Lime.AssetsBundle assetsBundle = Lime.AssetsBundle.Instance;
		private CitrusProject project;
		private Orange.TargetPlatform platform;

		public KumquatCooker(CitrusProject project, Orange.TargetPlatform platform)
		{
			this.project = project;
			this.platform = platform;
		}

		public void Cook()
		{
			if (Lime.World.Instance == null) {
				new Lime.World();
			}
			string bundlePath = Path.ChangeExtension(project.AssetsDirectory, Helpers.GetTargetPlatformString(platform));
			assetsBundle.Open(bundlePath, Lime.AssetBundleFlags.Writable);
			try {
				CookLocations();
			} finally {
				assetsBundle.Close();
			}
		}

		private void CookLocations()
		{
			LocDict locations = new LocDict();
			var locationPaths = GetLocationPaths();
			foreach(var path in locationPaths)
				locations.Add(path, new Lime.Frame(path));
			if (locations.Count > 0) {
				Lime.Serialization.WriteObjectToBundle<Router>(assetsBundle, "Router", new Router(locations));
				Lime.Serialization.WriteObjectToBundle<Warehouse>(assetsBundle, "Warehouse", new Warehouse(locations));
				new CodeGenerator(project.ProjectDirectory, project.Title, locations).Start();
			}

		}
		
		private List<string> GetLocationPaths()
		{
			var cookingRulesMap = CookingRulesBuilder.Build(project.AssetFiles);
			List<string> list = new List<string>();
			foreach (var srcFileInfo in project.AssetFiles.Enumerate(".scene")) {
				CookingRules rules = cookingRulesMap[srcFileInfo.Path];
				if (rules.KumquatLocation)
					list.Add(srcFileInfo.Path);
			}
			return list;
		}

	}
}
