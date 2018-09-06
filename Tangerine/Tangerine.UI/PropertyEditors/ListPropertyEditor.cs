using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Operations;

namespace Tangerine.UI
{
	public interface IListPropertyEditor
	{

	}

	public class ListPropertyEditor<TList, TElement> : ExpandablePropertyEditor<TList>, IListPropertyEditor where TList : IList<TElement>, IList, new() where TElement : new()
	{
		public ThemedAddButton AddButton;

		public ListPropertyEditor(IPropertyEditorParams editorParams, Action<Widget, int> onAdd, Action<Widget, int> onRemove) : base(editorParams)
		{
			if (EditorParams.Objects.Skip(1).Any()) {
				// Dont create editor interface if > 1 objects are selected
				EditorContainer.AddNode(new Widget() {
					Nodes = { new ThemedSimpleText { Text = "Edit of list properties isnt supported for multiple selection.", ForceUncutText = true} },
					// TODO: move color to theme
					Presenter = new WidgetFlatFillPresenter(new Color4(255, 194, 26))
				});
				return;
			}
			var pi = EditorParams.PropertyInfo;
			var o = EditorParams.Objects.First();
			var list = (IList)pi.GetValue(o);
			AddButton = new ThemedAddButton();
			EditorContainer.AddNode(AddButton);
			Func<int, Widget> makeElement = (index) => {
				var elementContainer = new Widget {
					Layout = new HBoxLayout()
				};
				var removeButton = new ThemedDeleteButton();
				removeButton.Clicked += () => {
					using (Document.Current.History.BeginTransaction()) {
						RemoveFromList.Perform(list, index);
						Document.Current.History.CommitTransaction();
					}
					elementContainer.UnlinkAndDispose();
					onRemove(elementContainer, index);
				};
				ExpandableContent.AddNode(elementContainer);
				onAdd(elementContainer, index);
				elementContainer.AddNode(removeButton);
				return elementContainer;
			};
			AddButton.Clicked += () => {
				if (list == null) {
					pi.SetValue(o, list = (IList)new TList());
				}
				var newElement = new TElement();
				using (Document.Current.History.BeginTransaction()) {
					InsertIntoList.Perform(list, list.Count, newElement);
					Document.Current.History.CommitTransaction();
				}
				makeElement(list.Count - 1);
			};
			if (list != null) {
				for (int i = 0; i < list.Count; i++) {
					makeElement(i);
				}
			}
		}
	}
}
