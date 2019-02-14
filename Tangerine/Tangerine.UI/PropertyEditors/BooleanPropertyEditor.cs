using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class BooleanPropertyEditor : CommonPropertyEditor<bool>
	{
		private CheckBox checkBox;

		public BooleanPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			checkBox = new ThemedCheckBox { LayoutCell = new LayoutCell(Alignment.LeftCenter) };
			EditorContainer.AddNode(checkBox);
			EditorContainer.AddNode(Spacer.HStretch());
			checkBox.Changed += args => {
				if (args.ChangedByUser) {
					SetProperty(args.Value);
				}
			};
			checkBox.AddChangeWatcher(CoalescedPropertyValue(), v => checkBox.State = v.IsDefined
				? v.Value ? CheckBoxState.Checked : CheckBoxState.Unchecked : CheckBoxState.Indeterminate);
		}

		protected override void EnabledChanged()
		{
			base.EnabledChanged();
			checkBox.Enabled = Enabled;
		}
	}
}
