using System.Collections.Generic;
using Tangerine.UI.FilesDropHandler;

namespace Tangerine.UI.Inspector
{
	public class InspectorFilesDropHandler : IFilesDropHandler
	{
		public List<string> Extensions { get; } = new List<string>();
		private readonly InspectorContent content;

		public InspectorFilesDropHandler(InspectorContent content)
		{
			this.content = content;
		}
		public void Handle(IEnumerable<string> files, IFilesDropCallbacks callbacks, out IEnumerable<string> handledFiles)
		{
			handledFiles = files;
			content.DropFiles(files);
		}
	}
}
