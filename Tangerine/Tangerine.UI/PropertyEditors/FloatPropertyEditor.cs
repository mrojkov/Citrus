using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class FloatPropertyEditor : CommonPropertyEditor<float>
	{
		private NumericEditBox editor;

		public FloatPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			editor = editorParams.NumericEditBoxFactory();
			ContainerWidget.AddNode(editor);
			var current = CoalescedPropertyValue();
			editor.Submitted += text => SetComponent(text, current);
			editor.AddChangeWatcher(current, v => editor.Text = v.ToString());
		}

		public void SetComponent(string text, IDataflowProvider<float> current)
		{
			float newValue;
			if (float.TryParse(text, out newValue)) {
				SetProperty(newValue);
			}

			editor.Text = current.GetValue().ToString();
		}

		public override void Submit()
		{
			var current = CoalescedPropertyValue();
			SetComponent(editor.Text, current);
		}
	}
}