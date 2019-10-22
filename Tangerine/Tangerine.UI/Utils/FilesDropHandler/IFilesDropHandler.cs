using System.Collections.Generic;

namespace Tangerine.UI.FilesDropHandler
{
	/// <summary>
	/// Interface for classes that will be able to handle files drop.
	/// </summary>
	public interface IFilesDropHandler
	{
		/// <summary>
		/// Acceptable extensions. Optional use.
		/// </summary>
		List<string> Extensions { get; }
		/// <summary>
		/// Handle dropped files.
		/// </summary>
		/// <param name="files">Dropped files to handle.</param>
		/// <param name="callbacks">Callbacks.</param>
		/// <param name="handledFiles">Handled files.</param>
		void Handle(IEnumerable<string> files, IFilesDropCallbacks callbacks, out IEnumerable<string> handledFiles);
	}
}
