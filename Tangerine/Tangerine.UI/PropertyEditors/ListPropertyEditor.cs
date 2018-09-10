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

	public class ListPropertyEditor<TList, TElement>
		: ExpandablePropertyEditor<TList>
		, IListPropertyEditor where TList : IList<TElement>
		, IList, new() where TElement : new()
	{
		private readonly Action<PropertyEditorParams, Widget, IList> onAdd;
		private IList list;
		private Action removeCallback;

		public ListPropertyEditor(IPropertyEditorParams editorParams, Action<PropertyEditorParams, Widget, IList> onAdd) : base(editorParams)
		{
			this.onAdd = onAdd;

			if (EditorParams.Objects.Skip(1).Any()) {
				// Dont create editor interface if > 1 objects are selected
				EditorContainer.AddNode(new Widget() {
					Nodes = { new ThemedSimpleText { Text = "Edit of list properties isnt supported for multiple selection.", ForceUncutText = true } },
					// TODO: move color to theme
					Presenter = new WidgetFlatFillPresenter(new Color4(255, 194, 26))
				});
				return;
			}

			list = (IList)EditorParams.PropertyInfo.GetValue(EditorParams.Objects.First());
			var addButton = new ThemedAddButton() {
				Clicked = () => {
					if (list == null) {
						var pi = EditorParams.PropertyInfo;
						var o = EditorParams.Objects.First();
						pi.SetValue(o, list = new TList());
					}
					var newElement = new TElement();
					using (Document.Current.History.BeginTransaction()) {
						int newIndex = list.Count;
						InsertIntoList.Perform(list, newIndex, newElement);
						Document.Current.History.CommitTransaction();
					}
				}
			};
			EditorContainer.AddNode(addButton);
			ContainerWidget.Updating += _ => removeCallback?.Invoke();
			ContainerWidget.AddChangeWatcher(() => list.Count, Build);
		}

		private void Build(int _)
		{
			ExpandableContent.Nodes.Clear();
			if (list != null) {
				for (int i = 0; i < list.Count; i++) {
					AfterInsertNewElement(i);
				}
			}
		}

		private Widget AfterInsertNewElement(int index)
		{
			var elementContainer = new Widget {
				Layout = new HBoxLayout()
			};
			var p = new PropertyEditorParams(elementContainer, new[] { list }, new[] { list }, EditorParams.PropertyInfo.PropertyType, "Item", "Item"
			) {
				NumericEditBoxFactory = EditorParams.NumericEditBoxFactory,
				History = EditorParams.History,
				DefaultValueGetter = () => default,
				PropertySetter = (@object, name, value) => Core.Operations.SetIndexedProperty.Perform(@object, name, index, value),
				IndexInList = index,
			};
			onAdd(p, elementContainer, list);

			var removeButton = new ThemedDeleteButton();
			Action removeClicked = () => {
				removeCallback = null;
				using (Document.Current.History.BeginTransaction()) {
					RemoveFromList.Perform(list, index);
					Document.Current.History.CommitTransaction();
				}
			};
			removeButton.Clicked += () => removeCallback = removeClicked;
			ExpandableContent.Nodes.Insert(index, elementContainer);
			elementContainer.AddNode(removeButton);
			return elementContainer;
		}
	}
}
