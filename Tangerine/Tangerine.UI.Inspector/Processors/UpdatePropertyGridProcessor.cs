using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ProtoBuf;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Inspector
{
	class UpdatePropertyGridProcessor : IProcessor
	{
		Inspector Inspector => Inspector.Instance;

		public IEnumerator<object> Loop()
		{
			var objects = Inspector.Objects;
			while (true) {
				var selectedObjects = Document.Current.SelectedObjects;
				if (!objects.SequenceEqual(selectedObjects)) {
					objects.Clear();
					objects.AddRange(selectedObjects);
					RebuildContent(selectedObjects);
				}
				yield return null;
			}
		}

		IEnumerable<Type> GetTypes(IEnumerable<object> objects)
		{
			var types = new List<Type>();
			foreach (var o in objects) {
				var inheritanceList = new List<Type>();
				for (var t = o.GetType(); t != typeof(object); t = t.BaseType) {
					inheritanceList.Add(t);
				}
				inheritanceList.Reverse();
				foreach (var t in inheritanceList) {
					if (!types.Contains(t)) {
						types.Add(t);
					}
				}
			}
			return types;
		}

		void RebuildContent(IEnumerable<object> objects)
		{
			Inspector.ContentWidget.Nodes.Clear();
			Inspector.Editors.Clear();
			foreach (var t in GetTypes(objects)) {
				var o = objects.Where(i => t.IsInstanceOfType(i)).ToList();
				PopulateContentForType(t, o);
			}
		}

		void PopulateContentForType(Type type, List<object> objects)
		{
			var categoryLabelAdded = false;
			foreach (var property in type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)) {
				if (property.Name == "Item") {
					// WTF, Bug in Mono?
					continue;
				}
				if (PropertyAttributes<ProtoMemberAttribute>.Get(type, property.Name) == null)
					continue;
				if (!categoryLabelAdded) {
					categoryLabelAdded = true;
					var label = new SimpleText {
						Text = type.Name,
						AutoSizeConstraints = false,
						LayoutCell = new LayoutCell { StretchY = 0 }
					};
					label.CompoundPresenter.Add(new WidgetFlatFillPresenter(Colors.Inspector.CategoryLabelBackground));
					Inspector.ContentWidget.AddNode(label);
				}
				PropertyEditorBuilder editorBuilder;
				var propType = property.PropertyType;
				if (!Inspector.EditorMap.TryGetValue(propType, out editorBuilder)) {
					continue;
				}
				var context = new PropertyEditorContext(Inspector.ContentWidget, objects, type, property.Name);
				var propertyEditor = editorBuilder(context);
				Inspector.Editors.Add(propertyEditor);
			}
		}
	}
}
