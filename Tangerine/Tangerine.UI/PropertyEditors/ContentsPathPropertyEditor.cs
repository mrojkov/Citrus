using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class ContentsPathPropertyEditor : FilePropertyEditor<string>
	{
		public ContentsPathPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams, Document.AllowedFileTypes)
		{
			editor.AddChangeWatcher(CoalescedPropertyValue(), v => editor.Text = v);
		}

		protected override bool IsValid(string path)
		{
			if (base.IsValid(path)) {
				var resolvedPath = Node.ResolveScenePath(path);
				if (resolvedPath == null || !AssetBundle.Current.FileExists(resolvedPath)) {
					editor.Text = CoalescedPropertyValue().GetValue();
					AlertDialog.Show($"{EditorParams.PropertyName}: Value is not valid");
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
				SetProperty(path);
				Document.Current.RefreshExternalScenes();
			} else {
				editor.Text = CoalescedPropertyValue().GetValue();
			}
		}
	}
}