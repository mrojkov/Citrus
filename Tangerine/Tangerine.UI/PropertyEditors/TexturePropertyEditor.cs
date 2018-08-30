using System.Collections.Generic;
using System.Linq;
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

		private static readonly IReadOnlyList<char> validchars =
			Enumerable.Range(1, 128).Select(i => (char)i).
			Where(c =>
				char.IsLetterOrDigit(c) ||
				c == '\\' || c == '/' ||
				c == '_' || c == '.' || c == '!' || c == '#').ToList();

		protected override IEnumerable<char> ValidChars => validchars;
	}
}
