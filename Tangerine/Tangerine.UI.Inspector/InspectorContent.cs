using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Yuzu;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Inspector
{
	class InspectorContent
	{
		private List<IPropertyEditor> editors;
		private Widget widget;

		public InspectorContent(Widget widget)
		{
			this.widget = widget;
			editors = new List<IPropertyEditor>();
		}

		public void BuildForObjects(IEnumerable<object> objects)
		{
			if (Widget.Focused != null && Widget.Focused.DescendantOf(widget)) {
				widget.SetFocus();
			}
			Clear();
			foreach (var t in GetTypes(objects)) {
				var o = objects.Where(i => t.IsInstanceOfType(i)).ToList();
				PopulateContentForType(t, o);
			}
		}

		public void Clear()
		{
			widget.Nodes.Clear();
			editors.Clear();
		}

		public void DropFiles(IEnumerable<string> files)
		{
			foreach (var e in editors) {
				e.DropFiles(files);
			}
		}

		private IEnumerable<Type> GetTypes(IEnumerable<object> objects)
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

		private void PopulateContentForType(Type type, List<object> objects)
		{
			var categoryLabelAdded = false;
			foreach (var property in type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)) {
				if (property.Name == "Item") {
					// WTF, Bug in Mono?
					continue;
				}
				var yuzuField = PropertyAttributes<YuzuField>.Get(type, property.Name);
				var tang = PropertyAttributes<TangerineKeyframeColorAttribute>.Get(type, property.Name);
				var tangIgnore = PropertyAttributes<TangerineIgnorePropertyAttribute>.Get(type, property.Name);
				if (yuzuField == null && tang == null || tangIgnore != null)
					continue;
				if (!categoryLabelAdded) {
					categoryLabelAdded = true;
					var text = type.Name;
					if (text == "Node" && objects.Count == 1) {
						text += $" of type '{objects[0].GetType().Name}'";
					}
					var label = new Widget {
						LayoutCell = new LayoutCell { StretchY = 0 },
						Layout = new StackLayout(),
						MinHeight = DesktopTheme.Metrics.DefaultButtonSize.Y,
						Nodes = {
							new SimpleText {
								Text = text,
								Padding = new Thickness(4, 0),
								VAlignment = VAlignment.Center,
								AutoSizeConstraints = false,
							}
						}
					};
					label.CompoundPresenter.Add(new WidgetFlatFillPresenter(ColorTheme.Current.Inspector.CategoryLabelBackground));
					widget.AddNode(label);
				}
				var context = new PropertyEditorParams(widget, objects, type, property.Name) {
					NumericEditBoxFactory = () => new TransactionalNumericEditBox(),
					PropertySetter = Core.Operations.SetAnimableProperty.Perform
				};
				foreach (var i in InspectorPropertyRegistry.Instance.Items) {
					if (i.Condition(context)) {
						var propertyEditor = i.Builder(context);
						if (propertyEditor != null) {
							DecoratePropertyEditor(propertyEditor);
							editors.Add(propertyEditor);
						}
						break;
					}
				}
			}
		}

		private void DecoratePropertyEditor(IPropertyEditor editor)
		{
			var ctr = editor.ContainerWidget;
			if (PropertyAttributes<TangerineStaticPropertyAttribute>.Get(editor.EditorParams.PropertyInfo) == null) {
				var keyFunctionButton = new KeyFunctionButton {
					LayoutCell = new LayoutCell(Alignment.LeftCenter, stretchX: 0),
				};
				var keyframeButton = new KeyframeButton {
					LayoutCell = new LayoutCell(Alignment.LeftCenter, stretchX: 0),
					KeyColor = KeyframePalette.Colors[editor.EditorParams.TangerineAttribute.ColorIndex],
				};
				keyFunctionButton.Clicked += editor.SetFocus;
				keyframeButton.Clicked += editor.SetFocus;
				ctr.Nodes.Insert(1, keyFunctionButton);
				ctr.Nodes.Insert(2, keyframeButton);
				ctr.Nodes.Insert(3, new HSpacer(4));
				ctr.Tasks.Add(new KeyframeButtonBinding(editor.EditorParams, keyframeButton));
				ctr.Tasks.Add(new KeyFunctionButtonBinding(editor.EditorParams, keyFunctionButton));
			} else {
				ctr.Nodes.Insert(1, new HSpacer(41));
			}
		}

		class TransactionalNumericEditBox : NumericEditBox
		{
			public TransactionalNumericEditBox()
			{
				Theme.Current.Apply(this, typeof(NumericEditBox));
				BeginSpin += () => Document.Current.History.BeginTransaction();
				EndSpin += () => Document.Current.History.EndTransaction();
			}
		}
	}
}