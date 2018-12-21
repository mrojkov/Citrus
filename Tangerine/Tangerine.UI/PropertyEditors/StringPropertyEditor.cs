using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class StringPropertyEditor : CommonPropertyEditor<string>
	{
		const int maxLines = 5;
		private EditBox editor;

		public StringPropertyEditor(IPropertyEditorParams editorParams, bool multiline = false) : base(editorParams)
		{
			editor = editorParams.EditBoxFactory();
			editor.LayoutCell = new LayoutCell(Alignment.Center);
			editor.Editor.EditorParams.MaxLines = multiline ? maxLines : 1;
			editor.MinHeight += multiline ? editor.TextWidget.FontHeight * (maxLines - 1) : 0;
			EditorContainer.AddNode(editor);
			editor.Submitted += text => SetProperty(text);
			editor.AddChangeWatcher(CoalescedPropertyValue(), v => editor.Text = v.IsDefined ? v.Value : ManyValuesText);
		}

		public override void Submit()
		{
			SetProperty(editor.Text);
		}

	}
}
