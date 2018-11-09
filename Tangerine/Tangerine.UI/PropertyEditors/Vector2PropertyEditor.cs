using System;
using Lime;
using Tangerine.Core;
using Tangerine.Core.ExpressionParser;

namespace Tangerine.UI
{
	public class Vector2PropertyEditor : CommonPropertyEditor<Vector2>
	{
		private NumericEditBox editorX;
		private NumericEditBox editorY;

		public Vector2PropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			EditorContainer.AddNode(new Widget {
				Layout = new HBoxLayout { DefaultCell = new DefaultLayoutCell(Alignment.Center), Spacing = 4 },
				Nodes = {
					//new SimpleText { Text = "X" },
					(editorX = editorParams.NumericEditBoxFactory()),
					// new SimpleText { Text = "Y" },
					(editorY = editorParams.NumericEditBoxFactory()),
					Spacer.HStretch(),
				}
			});
			var currentX = CoalescedPropertyComponentValue(v => v.X);
			var currentY = CoalescedPropertyComponentValue(v => v.Y);
			editorX.Submitted += text => SetComponent(editorParams, 0, editorX, currentX.GetValue());
			editorY.Submitted += text => SetComponent(editorParams, 1, editorY, currentY.GetValue());
			editorX.AddChangeWatcher(currentX, v => editorX.Text = v.IsUndefined ? v.Value.ToString("0.###") : ManyValuesText);
			editorY.AddChangeWatcher(currentY, v => editorY.Text = v.IsUndefined ? v.Value.ToString("0.###") : ManyValuesText);
		}

		void SetComponent(IPropertyEditorParams editorParams, int component, CommonEditBox editor, CoalescedValue<float> currentValue)
		{
			if (Parser.TryParse(editor.Text, out double newValue)) {
				DoTransaction(() => {
					SetProperty<Vector2>(current => {
						current[component] = (float)newValue;
						return current;
					});
				});
				editor.Text = newValue.ToString("0.###");
			} else {
				editor.Text = currentValue.IsUndefined ? currentValue.Value.ToString("0.###") : ManyValuesText;
			}
		}

		public override void Submit()
		{
			var currentX = CoalescedPropertyComponentValue(v => v.X);
			var currentY = CoalescedPropertyComponentValue(v => v.Y);
			SetComponent(EditorParams, 0, editorX, currentX.GetValue());
			SetComponent(EditorParams, 1, editorY, currentY.GetValue());
		}
	}
}
