using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class ContentsPathPropertyEditor : FilePropertyEditor<string>
	{
		public ContentsPathPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams, Document.AllowedFileTypes)
		{ }

		protected override bool IsValid(string path)
		{
			if (string.IsNullOrEmpty(path)) {
				return true;
			}
			if (base.IsValid(path)) {
				var resolvedPath = Node.ResolveScenePath(path);
				if (resolvedPath == null || !AssetBundle.Current.FileExists(resolvedPath)) {
					return false;
				}
				string assetPath;
				string assetType;
				return Utils.ExtractAssetPathOrShowAlert(path, out assetPath, out assetType)
				       && Utils.AssertCurrentDocument(assetPath, assetType);
			}
			return false;
		}

		protected override void AssignAsset(string path)
		{
			if (IsValid(path)) {
				DoTransaction(() => {
					SetProperty(path);
					Document.Current.RefreshExternalScenes();
				});
			} else {
				var value = CoalescedPropertyValue().GetValue();
				editor.Text = value.IsDefined ? value.Value : ManyValuesText;
			}
		}

		protected override string ValueToStringConverter(string obj)
		{
			return obj ?? "";
		}

		protected override string StringToValueConverter(string path) {
			return path;
		}
	}
}
