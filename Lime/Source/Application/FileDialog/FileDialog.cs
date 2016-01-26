using System;

namespace Lime
{
	public enum FileDialogMode
	{
		Open,
		Save,
		SelectFolder,
	}

	public interface IFileDialog
	{
		FileDialogMode Mode { get; set; }
		bool CanCreateDirectories { get; set; }
		bool AllowsMultipleSelection { get; set; }
		string[] AllowedFileTypes { get; set; }
		string FileName { get; }
		string[] FileNames { get; }
		bool RunModal();
	}
}