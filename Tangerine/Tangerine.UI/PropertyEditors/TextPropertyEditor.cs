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
			ContainerWidget.AddNode(new Widget {
				Layout = new HBoxLayout(),
				Nodes = {
					(editor = editorParams.EditBoxFactory()),
					new HSpacer(4),
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
			editor.AddChangeWatcher(CoalescedPropertyValue(), v => editor.Text = v);
			button.Clicked += () => {
				var window = new TextEditorDialog(editorParams.DisplayName ?? editorParams.PropertyName, editor.Text, (s) => {
					SetProperty(s);
				});
			};
		}

		public override void Submit()
		{
			SetProperty(editor.Text);
		}

	}
}
