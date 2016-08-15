using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ProtoBuf;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Inspector
{
	class InspectorBuilder
	{
		Inspector Inspector => Inspector.Instance;

		public void Build(IEnumerable<object> objects)
		{
			Inspector.ScrollableWidget.Nodes.Clear();
			Inspector.Editors.Clear();
			foreach (var t in GetTypes(objects)) {
				var o = objects.Where(i => t.IsInstanceOfType(i)).ToList();
				PopulateContentForType(t, o);
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
					Inspector.ScrollableWidget.AddNode(label);
				}
				var context = new PropertyEditorContext(Inspector.ScrollableWidget, objects, type, property.Name);
				foreach (var i in Inspector.PropertyEditorRegistry) {
					if (i.Condition(context)) {
						var propertyEditor = i.Builder(context);
						Inspector.Editors.Add(propertyEditor);
						break;
					}
				}
			}
		}
	}
}