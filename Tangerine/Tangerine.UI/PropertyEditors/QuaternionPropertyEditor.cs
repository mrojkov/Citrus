using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class QuaternionPropertyEditor : CommonPropertyEditor<Quaternion>
	{
		private NumericEditBox editorX, editorY, editorZ;

		public QuaternionPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			EditorContainer.AddNode(new Widget {
				Layout = new HBoxLayout { DefaultCell = new DefaultLayoutCell(Alignment.Center), Spacing = 4 },
				Nodes = {
					(editorX = editorParams.NumericEditBoxFactory()),
					(editorY = editorParams.NumericEditBoxFactory()),
					(editorZ = editorParams.NumericEditBoxFactory()),
					Spacer.HStretch(),
				}
			});
			var current = CoalescedPropertyValue();
			editorX.Submitted += text => SetComponent(editorParams, 0, editorX, current.GetValue());
			editorY.Submitted += text => SetComponent(editorParams, 1, editorY, current.GetValue());
			editorZ.Submitted += text => SetComponent(editorParams, 2, editorZ, current.GetValue());
			editorX.AddChangeWatcher(current, v => {
				var ea = v.ToEulerAngles() * Mathf.RadToDeg;
				editorX.Text = RoundAngle(ea.X).ToString("0.###");
				editorY.Text = RoundAngle(ea.Y).ToString("0.###");
				editorZ.Text = RoundAngle(ea.Z).ToString("0.###");
			});
		}

		float RoundAngle(float value) => (value * 1000f).Round() / 1000f;

		void SetComponent(IPropertyEditorParams editorParams, int component, NumericEditBox editor, Quaternion currentValue)
		{
			float newValue;
			if (float.TryParse(editor.Text, out newValue)) {
				DoTransaction(() => {
					SetProperty<Quaternion>((current) => {
						var eulerAngles = current.ToEulerAngles();
						eulerAngles[component] = newValue * Mathf.DegToRad;
						return Quaternion.CreateFromEulerAngles(eulerAngles);
					});
				});
			} else {
				editor.Text = RoundAngle(currentValue.ToEulerAngles()[component] * Mathf.RadToDeg).ToString("0.###");
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
