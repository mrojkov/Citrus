using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Operations;

namespace Tangerine.UI
{
	public class FilesDropHandler
	{
		public class NodeCreatingEventArgs : CancelEventArgs
		{
			public readonly string AssetPath;
			public readonly string AssetType;

			public NodeCreatingEventArgs(string assetPath, string assetType)
			{
				AssetPath = assetPath;
				AssetType = assetType;
			}
		}

		private class ImageDropCommands
		{
			private readonly ICommand asImage = new Command("Create Image");
			private readonly ICommand asDistortionMesh = new Command("Create Distortion Mesh");
			private readonly ICommand asNineGrid = new Command("Create Nine Grid");
			private readonly ICommand asParticleModifier = new Command("Create Particle Modifier");

			public readonly Dictionary<ICommand, Type> Commands;

			public ImageDropCommands()
			{
				Commands = new Dictionary<ICommand, Type> {
					{ asImage, typeof(Image) },
					{ asDistortionMesh, typeof(DistortionMesh) },
					{ asNineGrid, typeof(NineGrid) },
					{ asParticleModifier, typeof(ParticleModifier) },
				};
			}
		}

		private readonly Widget widget;
		private readonly ImageDropCommands imageDropCommands;
		private List<string> pendingImages;

		public event Action Handling;
		public event Action<NodeCreatingEventArgs> NodeCreating;
		public event Action<Node> NodeCreated;

		public bool Enabled { get; set; }

		public FilesDropHandler(Widget widget)
		{
			this.widget = widget;
			imageDropCommands = new ImageDropCommands();
		}

		public bool TryToHandle(IEnumerable<string> files)
		{
			var nodeUnderMouse = WidgetContext.Current.NodeUnderMouse;
			if (nodeUnderMouse == null || !nodeUnderMouse.SameOrDescendantOf(widget)) {
				return false;
			}
			Handle(files);
			return true;
		}

		private void Handle(IEnumerable<string> files)
		{
			Handling?.Invoke();
			using (Document.Current.History.BeginTransaction()) {
				pendingImages = new List<string>();
				foreach (var file in files) {
					try {
						string assetPath, assetType;
						if (!Utils.ExtractAssetPathOrShowAlert(file, out assetPath, out assetType) ||
							!Utils.AssertCurrentDocument(assetPath, assetType.Substring(1))) {
							continue;
						}

						var nodeCreatingEventArgs = new NodeCreatingEventArgs(assetPath, assetType);
						NodeCreating?.Invoke(nodeCreatingEventArgs);
						if (nodeCreatingEventArgs.Cancel) {
							continue;
						}

						var fileName = Path.GetFileNameWithoutExtension(assetPath);
						switch (assetType) {
							case ".png":
								pendingImages.Add(assetPath);
								break;
							case ".ogg": {
								var node = CreateNode.Perform(typeof(Audio));
								var sample = new SerializableSample(assetPath);
								SetProperty.Perform(node, nameof(Audio.Sample), sample);
								SetProperty.Perform(node, nameof(Node.Id), fileName);
								SetProperty.Perform(node, nameof(Audio.Volume), 1);
								var key = new Keyframe<AudioAction> {
									Frame = Document.Current.AnimationFrame,
									Value = AudioAction.Play
								};
								SetKeyframe.Perform(node, nameof(Audio.Action), Document.Current.AnimationId, key);
								OnNodeCreated(node);
								break;
							}
							case ".tan":
							case ".model":
							case ".scene": {
								var scene = Node.CreateFromAssetBundle(assetPath);
								var node = CreateNode.Perform(scene.GetType());
								SetProperty.Perform(node, nameof(Widget.ContentsPath), assetPath);
								SetProperty.Perform(node, nameof(Node.Id), fileName);
								if (scene is Widget) {
									SetProperty.Perform(node, nameof(Widget.Pivot), Vector2.Half);
									SetProperty.Perform(node, nameof(Widget.Size), widget.Size);
								}
								OnNodeCreated(node);
								node.LoadExternalScenes();
								break;
							}
						}
					} catch (System.Exception e) {
						AlertDialog.Show(e.Message);
					}
				}

				if (pendingImages.Count > 0) {
					var menu = new Menu();
					foreach (var kv in imageDropCommands.Commands) {
						if (NodeCompositionValidator.Validate(Document.Current.Container.GetType(), kv.Value)) {
							menu.Add(kv.Key);
						}
					}
					menu.Popup();
				}
				Document.Current.History.CommitTransaction();	
			}
		}

		public void HandleDropImage()
		{
			foreach (var kv in imageDropCommands.Commands) {
				if (pendingImages != null && kv.Key.WasIssued()) {
					kv.Key.Consume();
					using (Document.Current.History.BeginTransaction()) {
						foreach (var assetPath in pendingImages) {
							var node = CreateNode.Perform(kv.Value);
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
							OnNodeCreated(node);
						}
						Document.Current.History.CommitTransaction();
						pendingImages = null;
					}
				} else {
					kv.Key.Consume();
				}
			}
		}

		public void OnNodeCreated(Node node)
		{
			NodeCreated?.Invoke(node);
		}
	}
}
