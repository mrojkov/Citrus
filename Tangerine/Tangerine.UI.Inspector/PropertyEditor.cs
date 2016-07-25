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
				Layout = new HBoxLayout { IgnoreHidden = false},
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
				KeyColor = KeyframePalette.Colors[context.TangerineAttribute.ColorIndex],
			};
			keyFunctionButton.Clicked += RaiseOnKeyframeToggle;
			keyframeButton.Clicked += RaiseOnKeyframeToggle;
			containerWidget.Nodes.AddRange(
				keyFunctionButton,
				keyframeButton,
				new HSpacer(4)
			);
			containerWidget.Tasks.Add(new KeyframeButtonBinding(context, keyframeButton));
			containerWidget.Tasks.Add(new KeyFunctionButtonBinding(context, keyFunctionButton));
		}

		void RaiseOnKeyframeToggle()
		{
			OnKeyframeToggle?.Invoke();
		}

		protected static IDataflowProvider<string> EditBoxSubmittedText(EditBox editBox)
		{
			return new EventflowProvider<string>(editBox, "Submitted");
		}

		protected static IDataflowProvider<T> DropDownListSelectedItem<T>(DropDownList dropDownList)
		{
			return new EventflowProvider<int>(dropDownList, "Changed").Select(i => (T)dropDownList.Items[i].Value);
		}

		protected static IDataflowProvider<bool> CheckBoxChecked(CheckBox checkBox)
		{
			return new EventflowProvider<bool>(checkBox, "Changed");
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
			float unused;
			return float.TryParse(value, out unused);
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
				var editedXWithY = EditBoxSubmittedText(editorX).Where(IsNumericString).Select(float.Parse).
					Coalesce(originalXY).Select(i => new Vector2(i.Item1, i.Item2.Y));
				var xWithEditedY = EditBoxSubmittedText(editorY).Where(IsNumericString).Select(float.Parse).
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
			var editor = new EditBox { LayoutCell = new LayoutCell(Alignment.Center) };
			containerWidget.AddNode(editor);
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
			var selector = new DropDownList { LayoutCell = new LayoutCell(Alignment.Center) };
			containerWidget.AddNode(selector);
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

	class BooleanPropertyEditor : CommonPropertyEditor
	{
		public BooleanPropertyEditor(PropertyEditorContext context) : base(context)
		{
			var checkBox = new CheckBox { LayoutCell = new LayoutCell(Alignment.Center) };
			containerWidget.AddNode(checkBox);
			OnKeyframeToggle += checkBox.SetFocus;
			var propType = context.PropertyInfo.PropertyType;
			foreach (var obj in context.Objects) {
				checkBox.Tasks.Add(new AnimablePropertyBinding<bool>(obj, context.PropertyName, CheckBoxChecked(checkBox)));
			}
			checkBox.Tasks.Add(new CheckBoxBinding(checkBox, CoalescedPropertyValue<bool>(context).DistinctUntilChanged()));
		}
	}

	class FloatPropertyEditor : CommonPropertyEditor
	{
		public FloatPropertyEditor(PropertyEditorContext context) : base(context)
		{
			var editor = new EditBox { LayoutCell = new LayoutCell(Alignment.Center) };
			containerWidget.AddNode(editor);
			OnKeyframeToggle += editor.SetFocus;
			foreach (var obj in context.Objects) {
				var editorValue = EditBoxSubmittedText(editor).Where(IsNumericString).Select(float.Parse);
				editor.Tasks.Add(new AnimablePropertyBinding<float>(obj, context.PropertyName, editorValue));
			}
			editor.Tasks.Add(new EditBoxBinding(editor, CoalescedPropertyValue<float>(context).DistinctUntilChanged().Select(i => i.ToString())));
		}
	}

	class Color4PropertyEditor : CommonPropertyEditor
	{
		public Color4PropertyEditor(PropertyEditorContext context) : base(context)
		{
			EditBox editor;
			var color = CoalescedPropertyValue<Color4>(context).DistinctUntilChanged();
			containerWidget.AddNode(new Widget {
				Layout = new HBoxLayout(),
				Nodes = {
					(editor = new EditBox { LayoutCell = new LayoutCell(Alignment.Center) }),
					new HSpacer(4),
					new ColorBoxButton(color) { LayoutCell = new LayoutCell(Alignment.Center) },
				}
			});
			OnKeyframeToggle += editor.SetFocus;
			foreach (var obj in context.Objects) {
				var editorValue = EditBoxSubmittedText(editor).Where(IsColorString).Select(Color4.Parse);
				editor.Tasks.Add(new AnimablePropertyBinding<Color4>(obj, context.PropertyName, editorValue));
			}
			editor.Tasks.Add(new EditBoxBinding(editor, color.Select(i => i.ToString(Color4.StringPresentation.Hex))));
		}

		private bool IsColorString(string value)
		{
			Color4 unused;
			return Color4.TryParse(value, out unused);
		}

		class ColorBoxButton : Button
		{
			public ColorBoxButton(IDataflowProvider<Color4> colorProvider)
			{
				Nodes.Clear();
				Size = MinMaxSize = new Vector2(25, DesktopTheme.Metrics.DefaultButtonSize.Y);
				var color = colorProvider.GetDataflow();
				PostPresenter = new DelegatePresenter<Widget>(widget => {
					widget.PrepareRendererState();
					Renderer.DrawRect(Vector2.Zero, widget.Size, Color4.White);
					color.Poll();
					var checkSize = new Vector2(widget.Width / 4, widget.Height / 3);
					for (int i = 0; i < 3; i++) {
						var checkPos = new Vector2(widget.Width / 2 + ((i == 1) ? widget.Width / 4 : 0), i * checkSize.Y);
						Renderer.DrawRect(checkPos, checkPos + checkSize, Color4.Black);
					}
					Renderer.DrawRect(Vector2.Zero, widget.Size, color.Value);
					Renderer.DrawRectOutline(Vector2.Zero, widget.Size, Colors.Inspector.BorderAroundKeyframeColorbox);
				});
			}
		}
	}
}