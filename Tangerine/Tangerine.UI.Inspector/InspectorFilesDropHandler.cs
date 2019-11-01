using System.Collections.Generic;
using Tangerine.UI.Drop;

namespace Tangerine.UI.Inspector
{
	public class InspectorFilesDropHandler : IFilesDropHandler
	{
		private readonly InspectorContent content;

		public InspectorFilesDropHandler(InspectorContent content)
		{
			this.content = content;
		}
		public void Handle(IEnumerable<string> files, out IEnumerable<string> handledFiles)
		{
			handledFiles = files;
			content.DropFiles(files);
		}
	}
}
