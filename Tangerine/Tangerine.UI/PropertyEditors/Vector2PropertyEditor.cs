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
			ContainerWidget.AddNode(new Widget {
				Layout = new HBoxLayout { CellDefaults = new LayoutCell(Alignment.Center), Spacing = 4 },
				Nodes = {
					(editorX = editorParams.NumericEditBoxFactory()),
					(editorY = editorParams.NumericEditBoxFactory())
				}
			});
			var currentX = CoalescedPropertyComponentValue(v => v.X);
			var currentY = CoalescedPropertyComponentValue(v => v.Y);
			editorX.Submitted += text => SetComponent(editorParams, 0, editorX, currentX.GetValue());
			editorY.Submitted += text => SetComponent(editorParams, 1, editorY, currentY.GetValue());
			editorX.AddChangeWatcher(currentX, v => editorX.Text = v.ToString());
			editorY.AddChangeWatcher(currentY, v => editorY.Text = v.ToString());
		}

		void SetComponent(IPropertyEditorParams editorParams, int component, CommonEditBox editor, float currentValue)
		{
			if (Parser.TryParse(editor.Text, out double newValue)) {
				DoTransaction(() => {
					foreach (var obj in editorParams.Objects) {
						var current = new Property<Vector2>(obj, editorParams.PropertyName).Value;
						current[component] = (float)newValue;
						editorParams.PropertySetter(obj, editorParams.PropertyName, current);
					}
				});
			}
			editor.Text = currentValue.ToString();
		}

		public override void Submit()
		{
			string text = editorX.Text;
			var currentX = CoalescedPropertyComponentValue(v => v.X);
			var currentY = CoalescedPropertyComponentValue(v => v.Y);
			SetComponent(EditorParams, 0, editorX, currentX.GetValue());
			SetComponent(EditorParams, 1, editorY, currentY.GetValue());
		}
	}
}
