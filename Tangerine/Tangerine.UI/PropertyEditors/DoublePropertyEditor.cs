using Lime;
using Tangerine.Core;
using Tangerine.Core.ExpressionParser;

namespace Tangerine.UI
{
	public class DoublePropertyEditor : CommonPropertyEditor<double>
	{
		private NumericEditBox editor;

		public DoublePropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			editor = editorParams.NumericEditBoxFactory();
			EditorContainer.AddNode(editor);
			var current = CoalescedPropertyValue();
			editor.Submitted += text => {
				if (Parser.TryParse(text, out double newValue)) {
					SetProperty(newValue);
				} else {
					var currentValue = current.GetValue();
					editor.Text = currentValue.IsUndefined ? currentValue.Value.ToString("0.###") : ManyValuesText;
				}
			};
			editor.AddChangeWatcher(current, v => editor.Text = v.IsUndefined ? v.Value.ToString("0.###") : ManyValuesText);
		}
	}
}
