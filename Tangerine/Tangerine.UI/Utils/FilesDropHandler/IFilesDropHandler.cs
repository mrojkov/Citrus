using System.Collections;
using System.Collections.Generic;

namespace Tangerine.UI.FilesDropHandler
{
	public interface IFilesDropHandler
	{
		List<string> Extensions { get; }
		FilesDropManager Manager { get; set; }
		bool TryHandle(IEnumerable<string> files);
	}
}
