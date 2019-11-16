using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Operations;
using Tangerine.UI;

namespace Tangerine.Common.FilesDropHandlers
{
	public class AudiosDropHandler
	{
		public void Handle(List<string> files)
		{
			using (Document.Current.History.BeginTransaction()) {
				foreach (var file in files.Where(f => Path.GetExtension(f) == ".ogg").ToList()) {
					files.Remove(file);
					if (!Utils.ExtractAssetPathOrShowAlert(file, out var assetPath, out var assetType)) {
						continue;
					}
					var node = CreateNode.Perform(typeof(Audio));
					var sample = new SerializableSample(assetPath);
					SetProperty.Perform(node, nameof(Audio.Sample), sample);
					SetProperty.Perform(node, nameof(Node.Id), Path.GetFileNameWithoutExtension(assetPath));
					SetProperty.Perform(node, nameof(Audio.Volume), 1);
					var key = new Keyframe<AudioAction> {
						Frame = Document.Current.AnimationFrame,
						Value = AudioAction.Play
					};
					SetKeyframe.Perform(node, nameof(Audio.Action), Document.Current.AnimationId, key);
				}
				Document.Current.History.CommitTransaction();
			}
		}
	}
}
