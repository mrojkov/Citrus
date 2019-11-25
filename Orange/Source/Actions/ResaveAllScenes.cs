using System.ComponentModel.Composition;
using System;
using Lime;
using System.IO;

namespace Orange
{
	public static partial class Actions
	{
		[Export(nameof(Orange.OrangePlugin.MenuItems))]
		[ExportMetadata(nameof(IMenuItemMetadata.Label), "Resave All Scenes")]
		public static void ResaveAllScenes()
		{
			var previousFileEnumerator = The.Workspace.AssetFiles;
			AssetBundle.Current = new UnpackedAssetBundle(The.Workspace.AssetsDirectory);
			The.Workspace.AssetFiles = new Orange.FileEnumerator(The.Workspace.AssetsDirectory);
			foreach (var file in The.Workspace.AssetFiles.Enumerate()) {
				var filename = Path.GetFileName(file.Path);
				if (!filename.EndsWith("tan", StringComparison.OrdinalIgnoreCase)) {
					continue;
				}
				var node = Node.CreateFromAssetBundle(
					path: Path.ChangeExtension(file.Path, null),
					ignoreExternals: true
				);
				InternalPersistence.Instance.WriteObjectToBundle(
					bundle: AssetBundle.Current,
					path: file.Path,
					instance: node,
					format: Persistence.Format.Json,
					sourceExtension: "tan",
					time: File.GetLastWriteTime(file.Path),
					attributes: AssetAttributes.None,
					cookingRulesSHA1: null
				);
			}
			The.Workspace.AssetFiles = previousFileEnumerator;
		}
	}
}
