using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Operations;
using System.Collections;

namespace Tangerine.UI.Inspector
{
	public class InspectorContent
	{
		private readonly List<IPropertyEditor> editors;
		private readonly Widget widget;
		private int row = 1;
		private int totalObjectCount;
		public event Action<NodeComponent> OnComponentRemove;
		public DocumentHistory History { get; set; }
		public Widget Footer { get; set; }

		public InspectorContent(Widget widget)
		{
			this.widget = widget;
			editors = new List<IPropertyEditor>();
		}

		public void BuildForObjects(IEnumerable<object> objects)
		{
			totalObjectCount = objects.Count();
			if (Widget.Focused != null && Widget.Focused.DescendantOf(widget)) {
				widget.SetFocus();
			}
			Clear();
			BuildForObjectsHelper(objects).ToList();
			if (objects.Any() && objects.All(o => o is Node)) {
				var nodes = objects.Cast<Node>().ToList();
				foreach (var t in GetComponentsTypes(nodes)) {
					var components = new List<NodeComponent>();
					var nodesWithComponent = new List<Node>();
					foreach (var n in nodes) {
						var c = n.Components.Get(t);
						if (c != null && t.IsAssignableFrom(c.GetType())) {
							components.Add(c);
							nodesWithComponent.Add(n);
						}
					}
					PopulateContentForType(t, components, nodesWithComponent, widget, SerializeMutuallyExclusiveComponentGroupBaseType(t)).ToList();
				}
				AddComponentsMenu(nodes, widget);
			}

			if (Footer != null) {
				widget.AddNode(Footer);
			}
		}

		private string SerializeMutuallyExclusiveComponentGroupBaseType(Type t)
		{
			while (true) {
				var bt = t.BaseType;
				if (bt == typeof(NodeComponent)) {
					break;
				}
				t = bt;
				if (t?.GetCustomAttribute<MutuallyExclusiveDerivedComponentsAttribute>(true) != null) {
					break;
				}
			}
			return $"[{Yuzu.Util.TypeSerializer.Serialize(t)}]";
		}

		private IEnumerable<IPropertyEditor> BuildForObjectsHelper(IEnumerable<object> objects, IEnumerable<object> rootObjects = null, Widget widget = null, string propertyPath = "")
		{
			if (widget == null) {
				widget = this.widget;
			}
			if (objects.Any(o => o == null)) {
				yield break;
			}
			foreach (var t in GetTypes(objects)) {
				var o = objects.Where(i => t.IsInstanceOfType(i)).ToList();
				foreach (var e in PopulateContentForType(t, o, rootObjects ?? o, widget, propertyPath)) {
					yield return e;
				}
			}
		}

