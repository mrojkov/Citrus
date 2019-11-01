using System.Collections.Generic;

namespace Tangerine.UI.Drop
{
	/// <summary>
	/// Interface for classes that will be able to handle files drop.
	/// </summary>
	public interface IFilesDropHandler
	{
		/// <summary>
		/// Handle dropped files.
		/// </summary>
		/// <param name="files">Dropped files to handle.</param>
		/// <param name="handledFiles">Handled files.</param>
		void Handle(IEnumerable<string> files, out IEnumerable<string> handledFiles);
	}
}
