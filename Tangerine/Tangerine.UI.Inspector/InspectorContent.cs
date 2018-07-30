using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Yuzu;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Operations;

namespace Tangerine.UI.Inspector
{
	public class InspectorContent
	{
		private readonly List<IPropertyEditor> editors;
		private readonly Widget widget;

		public InspectorContent(Widget widget)
		{
			this.widget = widget;
			editors = new List<IPropertyEditor>();
		}

		public void BuildForObjects(IReadOnlyList<object> objects)
		{
			if (Widget.Focused != null && Widget.Focused.DescendantOf(widget)) {
				widget.SetFocus();
			}
			Clear();
			foreach (var t in GetTypes(objects)) {
				var o = objects.Where(i => t.IsInstanceOfType(i)).ToList();
				PopulateContentForType(t, o);
			}
			if (objects.Count > 0 && objects.All(o => o is Node)) {
				var nodes = objects.Cast<Node>().ToList();
				foreach (var t in GetComponentsTypes(nodes)) {
					PopulateContentForComponent(t, nodes);
				}
				AddComponentsMenu(nodes);
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

		private static IEnumerable<Type> GetTypes(IEnumerable<object> objects)
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
			var row = 0;
			var categoryLabelAdded = false;
			var editorParams = new Dictionary<string, List<PropertyEditorParams>>();
			foreach (var property in type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)) {
				if (property.Name == "Item") {
					// WTF, Bug in Mono?
					continue;
				}
				// Root must be always visible
				if (Document.Current.InspectRootNode && property.Name == nameof(Widget.Visible)) {
					continue;
				}
				var yuzuField = PropertyAttributes<YuzuField>.Get(type, property.Name);
				var tang = PropertyAttributes<TangerineKeyframeColorAttribute>.Get(type, property.Name);
				var tangIgnore = PropertyAttributes<TangerineIgnoreAttribute>.Get(type, property.Name);
				var tangInspect = PropertyAttributes<TangerineInspectAttribute>.Get(type, property.Name);
				if (tangInspect == null && (yuzuField == null && tang == null || tangIgnore != null)) {
					continue;
				}

				if (!categoryLabelAdded) {
					categoryLabelAdded = true;
					var text = type.Name;
					if (text == "Node" && objects.Count == 1) {
						text += $" of type '{objects[0].GetType().Name}'";
					}
					AddCategoryLabel(text);
				}
				var @params = new PropertyEditorParams(widget, objects, type, property.Name) {
					NumericEditBoxFactory = () => new TransactionalNumericEditBox(),
					History = Document.Current.History,
					PropertySetter = SetAnimableProperty,
					DefaultValueGetter = () => {
						var ctr = type.GetConstructor(new Type[] {});
						if (ctr == null) {
							return null;
						}
						var obj = ctr.Invoke(null);
						var prop = type.GetProperty(property.Name);
						return prop.GetValue(obj);
					}
				};

				if (!editorParams.Keys.Contains(@params.Group)) {
					editorParams.Add(@params.Group, new List<PropertyEditorParams>());
				}

				editorParams[@params.Group].Add(@params);
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

		private static IEnumerable<Type> GetComponentsTypes(IReadOnlyList<Node> nodes)
		{
			var types = new List<Type>();
			var node = nodes.FirstOrDefault();
			if (node != null) {
				foreach (var component in node.Components) {
					var type = component.GetType();
					if (type.IsDefined(typeof(TangerineRegisterComponentAttribute), true) && nodes.All(n => n.Components.Contains(type))) {
						types.Add(type);
					}
				}
			}
			return types;
		}

		private void PopulateContentForComponent(Type type, IReadOnlyList<Node> nodes)
		{
			var row = 0;
			var componentsAsObjects = nodes
				.Select(n => n.Components.Get(type))
				.Cast<object>()
				.ToList();
			var editorParams = new Dictionary<string, List<PropertyEditorParams>>();
			AddComponentLabel(type, nodes);
			foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
				if (property.Name == "Item") {
					// WTF, Bug in Mono?
					continue;
				}
				var yuzuField = PropertyAttributes<YuzuField>.Get(type, property.Name);
				var tang = PropertyAttributes<TangerineKeyframeColorAttribute>.Get(type, property.Name);
				var tangIgnore = PropertyAttributes<TangerineIgnoreAttribute>.Get(type, property.Name);
				var tangInspect = PropertyAttributes<TangerineInspectAttribute>.Get(type, property.Name);
				if (tangInspect == null && (yuzuField == null && tang == null || tangIgnore != null)) {
					continue;
				}

				var @params = new PropertyEditorParams(widget, componentsAsObjects, type, property.Name) {
					NumericEditBoxFactory = () => new TransactionalNumericEditBox(),
					History = Document.Current.History,
					PropertySetter = SetAnimableProperty,
					DefaultValueGetter = () => {
						var ctr = type.GetConstructor(new Type[] { });
						if (ctr == null) {
							return null;
						}
						var obj = ctr.Invoke(null);
						var prop = type.GetProperty(property.Name);
						return prop.GetValue(obj);
					}
				};

				if (!editorParams.Keys.Contains(@params.Group)) {
					editorParams.Add(@params.Group, new List<PropertyEditorParams>());
				}

				editorParams[@params.Group].Add(@params);
			}

			foreach (var header in editorParams.Keys.OrderBy((s) => s)) {
				AddGroupHeader(header);
				foreach (var param in editorParams[header]) {
					foreach (var i in InspectorPropertyRegistry.Instance.Items) {
						if (i.Condition(param)) {
							var propertyEditor = i.Builder(param);
							if (propertyEditor != null) {
								DecorateComponentPropertyEditor(propertyEditor, row++);
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

		private void AddComponentsMenu(IReadOnlyList<Node> nodes)
		{
			if (nodes.Any(n => !string.IsNullOrEmpty(n.ContentsPath))) {
				return;
			}

			var nodesTypes = nodes.Select(n => n.GetType()).ToList();
			var types = new List<Type>();
			foreach (var type in Project.Current.RegisteredComponentTypes) {
				if (
					!nodes.All(n => n.Components.Contains(type)) &&
					nodesTypes.All(t => NodeCompositionValidator.ValidateComponentType(t, type))
				) {
					types.Add(type);
				}
			}

			var label = new Widget {
				LayoutCell = new LayoutCell { StretchY = 0 },
				Layout = new HBoxLayout(),
				MinHeight = Theme.Metrics.DefaultButtonSize.Y,
				Nodes = {
					new ThemedAddButton {
						Clicked = () => {
							var menu = new Menu();
							foreach (var type in types) {
								ICommand command = new Command(CamelCaseToLabel(type.Name), () => CreateComponent(type, nodes));
								menu.Add(command);
							}
							menu.Popup();
						},
						Enabled = types.Count > 0
					},
					new ThemedSimpleText {
						Text = "Add Component",
						Padding = new Thickness(4, 0),
						VAlignment = VAlignment.Center,
						ForceUncutText = false,
					}
				}
			};
			label.CompoundPresenter.Add(new WidgetFlatFillPresenter(ColorTheme.Current.Inspector.CategoryLabelBackground));
			widget.AddNode(label);
		}

		private static void SetAnimableProperty(object obj, string propertyName, object value)
		{
			Core.Operations.SetAnimableProperty.Perform(obj, propertyName, value, CoreUserPreferences.Instance.AutoKeyframes);
		}

		private void AddCategoryLabel(string text)
		{
			if (string.IsNullOrEmpty(text)) {
				return;
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

		private void AddComponentLabel(Type type, IReadOnlyList<Node> nodes)
		{
			var label = new Widget {
				LayoutCell = new LayoutCell { StretchY = 0 },
				Layout = new HBoxLayout(),
				MinHeight = Theme.Metrics.DefaultButtonSize.Y,
				Nodes = {
					new ThemedDeleteButton {
						Clicked = () => RemoveComponent(type, nodes)
					},
					new ThemedSimpleText {
						Text = CamelCaseToLabel(type.Name),
						Padding = new Thickness(4, 0),
						VAlignment = VAlignment.Center,
						ForceUncutText = false,
					}
				}
			};
			label.CompoundPresenter.Add(new WidgetFlatFillPresenter(ColorTheme.Current.Inspector.CategoryLabelBackground));
			widget.AddNode(label);
		}

		private void AddGroupHeader(string text)
		{
			if (string.IsNullOrEmpty(text)) {
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

		private static void DecoratePropertyEditor(IPropertyEditor editor, int row)
		{
			var ctr = editor.ContainerWidget;
			if (!(editor is IExpandablePropertyEditor)) {
				ctr.Nodes.Insert(0, new HSpacer(20));
			}

			var index = ctr.Nodes.Count() - 1;
			if (
				PropertyAttributes<TangerineStaticPropertyAttribute>.Get(editor.EditorParams.PropertyInfo) == null &&
				AnimatorRegistry.Instance.Contains(editor.EditorParams.PropertyInfo.PropertyType)
			) {
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
			editor.ContainerWidget.Components.Add(new DocumentationComponent(editor.EditorParams.PropertyInfo.DeclaringType.Name + "." + editor.EditorParams.PropertyName));
		}

		private static void DecorateComponentPropertyEditor(IPropertyEditor editor, int row)
		{
			var ctr = editor.ContainerWidget;
			if (!(editor is IExpandablePropertyEditor)) {
				ctr.Nodes.Insert(0, new HSpacer(20));
			}

			ctr.Nodes.Insert(2, new HSpacer(42));
			editor.ContainerWidget.Padding = new Thickness { Left = 4, Top = 1, Right = 12, Bottom = 1 };
			editor.ContainerWidget.CompoundPresenter.Add(new WidgetFlatFillPresenter(
				row % 2 == 0 ?
					ColorTheme.Current.Inspector.StripeBackground1 :
					ColorTheme.Current.Inspector.StripeBackground2
			) { IgnorePadding = true });
		}

		private static string CamelCaseToLabel(string text)
		{
			return Regex.Replace(Regex.Replace(text, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
		}

		private static void CreateComponent(Type type, IReadOnlyList<Node> nodes)
		{
			var constructor = type.GetConstructor(Type.EmptyTypes);
			using (Document.Current.History.BeginTransaction()) {
				foreach (var node in nodes) {
					if (node.Components.Contains(type)) {
						continue;
					}

					var component = (NodeComponent)constructor.Invoke(new object[] { });
					SetComponent.Perform(node, component);
				}
				Document.Current.History.CommitTransaction();
			}
		}

		private static void RemoveComponent(Type type, IReadOnlyList<Node> nodes)
		{
			using (Document.Current.History.BeginTransaction()) {
				foreach (var node in nodes) {
					var component = node.Components.Get(type);
					if (component != null) {
						DeleteComponent.Perform(node, component);
					}
				}
				Document.Current.History.CommitTransaction();
			}
		}

		private class TransactionalNumericEditBox : ThemedNumericEditBox
		{
			public TransactionalNumericEditBox()
			{
				BeginSpin += () => Document.Current.History.BeginTransaction();
				EndSpin += () => {
					Document.Current.History.CommitTransaction();
					Document.Current.History.EndTransaction();
				};
				Submitted += s => {
					if (Document.Current.History.IsTransactionActive) {
						Document.Current.History.RollbackTransaction();
					}
				};
			}
		}
	}
}
