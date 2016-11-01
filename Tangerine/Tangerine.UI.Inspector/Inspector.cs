using System;
using Lime;
using System.Linq;
using Tangerine.Core;
using System.Collections.Generic;

namespace Tangerine.UI.Inspector
{
	public delegate IPropertyEditor PropertyEditorBuilder(PropertyEditorContext context);

	public class Inspector : IDocumentView
	{
		public static Inspector Instance { get; private set; }

		public readonly Widget PanelWidget;
		public readonly ScrollViewWidget RootWidget;
		public readonly Widget ContentWidget;
		public readonly Toolbar Toolbar;
		public readonly List<object> Objects;
		public readonly List<PropertyEditorRegistryItem> PropertyEditorRegistry;
		public readonly List<IPropertyEditor> Editors;
		public bool InspectRootNode { get; set; }

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
			RootWidget = new ScrollViewWidget();
			var toolbarArea = new Widget { Layout = new StackLayout(), Padding = new Thickness(4, 0) };
			ContentWidget = new Widget();
			RootWidget.Content.AddNode(toolbarArea);
			RootWidget.Content.AddNode(ContentWidget);
			RootWidget.Content.Layout = new VBoxLayout();
			Toolbar = new Toolbar(toolbarArea);
			ContentWidget.Layout = new VBoxLayout { Tag = "InspectorContent", Spacing = 4 };
			Objects = new List<object>();
			PropertyEditorRegistry = new List<PropertyEditorRegistryItem>();
			Editors = new List<IPropertyEditor>();
			RegisterEditors();
			RootWidget.Tasks.Add(RebuildInspectorWhenSelectedRowsChanged());
			SetupToolbar();
		}

		void SetupToolbar()
		{
			Toolbar.Add(new InspectRootNodeCommand());
		}

		private void RegisterEditors()
		{
			AddEditor(c => c.PropertyName == "ContentsPath", c => new ContentsPathPropertyEditor(c));
			AddEditor(typeof(Vector2), c => new Vector2PropertyEditor(c));
			AddEditor(typeof(NumericRange), c => new NumericRangePropertyEditor(c));
			AddEditor(c => c.PropertyName == "Text", c => new StringPropertyEditor(c, multiline: true));
			AddEditor(typeof(string), c => new StringPropertyEditor(c));
			AddEditor(typeof(float), c => new FloatPropertyEditor(c));
			AddEditor(typeof(bool), c => new BooleanPropertyEditor(c));
			AddEditor(typeof(int), c => new IntPropertyEditor(c));
			AddEditor(typeof(Color4), c => new Color4PropertyEditor(c));
			AddEditor(typeof(Blending), c => new EnumPropertyEditor<Blending>(c));
			AddEditor(typeof(ShaderId), c => new EnumPropertyEditor<ShaderId>(c));
			AddEditor(typeof(Anchors), c => new EnumPropertyEditor<Anchors>(c));
			AddEditor(typeof(RenderTarget), c => new EnumPropertyEditor<RenderTarget>(c));
			AddEditor(typeof(ITexture), c => new TexturePropertyEditor(c));
			AddEditor(typeof(SerializableSample), c => new AudioSamplePropertyEditor(c));
			AddEditor(typeof(HAlignment), c => new EnumPropertyEditor<HAlignment>(c));
			AddEditor(typeof(VAlignment), c => new EnumPropertyEditor<VAlignment>(c));
			AddEditor(typeof(AudioAction), c => new EnumPropertyEditor<AudioAction>(c));
			AddEditor(typeof(MovieAction), c => new EnumPropertyEditor<MovieAction>(c));
			AddEditor(typeof(EmitterShape), c => new EnumPropertyEditor<EmitterShape>(c));
			AddEditor(typeof(EmissionType), c => new EnumPropertyEditor<EmissionType>(c));
			AddEditor(typeof(TextOverflowMode), c => new EnumPropertyEditor<TextOverflowMode>(c));
		}

		private void AddEditor(Type type, PropertyEditorBuilder builder)
		{
			PropertyEditorRegistry.Add(new PropertyEditorRegistryItem(c => c.PropertyInfo.PropertyType == type, builder));
		}

		private void AddEditor(Func<PropertyEditorContext, bool> condition, PropertyEditorBuilder builder)
		{
			PropertyEditorRegistry.Add(new PropertyEditorRegistryItem(condition, builder));
		}

		ITaskProvider RebuildInspectorWhenSelectedRowsChanged()
		{
			var builder = new InspectorBuilder();
			return new Property<int>(() => Document.Current.SelectedRows.Version).WhenChanged(_ => builder.Build(Document.Current.SelectedNodes()));
		}

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
	}

	public class InspectRootNodeCommand : Command
	{
		public override string Text => "Root node properties";
		public override ITexture Icon => IconPool.GetTexture("Tools.Root");

		public override void Execute()
		{
			Inspector.Instance.InspectRootNode = !Inspector.Instance.InspectRootNode;
		}
	}
}
