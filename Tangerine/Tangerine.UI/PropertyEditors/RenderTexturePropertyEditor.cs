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
			ContainerWidget.AddNode(editor);
			editor.AddChangeWatcher(CoalescedPropertyValue(), v =>
				editor.Text = v == null ?
					"RenderTexture (null)" :
					$"RenderTexture ({v.ImageSize.Width}x{v.ImageSize.Height})"
			);
		}
	}
}