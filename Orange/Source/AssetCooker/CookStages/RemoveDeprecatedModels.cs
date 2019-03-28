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

		protected override void SetExtensions()
		{
			Extensions = new string[] { ".model" };
			ImportedExtension = null;
			ExportedExtension = null;
		}

		public override void Action()
		{
			foreach (var fileInfo in The.Workspace.AssetFiles.Enumerate(Extensions[0])) {
				var path = fileInfo.Path;
				if (AssetCooker.CookingRulesMap.ContainsKey(path)) {
					AssetCooker.CookingRulesMap.Remove(path);
				}
				Logger.Write($"Removing deprecated .model file: {path}");
				File.Delete(path);
			}
		}
	}
}
