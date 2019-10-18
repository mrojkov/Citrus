using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Operations;

namespace Tangerine.UI.FilesDropHandler
{
	public class AudiosDropHandler : IFilesDropHandler
	{
		public List<string> Extensions { get; } = new List<string> { ".ogg" };

		public void Handle(IEnumerable<string> files, IFilesDropCallbacks callbacks, out IEnumerable<string> handledFiles)
		{
			handledFiles = files.Where(f => Extensions.Contains(Path.GetExtension(f)));
			foreach (var file in handledFiles) {
				if (!Utils.ExtractAssetPathOrShowAlert(file, out var assetPath, out var assetType)) {
					continue;
				}
				var args = new FilesDropManager.NodeCreatingEventArgs(assetPath, assetType);
				callbacks.NodeCreating?.Invoke(args);
				if (args.Cancel) {
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
				callbacks.NodeCreated?.Invoke(node);
			}
		}
	}
}
