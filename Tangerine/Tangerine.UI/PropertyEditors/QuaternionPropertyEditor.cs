using System;
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
			editorX.Submitted += text => SetComponent(editorParams, 0, editorX, current.GetValue().Value, q => q.ToEulerAngles().X);
			editorY.Submitted += text => SetComponent(editorParams, 1, editorY, current.GetValue().Value, q => q.ToEulerAngles().Y);
			editorZ.Submitted += text => SetComponent(editorParams, 2, editorZ, current.GetValue().Value, q => q.ToEulerAngles().Z);
			editorX.AddChangeLateWatcher(current, v => {
				var ea = v.Value.ToEulerAngles() * Mathf.RadToDeg;
				editorX.Text = SameComponentValues(q => q.ToEulerAngles().X) ? RoundAngle(ea.X).ToString("0.###") : ManyValuesText;
				editorY.Text = SameComponentValues(q => q.ToEulerAngles().Y) ? RoundAngle(ea.Y).ToString("0.###") : ManyValuesText;
				editorZ.Text = SameComponentValues(q => q.ToEulerAngles().Z) ? RoundAngle(ea.Z).ToString("0.###") : ManyValuesText;
			});
		}

		float RoundAngle(float value) => (value * 1000f).Round() / 1000f;

		void SetComponent(IPropertyEditorParams editorParams, int component, NumericEditBox editor, Quaternion currentValue, Func<Quaternion, float> selector)
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
				editor.Text = SameComponentValues(selector)
					? RoundAngle(currentValue.ToEulerAngles()[component] * Mathf.RadToDeg).ToString("0.###")
					: ManyValuesText;
			}
		}

		public override void Submit()
		{
			var current = CoalescedPropertyValue();
			SetComponent(EditorParams, 0, editorX, current.GetValue().Value, q => q.ToEulerAngles().X);
			SetComponent(EditorParams, 1, editorY, current.GetValue().Value, q => q.ToEulerAngles().Y);
			SetComponent(EditorParams, 2, editorZ, current.GetValue().Value, q => q.ToEulerAngles().Z);
		}

		protected override void EnabledChanged()
		{
			base.EnabledChanged();
			editorX.Enabled = Enabled;
			editorY.Enabled = Enabled;
			editorZ.Enabled = Enabled;
		}
	}
}
