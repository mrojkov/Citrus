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
	/// <summary>
	/// Handles scenes drop.
	/// </summary>
	public class ScenesDropHandler
	{
		private readonly string[] extensions = { ".tan", ".model" };
		private readonly Action onBeforeDrop;
		private readonly Action<Node> postProcessNode;

		public bool ShouldCreateContextMenu { get; set; } = true;

		/// <summary>
		/// Constructs ScenesDropHandler.
		/// </summary>
		/// <param name="onBeforeDrop">Called before dropped files processing.</param>
		/// <param name="postProcessNode">Called after node creation.</param>
		public ScenesDropHandler(Action onBeforeDrop = null, Action<Node> postProcessNode = null)
		{
			this.onBeforeDrop = onBeforeDrop;
			this.postProcessNode = postProcessNode;
		}

		/// <summary>
		/// Handles files drop.
		/// </summary>
		/// <param name="files">Dropped files.</param>
		public void Handle(List<string> files)
		{
			onBeforeDrop?.Invoke();
			var assets = new List<(string assetPath, string assetType)>();
			foreach (var file in files.Where(f => extensions.Contains(Path.GetExtension(f))).ToList()) {
				if (
					Utils.ExtractAssetPathOrShowAlert(file, out var assetPath, out var assetType) &&
					Utils.AssertCurrentDocument(assetPath, assetType)
				) {
					files.Remove(file);
					assets.Add((assetPath, assetType));
				}
			}
			if (assets.Count > 0) {
				if (ShouldCreateContextMenu) {
					CreateContextMenu(assets);
				} else {
					foreach (var (path, _) in assets) {
						try {
							Project.Current.OpenDocument(path);
						}
						catch (InvalidOperationException e) {
							AlertDialog.Show(e.Message);
						}
					}
				}
			}
		}

		public void CreateContextMenu(List<(string assetPath, string assetType)> assets)
		{
			var menu = new Menu {
				new Command("Open in New Tab", () => assets.ForEach(asset => Project.Current.OpenDocument(asset.assetPath))),
				new Command("Add As External Scene", () => Document.Current.History.DoTransaction(() => {
					var nodes = new List<Node>(assets.Count);
					foreach (var (assetPath, _) in assets) {
						var scene = Node.CreateFromAssetBundle(assetPath, persistence: TangerinePersistence.Instance);
						var node = CreateNode.Perform(scene.GetType());
						SetProperty.Perform(node, nameof(Widget.ContentsPath), assetPath);
						if (node is IPropertyLocker propertyLocker) {
							var id = propertyLocker.IsPropertyLocked("Id", true) ? Path.GetFileName(assetPath) : scene.Id;
							SetProperty.Perform(node, nameof(Node.Id), id);
						}
						if (scene is Widget widget) {
							SetProperty.Perform(node, nameof(Widget.Pivot), Vector2.Half);
							SetProperty.Perform(node, nameof(Widget.Size), widget.Size);
						}
						postProcessNode?.Invoke(node);
						node.LoadExternalScenes();
						nodes.Add(node);
					}
					nodes.ForEach(n => SelectNode.Perform(n));
				})),
				new Command("Cancel")
			};
			menu[0].Enabled = assets.All(asset => asset.assetType != ".model");
			menu.Popup();
		}
	}
}
