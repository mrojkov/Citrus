using System;
using Lime;
using System.Linq;
using Tangerine.Core;
using System.Collections.Generic;
using System.Reflection;

namespace Tangerine.UI.Inspector
{
	public class Inspector
	{
		public delegate IPropertyEditor PropertyEditorBuilder(PropertyEditorContext context);

		public static Inspector Instance { get; private set; }

		public readonly Widget RootWidget;
		public readonly Frame ScrollViewWidget;
		public readonly Widget ContentWidget;
		public readonly List<object> Objects;
		public readonly Dictionary<Type, PropertyEditorBuilder> EditorMap;
		public readonly TaskList Tasks = new TaskList();
		public readonly List<IPropertyEditor> Editors;

		public static void Initialize(Widget rootWidget)
		{
			Instance = new Inspector(rootWidget);
		}

		private Inspector(Widget rootWidget)
		{
			RootWidget = rootWidget;
			ContentWidget = new ScrollView((Frame)RootWidget).Content;
			Objects = new List<object>();
			EditorMap = new Dictionary<Type, PropertyEditorBuilder>();
			Editors = new List<IPropertyEditor>();
			RegisterEditors();
			InitializeWidgets();
			CreateTasks();
			RootWidget.Updating += Update;
		}

		void InitializeWidgets()
		{
			RootWidget.Layout = new StackLayout { VerticallySizeable = true };
			ContentWidget.Layout = new VBoxLayout { Tag = "InspectorContent", Spacing = 4 };
			ContentWidget.Padding = new Thickness(4);
		}

		private void RegisterEditors()
		{
			EditorMap.Add(typeof(Vector2), c => new Vector2PropertyEditor(c));
			EditorMap.Add(typeof(string), c => new StringPropertyEditor(c));
		}

		void CreateTasks()
		{
			Tasks.Add(new UpdatePropertyGridProcessor());
		}

		void Update(float delta)
		{
			Tasks.Update(delta);
			Document.Current.History.Commit();
		}

		class UpdatePropertyGridProcessor : IProcessor
		{
			Inspector Inspector => Instance;

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
					if (PropertyRegistry.GetTangerineAttribute(type, property.Name) == null)
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
					if (!Inspector.EditorMap.TryGetValue(property.PropertyType, out editorBuilder)) {
						continue;
					}
					var context = new PropertyEditorContext(Inspector.ContentWidget, objects, type, property.Name);
					var propertyEditor = editorBuilder(context);
					Inspector.Editors.Add(propertyEditor);
				}
			}
		}
	}
}
