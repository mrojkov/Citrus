using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Components;
using Tangerine.Core.Operations;
using Tangerine.UI.Drop;
using Tangerine.UI.Timeline.Operations.CompoundAnimations;

namespace Tangerine.UI.Timeline
{
	internal class GridPaneFilesDropHandler : IFilesDropHandler
	{
		private GridPane grid => Timeline.Instance.Grid;

		public void Handle(IEnumerable<string> files, out IEnumerable<string> handledFiles)
		{
			var rowLocationUnderMouseOnFilesDrop =
				SelectAndDragRowsProcessor.MouseToRowLocation(grid.RootWidget.Input.MousePosition);
			var handled = new List<string>();
			var cellUnderMouseOnFilesDrop = grid.CellUnderMouse();
			var animateTextureCellOffset = 0;
			using (Document.Current.History.BeginTransaction()) {
				foreach (var file in files) {
					if (Document.Current.Animation.IsCompound) {
						try {
							// Dirty hack: using a file drag&drop mechanics for dropping animation clips on the grid.
							// Drop data will be cleaned before we leave modal window so there is no need
							// to return handled files
							handled.Clear();
							handledFiles = handled;
							var decodedAnimationId = Encoding.UTF8.GetString(Convert.FromBase64String(file));
							AddAnimationClip.Perform(
								new IntVector2(
									cellUnderMouseOnFilesDrop.X + animateTextureCellOffset,
									cellUnderMouseOnFilesDrop.Y),
								decodedAnimationId);
							return;
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
							var node = CreateNode.Perform(typeof(Audio));
							if (rowLocationUnderMouseOnFilesDrop.HasValue) {
								var location = rowLocationUnderMouseOnFilesDrop.Value;
								var row = Document.Current.Rows.FirstOrDefault(r => r.Components.Get<Core.Components.NodeRow>()?.Node == node);
								if (row != null) {
									if (location.Index >= row.Index) {
										location.Index++;
									}
									SelectAndDragRowsProcessor.Probers.Any(p => p.Probe(row, location));
								}
							}
							var sample = new SerializableSample(assetPath);
							SetProperty.Perform(node, nameof(Audio.Sample), sample);
							SetProperty.Perform(node, nameof(Node.Id), assetPath);
							SetProperty.Perform(node, nameof(Audio.Volume), 1);
							var key = new Keyframe<AudioAction> {
								Frame = cellUnderMouseOnFilesDrop.X,
								Value = AudioAction.Play
							};
							SetKeyframe.Perform(node, nameof(Audio.Action), Document.Current.AnimationId, key);
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
