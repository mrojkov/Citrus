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

			public IEnumerator<object> MainLoop()
			{
				var objects = Inspector.Objects;
				while (true) {
					var selectedObjects = Document.Current.SelectedObjects;
					if (!objects.SequenceEqual(selectedObjects)) {
						objects.Clear();
						objects.AddRange(selectedObjects);
						RebuildContent((IAnimationContext)Document.Current);
					}
					foreach (var i in Inspector.Editors) {
						i.Update(Task.Current.Delta);
					}
					yield return null;
				}
			}

			void RebuildContent(IAnimationContext animationContext)
			{
				Inspector.ContentWidget.Nodes.Clear();
				foreach (var o in Inspector.Objects) {
					PopulateContent(o, animationContext);
				}
			}

			void PopulateContent(object @object, IAnimationContext animationContext)
			{
				Inspector.Editors.Clear();
				PopulateContentForType(@object.GetType(), @object, animationContext);
			}

			void PopulateContentForType(Type type, object @object, IAnimationContext animationContext)
			{
				if (type == typeof(object)) {
					return;
				}
				var categoryLabelAdded = false;
				PopulateContentForType(type.BaseType, @object, animationContext);
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
					var context = new PropertyEditorContext(Inspector.ContentWidget, @object, property.Name, animationContext);
					var propertyEditor = editorBuilder(context);
					Inspector.Editors.Add(propertyEditor);
				}
			}
		}
	}
}
