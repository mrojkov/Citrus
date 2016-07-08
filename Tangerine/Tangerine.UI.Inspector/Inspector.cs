using System;
using Lime;
using System.Linq;
using Tangerine.Core;
using System.Collections.Generic;
using System.Reflection;
using ProtoBuf;

namespace Tangerine.UI.Inspector
{
	public delegate IPropertyEditor PropertyEditorBuilder(PropertyEditorContext context);

	public class Inspector
	{
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
			EditorMap.Add(typeof(Blending), c => new EnumPropertyEditor<Blending>(c));
			EditorMap.Add(typeof(Anchors), c => new EnumPropertyEditor<Anchors>(c));
			EditorMap.Add(typeof(RenderTarget), c => new EnumPropertyEditor<RenderTarget>(c));
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
	}
}
