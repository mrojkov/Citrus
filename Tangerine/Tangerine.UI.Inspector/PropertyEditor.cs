using System;
using Lime;
using System.Linq;
using Tangerine.Core;
using System.Collections.Generic;

namespace Tangerine.UI.Inspector
{
	public interface IPropertyEditor { }

	public class PropertyEditorContext
	{
		public readonly Widget InspectorPane;
		public readonly List<object> Objects;
		public readonly Type Type;
		public readonly string PropertyName;
		public readonly TangerineAttribute TangerineAttribute;
		public readonly System.Reflection.PropertyInfo PropertyInfo;

		public PropertyEditorContext(Widget inspectorPane, List<object> objects, Type type, string propertyName)
		{
			InspectorPane = inspectorPane;
			Objects = objects;
			Type = type;
			PropertyName = propertyName;
			TangerineAttribute = PropertyAttributes<TangerineAttribute>.Get(Type, PropertyName) ?? new TangerineAttribute(0);
			PropertyInfo = Type.GetProperty(PropertyName);
		}
	}

	class CommonPropertyEditor : IPropertyEditor
	{
		readonly KeyframeButton keyframeButton;
		readonly KeyFunctionButton keyFunctionButton;

		protected readonly PropertyEditorContext context;
		protected readonly Widget containerWidget;

		public event Action OnKeyframeToggle;

		public CommonPropertyEditor(PropertyEditorContext context)
		{
			this.context = context;
			containerWidget = new Widget {
				Layout = new HBoxLayout { IgnoreHidden = false },
				LayoutCell = new LayoutCell { StretchY = 0 },
			};
			context.InspectorPane.AddNode(containerWidget);
			containerWidget.AddNode(new SimpleText {
				Text = context.PropertyName,
				LayoutCell = new LayoutCell(Alignment.LeftCenter, stretchX: 0.5f),
				AutoSizeConstraints = false,
			});
			keyFunctionButton = new KeyFunctionButton {
				LayoutCell = new LayoutCell(Alignment.LeftCenter, stretchX: 0),
			};
			keyframeButton = new KeyframeButton {
				LayoutCell = new LayoutCell(Alignment.LeftCenter, stretchX: 0),
				KeyColor = KeyframePalette.Colors[context.TangerineAttribute.ColorIndex]
			};
			keyFunctionButton.Clicked += RaiseOnKeyframeToggle;
			keyframeButton.Clicked += RaiseOnKeyframeToggle;
			containerWidget.AddNode(keyFunctionButton);
			containerWidget.AddNode(keyframeButton);
			containerWidget.Tasks.Add(new KeyframeButtonBinding(context, keyframeButton));
			containerWidget.Tasks.Add(new KeyFunctionButtonBinding(context, keyFunctionButton));
		}

		void RaiseOnKeyframeToggle()
		{
			OnKeyframeToggle?.Invoke();
		}

		protected static IDataflowProvider<string> EditBoxSubmittedText(EditBox editor)
		{
			return new EventflowProvider<string>(editor, "OnSubmit");
		}

		protected static IDataflowProvider<T> DropDownListSelectedItem<T>(DropDownList selector)
		{
			return new Property<T>(() => selector.Value != null ? (T)selector.Value : default(T)).DistinctUntilChanged().Skip(1);
		}

		protected static IDataflowProvider<T> CoalescedPropertyValue<T>(PropertyEditorContext context)
		{
			IDataflowProvider<T> provider = null;
			foreach (var o in context.Objects) {
				var p = new Property<T>(o, context.PropertyName);
				provider = (provider == null) ? p : provider.SameOrDefault(p);
			}
			return provider;
		}

		protected static IDataflowProvider<T2> CoalescedPropertyValue<T1, T2>(PropertyEditorContext context, Func<T1, T2> selector)
		{
			IDataflowProvider<T2> provider = null;
			foreach (var o in context.Objects) {
				var p = new Property<T1>(o, context.PropertyName).Select(selector);
				provider = (provider == null) ? p : provider.SameOrDefault(p);
			}
			return provider;
		}

