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
			var current = CoalescedPropertyValue();
			editor.AddChangeLateWatcher(current, v =>
				editor.Text = v.Value == null ?
					"RenderTexture (null)" :
					$"RenderTexture ({v.Value.ImageSize.Width}x{v.Value.ImageSize.Height})"
			);
			ManageManyValuesOnFocusChange(editor, current);
		}

		protected override void EnabledChanged()
		{
			base.EnabledChanged();
			editor.Enabled = Enabled;
		}
	}
}
