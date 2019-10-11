using System.Collections.Generic;
using System.IO;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Operations;

namespace Tangerine.UI.FilesDropHandler
{
	public class AudiosDropHandler : IFilesDropHandler
	{
		public List<string> Extensions { get; } = new List<string> { ".ogg" };
		public FilesDropManager Manager { get; set; }
		public bool TryHandle(IEnumerable<string> files)
		{
			var result = false;
			foreach (var file in files) {
				if (!Utils.ExtractAssetPathOrShowAlert(file, out var assetPath, out var assetType) ||
					!Utils.AssertCurrentDocument(assetPath, assetType)) {
					continue;
				}
				result = true;
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
				Manager.OnNodeCreated(node);
			}
			return result;
		}
	}
}
