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
	public class ImagesDropHandler
	{
		private static readonly Type[] imageTypes = {
			typeof(Image), typeof(DistortionMesh), typeof(NineGrid),
			typeof(TiledImage), typeof(ParticleModifier),
		};

		private readonly Action onBeforeDrop;
		private readonly Action<Node> postProcessNode;

		public ImagesDropHandler(Action onBeforeDrop = null, Action<Node> postProcessNode = null)
		{
			this.onBeforeDrop = onBeforeDrop;
			this.postProcessNode = postProcessNode;
		}

		public void Handle(List<string> files)
		{
			var supportedFiles = files.Where(f => Path.GetExtension(f) == ".png" ).ToList();
			if (supportedFiles.Any()) {
				supportedFiles.ForEach(f => files.Remove(f));
				CreateContextMenu(supportedFiles);
			}
		}

		private void CreateContextMenu(List<string> files)
		{
			var menu = new Menu();
			foreach (var imageType in imageTypes) {
				if (NodeCompositionValidator.Validate(Document.Current.Container.GetType(), imageType)) {
					menu.Add(new Command($"Create {imageType.Name}",
						() => CreateImageTypeInstance(imageType, files)));
				}
			}
			menu.Popup();
		}

		private void CreateImageTypeInstance(Type type, IEnumerable<string> files)
		{
			onBeforeDrop?.Invoke();
			using (Document.Current.History.BeginTransaction()) {
				foreach (var file in files) {
					if (!Utils.ExtractAssetPathOrShowAlert(file, out var assetPath, out var assetType)) {
						continue;
					}
					var node = CreateNode.Perform(type);
					var texture = new SerializableTexture(assetPath);
					var nodeSize = (Vector2)texture.ImageSize;
					var nodeId = Path.GetFileNameWithoutExtension(assetPath);
					if (node is Widget) {
						SetProperty.Perform(node, nameof(Widget.Texture), texture);
						SetProperty.Perform(node, nameof(Widget.Pivot), Vector2.Half);
						SetProperty.Perform(node, nameof(Widget.Size), nodeSize);
						SetProperty.Perform(node, nameof(Widget.Id), nodeId);
					} else if (node is ParticleModifier) {
						SetProperty.Perform(node, nameof(ParticleModifier.Texture), texture);
						SetProperty.Perform(node, nameof(ParticleModifier.Size), nodeSize);
						SetProperty.Perform(node, nameof(ParticleModifier.Id), nodeId);
					}
					postProcessNode?.Invoke(node);
				}
				Document.Current.History.CommitTransaction();
			}
		}
	}
}
