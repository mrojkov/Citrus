using System.IO;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.PropertyEditors
{
	public class FolderPropertyEditor : FilePropertyEditor<string>
	{
		public FolderPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams, new string []{})
		{ }

		protected override string ValueToStringConverter(string obj)
		{
			return obj;
		}

		protected override string StringToValueConverter(string path)
		{
			return path;
		}

		protected override void AssignAsset(string path)
		{
			SetProperty(path);
		}

		protected override bool IsValid(string path)
		{
			return TangerineDefaultCharsetAttribute.IsValidPath(path, out var message) == ValidationResult.Ok;
		}

		protected override void OnSelectClicked()
		{
			var dlg = new FileDialog {
				Mode = FileDialogMode.SelectFolder,
				InitialDirectory = Directory.Exists(LastOpenedDirectory) ?
					LastOpenedDirectory : Path.GetDirectoryName(Document.Current.FullPath),
			};
			if (dlg.RunModal()) {
				SetProperty(dlg.FileName);
				LastOpenedDirectory = Path.GetDirectoryName(dlg.FileName);
			}
		}
	}
}
