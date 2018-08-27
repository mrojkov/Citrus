using Lime;
using Tangerine.Core;
using Tangerine.Core.ExpressionParser;

namespace Tangerine.UI
{
	public class FloatPropertyEditor : CommonPropertyEditor<float>
	{
		private NumericEditBox editor;

		public FloatPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			editor = editorParams.NumericEditBoxFactory();
			EditorContainer.AddNode(editor);
			EditorContainer.AddNode(Spacer.HStretch());
			var current = CoalescedPropertyValue();
			editor.Submitted += text => SetComponent(text, current);
			editor.AddChangeWatcher(current, v => editor.Text = v.ToString("0.###"));
		}

		public void SetComponent(string text, IDataflowProvider<float> current)
		{
			if (Parser.TryParse(text, out double newValue)) {
				SetProperty((float)newValue);
			}
			editor.Text = current.GetValue().ToString("0.###");
		}

		public override void Submit()
		{
			var current = CoalescedPropertyValue();
			SetComponent(editor.Text, current);
		}
	}
}