		protected static bool IsNumericString(string value)
		{
			float temp;
			return float.TryParse(value, out temp);
		}
	}

	class Vector2PropertyEditor : CommonPropertyEditor
	{
		public Vector2PropertyEditor(PropertyEditorContext context) : base(context)
		{
			EditBox editorX, editorY;
			containerWidget.AddNode(new Widget {
				Layout = new HBoxLayout(),
				Nodes = {
					new SimpleText { Text = "X", Padding = new Thickness(4, 0), LayoutCell = new LayoutCell(Alignment.Center) },
					(editorX = new EditBox()),
					new SimpleText { Text = "Y", Padding = new Thickness(4, 0), LayoutCell = new LayoutCell(Alignment.Center) },
					(editorY = new EditBox()),
				}
			});
			OnKeyframeToggle += editorX.SetFocus;
			foreach (var obj in context.Objects) {
				var originalXY = new Property<Vector2>(obj, context.PropertyName);
				var editedXWithY = EditBoxSubmittedText(editorX).Where(IsNumericString).Select(i => float.Parse(i)).
					Coalesce(originalXY).Select(i => new Vector2(i.Item1, i.Item2.Y));
				var xWithEditedY = EditBoxSubmittedText(editorY).Where(IsNumericString).Select(i => float.Parse(i)).
					Coalesce(originalXY).Select(i => new Vector2(i.Item2.X, i.Item1));
				editorX.Tasks.Add(new AnimablePropertyBinding<Vector2>(obj, context.PropertyName, editedXWithY));
				editorY.Tasks.Add(new AnimablePropertyBinding<Vector2>(obj, context.PropertyName, xWithEditedY));
			}
			editorX.Tasks.Add(new EditBoxBinding(editorX, CoalescedPropertyValue<Vector2, float>(context, v => v.X).DistinctUntilChanged().Select(i => i.ToString())));
			editorY.Tasks.Add(new EditBoxBinding(editorY, CoalescedPropertyValue<Vector2, float>(context, v => v.Y).DistinctUntilChanged().Select(i => i.ToString())));
		}
	}

	class StringPropertyEditor : CommonPropertyEditor
	{
		public StringPropertyEditor(PropertyEditorContext context) : base(context)
		{
			EditBox editor;
			containerWidget.AddNode(new Widget {
				Layout = new HBoxLayout(),
				Padding = new Thickness { Left = 4 },
				Nodes = {
					(editor = new EditBox())
				}
			});
			OnKeyframeToggle += editor.SetFocus;
			foreach (var obj in context.Objects) {
				var editorValue = EditBoxSubmittedText(editor);
				editor.Tasks.Add(new AnimablePropertyBinding<string>(obj, context.PropertyName, editorValue));
			}
			editor.Tasks.Add(new EditBoxBinding(editor, CoalescedPropertyValue<string>(context).DistinctUntilChanged()));
		}
	}

	class EnumPropertyEditor<T> : CommonPropertyEditor
	{
		public EnumPropertyEditor(PropertyEditorContext context) : base(context)
		{
			DropDownList selector;
			containerWidget.AddNode(new Widget {
				Layout = new HBoxLayout(),
				Padding = new Thickness { Left = 4 },
				Nodes = {
					(selector = new DropDownList())
				}
			});
			OnKeyframeToggle += selector.SetFocus;
			var propType = context.PropertyInfo.PropertyType;
			foreach (var i in Enum.GetNames(propType).Zip(Enum.GetValues(propType).Cast<object>(), (a, b) => new DropDownList.Item(a, b))) {
				selector.Items.Add(i);
			}
			foreach (var obj in context.Objects) {
				var pickedValue = DropDownListSelectedItem<T>(selector);
				selector.Tasks.Add(new AnimablePropertyBinding<T>(obj, context.PropertyName, pickedValue));
			}
			selector.Tasks.Add(new DropDownListBinding<T>(selector, CoalescedPropertyValue<T>(context).DistinctUntilChanged()));
		}
	}
}