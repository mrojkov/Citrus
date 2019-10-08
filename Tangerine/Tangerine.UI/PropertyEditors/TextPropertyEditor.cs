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
			var first = true;
			var submitted = false;
			var current = CoalescedPropertyValue();
			editor.AddChangeLateWatcher(current, v => editor.Text = v.IsDefined ? v.Value : ManyValuesText);
			button.Clicked += () => {
				var window = new TextEditorDialog(editorParams.DisplayName ?? editorParams.PropertyName, editor.Text, (s) => {
					SetProperty(s);
				});
			};
			editor.Submitted += text => Submit();
			editor.AddChangeLateWatcher(() => editor.Text, text => {
				if (first) {
					first = false;
					return;
				}
				if (!editor.IsFocused()) {
					return;
				}
				if (submitted) {
					Document.Current.History.Undo();
				}
				submitted = true;
				Submit();
			});
			editor.AddChangeLateWatcher(() => editor.IsFocused(), focused => {
				if (submitted) {
					Document.Current.History.Undo();
				}
				if (!focused) {
					submitted = false;
				}
			});
			ManageManyValuesOnFocusChange(editor, current);
		}

		public override void Submit()
		{
			SetProperty(editor.Text);
		}

		protected override void EnabledChanged()
		{
			base.EnabledChanged();
			editor.Enabled = Enabled;
			button.Enabled = Enabled;
		}
	}
}
