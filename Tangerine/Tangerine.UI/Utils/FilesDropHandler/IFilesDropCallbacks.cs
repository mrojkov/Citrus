using System;
using Lime;

namespace Tangerine.UI.FilesDropHandler
{
	public interface IFilesDropCallbacks
	{
		Action<FilesDropManager.NodeCreatingEventArgs> NodeCreating { get; }
		Action<Node> NodeCreated { get; }
	}
}
