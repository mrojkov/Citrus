using System;
using System.Linq;
using System.IO;
using Lime;

namespace Orange
{
	class SyncUpdated
	{
		public delegate bool Converter(string srcPath, string dstPath);

		public static int GetOperationsCount(string fileExtension) => The.Workspace.AssetFiles.Enumerate(fileExtension).Count();

		public static void Sync(string fileExtension, string bundleAssetExtension, AssetBundle bundle, Converter converter, Func<string, string, bool> extraOutOfDateChecker = null)
		{
			foreach (var srcFileInfo in The.Workspace.AssetFiles.Enumerate(fileExtension)) {
				UserInterface.Instance.IncreaseProgressBar();
				var srcPath = srcFileInfo.Path;
				var dstPath = Path.ChangeExtension(srcPath, bundleAssetExtension);
				var bundled = bundle.FileExists(dstPath);
				var srcRules = AssetCooker.CookingRulesMap[srcPath];
				var needUpdate = !bundled || srcFileInfo.LastWriteTime != bundle.GetFileLastWriteTime(dstPath);
				needUpdate = needUpdate || !srcRules.SHA1.SequenceEqual(bundle.GetCookingRulesSHA1(dstPath));
				needUpdate = needUpdate || (extraOutOfDateChecker?.Invoke(srcPath, dstPath) ?? false);
				if (needUpdate) {
					if (converter != null) {
						try {
							if (converter(srcPath, dstPath)) {
								Console.WriteLine((bundled ? "* " : "+ ") + dstPath);
								CookingRules rules = null;
								if (!string.IsNullOrEmpty(dstPath)) {
									AssetCooker.CookingRulesMap.TryGetValue(dstPath, out rules);
								}
								PluginLoader.AfterAssetUpdated(bundle, rules, dstPath);
							}
						}
						catch (System.Exception e) {
							Console.WriteLine(
								"An exception was caught while processing '{0}': {1}\n", srcPath, e.Message);
							throw;
						}
					}
					else {
						Console.WriteLine((bundled ? "* " : "+ ") + dstPath);
						using (Stream stream = new FileStream(srcPath, FileMode.Open, FileAccess.Read)) {
							bundle.ImportFile(dstPath, stream, 0, fileExtension, File.GetLastWriteTime(srcPath), AssetAttributes.None, AssetCooker.CookingRulesMap[srcPath].SHA1);
						}
					}
				}
			}
		}
	}
}
