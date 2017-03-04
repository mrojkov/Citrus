using System;
using System.IO;
using System.Linq;
using Lime;
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
		public readonly TangerineKeyframeColorAttribute TangerineAttribute;
		public readonly System.Reflection.PropertyInfo PropertyInfo;

		public PropertyEditorContext(Widget inspectorPane, List<object> objects, Type type, string propertyName)
		{
			InspectorPane = inspectorPane;
			Objects = objects;
			Type = type;
			PropertyName = propertyName;
			TangerineAttribute = PropertyAttributes<TangerineKeyframeColorAttribute>.Get(Type, PropertyName) ?? new TangerineKeyframeColorAttribute(0);
			PropertyInfo = Type.GetProperty(PropertyName);
		}
	}

	class CommonPropertyEditor : IPropertyEditor
	{
		readonly KeyframeButton keyframeButton;
		readonly KeyFunctionButton keyFunctionButton;

		protected readonly PropertyEditorContext context;
		protected readonly Widget containerWidget;
		protected readonly SimpleText propertyNameLabel;

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
			propertyNameLabel = new SimpleText {
				Text = context.PropertyName,
				VAlignment = VAlignment.Center,
				LayoutCell = new LayoutCell(Alignment.LeftCenter, stretchX: 0.5f),
				AutoSizeConstraints = false,
			};
			containerWidget.AddNode(propertyNameLabel);
			if (PropertyAttributes<TangerineStaticPropertyAttribute>.Get(context.PropertyInfo) == null) {
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
			} else {
				containerWidget.Nodes.Add(new HSpacer(41));
			}
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
			editorX.AddChangeWatcher(currentX, v => editorX.Text = v.ToString());
			editorY.AddChangeWatcher(currentY, v => editorY.Text = v.ToString());
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

	class Vector3PropertyEditor : CommonPropertyEditor
	{
		public Vector3PropertyEditor(PropertyEditorContext context) : base(context)
		{
			EditBox editorX, editorY, editorZ;
			containerWidget.AddNode(new Widget {
				Layout = new HBoxLayout { CellDefaults = new LayoutCell(Alignment.Center), Spacing = 4 },
				Nodes = {
					(editorX = new EditBox()),
					(editorY = new EditBox()),
					(editorZ = new EditBox())
				}
			});
			OnKeyframeToggle += editorX.SetFocus;
			var currentX = CoalescedPropertyValue<Vector3, float>(context, v => v.X);
			var currentY = CoalescedPropertyValue<Vector3, float>(context, v => v.Y);
			var currentZ = CoalescedPropertyValue<Vector3, float>(context, v => v.Z);
			editorX.TextWidget.HAlignment = HAlignment.Right;
			editorY.TextWidget.HAlignment = HAlignment.Right;
			editorZ.TextWidget.HAlignment = HAlignment.Right;
			editorX.Submitted += text => SetComponent(context, 0, editorX, currentX.GetValue());
			editorY.Submitted += text => SetComponent(context, 1, editorY, currentY.GetValue());
			editorZ.Submitted += text => SetComponent(context, 2, editorZ, currentZ.GetValue());
			editorX.AddChangeWatcher(currentX, v => editorX.Text = v.ToString());
			editorY.AddChangeWatcher(currentY, v => editorY.Text = v.ToString());
			editorZ.AddChangeWatcher(currentZ, v => editorZ.Text = v.ToString());
		}

		void SetComponent(PropertyEditorContext context, int component, EditBox editor, float currentValue)
		{
			float newValue;
			if (float.TryParse(editor.Text, out newValue)) {
				foreach (var obj in context.Objects) {
					var current = new Property<Vector3>(obj, context.PropertyName).Value;
					current[component] = newValue;
					Core.Operations.SetAnimableProperty.Perform(obj, context.PropertyName, current);
				}
			} else {
				editor.Text = currentValue.ToString();
			}
		}
	}

	class QuaternionPropertyEditor : CommonPropertyEditor
	{
		public QuaternionPropertyEditor(PropertyEditorContext context) : base(context)
		{
			EditBox editorX, editorY, editorZ;
			containerWidget.AddNode(new Widget {
				Layout = new HBoxLayout { CellDefaults = new LayoutCell(Alignment.Center), Spacing = 4 },
				Nodes = {
					(editorX = new EditBox()),
					(editorY = new EditBox()),
					(editorZ = new EditBox())
				}
			});

			OnKeyframeToggle += editorX.SetFocus;
			var current = CoalescedPropertyValue<Quaternion>(context);
			editorX.TextWidget.HAlignment = HAlignment.Right;
			editorY.TextWidget.HAlignment = HAlignment.Right;
			editorZ.TextWidget.HAlignment = HAlignment.Right;
			editorX.Submitted += text => SetComponent(context, 0, editorX, current.GetValue());
			editorY.Submitted += text => SetComponent(context, 1, editorY, current.GetValue());
			editorZ.Submitted += text => SetComponent(context, 2, editorZ, current.GetValue());
			editorX.AddChangeWatcher(current, v => {
				var ea = v.ToEulerAngles() * Mathf.RadToDeg;
				editorX.Text = RoundAngle(ea.X).ToString();
				editorY.Text = RoundAngle(ea.Y).ToString();
				editorZ.Text = RoundAngle(ea.Z).ToString();
			});
		}

		float RoundAngle(float value) => (value * 1000f).Round() / 1000f;

		void SetComponent(PropertyEditorContext context, int component, EditBox editor, Quaternion currentValue)
		{
			float newValue;
			if (float.TryParse(editor.Text, out newValue)) {
				foreach (var obj in context.Objects) {
					var current = new Property<Quaternion>(obj, context.PropertyName).Value.ToEulerAngles();
					current[component] = newValue * Mathf.DegToRad;
					Core.Operations.SetAnimableProperty.Perform(obj, context.PropertyName,
						Quaternion.CreateFromEulerAngles(current));
				}
			} else {
				editor.Text = RoundAngle(currentValue.ToEulerAngles()[component] * Mathf.RadToDeg).ToString();
			}
		}
	}

	class NumericRangePropertyEditor : CommonPropertyEditor
	{
		public NumericRangePropertyEditor(PropertyEditorContext context) : base(context)
		{
			EditBox medEditor, dispEditor;
			containerWidget.AddNode(new Widget {
				Layout = new HBoxLayout { CellDefaults = new LayoutCell(Alignment.Center), Spacing = 4 },
				Nodes = {
					(medEditor = new EditBox()),
					(dispEditor = new EditBox()),
				}
			});
			OnKeyframeToggle += medEditor.SetFocus;
			var currentMed = CoalescedPropertyValue<NumericRange, float>(context, v => v.Median);
			var currentDisp = CoalescedPropertyValue<NumericRange, float>(context, v => v.Dispersion);
			medEditor.TextWidget.HAlignment = HAlignment.Right;
			dispEditor.TextWidget.HAlignment = HAlignment.Right;
			medEditor.Submitted += text => SetComponent(context, 0, medEditor, currentMed.GetValue());
			dispEditor.Submitted += text => SetComponent(context, 1, dispEditor, currentDisp.GetValue());
			medEditor.AddChangeWatcher(currentMed, v => medEditor.Text = v.ToString());
			dispEditor.AddChangeWatcher(currentDisp, v => dispEditor.Text = v.ToString());
		}

		void SetComponent(PropertyEditorContext context, int component, EditBox editor, float currentValue)
		{
			float newValue;
			if (float.TryParse(editor.Text, out newValue)) {
				foreach (var obj in context.Objects) {
					var current = new Property<NumericRange>(obj, context.PropertyName).Value;
					if (component == 0) {
						current.Median = newValue;
					} else {
						current.Dispersion = newValue;
					}
					Core.Operations.SetAnimableProperty.Perform(obj, context.PropertyName, current);
				}
			} else {
				editor.Text = currentValue.ToString();
			}
		}
	}

	class NodeReferencePropertyEditor<T> : CommonPropertyEditor where T : Node
	{
		public NodeReferencePropertyEditor(PropertyEditorContext context) : base(context)
		{
			var propName = context.PropertyName;
			if (propName.EndsWith("Ref")) {
				propertyNameLabel.Text = propName.Substring(0, propName.Length - 3);
			}
			var editor = new EditBox { LayoutCell = new LayoutCell(Alignment.Center) };
			containerWidget.AddNode(editor);
			OnKeyframeToggle += editor.SetFocus;
			editor.Submitted += text => {
				var value = new NodeReference<T>(text);
				Core.Operations.SetAnimableProperty.Perform(context.Objects, context.PropertyName, value);
			};
			editor.AddChangeWatcher(CoalescedPropertyValue<NodeReference<T>>(context), v => editor.Text = v.Id);
		}
	}

	class StringPropertyEditor : CommonPropertyEditor
	{
		const int maxLines = 5;

		public StringPropertyEditor(PropertyEditorContext context, bool multiline = false) : base(context)
		{
			var editor = new EditBox { LayoutCell = new LayoutCell(Alignment.Center) };
			editor.Editor.EditorParams.MaxLines = multiline ? maxLines : 1;
			editor.MinHeight += multiline ? editor.TextWidget.FontHeight * (maxLines - 1) : 0;
			containerWidget.AddNode(editor);
			OnKeyframeToggle += editor.SetFocus;
			editor.Submitted += text => {
				Core.Operations.SetAnimableProperty.Perform(context.Objects, context.PropertyName, text);
			};
			editor.AddChangeWatcher(CoalescedPropertyValue<string>(context), v => editor.Text = v);
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
			selector.AddChangeWatcher(CoalescedPropertyValue<T>(context), v => selector.Value = v);
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
			checkBox.AddChangeWatcher(CoalescedPropertyValue<bool>(context), v => checkBox.Checked = v);
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
			editor.AddChangeWatcher(current, v => editor.Text = v.ToString());
		}
	}

	class IntPropertyEditor : CommonPropertyEditor
	{
		public IntPropertyEditor(PropertyEditorContext context) : base(context)
		{
			var editor = new EditBox { LayoutCell = new LayoutCell(Alignment.Center) };
			editor.TextWidget.HAlignment = HAlignment.Right;
			containerWidget.AddNode(editor);
			OnKeyframeToggle += editor.SetFocus;
			var current = CoalescedPropertyValue<int>(context);
			editor.Submitted += text => {
				int newValue;
				if (int.TryParse(text, out newValue)) {
					Core.Operations.SetAnimableProperty.Perform(context.Objects, context.PropertyName, newValue);
				} else {
					editor.Text = current.GetValue().ToString();
				}
			};
			editor.AddChangeWatcher(current, v => editor.Text = v.ToString());
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
				var dlg = new FileDialog {
					AllowedFileTypes = allowedFileTypes,
					Mode = FileDialogMode.Open,
					InitialDirectory = Project.Current.GetSystemDirectory(Document.Current.Path),
				};
				if (dlg.RunModal()) {
					if (!dlg.FileName.StartsWith(Project.Current.AssetsDirectory)) {
						var alert = new AlertDialog("Tangerine", "Can't open an assset outside the project assets directory", "Ok");
						alert.Show();
					} else {
						var assetPath = dlg.FileName.Substring(Project.Current.AssetsDirectory.Length + 1);
						var path = Path.ChangeExtension(assetPath, null);
						SetFilePath(path);
					}
				}
			};
		}

		protected abstract void SetFilePath(string path);
	}

	class TexturePropertyEditor : FilePropertyEditor
	{
		public TexturePropertyEditor(PropertyEditorContext context) : base(context, new string[] { "png" })
		{
			editor.Submitted += text => {
				Core.Operations.SetAnimableProperty.Perform(context.Objects, context.PropertyName, new SerializableTexture(text));
			};
			editor.AddChangeWatcher(CoalescedPropertyValue<ITexture>(context), v => editor.Text = v?.SerializationPath ?? "");
		}

		protected override void SetFilePath(string path)
		{
			foreach (var obj in context.Objects) {
				Core.Operations.SetAnimableProperty.Perform(obj, context.PropertyName, new SerializableTexture(path));
			}
		}
	}

	class AudioSamplePropertyEditor : FilePropertyEditor
	{
		public AudioSamplePropertyEditor(PropertyEditorContext context) : base(context, new string[] { "ogg" })
		{
			editor.Submitted += text => {
				Core.Operations.SetAnimableProperty.Perform(context.Objects, context.PropertyName, new SerializableSample(text));
			};
			editor.AddChangeWatcher(CoalescedPropertyValue<SerializableSample>(context), v => editor.Text = v?.SerializationPath ?? "");
		}

		protected override void SetFilePath(string path)
		{
			foreach (var obj in context.Objects) {
				Core.Operations.SetAnimableProperty.Perform(obj, context.PropertyName, new SerializableSample(path));
			}
		}
	}

	class ContentsPathPropertyEditor : FilePropertyEditor
	{
		public ContentsPathPropertyEditor(PropertyEditorContext context) : base(context, Document.AllowedFileTypes)
		{
			editor.Submitted += text => {
				Core.Operations.SetAnimableProperty.Perform(context.Objects, context.PropertyName, text);
			};
			editor.AddChangeWatcher(CoalescedPropertyValue<string>(context), v => editor.Text = v);
		}

		protected override void SetFilePath(string path)
		{
			foreach (var obj in context.Objects) {
				Core.Operations.SetAnimableProperty.Perform(obj, context.PropertyName, path);
			}
			Document.Current.Container.LoadExternalScenes();
		}
	}

	class FontPropertyEditor : CommonPropertyEditor
	{
		public FontPropertyEditor(PropertyEditorContext context) : base(context)
		{
			var selector = new DropDownList { LayoutCell = new LayoutCell(Alignment.Center) };
			containerWidget.AddNode(selector);
			OnKeyframeToggle += selector.SetFocus;
			var propType = context.PropertyInfo.PropertyType;
			var items = AssetBundle.Instance.EnumerateFiles("Fonts").
				Where(i => i.EndsWith(".fnt") || i.EndsWith(".tft")).
				Select(i => new DropDownList.Item(Path.ChangeExtension(Path.GetFileName(i), null)));
			foreach (var i in items) {
				selector.Items.Add(i);
			}
			selector.Changed += index => {
				var font = new SerializableFont(selector.Items[index].Text);
				Core.Operations.SetAnimableProperty.Perform(context.Objects, context.PropertyName, font);
			};
			selector.AddChangeWatcher(CoalescedPropertyValue<SerializableFont>(context), i => {
				selector.Text = string.IsNullOrEmpty(i.Name) ? "Default" : i.Name;
			});
		}
	}
}