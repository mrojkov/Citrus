using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Lime;
using Tangerine.UI.Drop;

namespace Tangerine.UI
{
	/// <summary>
	/// Manages a collection of IFilesDropHandlers
	/// </summary>
	public class FilesDropManager
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

		/// <summary>
		/// Called when manager starts handling dropped files.
		/// </summary>
		public event Action Handling;

		/// <param name="widget">Managed area.</param>
		public FilesDropManager(Widget widget)
		{
			this.widget = widget;
		}
		/// <summary>
		/// Add an object that extends IFilesDropHandler to collection of IFilesDropHandlers.
		/// </summary>
		/// <param name="handler">Object that extends IFilesDropHandler.</param>
		public void AddFilesDropHandler(IFilesDropHandler handler)
		{
			filesDropHandlers.Add(handler);
		}
		/// <summary>
		/// Add objects that extend IFilesDropHandler to collection of IFilesDropHandlers.
		/// </summary>
		/// <param name="handlers">Objects that extend IFilesDropHandler interface.</param>
		public void AddFilesDropHandlers(params IFilesDropHandler[] handlers)
		{
			foreach (var handler in handlers) {
				AddFilesDropHandler(handler);
			}
		}
		/// <summary>
		/// Add objects that extend IFilesDropHandler to collection of IFilesDropHandlers.
		/// </summary>
		/// <param name="handlers">Objects that extend IFilesDropHandler interface.</param>
		public void AddFilesDropHandlers(IEnumerable<IFilesDropHandler> handlers)
		{
			foreach (var handler in handlers) {
				AddFilesDropHandler(handler);
			}
		}
		/// <summary>
		/// Remove object that extends IFilesDropHandler from collection of IFilesDropHandlers
		/// </summary>
		/// <param name="handler">Object that extends IFilesDropHandler interface.</param>
		public void RemoveFilesDropHandler(IFilesDropHandler handler)
		{
			filesDropHandlers.Remove(handler);
		}

		/// <summary>
		/// Tries to handle files if they were dropped on managed area.
		/// </summary>
		/// <param name="files">Files.</param>
		/// <returns></returns>
		public bool TryToHandle(IEnumerable<string> files)
		{
			var nodeUnderMouse = WidgetContext.Current.NodeUnderMouse;
			if (nodeUnderMouse == null || !nodeUnderMouse.SameOrDescendantOf(widget)) {
				return false;
			}
			Handling?.Invoke();
			foreach (var handlers in filesDropHandlers) {
				handlers.Handle(files, out var handledFiles);
				files = files.Except(handledFiles);
			}
			return true;
		}
	}
}
