using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Operations;

namespace Tangerine.UI.FilesDropHandler
{
	public class ScenesDropHandler : IFilesDropHandler
	{
		public List<string> Extensions { get; } = new List<string> { ".scene", ".tan", ".model" };

		public void Handle(IEnumerable<string> files, IFilesDropCallbacks callbacks, out IEnumerable<string> handledFiles)
		{
			handledFiles = files.Where(f => Extensions.Contains(Path.GetExtension(f)));
			foreach (var file in handledFiles) {
				if (!Utils.ExtractAssetPathOrShowAlert(file, out var assetPath, out var assetType) ||
					!Utils.AssertCurrentDocument(assetPath, assetType)) {
					continue;
				}
				CreateContextMenu(assetPath, assetType, callbacks);
			}
		}

		public static void CreateContextMenu(string assetPath, string assetType, IFilesDropCallbacks callbacks)
		{
			var fileName = Path.GetFileNameWithoutExtension(assetPath);
			var menu = new Menu {
				new Command("Open in New Tab", () => Project.Current.OpenDocument(assetPath)),
				new Command("Add As External Scene", () => Document.Current.History.DoTransaction(() => {
					var args = new FilesDropManager.NodeCreatingEventArgs(assetPath, assetType);
					callbacks.NodeCreating?.Invoke(args);
					if (args.Cancel) {
						return;
					}
					var scene = Node.CreateFromAssetBundle(assetPath, yuzu: TangerineYuzu.Instance.Value);
					var node = CreateNode.Perform(scene.GetType());
					SetProperty.Perform(node, nameof(Widget.ContentsPath), assetPath);
					if (node is IPropertyLocker propertyLocker) {
						string id = propertyLocker.IsPropertyLocked("Id", true) ? fileName : scene.Id;
						SetProperty.Perform(node, nameof(Node.Id), id);
					}
					if (scene is Widget widget) {
						SetProperty.Perform(node, nameof(Widget.Pivot), Vector2.Half);
						SetProperty.Perform(node, nameof(Widget.Size), widget.Size);
					}
					callbacks.NodeCreated?.Invoke(node);
					node.LoadExternalScenes();
					})),
				new Command("Cancel")
			};
			menu[0].Enabled = assetType != ".model";
			menu.Popup();
		}
	}
}
