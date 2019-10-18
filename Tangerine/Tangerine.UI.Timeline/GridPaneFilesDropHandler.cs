using System;
using System.Collections.Generic;
using System.IO;
using Lime;
using Tangerine.Core;
using Tangerine.UI.FilesDropHandler;

namespace Tangerine.UI.Timeline
{
	internal class GridPaneFilesDropHandler : IFilesDropHandler
	{
		private GridPane grid => Timeline.Instance.Grid;
		public List<string> Extensions { get; } = new List<string>();
		public void Handle(IEnumerable<string> files, IFilesDropCallbacks callbacks, out IEnumerable<string> handledFiles)
		{
			var handled = new List<string>();
			var cellUnderMouseOnFilesDrop = grid.CellUnderMouse();
			var animateTextureCellOffset = 0;
			foreach (var file in files) {
				if (Document.Current.Animation.IsCompound) {
					try {
						// Dirty hack: using a file drag&drop mechanics for dropping animation clips on the grid.
						var decodedAnimationId = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(file));
						Operations.CompoundAnimations.AddAnimationClip.Perform(
							new IntVector2(
								cellUnderMouseOnFilesDrop.X + animateTextureCellOffset,
								cellUnderMouseOnFilesDrop.Y),
							decodedAnimationId);
					} catch { }
				}
				if (
					!Utils.ExtractAssetPathOrShowAlert(file, out var assetPath, out var assetType) ||
					!Utils.AssertCurrentDocument(assetPath, assetType)
				) {
					continue;
				}
				switch (assetType) {
					case ".png": {
						if (Document.Current.Rows.Count == 0) {
							continue;
						}
						var widget = Document.Current.Rows[cellUnderMouseOnFilesDrop.Y].Components.Get<Core.Components.NodeRow>()?.Node as Widget;
						if (widget == null) {
							continue;
						}
						var key = new Keyframe<ITexture> {
							Frame = cellUnderMouseOnFilesDrop.X + animateTextureCellOffset,
							Value = new SerializableTexture(assetPath)
						};
						Core.Operations.SetKeyframe.Perform(widget, nameof(Widget.Texture), Document.Current.AnimationId, key);
						animateTextureCellOffset++;
						break;
					}
					case ".ogg": {
						var args = new FilesDropManager.NodeCreatingEventArgs(assetPath, assetType);
						callbacks.NodeCreating?.Invoke(args);
						if (args.Cancel) {
							continue;
						}
						var node = Core.Operations.CreateNode.Perform(typeof(Audio));
						var sample = new SerializableSample(assetPath);
						Core.Operations.SetProperty.Perform(node, nameof(Audio.Sample), sample);
						Core.Operations.SetProperty.Perform(node, nameof(Node.Id), assetPath);
						Core.Operations.SetProperty.Perform(node, nameof(Audio.Volume), 1);
						var key = new Keyframe<AudioAction> {
							Frame = cellUnderMouseOnFilesDrop.X,
							Value = AudioAction.Play
						};
						Core.Operations.SetKeyframe.Perform(node, nameof(Audio.Action), Document.Current.AnimationId, key);
						callbacks.NodeCreated?.Invoke(node);
						break;
					}
				}
				handled.Add(file);
			}
			handledFiles = handled;
		}
	}
}
