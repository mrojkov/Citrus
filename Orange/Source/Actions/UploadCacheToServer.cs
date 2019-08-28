using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace Orange
{
	public static class UploadCacheToServer
	{
		public static void UploadCacheToServerAction()
		{
			AssetCache.Instance.Initialize();
			if (AssetCache.Instance.Mode != (AssetCacheMode.Local | AssetCacheMode.Remote)) {
				Console.WriteLine("Both LOCAL and REMOTE cache should be active. Execution aborted");
				return;
			}
			if (!The.UI.AskConfirmation(
				$"You are going to upload all cache from {The.Workspace.AssetCacheLocalPath} to remote server." +
				" It will overwrite all existing files with same names on it. Are you sure?")) {
				return;
			}
			if (!Directory.Exists(The.Workspace.AssetCacheLocalPath)) {
				Console.WriteLine($"{The.Workspace.AssetCacheLocalPath} does not exist. Execution aborted");
			}
			var filePaths = Directory.GetFiles(The.Workspace.AssetCacheLocalPath, "*", SearchOption.AllDirectories);
			var hashStrings = filePaths.Select(path => Path.GetFileName(path)).ToList();
			// Since we are downloading cache to temporary file, we should make sure that it won't be uploaded to server
			hashStrings.Remove(AssetCache.TempFileName);
			The.UI.SetupProgressBar(hashStrings.Count);
			foreach (var hashString in hashStrings) {
				if (!AssetCache.Instance.UploadFromLocal(hashString)) {
					Console.WriteLine("Execution aborted");
					return;
				}
				Console.WriteLine($"+{hashString}");
				The.UI.IncreaseProgressBar();
			}
			The.UI.StopProgressBar();
		}
	}
}
