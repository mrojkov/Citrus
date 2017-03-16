using System;
using System.IO;
using System.Linq;
using Lime;
using Tangerine.Core;
using System.Collections.Generic;

namespace Tangerine.UI
{
	public interface IPropertyEditor
	{
		PropertyEditorContext Context { get; }
		Widget ContainerWidget { get; }
		void SetFocus();
	}

	public class PropertyEditorContext
	{
		public delegate void PropertySetterDelegate(object obj, string propertyName, object value);

		public readonly Widget InspectorPane;
		public readonly List<object> Objects;
		public readonly Type Type;
		public readonly string PropertyName;
		public string DisplayName;
		public readonly TangerineKeyframeColorAttribute TangerineAttribute;
		public readonly System.Reflection.PropertyInfo PropertyInfo;
		public Func<NumericEditBox> NumericEditBoxFactory = () => new NumericEditBox();
		public PropertySetterDelegate PropertySetter;

		public PropertyEditorContext(Widget inspectorPane, List<object> objects, Type type, string propertyName)
		{
			InspectorPane = inspectorPane;
			Objects = objects;
			Type = type;
			PropertyName = propertyName;
			TangerineAttribute = PropertyAttributes<TangerineKeyframeColorAttribute>.Get(Type, PropertyName) ?? new TangerineKeyframeColorAttribute(0);
			PropertyInfo = Type.GetProperty(PropertyName);
			PropertySetter = SetProperty;
		}

		public PropertyEditorContext(Widget inspectorPane, object obj, string propertyName, string displayName = null)
			: this(inspectorPane, new List<object> { obj }, obj.GetType(), propertyName)
		{
			DisplayName = displayName;
		}

		private void SetProperty(object obj, string propertyName, object value) => PropertyInfo.SetValue(obj, value);
	}

	public class CommonPropertyEditor : IPropertyEditor
	{
		public PropertyEditorContext Context { get; private set; }
		public Widget ContainerWidget { get; private set; }
		public SimpleText PropertyNameLabel { get; private set; }

		public CommonPropertyEditor(PropertyEditorContext context)
		{
			Context = context;
			ContainerWidget = new Widget {
				Layout = new HBoxLayout { IgnoreHidden = false },
				Padding = new Thickness { Left = 4, Right = 12 },
				LayoutCell = new LayoutCell { StretchY = 0 },
			};
			context.InspectorPane.AddNode(ContainerWidget);
			PropertyNameLabel = new SimpleText {
				Text = context.DisplayName ?? context.PropertyName,
				VAlignment = VAlignment.Center,
				LayoutCell = new LayoutCell(Alignment.LeftCenter, stretchX: 0.5f),
				AutoSizeConstraints = false,
			};
			ContainerWidget.AddNode(PropertyNameLabel);
		}

		public virtual void SetFocus() { }

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

		protected void SetProperty(string name, object value)
		{
			foreach (var o in Context.Objects) {
				Context.PropertySetter(o, name, value);
			}
		}
	}

	public class Vector2PropertyEditor : CommonPropertyEditor
	{
		private NumericEditBox editorX, editorY;

		public Vector2PropertyEditor(PropertyEditorContext context) : base(context)
		{
			ContainerWidget.AddNode(new Widget {
				Layout = new HBoxLayout { CellDefaults = new LayoutCell(Alignment.Center), Spacing = 4 },
				Nodes = {
					(editorX = context.NumericEditBoxFactory()),
					(editorY = context.NumericEditBoxFactory()),
				}
			});
			var currentX = CoalescedPropertyValue<Vector2, float>(context, v => v.X);
			var currentY = CoalescedPropertyValue<Vector2, float>(context, v => v.Y);
			editorX.TextWidget.HAlignment = HAlignment.Right;
			editorY.TextWidget.HAlignment = HAlignment.Right;
			editorX.Submitted += text => SetComponent(context, 0, editorX, currentX.GetValue());
			editorY.Submitted += text => SetComponent(context, 1, editorY, currentY.GetValue());
			editorX.AddChangeWatcher(currentX, v => editorX.Text = v.ToString());
			editorY.AddChangeWatcher(currentY, v => editorY.Text = v.ToString());
		}

