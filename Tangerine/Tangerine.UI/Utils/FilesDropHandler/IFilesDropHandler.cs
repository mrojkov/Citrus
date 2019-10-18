using System.Collections.Generic;

namespace Tangerine.UI.FilesDropHandler
{
	public interface IFilesDropHandler
	{
		List<string> Extensions { get; }
		void Handle(IEnumerable<string> files, IFilesDropCallbacks callbacks, out IEnumerable<string> handledFiles);
	}
}
