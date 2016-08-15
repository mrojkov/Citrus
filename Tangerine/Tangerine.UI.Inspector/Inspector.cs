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

	public class Inspector : IDocumentView
	{
		public class PropertyEditorRegistryItem
		{
			public readonly Func<PropertyEditorContext, bool> Condition;
			public readonly PropertyEditorBuilder Builder;

			public PropertyEditorRegistryItem(Func<PropertyEditorContext, bool> condition, PropertyEditorBuilder builder)
			{
				Condition = condition;
				Builder = builder;
			}
		}

		public static Inspector Instance { get; private set; }

		public readonly Widget PanelWidget;
		public readonly Frame RootWidget;
		public readonly Widget ScrollableWidget;
		public readonly List<object> Objects;
		public readonly List<PropertyEditorRegistryItem> PropertyEditorRegistry;
		public readonly List<IPropertyEditor> Editors;

		public TaskList Tasks => RootWidget.Tasks;

		public void Attach()
		{
			Instance = this;
			PanelWidget.PushNode(RootWidget);
		}

		public void Detach()
		{
			Instance = null;
			RootWidget.Unlink();
		}

		public Inspector(Widget panelWidget)
		{
			PanelWidget = panelWidget;
			RootWidget = new Frame();
			ScrollableWidget = new ScrollView((Frame)RootWidget).Content;
			Objects = new List<object>();
			PropertyEditorRegistry = new List<PropertyEditorRegistryItem>();
			Editors = new List<IPropertyEditor>();
			RegisterEditors();
			InitializeWidgets();
			CreateTasks();
		}

		void InitializeWidgets()
		{
			RootWidget.Layout = new StackLayout { VerticallySizeable = true };
			ScrollableWidget.Layout = new VBoxLayout { Tag = "InspectorContent", Spacing = 4 };
			ScrollableWidget.Padding = new Thickness(4);
		}

		private void RegisterEditors()
		{
			AddEditor(c => c.PropertyName == "ContentsPath", c => new ContentsPathPropertyEditor(c));
			AddEditor(typeof(Vector2), c => new Vector2PropertyEditor(c));
			AddEditor(typeof(string), c => new StringPropertyEditor(c));
			AddEditor(typeof(float), c => new FloatPropertyEditor(c));
			AddEditor(typeof(bool), c => new BooleanPropertyEditor(c));
			AddEditor(typeof(Color4), c => new Color4PropertyEditor(c));
			AddEditor(typeof(Blending), c => new EnumPropertyEditor<Blending>(c));
			AddEditor(typeof(Anchors), c => new EnumPropertyEditor<Anchors>(c));
			AddEditor(typeof(RenderTarget), c => new EnumPropertyEditor<RenderTarget>(c));
			AddEditor(typeof(ITexture), c => new TexturePropertyEditor<ITexture>(c));
		}

		private void AddEditor(Type type, PropertyEditorBuilder builder)
		{
			PropertyEditorRegistry.Add(new PropertyEditorRegistryItem(c => c.PropertyInfo.PropertyType == type, builder));
		}

		private void AddEditor(Func<PropertyEditorContext, bool> condition, PropertyEditorBuilder builder)
		{
			PropertyEditorRegistry.Add(new PropertyEditorRegistryItem(condition, builder));
		}

		void CreateTasks()
		{
			Tasks.Add(new UpdatePropertyGridProcessor());
		}
	}
}
