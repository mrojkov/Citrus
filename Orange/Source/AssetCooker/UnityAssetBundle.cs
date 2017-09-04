using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Orange
{
	public class UnityAssetBundle : Lime.UnpackedAssetBundle
	{
		private bool needToUpdateFilesTXT = false;

		public UnityAssetBundle(string baseDirectory)
			: base(baseDirectory)
		{
		}

		private string AdjustExtensionForBinaryAsset(string path)
		{
			var ext = Path.GetExtension(path);
			if (ext != ".png" && ext != ".txt" && ext != ".ogg" && ext != ".shader" && ext != ".ogv") {
				path += ".bytes";
			}
			return path;
		}

		public override Stream OpenFile(string path)
		{
			return base.OpenFile(AdjustExtensionForBinaryAsset(path));
		}

		public override DateTime GetFileLastWriteTime(string path)
		{
			return base.GetFileLastWriteTime(AdjustExtensionForBinaryAsset(path));
		}

		public override void DeleteFile(string path)
		{
			base.DeleteFile(AdjustExtensionForBinaryAsset(path));
			needToUpdateFilesTXT = true;
		}

		public override bool FileExists(string path)
		{
			return base.FileExists(AdjustExtensionForBinaryAsset(path));
		}

		public override void ImportFile(string path, Stream stream, int reserve, string sourceExtension, Lime.AssetAttributes attributes, byte[] cookingRulesSHA1)
		{
			base.ImportFile(AdjustExtensionForBinaryAsset(path), stream, reserve, sourceExtension, attributes, cookingRulesSHA1);
			needToUpdateFilesTXT = true;
		}

		public override IEnumerable<string> EnumerateFiles(string path = null)
		{
			if (path != null) {
				throw new NotImplementedException();
			}
			foreach (var i in base.EnumerateFiles()) {
				string ext = Path.GetExtension(i);
				if (ext == ".meta") {
					continue;
				}
				if (i == "Files.txt") {
					continue;
				}
				if (ext == ".bytes") {
					yield return Path.ChangeExtension(i, null);
				} else {
					yield return i;
				}
			}
		}

		public override void Dispose()
		{
			SaveFileList();
			base.Dispose();
		}

		private void SaveFileList()
		{
			if (needToUpdateFilesTXT) {
				File.WriteAllLines(BaseDirectory + "/Files.txt", EnumerateFiles());
			}
		}
	}
}
