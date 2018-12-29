using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class TextPropertyEditor : CommonPropertyEditor<string>
	{
		const int maxLines = 5;
		private EditBox editor;
		private ThemedButton button;

		public TextPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			EditorContainer.AddNode(new Widget {
				Layout = new HBoxLayout(),
				Nodes = {
					(editor = editorParams.EditBoxFactory()),
					Spacer.HSpacer(4),
					(button = new ThemedButton {
						Text = "...",
						MinMaxWidth = 20,
						LayoutCell = new LayoutCell(Alignment.Center)
					})
				}
			});
			editor.LayoutCell = new LayoutCell(Alignment.Center);
			editor.Editor.EditorParams.MaxLines = maxLines;
			editor.MinHeight += editor.TextWidget.FontHeight * (maxLines - 1);
			editor.Submitted += text => SetProperty(text);
			var current = CoalescedPropertyValue();
			editor.AddChangeWatcher(current, v => editor.Text = v.IsDefined ? v.Value : ManyValuesText);
			button.Clicked += () => {
				var window = new TextEditorDialog(editorParams.DisplayName ?? editorParams.PropertyName, editor.Text, (s) => {
					SetProperty(s);
				});
			};
			ManageManyValuesOnFocusChange(editor, current);
		}

		public override void Submit()
		{
			SetProperty(editor.Text);
		}

	}
}
