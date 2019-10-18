using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Components;
using Tangerine.Core.Operations;
using Tangerine.UI.FilesDropHandler;
using Tangerine.UI.Timeline.Operations.CompoundAnimations;

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
			using (Document.Current.History.BeginTransaction()) {
				foreach (var file in files) {
					if (Document.Current.Animation.IsCompound) {
						try {
							// Dirty hack: using a file drag&drop mechanics for dropping animation clips on the grid.
							var decodedAnimationId = Encoding.UTF8.GetString(Convert.FromBase64String(file));
							AddAnimationClip.Perform(
								new IntVector2(
									cellUnderMouseOnFilesDrop.X + animateTextureCellOffset,
									cellUnderMouseOnFilesDrop.Y),
								decodedAnimationId);
						} catch { }
					}
					if (!Utils.ExtractAssetPathOrShowAlert(file, out var assetPath, out var assetType)) {
						continue;
					}
					switch (assetType) {
						case ".png": {
							if (Document.Current.Rows.Count == 0) {
								continue;
							}
							var widget = Document.Current.Rows[cellUnderMouseOnFilesDrop.Y].Components.Get<NodeRow>()?.Node as Widget;
							if (widget == null) {
								continue;
							}
							var key = new Keyframe<ITexture> {
								Frame = cellUnderMouseOnFilesDrop.X + animateTextureCellOffset,
								Value = new SerializableTexture(assetPath),
								Function = KeyFunction.Steep,
							};
							SetKeyframe.Perform(widget, nameof(Widget.Texture), Document.Current.AnimationId, key);
							animateTextureCellOffset++;
							break;
						}
						case ".ogg": {
							var args = new FilesDropManager.NodeCreatingEventArgs(assetPath, assetType);
							callbacks.NodeCreating?.Invoke(args);
							if (args.Cancel) {
								continue;
							}
							var node = CreateNode.Perform(typeof(Audio));
							var sample = new SerializableSample(assetPath);
							SetProperty.Perform(node, nameof(Audio.Sample), sample);
							SetProperty.Perform(node, nameof(Node.Id), assetPath);
							SetProperty.Perform(node, nameof(Audio.Volume), 1);
							var key = new Keyframe<AudioAction> {
								Frame = cellUnderMouseOnFilesDrop.X,
								Value = AudioAction.Play
							};
							SetKeyframe.Perform(node, nameof(Audio.Action), Document.Current.AnimationId, key);
							callbacks.NodeCreated?.Invoke(node);
							break;
						}
					}
					handled.Add(file);
				}
				Document.Current.History.CommitTransaction();
			}
			handledFiles = handled;
		}
	}
}