		private void Clear()
		{
			row = 1;
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
					if (t.IsSubclassOf(typeof(NodeComponent))) {
						break;
					}
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

		private bool ShouldInspectProperty(Type type, IEnumerable<object> objects, PropertyInfo property)
		{
			if (property.GetIndexParameters().Length > 0) {
				// we dont inspect indexers (they have "Item" name by default
				return false;
			}
			var yuzuItem = Yuzu.Metadata.Meta.Get(type, Serialization.YuzuCommonOptions).Items.Find(i => i.PropInfo == property);
			var tang = PropertyAttributes<TangerineKeyframeColorAttribute>.Get(type, property.Name);
			var tangIgnore = PropertyAttributes<TangerineIgnoreAttribute>.Get(type, property.Name);
			var tangInspect = PropertyAttributes<TangerineInspectAttribute>.Get(type, property.Name);
			if (tangInspect == null && (yuzuItem == null && tang == null || tangIgnore != null)) {
				return false;
			}
			if (type.IsSubclassOf(typeof(Node))) {
				// Root must be always visible
				if (Document.Current.InspectRootNode && property.Name == nameof(Widget.Visible)) {
					return false;
				}
				if (objects.Any(obj =>
					obj is Node node &&
					!string.IsNullOrEmpty(node.ContentsPath) &&
					obj is IExternalScenePropertyOverrideChecker checker &&
					!checker.IsPropertyOverridden(property)
				)) {
					return false;
				}
			}
			return true;
		}

		private IEnumerable<IPropertyEditor> PopulateContentForType(Type type, IEnumerable<object> objects, IEnumerable<object> rootObjects, Widget widget, string propertyPath)
		{
			var categoryLabelAdded = false;
			var editorParams = new Dictionary<string, List<PropertyEditorParams>>();
			bool isSubclassOfNode = type.IsSubclassOf(typeof(Node));
			bool isSubclassOfNodeComponent = type.IsSubclassOf(typeof(NodeComponent));
			if (isSubclassOfNodeComponent) {
				var label = CreateComponentLabel(type, objects.Cast<NodeComponent>());
				if (label != null) {
					widget.AddNode(label);
				}
			}
			var bindingFlags = BindingFlags.Instance | BindingFlags.Public;
			if (!isSubclassOfNodeComponent) {
				bindingFlags |= BindingFlags.DeclaredOnly;
			}
			foreach (var property in type.GetProperties(bindingFlags)) {
				if (!ShouldInspectProperty(type, objects, property)) {
					continue;
				}
				if (isSubclassOfNode && !categoryLabelAdded) {
					categoryLabelAdded = true;
					var text = type.Name;
					if (text == "Node" && !objects.Skip(1).Any()) {
						text += $" of type '{objects.First().GetType().Name}'";
					}
					if (totalObjectCount > 1) {
						text += $" ({objects.Count()}/{totalObjectCount})";
					}
					var label = CreateCategoryLabel(text, ColorTheme.Current.Inspector.CategoryLabelBackground);
					if (label != null) {
						widget.AddNode(label);
					}
				}
				var isAnimable = rootObjects.All(a => a is IAnimationHost) &&
				                 PropertyAttributes<TangerineStaticPropertyAttribute>.Get(property) == null &&
				                 AnimatorRegistry.Instance.Contains(property.PropertyType) &&
				                 !Document.Current.InspectRootNode;
				var @params = new PropertyEditorParams(widget, objects, rootObjects, type, property.Name,
					string.IsNullOrEmpty(propertyPath)
						? property.Name
						: propertyPath + "." + property.Name
				) {
					NumericEditBoxFactory = () => new TransactionalNumericEditBox(History),
					History = History,
					DefaultValueGetter = () => {
						var ctr = type.GetConstructor(new Type[] {});
						if (ctr == null) {
							return null;
						}
						var obj = ctr.Invoke(null);
						var prop = type.GetProperty(property.Name);
						return prop.GetValue(obj);
					},
					PropertySetter = isAnimable ? (PropertySetterDelegate)SetAnimableProperty : SetProperty
			};
				if (!editorParams.Keys.Contains(@params.Group)) {
					editorParams.Add(@params.Group, new List<PropertyEditorParams>());
				}
				editorParams[@params.Group].Add(@params);
			}

			foreach (var propertyEditor in PopulatePropertyEditors(type, objects, rootObjects, widget, editorParams)) {
				yield return propertyEditor;
			}
		}

		private IEnumerable<IPropertyEditor> PopulatePropertyEditors(Type type, IEnumerable<object> objects, IEnumerable<object> rootObjects, Widget widget, Dictionary<string, List<PropertyEditorParams>> editorParams)
		{
			foreach (var header in editorParams.Keys.OrderBy((s) => s)) {
				AddGroupHeader(header, widget);
				foreach (var param in editorParams[header]) {
					bool isPropertyRegistered = false;
					IPropertyEditor editor = null;
					foreach (var i in InspectorPropertyRegistry.Instance.Items) {
						if (i.Condition(param)) {
							isPropertyRegistered = true;
							editor = i.Builder(param);
							break;
						}
					}

					if (!isPropertyRegistered) {
						var propertyType = param.PropertyInfo.PropertyType;
						var iListInterface = propertyType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>));
						if (propertyType.IsEnum) {
							editor = CreateEditorForEnum(param);
						} else if (iListInterface != null) {
							editor = PopulateEditorsForListType(objects, rootObjects, param, iListInterface);
						} else if ((propertyType.IsClass || propertyType.IsInterface) && !propertyType.GetInterfaces().Contains(typeof(IEnumerable))) {
							editor = PopulateEditorsForInstanceType(objects, rootObjects, param);
						}
					}

					if (editor != null) {
						DecoratePropertyEditor(editor, row++);
						editors.Add(editor);
						var showCondition = PropertyAttributes<TangerineIgnoreIfAttribute>.Get(type, param.PropertyInfo.Name);
						if (showCondition != null) {
							editor.ContainerWidget.Updated += (delta) => {
								editor.ContainerWidget.Visible = !showCondition.Check(param.Objects.First());
							};
						}
						yield return editor;
					}
				}
			}
		}

