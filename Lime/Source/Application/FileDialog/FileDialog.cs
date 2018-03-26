using System;

namespace Lime
{
	/// <summary>
	/// Specifies possible dialog modes.
	/// </summary>
	public enum FileDialogMode
	{
		/// <summary>
		/// Open file dialog.
		/// </summary>
		Open,

		/// <summary>
		/// Save file dialog.
		/// </summary>
		Save,

		/// <summary>
		/// Select folder dialog.
		/// </summary>
		SelectFolder
	}

	/// <summary>
	/// Brings access to files and folders through the dialog screen.
	/// </summary>
	public interface IFileDialog
	{
		/// <summary>
		/// Gets or sets an array of allowed file types. Example { "txt", "png" }.
		/// </summary>
		string[] AllowedFileTypes { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether it is allowed to select more then one file in the open file dialog.
		/// </summary>
		bool AllowsMultipleSelection { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the dialog can create directories.
		/// </summary>
		bool CanCreateDirectories { get; set; }

		/// <summary>
		/// Gets a string containing file or folder path selected in the dialog.
		/// </summary>
		string FileName { get; }

		/// <summary>
		/// Gets an array of strings containing paths of all selected files.
		/// </summary>
		string[] FileNames { get; }

		/// <summary>
		/// Gets or sets a mode indicating whether this dialog opens file, saves file or selects folder.
		/// </summary>
		FileDialogMode Mode { get; set; }

		/// <summary>
		/// Gets or sets the dialog's initial directory. 
		/// </summary>
		string InitialDirectory { get; set; }

		/// <summary>
		/// Gets or sets initial file name.
		/// </summary>
		string InitialFileName { get; set; }

		/// <summary>
		/// Shows dialog.
		/// </summary>
		/// <returns>return true if user clicks OK in the dialog.</returns>
		bool RunModal();
	}
}
