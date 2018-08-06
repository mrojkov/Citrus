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
			ContainerWidget.AddNode(checkBox);
			checkBox.Changed += args => {
				if (args.ChangedByUser)
				{
					SetProperty(args.Value);
				}
			};
			checkBox.AddChangeWatcher(CoalescedPropertyValue(), v => checkBox.Checked = v);
		}
	}
}