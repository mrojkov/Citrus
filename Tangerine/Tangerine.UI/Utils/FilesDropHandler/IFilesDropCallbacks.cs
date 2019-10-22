using System;
using Lime;

namespace Tangerine.UI.FilesDropHandler
{
	/// <summary>
	/// Files drop callbacks holder. These call back are used in order
	/// to let a model that accepts files drop response to important events
	/// (such as node creation).
	/// </summary>
	public interface IFilesDropCallbacks
	{
		/// <summary>
		/// Node creating event. Model that accepts files drop may be able to cancel node creation.
		/// </summary>
		Action<FilesDropManager.NodeCreatingEventArgs> NodeCreating { get; }
		/// <summary>
		/// Node created event.
		/// </summary>
		Action<Node> NodeCreated { get; }
	}
}
