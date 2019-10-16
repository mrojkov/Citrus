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
			return TangerineDefaultCharsetAttribute.IsValid(path, out var message) == ValidationResult.Ok;
		}

		protected override void OnSelectClicked()
		{
			var current = CoalescedPropertyValue().GetDataflow();
			current.Poll();
			var value = current.Value;
			var dlg = new FileDialog {
				Mode = FileDialogMode.SelectFolder,
				InitialDirectory =
					current.GotValue && value.IsDefined && !string.IsNullOrEmpty(value.Value) && TryGetClosestAvailableDirectory(
						AssetPath.Combine(Project.Current.AssetsDirectory, value.Value), out var dir) ?
						dir : Directory.Exists(LastOpenedDirectory) ?
							LastOpenedDirectory : Project.Current.AssetsDirectory
			};
			if (dlg.RunModal()) {
				SetProperty(dlg.FileName);
				LastOpenedDirectory = Path.GetDirectoryName(dlg.FileName);
			}
		}
	}
}
