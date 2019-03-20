using System;
using System.IO;
using Lime;

namespace Orange
{
	class RemoveDeprecatedModels: CookStage
	{
		public RemoveDeprecatedModels() : base()
		{

		}

		public override void Action()
		{
			foreach (var fileInfo in The.Workspace.AssetFiles.Enumerate(".model")) {
				var path = fileInfo.Path;
				if (AssetCooker.CookingRulesMap.ContainsKey(path)) {
					AssetCooker.CookingRulesMap.Remove(path);
				}
				Logger.Write($"Removing deprecated .model file: {path}");
				File.Delete(path);
			}
		}

		protected override void SetExtensions()
		{
			Extensions = new string[] { ".model" };
		}
	}
}
