#if MAC || MONOMAC
using System;
using AppKit;
using System.Linq;

namespace Lime
{
	public class FileDialog : IFileDialog
	{
		public FileDialogMode Mode { get; set; }
		public bool AllowsMultipleSelection { get; set; }
		public bool CanCreateDirectories { get; set; }
		public string FileName { get; private set; }
		public string[] FileNames { get; private set; }
		public string[] AllowedFileTypes { get; set; }

		public FileDialog() { }

		public bool RunModal()
		{
			FileName = null;
			FileNames = null;
			if (Mode == FileDialogMode.Save) {
				using (var nsPanel = new NSSavePanel()) {
					if (AllowedFileTypes != null) {
						nsPanel.AllowedFileTypes = AllowedFileTypes;
					}
					nsPanel.CanCreateDirectories = CanCreateDirectories;
					if (nsPanel.RunModal() == (int)NSPanelButtonType.Ok) {
						FileName = nsPanel.Url.Path;
						FileNames = new[] { FileName };
						return true;
					}
				}
			} else {
				using (var nsPanel = new NSOpenPanel()) {
					if (AllowedFileTypes != null) {
						nsPanel.AllowedFileTypes = AllowedFileTypes;
					}
					nsPanel.AllowsMultipleSelection = AllowsMultipleSelection;
					nsPanel.CanChooseFiles = Mode == FileDialogMode.Open;
					nsPanel.CanChooseDirectories = !nsPanel.CanChooseFiles;
					nsPanel.CanCreateDirectories = CanCreateDirectories;
					if (nsPanel.RunModal() == (int)NSPanelButtonType.Ok) {
						FileName = nsPanel.Url.Path;
						FileNames = nsPanel.Urls.Select(i => i.Path).ToArray();
						return true;
					}
				}
			}
			return false;
		}
	}
}
#endif