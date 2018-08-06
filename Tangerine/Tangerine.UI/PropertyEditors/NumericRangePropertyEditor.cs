using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class NumericRangePropertyEditor : CommonPropertyEditor<NumericRange>
	{
		private NumericEditBox medEditor, dispEditor;

		public NumericRangePropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			ContainerWidget.AddNode(new Widget {
				Layout = new HBoxLayout { CellDefaults = new LayoutCell(Alignment.Center), Spacing = 4 },
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
			float newValue;
			if (float.TryParse(editor.Text, out newValue)) {
				DoTransaction(() => {
					foreach (var obj in editorParams.Objects) {
						var current = new Property<NumericRange>(obj, editorParams.PropertyName).Value;
						if (component == 0) {
							current.Median = newValue;
						} else {
							current.Dispersion = newValue;
						}
						editorParams.PropertySetter(obj, editorParams.PropertyName, current);
					}
				});
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