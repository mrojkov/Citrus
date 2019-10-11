using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace Tangerine.UI.FilesDropHandler
{
	public class ScenesDropHandler : IFilesDropHandler
	{
		public List<string> Extensions { get; } = new List<string> { ".scene", ".tan", ".model" };
		public FilesDropManager Manager { get; set; }
		public bool TryHandle(IEnumerable<string> files)
		{
			foreach (var file in files) {
				if (!Utils.ExtractAssetPathOrShowAlert(file, out var assetPath, out var assetType) ||
					!Utils.AssertCurrentDocument(assetPath, assetType)) {
					continue;
				}
				DropSceneContextMenu.Create(assetPath, assetType, Manager.OnNodeCreated);
			}
			return true;
		}
	}
}
