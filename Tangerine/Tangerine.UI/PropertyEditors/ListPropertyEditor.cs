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
		// Terekhov Dmitry: We have to store list of PropertyEditorParams in order to
		// Terekhov Dmitry: avoid nodes recreation (e.g. for multiple expandable contents)
		private readonly List<PropertyEditorParams> propertyEditorParamsList = new List<PropertyEditorParams>();

		public ListPropertyEditor(IPropertyEditorParams editorParams, Func<PropertyEditorParams, Widget, IList, IEnumerable<IPropertyEditor>> onAdd) : base(editorParams)
		{
			this.onAdd = onAdd;
			buttons = new List<Button>();
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
				int prevCount = ExpandableContent.Nodes.Count;
				for (int i = 0; i < newCount - prevCount; i++) {
					AfterInsertNewElement(prevCount + i);
				}
			}
		}

		private void AdjustEditors(int startFrom)
		{
			propertyEditorParamsList.RemoveAt(startFrom);
			for (int index = startFrom; index < propertyEditorParamsList.Count; ++index) {
				var closureIndex = index;
				propertyEditorParamsList[index].IndexInList = index;
				propertyEditorParamsList[index].DisplayName = $"{index}:";
				propertyEditorParamsList[index].PropertySetter = propertyEditorParamsList[index].IsAnimable
				? (PropertySetterDelegate)((@object, name, value) =>
						SetAnimableProperty.Perform(@object, name, value, CoreUserPreferences.Instance.AutoKeyframes))
				: (@object, name, value) => SetIndexedProperty.Perform(@object, name, closureIndex, value);
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
			propertyEditorParamsList.Insert(index, p);
			void RemoveClicked()
			{
				removeCallback = null;
				using (Document.Current.History.BeginTransaction()) {
					RemoveFromList.Perform(list, p.IndexInList);
					Document.Current.History.CommitTransaction();
					ExpandableContent.Nodes[p.IndexInList].UnlinkAndDispose();
					buttons.RemoveAt(p.IndexInList);
					AdjustEditors(p.IndexInList);
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
