using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime;

namespace Tangerine.UI.Drop
{
	public class DropManager
	{
		private readonly List<FilesDropManager> filesDropManagers = new List<FilesDropManager>();
		public static DropManager Instance { get; private set; }

		public static void Initialize()
		{
			if (Instance != null) {
				throw new InvalidOperationException("DropManager already exists!");
			}
			Instance = new DropManager();
		}

		public void ManageFileDrop(IWindow window)
		{
			window.AllowDropFiles = true;
			window.FilesDropped += OnFilesDropped;
		}

		private void OnFilesDropped(IEnumerable<string> files)
		{
			// Later added FilesDropManagers try to handle files drop earlier
			// This approach is used in order to allow hierarchical files drop
			// management (e.g. multiple files droppable areas in multiple
			// files droppable areas and etc.).

			for (var i = filesDropManagers.Count - 1; i >= 0; i--) {
				var filesDropManager = filesDropManagers[i];
				if (filesDropManager.TryToHandle(files)) {
					break;
				}
			}
		}

		/// <summary>
		/// Add an instance of FilesDropManager.
		/// </summary>
		/// <param name="filesDropManager">Instance of FilesDropManager</param>
		public void AddFilesDropManager(FilesDropManager filesDropManager) => filesDropManagers.Add(filesDropManager);
		/// <summary>
		/// Remove an instance of FilesDropManager.
		/// </summary>
		/// <param name="filesDropManager">Instance of FilesDropManager</param>
		public void RemoveFilesDropManager(FilesDropManager filesDropManager) => filesDropManagers.Remove(filesDropManager);
	}
}
