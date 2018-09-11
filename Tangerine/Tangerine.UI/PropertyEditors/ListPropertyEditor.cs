using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Operations;

namespace Tangerine.UI
{
	public class ListPropertyEditor<TList, TElement>
		: ExpandablePropertyEditor<TList> where TList : IList<TElement>, IList, new() where TElement : new()
	{
		private readonly Func<PropertyEditorParams, Widget, IList, IEnumerable<IPropertyEditor>> onAdd;
		private IList list;
		private Action removeCallback;

		public ListPropertyEditor(IPropertyEditorParams editorParams, Func<PropertyEditorParams, Widget, IList, IEnumerable<IPropertyEditor>> onAdd) : base(editorParams)
		{
			this.onAdd = onAdd;

			if (EditorParams.Objects.Skip(1).Any()) {
				// Dont create editor interface if > 1 objects are selected
				EditorContainer.AddNode(new Widget() {
					Layout = new HBoxLayout(),
					Nodes = { new ThemedSimpleText { Text = "Edit of list properties isnt supported for multiple selection.", ForceUncutText = false } },
					// TODO: move color to theme
					Presenter = new WidgetFlatFillPresenter(Theme.Colors.WarningBackground)
				});
				return;
			}

			ExpandableContent.Padding = new Thickness(left: 4.0f, right: 0.0f, top: 4.0f, bottom: 4.0f);

						list = (IList)EditorParams.PropertyInfo.GetValue(EditorParams.Objects.First());
			var addButton = new ThemedAddButton() {
				Clicked = () => {
					Expanded = true;
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
			ContainerWidget.AddChangeWatcher(() => list?.Count ?? 0, Build);
		}

		private void Build(int newCount)
		{
			if (list != null) {
				int prevCount = ExpandableContent.Nodes.Count;
				for (int i = 0; i < prevCount - newCount; i++) {
					ExpandableContent.Nodes.Last().UnlinkAndDispose();
				}
				for (int i = 0; i < newCount - prevCount; i++) {
					AfterInsertNewElement(prevCount + i);
				}
			}
		}

		private void AfterInsertNewElement(int index)
		{
			var p = new PropertyEditorParams(ExpandableContent, new[] { list }, EditorParams.RootObjects, EditorParams.PropertyInfo.PropertyType, "Item", EditorParams.PropertyPath + $".Item[{index}]"
			) {
				NumericEditBoxFactory = EditorParams.NumericEditBoxFactory,
				History = EditorParams.History,
				DefaultValueGetter = () => default,
				PropertySetter = (@object, name, value) => Core.Operations.SetIndexedProperty.Perform(@object, name, index, value),
				IndexInList = index,
				IsAnimableByPath = EditorParams.IsAnimableByPath && list is IAnimable,
				DisplayName = $"{index}:"
			};
			var editor = onAdd(p, ExpandableContent, list).ToList().First();

			var removeButton = new ThemedDeleteButton();
			Action removeClicked = () => {
				removeCallback = null;
				using (Document.Current.History.BeginTransaction()) {
					RemoveFromList.Perform(list, index);
					Document.Current.History.CommitTransaction();
				}
			};
			removeButton.Clicked += () => removeCallback = removeClicked;
			editor.ContainerWidget.AddNode(removeButton);
		}
	}
}
