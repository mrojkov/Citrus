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

		private void Clear()
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
			int row = 0;
			var categoryLabelAdded = false;
			var editorParams = new Dictionary<string, List<PropertyEditorParams>>();
			foreach (var property in type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)) {
				if (property.Name == "Item") {
					// WTF, Bug in Mono?
					continue;
				}
				var yuzuField = PropertyAttributes<YuzuField>.Get(type, property.Name);
				var tang = PropertyAttributes<TangerineKeyframeColorAttribute>.Get(type, property.Name);
				var tangIgnore = PropertyAttributes<TangerineIgnoreAttribute>.Get(type, property.Name);
				var tangInspect = PropertyAttributes<TangerineInspectAttribute>.Get(type, property.Name);
				if (tangInspect == null && (yuzuField == null && tang == null || tangIgnore != null))
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
						MinHeight = Theme.Metrics.DefaultButtonSize.Y,
						Nodes = {
							new ThemedSimpleText {
								Text = text,
								Padding = new Thickness(4, 0),
								VAlignment = VAlignment.Center,
								ForceUncutText = false,
							}
						}
					};
					label.CompoundPresenter.Add(new WidgetFlatFillPresenter(ColorTheme.Current.Inspector.CategoryLabelBackground));
					widget.AddNode(label);
				}
				var context = new PropertyEditorParams(widget, objects, type, property.Name) {
					NumericEditBoxFactory = () => new TransactionalNumericEditBox(),
					PropertySetter = SetAnimableProperty,
					DefaultValueGetter = () => {
						var ctr = type.GetConstructor(new Type[] {});
						if (ctr != null) {
							var obj = ctr.Invoke(null);
							var prop = type.GetProperty(property.Name);
							return prop.GetValue(obj);
						}
						return null;
					}
				};

				if (!editorParams.Keys.Contains(context.Group)) {
					editorParams.Add(context.Group, new List<PropertyEditorParams>());
				}

				editorParams[context.Group].Add(context);
			}

			foreach (var header in editorParams.Keys.OrderBy((s) => s)) {
				AddGroupHeader(header);
				foreach (var param in editorParams[header]) {
					foreach (var i in InspectorPropertyRegistry.Instance.Items) {
						if (i.Condition(param)) {
							var propertyEditor = i.Builder(param);
							if (propertyEditor != null) {
								DecoratePropertyEditor(propertyEditor, row++);
								editors.Add(propertyEditor);

								var showCondition = PropertyAttributes<TangerineIgnoreIfAttribute>.Get(type, param.PropertyInfo.Name);
								if (showCondition != null) {
									propertyEditor.ContainerWidget.Updated += (delta) => {
										propertyEditor.ContainerWidget.Visible = !showCondition.Check(param.Objects[0]);
									};
								}
							}
							break;
						}
					}
				}
			}
		}

		private void SetAnimableProperty(object obj, string propertyName, object value)
		{
			Core.Operations.SetAnimableProperty.Perform(obj, propertyName, value, CoreUserPreferences.Instance.AutoKeyframes);
		}

		private void AddGroupHeader(string text)
		{
			if (String.IsNullOrEmpty(text)) {
				return;
			}

			var label = new Widget {
				LayoutCell = new LayoutCell { StretchY = 0 },
				Layout = new StackLayout(),
				MinHeight = Theme.Metrics.DefaultButtonSize.Y,
				Nodes = {
					new ThemedSimpleText {
						Text = text,
						Padding = new Thickness(12, 0),
						VAlignment = VAlignment.Center,
						ForceUncutText = false,
						FontHeight = 14
					}
				}
			};
			label.CompoundPresenter.Add(new WidgetFlatFillPresenter(ColorTheme.Current.Inspector.GroupHeaderLabelBackground));

			widget.AddNode(label);
		}

		private void DecoratePropertyEditor(IPropertyEditor editor, int row)
		{
			var ctr = editor.ContainerWidget;
			if (!(editor is IExpandablePropertyEditor)) {
				ctr.Nodes.Insert(0, new HSpacer(20));
			}

			var index = ctr.Nodes.Count() - 1;
			if (PropertyAttributes<TangerineStaticPropertyAttribute>.Get(editor.EditorParams.PropertyInfo) == null) {
				var keyFunctionButton = new KeyFunctionButton {
					LayoutCell = new LayoutCell(Alignment.LeftCenter, stretchX: 0),
				};
				var keyframeButton = new KeyframeButton {
					LayoutCell = new LayoutCell(Alignment.LeftCenter, stretchX: 0),
					KeyColor = KeyframePalette.Colors[editor.EditorParams.TangerineAttribute.ColorIndex],
				};
				keyFunctionButton.Clicked += editor.PropertyLabel.SetFocus;
				keyframeButton.Clicked += editor.PropertyLabel.SetFocus;
				ctr.Nodes.Insert(index++, keyFunctionButton);
				ctr.Nodes.Insert(index++, keyframeButton);
				ctr.Nodes.Insert(index, new HSpacer(4));
				ctr.Tasks.Add(new KeyframeButtonBinding(editor.EditorParams, keyframeButton));
				ctr.Tasks.Add(new KeyFunctionButtonBinding(editor.EditorParams, keyFunctionButton));
			} else {
				ctr.Nodes.Insert(2, new HSpacer(42));
			}
			editor.ContainerWidget.Padding = new Thickness { Left = 4, Top = 1, Right = 12, Bottom = 1 };
			editor.ContainerWidget.CompoundPresenter.Add(new WidgetFlatFillPresenter(
				row % 2 == 0 ?
				ColorTheme.Current.Inspector.StripeBackground1 :
				ColorTheme.Current.Inspector.StripeBackground2
			) { IgnorePadding = true });
		}

		class TransactionalNumericEditBox : ThemedNumericEditBox
		{
			public TransactionalNumericEditBox()
			{
				BeginSpin += () => Document.Current.History.BeginTransaction();
				EndSpin += () => Document.Current.History.EndTransaction();
			}
		}
	}
}