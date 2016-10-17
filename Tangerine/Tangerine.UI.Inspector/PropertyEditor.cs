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
				Padding = new Thickness { Left = 4, Right = 12 },
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

		protected static IDataflowProvider<T> CoalescedPropertyValue<T>(PropertyEditorContext context, T defaultValue = default(T))
		{
			IDataflowProvider<T> provider = null;
			foreach (var o in context.Objects) {
				var p = new Property<T>(o, context.PropertyName);
				provider = (provider == null) ? p : provider.SameOrDefault(p, defaultValue);
			}
			return provider;
		}

		protected static IDataflowProvider<T2> CoalescedPropertyValue<T1, T2>(PropertyEditorContext context, Func<T1, T2> selector, T2 defaultValue = default(T2))
		{
			IDataflowProvider<T2> provider = null;
			foreach (var o in context.Objects) {
				var p = new Property<T1>(o, context.PropertyName).Select(selector);
				provider = (provider == null) ? p : provider.SameOrDefault(p, defaultValue);
			}
			return provider;
		}
	}

	class Vector2PropertyEditor : CommonPropertyEditor
	{
		public Vector2PropertyEditor(PropertyEditorContext context) : base(context)
		{
			EditBox editorX, editorY;
			containerWidget.AddNode(new Widget {
				Layout = new HBoxLayout { CellDefaults = new LayoutCell(Alignment.Center), Spacing = 4 },
				Nodes = {
					(editorX = new EditBox()),
					(editorY = new EditBox()),
				}
			});
			OnKeyframeToggle += editorX.SetFocus;
			var currentX = CoalescedPropertyValue<Vector2, float>(context, v => v.X);
			var currentY = CoalescedPropertyValue<Vector2, float>(context, v => v.Y);
			editorX.TextWidget.HAlignment = HAlignment.Right;
			editorY.TextWidget.HAlignment = HAlignment.Right;
			editorX.Submitted += text => SetComponent(context, 0, editorX, currentX.GetValue());
			editorY.Submitted += text => SetComponent(context, 1, editorY, currentY.GetValue());
			editorX.Tasks.Add(currentX.DistinctUntilChanged().Consume(v => editorX.Text = v.ToString()));
			editorY.Tasks.Add(currentY.DistinctUntilChanged().Consume(v => editorY.Text = v.ToString()));
		}

		void SetComponent(PropertyEditorContext context, int component, EditBox editor, float currentValue)
		{
			float newValue;
			if (float.TryParse(editor.Text, out newValue)) {
				foreach (var obj in context.Objects) {
					var current = new Property<Vector2>(obj, context.PropertyName).Value;
					current[component] = newValue;
					Core.Operations.SetAnimableProperty.Perform(obj, context.PropertyName, current);
				}
			} else {
				editor.Text = currentValue.ToString();
			}
		}
	}

	class StringPropertyEditor : CommonPropertyEditor
	{
		public StringPropertyEditor(PropertyEditorContext context) : base(context)
		{
			var editor = new EditBox { LayoutCell = new LayoutCell(Alignment.Center) };
			containerWidget.AddNode(editor);
			OnKeyframeToggle += editor.SetFocus;
			editor.Submitted += text => {
				Core.Operations.SetAnimableProperty.Perform(context.Objects, context.PropertyName, text);
			};
			editor.Tasks.Add(CoalescedPropertyValue<string>(context).DistinctUntilChanged().Consume(v => editor.Text = v));
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
			selector.Changed += index => {
				Core.Operations.SetAnimableProperty.Perform(context.Objects, context.PropertyName, (T)selector.Items[index].Value);
			};
			selector.Tasks.Add(CoalescedPropertyValue<T>(context).DistinctUntilChanged().Consume(v => selector.Value = v));
		}
	}

	class BooleanPropertyEditor : CommonPropertyEditor
	{
		public BooleanPropertyEditor(PropertyEditorContext context) : base(context)
		{
			var checkBox = new CheckBox { LayoutCell = new LayoutCell(Alignment.Center) };
			containerWidget.AddNode(checkBox);
			OnKeyframeToggle += checkBox.SetFocus;
			checkBox.Changed += value => {
				Core.Operations.SetAnimableProperty.Perform(context.Objects, context.PropertyName, value);
			};
			checkBox.Tasks.Add(CoalescedPropertyValue<bool>(context).DistinctUntilChanged().Consume(v => checkBox.Checked = v));
		}
	}

	class FloatPropertyEditor : CommonPropertyEditor
	{
		public FloatPropertyEditor(PropertyEditorContext context) : base(context)
		{
			var editor = new EditBox { LayoutCell = new LayoutCell(Alignment.Center) };
			editor.TextWidget.HAlignment = HAlignment.Right;
			containerWidget.AddNode(editor);
			OnKeyframeToggle += editor.SetFocus;
			var current = CoalescedPropertyValue<float>(context);
			editor.Submitted += text => {
				float newValue;
				if (float.TryParse(text, out newValue)) {
					Core.Operations.SetAnimableProperty.Perform(context.Objects, context.PropertyName, newValue);
				} else {
					editor.Text = current.GetValue().ToString();
				}
			};
			editor.Tasks.Add(current.DistinctUntilChanged().Consume(v => editor.Text = v.ToString()));
		}
	}

	class Color4PropertyEditor : CommonPropertyEditor
	{
		public Color4PropertyEditor(PropertyEditorContext context) : base(context)
		{
			EditBox editor;
			ColorBoxButton colorBox;
			var currentColor = CoalescedPropertyValue(context, Color4.White).DistinctUntilChanged();
			containerWidget.AddNode(new Widget {
				Layout = new HBoxLayout { CellDefaults = new LayoutCell(Alignment.Center) },
				Nodes = {
					(editor = new EditBox()),
					new HSpacer(4),
					(colorBox = new ColorBoxButton(currentColor)),
				}
			});
			colorBox.Clicked += () => {
				var dlg = new ColorPickerDialog(currentColor.GetValue());
				if (dlg.Show()) {
					Core.Operations.SetAnimableProperty.Perform(context.Objects, context.PropertyName, dlg.Color);
				}
			};
			var currentColorString = currentColor.Select(i => i.ToString(Color4.StringPresentation.Dec));
			OnKeyframeToggle += editor.SetFocus;
			editor.Submitted += text => {
				Color4 newColor;
				if (Color4.TryParse(text, out newColor)) {
					Core.Operations.SetAnimableProperty.Perform(context.Objects, context.PropertyName, newColor);
				} else {
					editor.Text = currentColorString.GetValue();
				}
			};
			editor.Tasks.Add(currentColorString.Consume(v => editor.Text = v));
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
					Renderer.DrawRectOutline(Vector2.Zero, widget.Size, InspectorColors.BorderAroundKeyframeColorbox);
				});
			}
		}
	}

	abstract class FilePropertyEditor : CommonPropertyEditor
	{
		protected readonly EditBox editor;
		protected readonly Button button;

		protected FilePropertyEditor(PropertyEditorContext context, string[] allowedFileTypes) : base(context)
		{
			containerWidget.AddNode(new Widget {
				Layout = new HBoxLayout(),
				Nodes = {
					(editor = new EditBox { LayoutCell = new LayoutCell(Alignment.Center) }),
					new HSpacer(4),
					(button = new Button {
						Text = "...",
						MinMaxWidth = 20,
						Draggable = true,
						LayoutCell = new LayoutCell(Alignment.Center)
					})
				}
			});
			OnKeyframeToggle += editor.SetFocus;
			button.Clicked += () => {
				var dlg = new FileDialog { AllowedFileTypes = allowedFileTypes, Mode = FileDialogMode.Open };
				if (dlg.RunModal()) {
					if (!dlg.FileName.StartsWith(Project.Current.AssetsDirectory)) {
						var alert = new AlertDialog("Tangerine", "Can't open an assset outside the project assets directory", "Ok");
						alert.Show();
					} else {
						var path = System.IO.Path.ChangeExtension(dlg.FileName.Substring(Project.Current.AssetsDirectory.Length + 1), null);
						SetFilePath(path);
					}
				}
			};
		}

		protected abstract void SetFilePath(string path);
	}

	class TexturePropertyEditor<T> : FilePropertyEditor
	{
		public TexturePropertyEditor(PropertyEditorContext context) : base(context, new string[] { "png" })
		{
			editor.Submitted += text => {
				Core.Operations.SetAnimableProperty.Perform(context.Objects, context.PropertyName, new SerializableTexture(text));
			};
			editor.Tasks.Add(CoalescedPropertyValue<ITexture>(context).DistinctUntilChanged().Select(i => i != null ? i.SerializationPath : "").Consume(v => editor.Text = v));
		}

		protected override void SetFilePath(string path)
		{
			foreach (var obj in context.Objects) {
				Core.Operations.SetAnimableProperty.Perform(obj, context.PropertyName, new SerializableTexture(path));
			}
		}
	}

	class ContentsPathPropertyEditor : FilePropertyEditor
	{
		public ContentsPathPropertyEditor(PropertyEditorContext context) : base(context, new string[] { Document.SceneFileExtension })
		{
			editor.Submitted += text => {
				Core.Operations.SetAnimableProperty.Perform(context.Objects, context.PropertyName, text);
			};
			editor.Tasks.Add(CoalescedPropertyValue<string>(context).DistinctUntilChanged().Consume(v => editor.Text = v));
		}

		protected override void SetFilePath(string path)
		{
			foreach (var obj in context.Objects) {
				Core.Operations.SetAnimableProperty.Perform(obj, context.PropertyName, path);
			}
		}
	}
}