		public override void SetFocus() => editorX.SetFocus();

		void SetComponent(PropertyEditorContext context, int component, CommonEditBox editor, float currentValue)
		{
			float newValue;
			if (float.TryParse(editor.Text, out newValue)) {
				foreach (var obj in context.Objects) {
					var current = new Property<Vector2>(obj, context.PropertyName).Value;
					current[component] = newValue;
					context.PropertySetter(obj, context.PropertyName, current);
				}
			} else {
				editor.Text = currentValue.ToString();
			}
		}
	}

	public class Vector3PropertyEditor : CommonPropertyEditor
	{
		private NumericEditBox editorX, editorY, editorZ;

		public Vector3PropertyEditor(PropertyEditorContext context) : base(context)
		{
			ContainerWidget.AddNode(new Widget {
				Layout = new HBoxLayout { CellDefaults = new LayoutCell(Alignment.Center), Spacing = 4 },
				Nodes = {
					(editorX = context.NumericEditBoxFactory()),
					(editorY = context.NumericEditBoxFactory()),
					(editorZ = context.NumericEditBoxFactory())
				}
			});
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

		public override void SetFocus() => editorX.SetFocus();

		void SetComponent(PropertyEditorContext context, int component, NumericEditBox editor, float currentValue)
		{
			float newValue;
			if (float.TryParse(editor.Text, out newValue)) {
				foreach (var obj in context.Objects) {
					var current = new Property<Vector3>(obj, context.PropertyName).Value;
					current[component] = newValue;
					context.PropertySetter(obj, context.PropertyName, current);
				}
			} else {
				editor.Text = currentValue.ToString();
			}
		}
	}

	public class QuaternionPropertyEditor : CommonPropertyEditor
	{
		private NumericEditBox editorX, editorY, editorZ;

		public QuaternionPropertyEditor(PropertyEditorContext context) : base(context)
		{
			ContainerWidget.AddNode(new Widget {
				Layout = new HBoxLayout { CellDefaults = new LayoutCell(Alignment.Center), Spacing = 4 },
				Nodes = {
					(editorX = context.NumericEditBoxFactory()),
					(editorY = context.NumericEditBoxFactory()),
					(editorZ = context.NumericEditBoxFactory())
				}
			});
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

		public override void SetFocus() => editorX.SetFocus();

		float RoundAngle(float value) => (value * 1000f).Round() / 1000f;

		void SetComponent(PropertyEditorContext context, int component, NumericEditBox editor, Quaternion currentValue)
		{
			float newValue;
			if (float.TryParse(editor.Text, out newValue)) {
				foreach (var obj in context.Objects) {
					var current = new Property<Quaternion>(obj, context.PropertyName).Value.ToEulerAngles();
					current[component] = newValue * Mathf.DegToRad;
					context.PropertySetter(obj, context.PropertyName,
						Quaternion.CreateFromEulerAngles(current));
				}
			} else {
				editor.Text = RoundAngle(currentValue.ToEulerAngles()[component] * Mathf.RadToDeg).ToString();
			}
		}
	}

	public class NumericRangePropertyEditor : CommonPropertyEditor
	{
		private NumericEditBox medEditor, dispEditor;

		public NumericRangePropertyEditor(PropertyEditorContext context) : base(context)
		{
			ContainerWidget.AddNode(new Widget {
				Layout = new HBoxLayout { CellDefaults = new LayoutCell(Alignment.Center), Spacing = 4 },
				Nodes = {
					(medEditor = context.NumericEditBoxFactory()),
					(dispEditor = context.NumericEditBoxFactory()),
				}
			});
			var currentMed = CoalescedPropertyValue<NumericRange, float>(context, v => v.Median);
			var currentDisp = CoalescedPropertyValue<NumericRange, float>(context, v => v.Dispersion);
			medEditor.TextWidget.HAlignment = HAlignment.Right;
			dispEditor.TextWidget.HAlignment = HAlignment.Right;
			medEditor.Submitted += text => SetComponent(context, 0, medEditor, currentMed.GetValue());
			dispEditor.Submitted += text => SetComponent(context, 1, dispEditor, currentDisp.GetValue());
			medEditor.AddChangeWatcher(currentMed, v => medEditor.Text = v.ToString());
			dispEditor.AddChangeWatcher(currentDisp, v => dispEditor.Text = v.ToString());
		}

