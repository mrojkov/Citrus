using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class QuaternionPropertyEditor : CommonPropertyEditor<Quaternion>
	{
		private NumericEditBox editorX, editorY, editorZ;

		public QuaternionPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			ContainerWidget.AddNode(new Widget {
				Layout = new HBoxLayout { DefaultCell = new LayoutCell(Alignment.Center), Spacing = 4 },
				Nodes = {
					(editorX = editorParams.NumericEditBoxFactory()),
					(editorY = editorParams.NumericEditBoxFactory()),
					(editorZ = editorParams.NumericEditBoxFactory())
				}
			});
			var current = CoalescedPropertyValue();
			editorX.Submitted += text => SetComponent(editorParams, 0, editorX, current.GetValue());
			editorY.Submitted += text => SetComponent(editorParams, 1, editorY, current.GetValue());
			editorZ.Submitted += text => SetComponent(editorParams, 2, editorZ, current.GetValue());
			editorX.AddChangeWatcher(current, v => {
				var ea = v.ToEulerAngles() * Mathf.RadToDeg;
				editorX.Text = RoundAngle(ea.X).ToString();
				editorY.Text = RoundAngle(ea.Y).ToString();
				editorZ.Text = RoundAngle(ea.Z).ToString();
			});
		}

		float RoundAngle(float value) => (value * 1000f).Round() / 1000f;

		void SetComponent(IPropertyEditorParams editorParams, int component, NumericEditBox editor, Quaternion currentValue)
		{
			float newValue;
			if (float.TryParse(editor.Text, out newValue)) {
				DoTransaction(() => {
					foreach (var obj in editorParams.Objects) {
						var current = new Property<Quaternion>(obj, editorParams.PropertyName).Value.ToEulerAngles();
						current[component] = newValue * Mathf.DegToRad;
						editorParams.PropertySetter(obj, editorParams.PropertyName,
							Quaternion.CreateFromEulerAngles(current));
					}
				});
			} else {
				editor.Text = RoundAngle(currentValue.ToEulerAngles()[component] * Mathf.RadToDeg).ToString();
			}
		}

		public override void Submit()
		{
			var current = CoalescedPropertyValue();
			SetComponent(EditorParams, 0, editorX, current.GetValue());
			SetComponent(EditorParams, 1, editorY, current.GetValue());
			SetComponent(EditorParams, 2, editorZ, current.GetValue());
		}
	}
}