		private IPropertyEditor PopulateEditorsForListType(IEnumerable<object> objects, IEnumerable<object> rootObjects, PropertyEditorParams param, Type iListInterface)
		{
			var listGenericArgument = iListInterface.GetGenericArguments().First();
			Action<PropertyEditorParams, Widget, IList> onAdd = (p, w, list) => {
				PopulatePropertyEditors(param.PropertyInfo.PropertyType, new[] {list}, rootObjects, w,
					new Dictionary<string, List<PropertyEditorParams>> {{"", new List<PropertyEditorParams> {p}}}).ToList();
			};
			var specializedICollectionPropertyEditorType = typeof(ListPropertyEditor<,>).MakeGenericType(param.PropertyInfo.PropertyType, listGenericArgument);
			var editor = Activator.CreateInstance(specializedICollectionPropertyEditorType, param, onAdd) as IPropertyEditor;
			return editor;
		}

		private IPropertyEditor PopulateEditorsForInstanceType(IEnumerable<object> objects, IEnumerable<object> rootObjects, PropertyEditorParams param)
		{
			var instanceEditors = new List<IPropertyEditor>();
			var onValueChanged = new Action<Widget>((w) => {
				w.Nodes.Clear();
				foreach (var e in instanceEditors) {
					editors.Remove(e);
				}

				instanceEditors.Clear();
				instanceEditors.AddRange(BuildForObjectsHelper(objects.Select(o => param.IndexProvider == null ? param.PropertyInfo.GetValue(o) : param.PropertyInfo.GetValue(o, new object[] {param.IndexProvider()})), rootObjects,
					w, param.PropertyPath));
			});
			Type et = typeof(InstancePropertyEditor<>).MakeGenericType(param.PropertyInfo.PropertyType);
			var editor = Activator.CreateInstance(et, param, onValueChanged) as IPropertyEditor;
			return editor;
		}

		private static IPropertyEditor CreateEditorForEnum(PropertyEditorParams param)
		{
			IPropertyEditor editor;
			var specializedEnumPropertyEditorType = typeof(EnumPropertyEditor<>).MakeGenericType(param.PropertyInfo.PropertyType);
			editor = Activator.CreateInstance(specializedEnumPropertyEditorType, param) as IPropertyEditor;
			return editor;
		}

		private static IEnumerable<Type> GetComponentsTypes(IReadOnlyList<Node> nodes)
		{
			var types = new List<Type>();
			foreach (var node in nodes) {
				foreach (var component in node.Components) {
					var type = component.GetType();
					if (type.IsDefined(typeof(TangerineRegisterComponentAttribute), true)) {
						if (!types.Contains(type)) {
							types.Add(type);
						}
					}
				}
			}
			return types;
		}

		private void AddComponentsMenu(IReadOnlyList<Node> nodes, Widget widget)
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
			types.Sort((a, b) => a.Name.CompareTo(b.Name));

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

		private void SetProperty(object obj, string propertyName, object value)
		{
			Core.Operations.SetProperty.Perform(obj, propertyName, value);
		}

