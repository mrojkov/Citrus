using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Operations;

namespace Tangerine.UI
{
	public class Ref<T>
	{
		public Ref(T value) { Value = value; }
		public T Value { get; set; }
		public static implicit operator T(Ref<T> wrapper) => wrapper.Value;
		public static implicit operator Ref<T>(T value) => new Ref<T>(value);
	}

	public interface IListPropertyEditor
	{

	}

	public class RemoveFromListPropertyEditor : RemoveFromList
	{
		private Action<Widget> onRemove;
		private Func<Widget> onInsert;
		private Widget widget;

		protected RemoveFromListPropertyEditor(IList collection, int index, Widget widget, Action<Widget> onRemove, Func<Widget> onInsert) : base(collection, index)
		{
			this.onRemove = onRemove;
			this.onInsert = onInsert;
			this.widget = widget;
		}

		public new static void Perform(IList collection, int index, Widget widget, Action<Widget> onRemove, Func<Widget> onInsert) =>
			DocumentHistory.Current.Perform(new RemoveFromListPropertyEditor(collection, index, widget, onRemove, onInsert));

		public new class Processor : OperationProcessor<RemoveFromListPropertyEditor>
		{
			protected override void InternalRedo(RemoveFromListPropertyEditor op)
			{
				op.onRemove.Invoke(op.widget);
			}

			protected override void InternalUndo(RemoveFromListPropertyEditor op)
			{
				op.widget = op.onInsert();
			}
		}
	}

	public class InsertIntoListPropertyEditor : InsertIntoList
	{
		private Action<Widget> onRemove;
		private Func<Widget> onInsert;
		private Widget widget;
		protected InsertIntoListPropertyEditor(IList collection, int index, object element, Action<Widget> onRemove, Func<Widget> onInsert) : base(collection, index, element)
		{
			this.onRemove = onRemove;
			this.onInsert = onInsert;
		}

		public static void Perform(IList collection, int index, object element, Action<Widget> onRemove, Func<Widget> onInsert) =>
			DocumentHistory.Current.Perform(new InsertIntoListPropertyEditor(collection, index, element, onRemove, onInsert));

		public new class Processor : OperationProcessor<InsertIntoListPropertyEditor>
		{
			protected override void InternalRedo(InsertIntoListPropertyEditor op)
			{
				op.widget = op.onInsert();
			}

			protected override void InternalUndo(InsertIntoListPropertyEditor op)
			{
				op.onRemove.Invoke(op.widget);
			}
		}
	}

	public class ListPropertyEditor<TList, TElement> : ExpandablePropertyEditor<TList>, IListPropertyEditor where TList : IList<TElement>, IList, new() where TElement : new()
	{
		private readonly List<Ref<int>> indexers = new List<Ref<int>>();
		private readonly Action<PropertyEditorParams, Widget, IList> onAdd;
		private IList list;

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
						InsertIntoListPropertyEditor.Perform(list, list.Count, newElement,
							(w) => RemoveEditorsAndUpdateIndexers(w, list.Count - 1),
							() => AfterInsertNewElement(list.Count - 1));
						Document.Current.History.CommitTransaction();
					}
				}
			};
			EditorContainer.AddNode(addButton);

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
			var indexRef = new Ref<int>(index);
			foreach (var indexer in indexers) {
				if (indexer >= indexRef) {
					indexer.Value = indexer.Value + 1;
				}
			}
			indexers.Add(indexRef);
			var p = new PropertyEditorParams(elementContainer, new[] { list }, new[] { list }, EditorParams.PropertyInfo.PropertyType, "Item", "Item"
			) {
				NumericEditBoxFactory = EditorParams.NumericEditBoxFactory,
				History = EditorParams.History,
				DefaultValueGetter = () => default,
				PropertySetter = (@object, name, value) => Core.Operations.SetIndexedProperty.Perform(@object, name, () => indexRef, value),
				IndexProvider = () => indexRef,
			};
			onAdd(p, elementContainer, list);

			var removeButton = new ThemedDeleteButton();
			removeButton.Clicked += () => {
				using (Document.Current.History.BeginTransaction()) {
					RemoveFromListPropertyEditor.Perform(list, indexRef, elementContainer,
						(w) => RemoveEditorsAndUpdateIndexers(w, indexRef),
						() => AfterInsertNewElement(indexRef));
					Document.Current.History.CommitTransaction();
				}
			};
			ExpandableContent.Nodes.Insert(index, elementContainer);
			elementContainer.AddNode(removeButton);
			return elementContainer;
		}

		private void RemoveEditorsAndUpdateIndexers(Widget elementContainer, int removedIndex)
		{
			elementContainer.UnlinkAndDispose();
			for (int i = indexers.Count - 1; i >= 0; i--) {
				var indexRef = indexers[i];
				if (indexRef == removedIndex) {
					indexers.RemoveAt(i);
				}
				if (indexRef > removedIndex) {
					indexRef.Value--;
				}
			}
		}
	}
}