		public override void SetFocus() => medEditor.SetFocus();

		void SetComponent(PropertyEditorContext context, int component, NumericEditBox editor, float currentValue)
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
					context.PropertySetter(obj, context.PropertyName, current);
				}
			} else {
				editor.Text = currentValue.ToString();
			}
		}
	}

	public class NodeReferencePropertyEditor<T> : CommonPropertyEditor where T : Node
	{
		private EditBox editor;

		public NodeReferencePropertyEditor(PropertyEditorContext context) : base(context)
		{
			var propName = context.PropertyName;
			if (propName.EndsWith("Ref")) {
				PropertyNameLabel.Text = propName.Substring(0, propName.Length - 3);
			}
			editor = new EditBox { LayoutCell = new LayoutCell(Alignment.Center) };
			ContainerWidget.AddNode(editor);
			editor.Submitted += text => {
				var value = new NodeReference<T>(text);
				SetProperty(context.PropertyName, value);
			};
			editor.AddChangeWatcher(CoalescedPropertyValue<NodeReference<T>>(context), v => editor.Text = v.Id);
		}

		public override void SetFocus() => editor.SetFocus();
	}

	public class StringPropertyEditor : CommonPropertyEditor
	{
		const int maxLines = 5;
		private EditBox editor;

		public StringPropertyEditor(PropertyEditorContext context, bool multiline = false) : base(context)
		{
			editor = new EditBox { LayoutCell = new LayoutCell(Alignment.Center) };
			editor.Editor.EditorParams.MaxLines = multiline ? maxLines : 1;
			editor.MinHeight += multiline ? editor.TextWidget.FontHeight * (maxLines - 1) : 0;
			ContainerWidget.AddNode(editor);
			editor.Submitted += text => SetProperty(context.PropertyName, text);
			editor.AddChangeWatcher(CoalescedPropertyValue<string>(context), v => editor.Text = v);
		}

		public override void SetFocus() => editor.SetFocus();
	}

	public class EnumPropertyEditor<T> : CommonPropertyEditor
	{
		private DropDownList selector;

		public EnumPropertyEditor(PropertyEditorContext context) : base(context)
		{
			selector = new DropDownList { LayoutCell = new LayoutCell(Alignment.Center) };
			ContainerWidget.AddNode(selector);
			var propType = context.PropertyInfo.PropertyType;
			foreach (var i in Enum.GetNames(propType).Zip(Enum.GetValues(propType).Cast<object>(), (a, b) => new DropDownList.Item(a, b))) {
				selector.Items.Add(i);
			}
			selector.Changed += index => SetProperty(context.PropertyName, (T)selector.Items[index].Value);
			selector.AddChangeWatcher(CoalescedPropertyValue<T>(context), v => selector.Value = v);
		}

		public override void SetFocus() => selector.SetFocus();
	}

	public class BooleanPropertyEditor : CommonPropertyEditor
	{
		private CheckBox checkBox;

		public BooleanPropertyEditor(PropertyEditorContext context) : base(context)
		{
			checkBox = new CheckBox { LayoutCell = new LayoutCell(Alignment.Center) };
			ContainerWidget.AddNode(checkBox);
			checkBox.Changed += value => SetProperty(context.PropertyName, value);
			checkBox.AddChangeWatcher(CoalescedPropertyValue<bool>(context), v => checkBox.Checked = v);
		}

		public override void SetFocus() => checkBox.SetFocus();
	}

	public class FloatPropertyEditor : CommonPropertyEditor
	{
		private NumericEditBox editor;

		public FloatPropertyEditor(PropertyEditorContext context) : base(context)
		{
			editor = new NumericEditBox { LayoutCell = new LayoutCell(Alignment.Center) };
			editor.TextWidget.HAlignment = HAlignment.Right;
			ContainerWidget.AddNode(editor);
			var current = CoalescedPropertyValue<float>(context);
			editor.Submitted += text => {
				float newValue;
				if (float.TryParse(text, out newValue)) {
					SetProperty(context.PropertyName, newValue);
				} else {
					editor.Text = current.GetValue().ToString();
				}
			};
			editor.AddChangeWatcher(current, v => editor.Text = v.ToString());
		}

		public override void SetFocus() => editor.SetFocus();
	}

	public class IntPropertyEditor : CommonPropertyEditor
	{
		private EditBox editor;

		public IntPropertyEditor(PropertyEditorContext context) : base(context)
		{
			editor = new EditBox { LayoutCell = new LayoutCell(Alignment.Center) };
			editor.TextWidget.HAlignment = HAlignment.Right;
			ContainerWidget.AddNode(editor);
			var current = CoalescedPropertyValue<int>(context);
			editor.Submitted += text => {
				int newValue;
				if (int.TryParse(text, out newValue)) {
					SetProperty(context.PropertyName, newValue);
				} else {
					editor.Text = current.GetValue().ToString();
				}
			};
			editor.AddChangeWatcher(current, v => editor.Text = v.ToString());
		}

		public override void SetFocus() => editor.SetFocus();
	}

	public class Color4PropertyEditor : CommonPropertyEditor
	{
		private EditBox editor;

		public Color4PropertyEditor(PropertyEditorContext context) : base(context)
		{
			ColorBoxButton colorBox;
			var currentColor = CoalescedPropertyValue(context, Color4.White).DistinctUntilChanged();
			ContainerWidget.AddNode(new Widget {
				Layout = new HBoxLayout { CellDefaults = new LayoutCell(Alignment.Center) },
				Nodes = {
					(editor = new EditBox()),
					new HSpacer(4),
					(colorBox = new ColorBoxButton(currentColor)),
				}
			});
			var panel = new ColorPickerPanel();
			context.InspectorPane.AddNode(panel.RootWidget);
			panel.RootWidget.Visible = false;
			panel.RootWidget.Padding.Right = 12;
			panel.RootWidget.Tasks.Add(currentColor.Consume(v => panel.Color = v));
			panel.Changed += () => SetProperty(context.PropertyName, panel.Color);
			panel.DragStarted += Document.Current.History.BeginTransaction;
			panel.DragEnded += Document.Current.History.EndTransaction;
			colorBox.Clicked += () => panel.RootWidget.Visible = !panel.RootWidget.Visible;
			var currentColorString = currentColor.Select(i => i.ToString(Color4.StringPresentation.Dec));
			editor.Submitted += text => {
				Color4 newColor;
				if (Color4.TryParse(text, out newColor)) {
					SetProperty(context.PropertyName, newColor);
				} else {
					editor.Text = currentColorString.GetValue();
				}
			};
			editor.Tasks.Add(currentColorString.Consume(v => editor.Text = v));
		}

		public override void SetFocus() => editor.SetFocus();

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
					Renderer.DrawRectOutline(Vector2.Zero, widget.Size, ColorTheme.Current.Inspector.BorderAroundKeyframeColorbox);
				});
			}
		}
	}

	public abstract class FilePropertyEditor : CommonPropertyEditor
	{
		protected readonly EditBox editor;
		protected readonly Button button;

		protected FilePropertyEditor(PropertyEditorContext context, string[] allowedFileTypes) : base(context)
		{
			ContainerWidget.AddNode(new Widget {
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
						SetFilePath(CorrectSlashes(path));
					}
				}
			};
		}

		public override void SetFocus() => editor.SetFocus();

		protected static string CorrectSlashes(string path) => AssetPath.CorrectSlashes(path);

		protected abstract void SetFilePath(string path);
	}

	public class TexturePropertyEditor : FilePropertyEditor
	{
		public TexturePropertyEditor(PropertyEditorContext context) : base(context, new string[] { "png" })
		{
			editor.Submitted += text => SetProperty(context.PropertyName, new SerializableTexture(CorrectSlashes(text)));
			editor.AddChangeWatcher(CoalescedPropertyValue<ITexture>(context), v => editor.Text = v?.SerializationPath ?? "");
		}

		protected override void SetFilePath(string path)
		{
			SetProperty(Context.PropertyName, new SerializableTexture(path));
		}
	}

	public class AudioSamplePropertyEditor : FilePropertyEditor
	{
		public AudioSamplePropertyEditor(PropertyEditorContext context) : base(context, new string[] { "ogg" })
		{
			editor.Submitted += text => SetProperty(context.PropertyName, new SerializableSample(CorrectSlashes(text)));
			editor.AddChangeWatcher(CoalescedPropertyValue<SerializableSample>(context), v => editor.Text = v?.SerializationPath ?? "");
		}

		protected override void SetFilePath(string path)
		{
			SetProperty(Context.PropertyName, new SerializableSample(path));
		}
	}

	public class ContentsPathPropertyEditor : FilePropertyEditor
	{
		public ContentsPathPropertyEditor(PropertyEditorContext context) : base(context, Document.AllowedFileTypes)
		{
			editor.Submitted += text => SetProperty(context.PropertyName, CorrectSlashes(text));
			editor.AddChangeWatcher(CoalescedPropertyValue<string>(context), v => editor.Text = v);
		}

		protected override void SetFilePath(string path)
		{
			SetProperty(Context.PropertyName, path);
			Document.Current.Container.LoadExternalScenes();
		}
	}

	public class FontPropertyEditor : CommonPropertyEditor
	{
		private DropDownList selector;

		public FontPropertyEditor(PropertyEditorContext context) : base(context)
		{
			selector = new DropDownList { LayoutCell = new LayoutCell(Alignment.Center) };
			ContainerWidget.AddNode(selector);
			var propType = context.PropertyInfo.PropertyType;
			var items = AssetBundle.Instance.EnumerateFiles("Fonts").
				Where(i => i.EndsWith(".fnt") || i.EndsWith(".tft")).
				Select(i => new DropDownList.Item(Path.ChangeExtension(Path.GetFileName(i), null)));
			foreach (var i in items) {
				selector.Items.Add(i);
			}
			selector.Changed += index => {
				var font = new SerializableFont(selector.Items[index].Text);
				SetProperty(context.PropertyName, font);
			};
			selector.AddChangeWatcher(CoalescedPropertyValue<SerializableFont>(context), i => {
				selector.Text = string.IsNullOrEmpty(i.Name) ? "Default" : i.Name;
			});
		}

		public override void SetFocus() => selector.SetFocus();
	}

	public class TriggerPropertyEditor : CommonPropertyEditor
	{
		private ComboBox comboBox;

		public TriggerPropertyEditor(PropertyEditorContext context, bool multiline = false) : base(context)
		{
			comboBox = new ComboBox { LayoutCell = new LayoutCell(Alignment.Center) };
			ContainerWidget.AddNode(comboBox);
			comboBox.Changed += index => SetProperty(context.PropertyName, comboBox.Items[index].Text);
			if (context.Objects.Count == 1) {
				var node = (Node)context.Objects[0];
				foreach (var a in node.Animations) {
					foreach (var m in a.Markers.Where(i => i.Action != MarkerAction.Jump)) {
						var id = a.Id != null ? m.Id + '@' + a.Id : m.Id;
						comboBox.Items.Add(new DropDownList.Item(id));
					}
				}
			}
			comboBox.AddChangeWatcher(CoalescedPropertyValue<string>(context), v => comboBox.Text = v);
		}

		public override void SetFocus() => comboBox.SetFocus();
	}
}