using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class RenderTexturePropertyEditor : CommonPropertyEditor<ITexture>
	{
		private EditBox editor;

		public RenderTexturePropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			editor = editorParams.EditBoxFactory();
			editor.IsReadOnly = true;
			editor.LayoutCell = new LayoutCell(Alignment.Center);
			EditorContainer.AddNode(editor);
			editor.AddChangeWatcher(CoalescedPropertyValue(), v =>
				editor.Text = v.Value == null ?
					"RenderTexture (null)" :
					$"RenderTexture ({v.Value.ImageSize.Width}x{v.Value.ImageSize.Height})"
			);
		}
	}
}
