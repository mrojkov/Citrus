using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class DropDownListPropertyEditor<T> : CommonPropertyEditor<T>
	{
		protected DropDownList Selector { get; }

		public DropDownListPropertyEditor(IPropertyEditorParams editorParams, IEnumerable<(string, object)> itemLister) : base(editorParams)
		{
			Selector = editorParams.DropDownListFactory();
			Selector.LayoutCell = new LayoutCell(Alignment.Center);
			EditorContainer.AddNode(Selector);
			foreach (var (key, value) in itemLister) {
				Selector.Items.Add(new CommonDropDownList.Item(key, value));
			}
			Selector.Changed += a => {
				if (a.ChangedByUser)
					SetProperty((T)Selector.Items[a.Index].Value);
			};
			Selector.AddChangeWatcher(CoalescedPropertyValue(), v => Selector.Value = v);
		}
	}
}
