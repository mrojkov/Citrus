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
		: ExpandablePropertyEditor<TList> where TList : IList<TElement>, IList
	{
		private readonly Func<PropertyEditorParams, Widget, IList, IEnumerable<IPropertyEditor>> onAdd;
		private IList list;
		private ThemedAddButton addButton;
		private Action removeCallback;
		private List<Button> buttons;

		public ListPropertyEditor(IPropertyEditorParams editorParams, Func<PropertyEditorParams, Widget, IList, IEnumerable<IPropertyEditor>> onAdd) : base(editorParams)
		{
			this.onAdd = onAdd;
			buttons = new List<Button>();
			if (EditorParams.Objects.Skip(1).Any()) {
				// Dont create editor interface if > 1 objects are selected
				EditorContainer.AddNode(new Widget() {
					Layout = new HBoxLayout(),
					Nodes = { new ThemedSimpleText { Text = "Edit of list properties isnt supported for multiple selection.", ForceUncutText = false } },
					Presenter = new WidgetFlatFillPresenter(Theme.Colors.WarningBackground)
				});
				return;
			}
			ExpandableContent.Padding = new Thickness(left: 4.0f, right: 0.0f, top: 4.0f, bottom: 4.0f);
			list = (IList)EditorParams.PropertyInfo.GetValue(EditorParams.Objects.First());
			addButton = new ThemedAddButton() {
				Clicked = () => {
					Expanded = true;
					if (list == null) {
						var pi = EditorParams.PropertyInfo;
						var o = EditorParams.Objects.First();
						pi.SetValue(o, list = Activator.CreateInstance<TList>());
					}
					var newElement = typeof(TElement) == typeof(string) ? (TElement)(object)string.Empty : Activator.CreateInstance<TElement>();
					using (Document.Current.History.BeginTransaction()) {
						int newIndex = list.Count;
						InsertIntoList.Perform(list, newIndex, newElement);
						Document.Current.History.CommitTransaction();
					}
				}
			};
			Expanded = true;
			EditorContainer.AddNode(addButton);
			ContainerWidget.Updating += _ => removeCallback?.Invoke();
			ContainerWidget.AddChangeWatcher(() => list?.Count ?? 0, Build);
		}

		private void Build(int newCount)
		{
			if (list != null) {
				for (int i = ExpandableContent.Nodes.Count - 1; i >= 0; i--) {
					ExpandableContent.Nodes[i].UnlinkAndDispose();
				}
				for (int i = 0; i < newCount; i++) {
					AfterInsertNewElement(i);
				}
			}
		}

		private void AfterInsertNewElement(int index)
		{
			var elementContainer = new Widget { Layout = new VBoxLayout() };
			var p = new PropertyEditorParams(
				elementContainer, new[] { list }, EditorParams.RootObjects,
				EditorParams.PropertyInfo.PropertyType, "Item", EditorParams.PropertyPath + $".Item[{index}]"
			) {
				NumericEditBoxFactory = EditorParams.NumericEditBoxFactory,
				History = EditorParams.History,
				DefaultValueGetter = () => default,
				IndexInList = index,
				IsAnimableByPath = EditorParams.IsAnimableByPath && list is IAnimable,
				DisplayName = $"{index}:"
			};
			p.PropertySetter = p.IsAnimable
				? (PropertySetterDelegate)((@object, name, value) =>
					SetAnimableProperty.Perform(@object, name, value, CoreUserPreferences.Instance.AutoKeyframes))
				: (@object, name, value) => SetIndexedProperty.Perform(@object, name, index, value);
			var editor = onAdd(p, elementContainer, list).ToList().First();
			ThemedDeleteButton removeButton;
			buttons.Add(removeButton = new ThemedDeleteButton {
				Enabled = Enabled
			});
			void RemoveClicked()
			{
				removeCallback = null;
				using (Document.Current.History.BeginTransaction()) {
					RemoveFromList.Perform(list, p.IndexInList);
					Document.Current.History.CommitTransaction();
					var i = ExpandableContent.Nodes.Count - 1;
					// We have to rebuild every list item with index greater than current item's
					// because there is no mechanism to update dataflows (in this case --
					// CoalescedPropertyValue with IndexedProperty underneath)
					buttons.RemoveAt(p.IndexInList);
				}
			}
			ExpandableContent.Nodes.Insert(index, elementContainer);
			removeButton.Clicked += () => removeCallback = RemoveClicked;
			editor.EditorContainer.AddNode(removeButton);
		}

		protected override void EnabledChanged()
		{
			base.EnabledChanged();
			buttons.ForEach(b => b.Enabled = Enabled);
			addButton.Enabled = Enabled;
		}
	}
}
