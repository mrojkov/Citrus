using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.UI.FilesDropHandler;

namespace Tangerine.UI
{
	public class FilesDropManager : IFilesDropCallbacks
	{
		private List<IFilesDropHandler> filesDropHandlers { get; } = new List<IFilesDropHandler>();

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

		private readonly Widget widget;

		public event Action Handling;
		public Action<NodeCreatingEventArgs> NodeCreating { get; set; }
		public Action<Node> NodeCreated { get; set; }

		public FilesDropManager(Widget widget)
		{
			this.widget = widget;
		}

		public void AddFilesDropHandler(IFilesDropHandler handler)
		{
			filesDropHandlers.Add(handler);
		}

		public void AddFilesDropHandlers(params IFilesDropHandler[] handlers)
		{
			foreach (var handler in handlers) {
				AddFilesDropHandler(handler);
			}
		}

		public void AddFilesDropHandlers(IEnumerable<IFilesDropHandler> handlers)
		{
			foreach (var handler in handlers) {
				AddFilesDropHandler(handler);
			}
		}

		public bool TryToHandle(IEnumerable<string> files)
		{
			var nodeUnderMouse = WidgetContext.Current.NodeUnderMouse;
			if (nodeUnderMouse == null || !nodeUnderMouse.SameOrDescendantOf(widget)) {
				return false;
			}
			Handling?.Invoke();
			using (Document.Current.History.BeginTransaction()) {
				foreach (var handlers in filesDropHandlers) {
					handlers.Handle(files, this, out var handledFiles);
					files = files.Except(handledFiles);
				}
				Document.Current.History.CommitTransaction();
			}
			return true;
		}
	}
}
