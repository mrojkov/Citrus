using Lime;
using Tangerine.Core;
using Tangerine.Core.ExpressionParser;

namespace Tangerine.UI
{
	public class NumericRangePropertyEditor : CommonPropertyEditor<NumericRange>
	{
		private NumericEditBox medEditor, dispEditor;

		public NumericRangePropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			ContainerWidget.AddNode(new Widget {
				Layout = new HBoxLayout { DefaultCell = new DefaultLayoutCell(Alignment.Center), Spacing = 4 },
				Nodes = {
					(medEditor = editorParams.NumericEditBoxFactory()),
					(dispEditor = editorParams.NumericEditBoxFactory()),
				}
			});
			var currentMed = CoalescedPropertyComponentValue(v => v.Median);
			var currentDisp = CoalescedPropertyComponentValue(v => v.Dispersion);
			medEditor.Submitted += text => SetComponent(editorParams, 0, medEditor, currentMed.GetValue());
			dispEditor.Submitted += text => SetComponent(editorParams, 1, dispEditor, currentDisp.GetValue());
			medEditor.AddChangeWatcher(currentMed, v => medEditor.Text = v.ToString());
			dispEditor.AddChangeWatcher(currentDisp, v => dispEditor.Text = v.ToString());
		}

		void SetComponent(IPropertyEditorParams editorParams, int component, NumericEditBox editor, float currentValue)
		{
			if (Parser.TryParse(editor.Text, out double newValue)) {
				DoTransaction(() => {
					SetProperty<NumericRange>((current) => {
						if (component == 0) {
							current.Median = (float)newValue;
						} else {
							current.Dispersion = (float)newValue;
						}
						return current;
					});
				});
				editor.Text = newValue.ToString();
			} else {
				editor.Text = currentValue.ToString();
			}
		}

		public override void Submit()
		{
			var currentMed = CoalescedPropertyComponentValue(v => v.Median);
			var currentDisp = CoalescedPropertyComponentValue(v => v.Dispersion);
			SetComponent(EditorParams, 0, medEditor, currentMed.GetValue());
			SetComponent(EditorParams, 1, dispEditor, currentDisp.GetValue());
		}
	}
}
