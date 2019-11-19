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
	/// Handles images drop.
	/// </summary>
	public class ImagesDropHandler
	{
		private static readonly Type[] imageTypes = {
			typeof(Image), typeof(DistortionMesh), typeof(NineGrid),
			typeof(TiledImage), typeof(ParticleModifier),
		};

		private readonly Action onBeforeDrop;
		private readonly Action<Node> postProcessNode;

		/// <summary>
		/// Constructs ImagesDropHandler.
		/// </summary>
		/// <param name="onBeforeDrop">Called before dropped files processing.</param>
		/// <param name="postProcessNode">Called after node creation.</param>
		public ImagesDropHandler(Action onBeforeDrop = null, Action<Node> postProcessNode = null)
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
			menu.Add(new Command("Create sprite animated Image", () => CreateSpriteAnimatedImage(files)));
			menu.Popup();
		}

		private void CreateSpriteAnimatedImage(List<string> files)
		{
			onBeforeDrop?.Invoke();
			using (Document.Current.History.BeginTransaction()) {
				var node = CreateNode.Perform(typeof(Image));
				SetProperty.Perform(node, nameof(Widget.Pivot), Vector2.Half);
				SetProperty.Perform(node, nameof(Widget.Id), "Temp");
				postProcessNode?.Invoke(node);
				var i = 0;
				ITexture first = null;
				foreach (var file in files) {
					if (!Utils.ExtractAssetPathOrShowAlert(file, out var assetPath, out var assetType)) {
						continue;
					}
					var text = new SerializableTexture(assetPath);
					first = first ?? text;
					SetKeyframe.Perform(node, nameof(Widget.Texture), Document.Current.AnimationId,
						new Keyframe<ITexture> {
							Value = text,
							Frame = i++,
							Function = KeyFunction.Steep,
					});
				}
				SetProperty.Perform(node, nameof(Widget.Size), (Vector2)first.ImageSize);
				Document.Current.History.CommitTransaction();
			}

		}

		private void CreateImageTypeInstance(Type type, List<string> files)
		{
			onBeforeDrop?.Invoke();
			using (Document.Current.History.BeginTransaction()) {
				var nodes = new List<Node>(files.Count);
				foreach (var file in files) {
					if (!Utils.ExtractAssetPathOrShowAlert(file, out var assetPath, out var assetType)) {
						continue;
					}
					var node = CreateNode.Perform(type);
					nodes.Add(node);
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
				foreach (var node in nodes) {
					SelectNode.Perform(node);
				}
				Document.Current.History.CommitTransaction();
			}
		}
	}
}
