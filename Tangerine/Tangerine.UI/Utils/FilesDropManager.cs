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
	public class FilesDropManager
	{
		public Dictionary<string, IFilesDropHandler> FilesDropHandlersByExtension { get; private set; }
			= new Dictionary<string, IFilesDropHandler>();

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
		private List<string> pendingImages;

		public event Action Handling;
		public event Action<NodeCreatingEventArgs> NodeCreating;
		public event Action<Node> NodeCreated;

		public bool Enabled { get; set; }

		public FilesDropManager(Widget widget)
		{
			this.widget = widget;
		}

		public void AddFilesDropHandler(IFilesDropHandler handler)
		{
			handler.Manager = this;
			foreach (var extension in handler.Extensions) {
				FilesDropHandlersByExtension[extension] = handler;
			}
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
			var result = false;
			using (Document.Current.History.BeginTransaction()) {
				foreach (var extensionGroup in files.GroupBy(Path.GetExtension)) {
					if (FilesDropHandlersByExtension.TryGetValue(extensionGroup.Key, out var handler)) {
						result |= handler.TryHandle(extensionGroup);
					}
				}
				Document.Current.History.CommitTransaction();
			}
			return result;
		}

		public void OnNodeCreating(NodeCreatingEventArgs args)
		{
			NodeCreating?.Invoke(args);
		}

		public void OnNodeCreated(Node node)
		{
			NodeCreated?.Invoke(node);
		}
	}
}
