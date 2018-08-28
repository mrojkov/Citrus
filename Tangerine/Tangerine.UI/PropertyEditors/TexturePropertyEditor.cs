using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class TexturePropertyEditor<T> : FilePropertyEditor<T> where T: ITexture
	{
		public TexturePropertyEditor(IPropertyEditorParams editorParams) : base(editorParams, new string[] { "png" })
		{
			editor.AddChangeWatcher(CoalescedPropertyValue(), v => editor.Text = (v as SerializableTexture)?.SerializationPath ?? "");
		}

		protected override void AssignAsset(string path)
		{
			if (IsValid(path)) {
				SetProperty(new SerializableTexture(path));
			}
			else {
				editor.Text = (CoalescedPropertyValue().GetValue() as SerializableTexture)?.SerializationPath;
				new AlertDialog($"{EditorParams.PropertyName}: Value is not valid", "Ok").Show();
			}
		}
	}
}