		private void SetAnimableProperty(object obj, string propertyName, object value)
		{
			Core.Operations.SetAnimableProperty.Perform(obj, propertyName, value, CoreUserPreferences.Instance.AutoKeyframes);
		}

		private Widget CreateCategoryLabel(string text, Color4 color)
		{
			if (string.IsNullOrEmpty(text)) {
				return null;
			}
			var label = new Widget {
				LayoutCell = new LayoutCell { StretchY = 0 },
				Layout = new HBoxLayout(),
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
			label.CompoundPresenter.Add(new WidgetFlatFillPresenter(color));
			return label;
		}

		private Widget CreateComponentLabel(Type type, IEnumerable<NodeComponent> components)
		{
			var text = CamelCaseToLabel(type.Name);
			if (totalObjectCount > 1) {
				text += $"({components.Count()}/{totalObjectCount})";
			}
			var label = CreateCategoryLabel(text, ColorTheme.Current.Inspector.ComponentHeaderLabelBackground);
			label.Nodes.Insert(0, new ThemedDeleteButton {
				Clicked = () => RemoveComponents(components)
			});
			return label;
		}

		private void AddGroupHeader(string text, Widget widget)
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
			var index = 0;
			bool allRootObjectsAnimable = editor.EditorParams.RootObjects.All(a => a is IAnimationHost);
			if (
				allRootObjectsAnimable &&
				PropertyAttributes<TangerineStaticPropertyAttribute>.Get(editor.EditorParams.PropertyInfo) == null &&
				AnimatorRegistry.Instance.Contains(editor.EditorParams.PropertyInfo.PropertyType) &&
				!Document.Current.InspectRootNode
			) {
				var keyColor = KeyframePalette.Colors[editor.EditorParams.TangerineAttribute.ColorIndex];
				var keyframeButton = new KeyframeButton {
					LayoutCell = new LayoutCell(Alignment.LeftCenter, stretchX: 0),
					KeyColor = keyColor,
				};
				keyframeButton.Clicked += editor.PropertyLabel.SetFocus;
				editor.LabelContainer.Nodes.Insert(index++, keyframeButton);
				editor.ContainerWidget.Tasks.Add(new KeyframeButtonBinding(editor.EditorParams, keyframeButton));
			}
			editor.ContainerWidget.Padding = new Thickness { Left = 4, Top = 3, Right = 12, Bottom = 4 };
			editor.ContainerWidget.CompoundPresenter.Add(new WidgetFlatFillPresenter(
				row % 2 == 0 ?
				ColorTheme.Current.Inspector.StripeBackground1 :
				ColorTheme.Current.Inspector.StripeBackground2
			) { IgnorePadding = true });
			editor.ContainerWidget.Components.Add(new DocumentationComponent(editor.EditorParams.PropertyInfo.DeclaringType.Name + "." + editor.EditorParams.PropertyName));
		}

		private static string CamelCaseToLabel(string text)
		{
			return Regex.Replace(Regex.Replace(text, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
		}

		private static void CreateComponent(Type type, IEnumerable<Node> nodes)
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

		private void RemoveComponents(IEnumerable<NodeComponent> components)
		{
			using (Document.Current.History.BeginTransaction()) {
				foreach (var c in components) {
					if (c.Owner != null) {
						DeleteComponent.Perform(c.Owner, c);
					}
					OnComponentRemove?.Invoke(c);
				}
				Document.Current.History.CommitTransaction();
			}
		}

		public class TransactionalNumericEditBox : ThemedNumericEditBox
		{
			public TransactionalNumericEditBox(DocumentHistory history)
			{
				BeginSpin += () => history.BeginTransaction();
				EndSpin += () => {
					history.CommitTransaction();
					history.EndTransaction();
				};
				Submitted += s => {
					if (history.IsTransactionActive) {
						history.RollbackTransaction();
					}
				};
			}
		}
	}
}
