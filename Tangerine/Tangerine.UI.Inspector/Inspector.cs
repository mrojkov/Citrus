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

		public readonly KeyboardFocusController Focus;
		public readonly Widget RootWidget;
		public readonly Frame ScrollViewWidget;
		public readonly Widget ContentWidget;
		public readonly List<Node> Nodes;
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
			// ScrollViewWidget = new Frame { Layout = new ScrollableLayout() };
			ContentWidget = new ScrollView((Frame)RootWidget).Content;
			Focus = new KeyboardFocusController(RootWidget);
			Nodes = new List<Node>();
			EditorMap = new Dictionary<Type, PropertyEditorBuilder>();
			Editors = new List<IPropertyEditor>();
			RegisterEditors();
			InitializeWidgets();
			CreateTasks();
			RootWidget.Updating += Update;
		}

		void InitializeWidgets()
		{
			RootWidget.Layout = new ScrollableLayout { ScrollDirection = ScrollDirection.Vertical };
			// RootWidget.AddNode(ScrollViewWidget);
			ContentWidget.Layout = new VBoxLayout { Tag = "InspectorContent", Spacing = 4 };
			ContentWidget.Padding = new Thickness(4);
			// RootWidget.Layout = new StackLayout();
		}

		private void RegisterEditors()
		{
			EditorMap.Add(typeof(Vector2), c => new Vector2PropertyEditor(c));
			EditorMap.Add(typeof(string), c => new StringPropertyEditor(c));
		}

		void CreateTasks()
		{
			Tasks.Add(new UpdatePropertyGridTask().Main());
		}

		void Update(float delta)
		{
			Tasks.Update(delta);
			Document.Current.History.Commit();
		}

		class UpdatePropertyGridTask
		{
			Inspector Inspector => Instance;

			public IEnumerator<object> Main()
			{
				var nodes = Inspector.Nodes;
				while (true) {
					var selectedNodes = Document.Current.SelectedNodes;
					if (!nodes.SequenceEqual(selectedNodes)) {
						nodes.Clear();
						nodes.AddRange(selectedNodes);
						RebuildContent();
					}
					foreach (var i in Inspector.Editors) {
						i.Update(Task.Current.Delta);
					}
					yield return null;
				}
			}

			void RebuildContent()
			{
				Inspector.ContentWidget.Nodes.Clear();
				if (Inspector.Nodes.Count > 0) {
					PopulateContent(Inspector.Nodes[0], Inspector.Nodes[0], null);
				}
			}

			void PopulateContent(Node node, IAnimable animable, string animationId)
			{
				Inspector.Editors.Clear();
				PopulateContentForType(animable.GetType(), node, animable, animationId);
			}

			void PopulateContentForType(Type type, Node node, IAnimable animable, string animationId)
			{
				if (type == typeof(object)) {
					return;
				}
				var categoryLabelAdded = false;
				PopulateContentForType(type.BaseType, node, animable, animationId);
				foreach (var prop in type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)) {
					if (prop.Name == "Item") {
						// WTF, Bug in Mono?
						continue;
					}
					if (PropertyRegistry.GetTangerineAttribute(type, prop.Name) == null)
						continue;
					if (!categoryLabelAdded) {
						categoryLabelAdded = true;
						var label = new SimpleText {
							Text = type.Name,
							AutoSizeConstraints = false,
							LayoutCell = new LayoutCell { StretchY = 0 }
						};
						label.CompoundPresenter.Add(new WidgetFlatFillPresenter(Colors.InspectorCategoryLabelBackground));
						Inspector.ContentWidget.AddNode(label);
					}
					PropertyEditorBuilder editorBuilder;
					if (!Inspector.EditorMap.TryGetValue(prop.PropertyType, out editorBuilder)) {
						continue;
					}
					var context = new PropertyEditorContext(Inspector.ContentWidget, node, animable, prop.Name, animationId);
					var propertyEditor = editorBuilder(context);
					Inspector.Editors.Add(propertyEditor);
				}
			}
		}
	}
}
