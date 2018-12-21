using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class ThicknessPropertyEditor : CommonPropertyEditor<Thickness>
	{
		private NumericEditBox editorLeft;
		private NumericEditBox editorRight;
		private NumericEditBox editorTop;
		private NumericEditBox editorBottom;

		public ThicknessPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			EditorContainer.AddNode(new Widget {
				Layout = new HBoxLayout { DefaultCell = new DefaultLayoutCell(Alignment.Center), Spacing = 4 },
				Nodes = {
					(editorLeft = editorParams.NumericEditBoxFactory()),
					(editorRight = editorParams.NumericEditBoxFactory()),
					(editorTop = editorParams.NumericEditBoxFactory()),
					(editorBottom = editorParams.NumericEditBoxFactory()),
					Spacer.HStretch(),
				}
			});
			var currentLeft = CoalescedPropertyComponentValue(v => v.Left);
			var currentRight = CoalescedPropertyComponentValue(v => v.Right);
			var currentTop = CoalescedPropertyComponentValue(v => v.Top);
			var currentBottom = CoalescedPropertyComponentValue(v => v.Bottom);
			editorLeft.Submitted += text => SetComponent(editorParams, 0, editorLeft, currentLeft.GetValue());
			editorRight.Submitted += text => SetComponent(editorParams, 1, editorRight, currentRight.GetValue());
			editorTop.Submitted += text => SetComponent(editorParams, 2, editorTop, currentTop.GetValue());
			editorBottom.Submitted += text => SetComponent(editorParams, 3, editorBottom, currentBottom.GetValue());
			editorLeft.AddChangeWatcher(currentLeft, v => editorLeft.Text = v.IsDefined ? v.Value.ToString("0.###") : ManyValuesText);
			editorRight.AddChangeWatcher(currentRight, v => editorRight.Text = v.IsDefined ? v.Value.ToString("0.###") : ManyValuesText);
			editorTop.AddChangeWatcher(currentTop, v => editorTop.Text = v.IsDefined ? v.Value.ToString("0.###") : ManyValuesText);
			editorBottom.AddChangeWatcher(currentBottom, v => editorBottom.Text = v.IsDefined ? v.Value.ToString("0.###") : ManyValuesText);
			ManageManyValuesOnFocusChange(editorLeft, currentLeft);
			ManageManyValuesOnFocusChange(editorRight, currentRight);
			ManageManyValuesOnFocusChange(editorTop, currentTop);
			ManageManyValuesOnFocusChange(editorBottom, currentBottom);
		}

		void SetComponent(IPropertyEditorParams editorParams, int component, NumericEditBox editor, CoalescedValue<float> currentValue)
		{
			float newValue;
			if (float.TryParse(editor.Text, out newValue)) {
				DoTransaction(() => {
					SetProperty<Thickness>((current) => {
						switch (component) {
							case 0: current.Left = newValue; break;
							case 1: current.Right = newValue; break;
							case 2: current.Top = newValue; break;
							case 3: current.Bottom = newValue; break;
						}
						return current;
					});
				});
			} else {
				switch (component) {
					case 0: editor.Text = currentValue.IsDefined ? currentValue.Value.ToString("0.###") : ManyValuesText; break;
					case 1: editor.Text = currentValue.IsDefined ? currentValue.Value.ToString("0.###") : ManyValuesText; break;
					case 2: editor.Text = currentValue.IsDefined ? currentValue.Value.ToString("0.###") : ManyValuesText; break;
					case 3: editor.Text = currentValue.IsDefined ? currentValue.Value.ToString("0.###") : ManyValuesText; break;
				}
			}
		}

		public override void Submit()
		{
			var currentLeft = CoalescedPropertyComponentValue(v => v.Left);
			var currentRight = CoalescedPropertyComponentValue(v => v.Right);
			var currentTop = CoalescedPropertyComponentValue(v => v.Top);
			var currentBottom = CoalescedPropertyComponentValue(v => v.Bottom);
			SetComponent(EditorParams, 0, editorLeft, currentLeft.GetValue());
			SetComponent(EditorParams, 1, editorRight, currentRight.GetValue());
			SetComponent(EditorParams, 2, editorTop, currentTop.GetValue());
			SetComponent(EditorParams, 3, editorBottom, currentBottom.GetValue());
		}
	}
}
