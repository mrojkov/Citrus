#if MAC || MONOMAC
using System;
using System.Linq;
#if MAC
using AppKit;
#else
using MonoMac.AppKit;
#endif

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
		public string InitialDirectory { get; set; }
		public string InitialFileName { get; set; }

		public bool RunModal()
		{
			FileName = null;
			FileNames = null;
			if (Mode == FileDialogMode.Save) {
				using (var nsPanel = new NSSavePanel()) {
					if (InitialDirectory != null) {
						nsPanel.Directory = InitialDirectory;
					}
					if (AllowedFileTypes != null) {
						nsPanel.AllowedFileTypes = AllowedFileTypes;
					}
					nsPanel.CanCreateDirectories = CanCreateDirectories;
					nsPanel.NameFieldLabel = InitialFileName ?? System.String.Empty;
					if (nsPanel.RunModal() == (int)NSPanelButtonType.Ok) {
						FileName = nsPanel.Url.Path;
						FileNames = new[] { FileName };
						return true;
					}
				}
			} else {
				using (var nsPanel = new NSOpenPanel()) {
					if (InitialDirectory != null) {
						nsPanel.Directory = InitialDirectory;
					}
					if (AllowedFileTypes != null) {
						nsPanel.AllowedFileTypes = AllowedFileTypes;
					}
					nsPanel.AllowsMultipleSelection = AllowsMultipleSelection;
					nsPanel.CanChooseFiles = Mode == FileDialogMode.Open;
					nsPanel.CanChooseDirectories = !nsPanel.CanChooseFiles;
					nsPanel.CanCreateDirectories = CanCreateDirectories;
					nsPanel.NameFieldLabel = InitialFileName ?? System.String.Empty;
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
