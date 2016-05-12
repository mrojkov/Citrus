using System;
using Lime;
using System.Linq;
using Tangerine.Core;
using System.Collections.Generic;

namespace Tangerine.UI.Inspector
{
	public interface IPropertyEditor
	{
		Type GetPropertyType();
		void CreateWidgets(Widget container, object @object, string property);
	}

	public class Inspector
	{
		public static Inspector Instance { get; private set; }

		public readonly KeyboardFocusController Focus;
		public readonly Widget RootWidget;
		public readonly Widget ContentWidget;
		public readonly List<Node> Nodes;
		public readonly Dictionary<Type, IPropertyEditor> EditorMap;
		public readonly TaskList Tasks = new TaskList();

		public static void Initialize(Widget rootWidget)
		{
			Instance = new Inspector(rootWidget);
		}

		private Inspector(Widget rootWidget)
		{
			RootWidget = rootWidget;
			ContentWidget = new Widget();
			Focus = new KeyboardFocusController(RootWidget);
			Nodes = new List<Node>();
			EditorMap = new Dictionary<Type, IPropertyEditor>();
			RegisterEditors();
			InitializeWidgets();
			CreateTasks();
			RootWidget.Updating += Update;
		}

		void InitializeWidgets()
		{
			ContentWidget.Layout = new TableLayout { Tag = "InspectorContent", Spacing = 4, ColCount = 2, RowCount = 6 };
			ContentWidget.Padding = new Thickness(4);
			RootWidget.Layout = new StackLayout();
			RootWidget.AddNode(ContentWidget);
		}

		private void RegisterEditors()
		{
			EditorMap.Add(typeof(Vector2), new Vector2Editor());
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

		class CommonPropertyEditor
		{
			public void CreateWidgets(Widget container, object @object, string property)
			{
				container.AddNode(new SimpleText { Text = property, Padding = new Thickness(8, 0), LayoutCell = new LayoutCell(Alignment.LeftCenter, 0.66f, 0) });
				CreateEditorWidgets(container, @object, property);
			}

			protected virtual void CreateEditorWidgets(Widget container, object @object, string property) { }
		}

		class Vector2Editor : CommonPropertyEditor, IPropertyEditor
		{
			public Type GetPropertyType() { return typeof(Vector2); }

			protected override void CreateEditorWidgets(Widget container, object @object, string property)
			{
				var prop = @object.GetType().GetProperty(property);
				var getter = prop.GetGetMethod();
				var editorX = new EditBox();
				var editorY = new EditBox();
				//new ValueBinder(editorX, (v, c) => v.X = c, v => v.X);
				Vector2? prevValue = null;
				editorX.Updating += delta => {
					var value = (Vector2)getter.Invoke(@object, null);
					if (!prevValue.HasValue || value != prevValue) {
						prevValue = value;
						editorX.Text = value.X.ToString();
						editorY.Text = value.Y.ToString();
					}
				};
				container.AddNode(new Widget {
					LayoutCell = new LayoutCell { StretchY = 0 },
					Layout = new HBoxLayout(),
					Nodes = {
						new SimpleText { Text = "X", Padding = new Thickness(4, 0), LayoutCell = new LayoutCell(Alignment.Center) },
						editorX,
						new SimpleText { Text = "Y", Padding = new Thickness(4, 0), LayoutCell = new LayoutCell(Alignment.Center) },
						editorY,
					}
				});
			}
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
					yield return null;
				}
			}

			private void RebuildContent()
			{
				Inspector.ContentWidget.Nodes.Clear();
				if (Inspector.Nodes.Count > 0) {
					PopulateContent(Inspector.Nodes[0]);
				}
			}

			void PopulateContent(object @object)
			{
				foreach (var prop in @object.GetType().GetProperties()) {
					var a = prop.GetCustomAttributes(typeof(TangerineAttribute), false);
					if (a.Length == 0) {
						continue;
					}
					IPropertyEditor e;
					if (!Inspector.EditorMap.TryGetValue(prop.PropertyType, out e)) {
						continue;
					}
					e.CreateWidgets(Inspector.ContentWidget, @object, prop.Name);
				}
			}
		}
	}
}
