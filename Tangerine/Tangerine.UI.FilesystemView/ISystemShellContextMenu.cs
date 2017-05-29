namespace Tangerine.UI.FilesystemView
{
	public interface ISystemShellContextMenu
	{
		void Show(string[] multiplePaths);
		void Show(string path);
	}
}