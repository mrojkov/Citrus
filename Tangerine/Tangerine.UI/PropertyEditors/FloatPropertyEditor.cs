using System.Collections.Generic;
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
			editor.Submitted += text => SetComponent(text, current.GetValue());
			editor.AddChangeWatcher(current, v => editor.Text = v.IsDefined ? v.Value.ToString("0.###") : ManyValuesText);
			ManageManyValuesOnFocusChange(editor, current);
		}

		public void SetComponent(string text, CoalescedValue<float> current)
		{
			if (Parser.TryParse(text, out double newValue)) {
				SetProperty((float)newValue);
				editor.Text = ((float) newValue).ToString("0.###");
			} else {
				editor.Text = current.IsDefined ? current.Value.ToString("0.###") : ManyValuesText;
			}
		}

		public override void Submit()
		{
			var current = CoalescedPropertyValue();
			SetComponent(editor.Text, current.GetValue());
		}

		protected override void EnabledChanged()
		{
			base.EnabledChanged();
			editor.Enabled = Enabled;
		}
	}
}
