using System.Collections.Generic;
using System.Linq;
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
			RefreshDropDownList();
			Selector.Changed += a => {
				if (a.ChangedByUser) {
					RefreshDropDownList();
					SetProperty((T)itemLister.ElementAt(a.Index).Item2);
				}
			};
			Selector.AddChangeWatcher(CoalescedPropertyValue(), v => Selector.Value = v);
			Selector.ShowingDropDownList += RefreshDropDownList;

			void RefreshDropDownList()
			{
				Selector.Items.Clear();
				foreach (var (key, value) in itemLister) {
					Selector.Items.Add(new CommonDropDownList.Item(key, value));
				}
			}
		}
	}
}
