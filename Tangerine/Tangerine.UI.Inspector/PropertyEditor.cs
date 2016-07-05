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
			TangerineAttribute = PropertyRegistry.GetTangerineAttribute(Type, PropertyName) ?? new TangerineAttribute(0);
			PropertyInfo = Type.GetProperty(PropertyName);
		}
	}

	class CommonPropertyEditor : IPropertyEditor
	{
		readonly KeyframeButton keyframeButton;
		readonly KeyFunctionButton keyFunctionButton;

		protected readonly PropertyEditorContext context;
		protected readonly Widget containerWidget;

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
			containerWidget.AddNode(keyFunctionButton);
			containerWidget.AddNode(keyframeButton);
			containerWidget.Tasks.Add(new KeyframeButtonBinding(context, keyframeButton));
			containerWidget.Tasks.Add(new KeyFunctionButtonBinding(context, keyFunctionButton));
		}

		protected static IDataflowProvider<string> EditBoxCommittedText(EditBox editor)
		{
			return new Property<bool>(editor.IsFocused).DistinctUntilChanged().Skip(1).Where(focused => !focused).Select(_ => editor.Text);
		}

		protected static IDataflowProvider<T> CoalescedPropertyValue<T>(PropertyEditorContext context) where T: IEquatable<T>
		{
			IDataflowProvider<T> provider = null;
			foreach (var o in context.Objects) {
				var p = new Property<T>(o, context.PropertyName);
				provider = (provider == null) ? p : provider.SameOrDefault(p);
			}
			return provider;
		}

		protected static IDataflowProvider<T2> CoalescedPropertyValue<T1, T2>(PropertyEditorContext context, Func<T1, T2> selector) where T1: IEquatable<T1> where T2: IEquatable<T2>
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
			foreach (var obj in context.Objects) {
				var propValue = new Property<Vector2>(obj, context.PropertyName);
				var editorXValue = EditBoxCommittedText(editorX).Where(IsNumericString).Select(i => float.Parse(i)).
					Coalesce(propValue).Select(i => new Vector2(i.Item1, i.Item2.Y));
				var editorYValue = EditBoxCommittedText(editorY).Where(IsNumericString).Select(i => float.Parse(i)).
					Coalesce(propValue).Select(i => new Vector2(i.Item2.X, i.Item1));
				containerWidget.Tasks.Add(
					new AnimablePropertyBinding<Vector2>(obj, context.PropertyName, editorXValue),
					new AnimablePropertyBinding<Vector2>(obj, context.PropertyName, editorYValue)
				);
			}
			containerWidget.Tasks.Add(
				new EditBoxBinding(editorX, CoalescedPropertyValue<Vector2, float>(context, v => v.X).DistinctUntilChanged().Select(i => i.ToString())),
				new EditBoxBinding(editorY, CoalescedPropertyValue<Vector2, float>(context, v => v.Y).DistinctUntilChanged().Select(i => i.ToString()))
			);
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
			foreach (var obj in context.Objects) {
				var editorValue = EditBoxCommittedText(editor);
				containerWidget.Tasks.Add(new AnimablePropertyBinding<string>(obj, context.PropertyName, editorValue));
			}
			containerWidget.Tasks.Add(new EditBoxBinding(editor, CoalescedPropertyValue<string>(context).DistinctUntilChanged()));
		}
	}
}