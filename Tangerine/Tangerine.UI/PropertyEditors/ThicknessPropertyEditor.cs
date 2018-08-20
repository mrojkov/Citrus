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
			ContainerWidget.AddNode(new Widget {
				Layout = new HBoxLayout { DefaultCell = new LayoutCell(Alignment.Center), Spacing = 4 },
				Nodes = {
					(editorLeft = editorParams.NumericEditBoxFactory()),
					(editorRight = editorParams.NumericEditBoxFactory()),
					(editorTop = editorParams.NumericEditBoxFactory()),
					(editorBottom = editorParams.NumericEditBoxFactory()),
				}
			});
			var current = CoalescedPropertyValue();
			editorLeft.Submitted += text => SetComponent(editorParams, 0, editorLeft, current.GetValue());
			editorRight.Submitted += text => SetComponent(editorParams, 1, editorRight, current.GetValue());
			editorTop.Submitted += text => SetComponent(editorParams, 2, editorTop, current.GetValue());
			editorBottom.Submitted += text => SetComponent(editorParams, 3, editorBottom, current.GetValue());
			editorLeft.AddChangeWatcher(current, v => {
				editorLeft.Text = v.Left.ToString();
				editorRight.Text = v.Right.ToString();
				editorTop.Text = v.Top.ToString();
				editorBottom.Text = v.Bottom.ToString();
			});
		}

		void SetComponent(IPropertyEditorParams editorParams, int component, NumericEditBox editor, Thickness currentValue)
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
					case 0: editor.Text = currentValue.Left.ToString(); break;
					case 1: editor.Text = currentValue.Right.ToString(); break;
					case 2: editor.Text = currentValue.Top.ToString(); break;
					case 3: editor.Text = currentValue.Bottom.ToString(); break;
				}
			}
		}

		public override void Submit()
		{
			var current = CoalescedPropertyValue();
			SetComponent(EditorParams, 0, editorLeft, current.GetValue());
			SetComponent(EditorParams, 1, editorRight, current.GetValue());
			SetComponent(EditorParams, 2, editorTop, current.GetValue());
			SetComponent(EditorParams, 3, editorBottom, current.GetValue());
		}
	}
}
