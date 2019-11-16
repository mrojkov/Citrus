using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Operations;
using Tangerine.UI;

namespace Tangerine.Common.FilesDropHandlers
{
	public class ScenesDropHandler
	{
		private readonly string[] extensions = { ".tan", ".model" };
		private readonly Action onBeforeDrop;
		private readonly Action<Node> nodePostprocessor;

		public bool ShouldCreateContextMenu { get; set; } = true;

		public ScenesDropHandler(Action onBeforeDrop = null, Action<Node> nodePostprocessor = null)
		{
			this.onBeforeDrop = onBeforeDrop;
			this.nodePostprocessor = nodePostprocessor;
		}

		public void Handle(List<string> files)
		{
			onBeforeDrop?.Invoke();
			foreach (var file in files.Where(f => extensions.Contains(Path.GetExtension(f))).ToList()) {
				files.Remove(file);
				if (
					!Utils.ExtractAssetPathOrShowAlert(file, out var assetPath, out var assetType) ||
					!Utils.AssertCurrentDocument(assetPath, assetType)
				) {
					continue;
				}
				if (ShouldCreateContextMenu) {
					CreateContextMenu(assetPath, assetType);
				} else {
					try {
						Project.Current.OpenDocument(file, true);
					} catch (InvalidOperationException e) {
						AlertDialog.Show(e.Message);
					}
				}
			}
		}

		public void CreateContextMenu(string assetPath, string assetType)
		{
			var fileName = Path.GetFileNameWithoutExtension(assetPath);
			var menu = new Menu {
				new Command("Open in New Tab", () => Project.Current.OpenDocument(assetPath)),
				new Command("Add As External Scene", () => Document.Current.History.DoTransaction(() => {
					var scene = Node.CreateFromAssetBundle(assetPath, yuzu: TangerineYuzu.Instance.Value);
					var node = CreateNode.Perform(scene.GetType());
					SetProperty.Perform(node, nameof(Widget.ContentsPath), assetPath);
					if (node is IPropertyLocker propertyLocker) {
						var id = propertyLocker.IsPropertyLocked("Id", true) ? fileName : scene.Id;
						SetProperty.Perform(node, nameof(Node.Id), id);
					}
					if (scene is Widget widget) {
						SetProperty.Perform(node, nameof(Widget.Pivot), Vector2.Half);
						SetProperty.Perform(node, nameof(Widget.Size), widget.Size);
					}
					nodePostprocessor?.Invoke(node);
					node.LoadExternalScenes();
				})),
				new Command("Cancel")
			};
			menu[0].Enabled = assetType != ".model";
			menu.Popup();
		}
	}
}
