using System;
using System.IO;
using System.Linq;
using Lime;
using Tangerine.Core;
using System.Collections.Generic;
using System.Reflection;

namespace Tangerine.UI
{
	public interface IPropertyEditor
	{
		IPropertyEditorParams EditorParams { get; }
		Widget ContainerWidget { get; }
		SimpleText PropertyLabel { get; }
		void DropFiles(IEnumerable<string> files);
	}

	public interface IPropertyEditorParams
	{
		Widget InspectorPane { get; set; }
		List<object> Objects { get; set; }
		Type Type { get; set; }
		string PropertyName { get; set; }
		string DisplayName { get; set; }
		bool ShowLabel { get; set; }
		TangerineKeyframeColorAttribute TangerineAttribute { get; set; }
		System.Reflection.PropertyInfo PropertyInfo { get; set; }
		Func<NumericEditBox> NumericEditBoxFactory { get; set; }
		Func<DropDownList> DropDownListFactory { get; set; }
		Func<EditBox> EditBoxFactory { get; set; }
		Func<object> DefaultValueGetter { get; set; }
		PropertySetterDelegate PropertySetter { get; set; }
		ITransactionalHistory History { get; set; }
	}

	// Used to unify generic descendants of ExpandableProperty for type checking
	public interface IExpandablePropertyEditor
	{ }

	public delegate void PropertySetterDelegate(object obj, string propertyName, object value);

	public class PropertyEditorParams : IPropertyEditorParams
	{
		public bool ShowLabel { get; set; } = true;
		public Widget InspectorPane { get; set; }
		public List<object> Objects { get; set; }
		public Type Type { get; set; }
		public string PropertyName { get; set; }
		public string DisplayName { get; set; }
		public TangerineKeyframeColorAttribute TangerineAttribute { get; set; }
		public string Group { get; set; }
		public System.Reflection.PropertyInfo PropertyInfo { get; set; }
		public Func<NumericEditBox> NumericEditBoxFactory { get; set; }
		public Func<EditBox> EditBoxFactory { get; set; }
		public Func<DropDownList> DropDownListFactory { get; set; }
		public Func<object> DefaultValueGetter { get; set; }
		public PropertySetterDelegate PropertySetter { get; set; }
		public ITransactionalHistory History { get; set; }

		public PropertyEditorParams(Widget inspectorPane, List<object> objects, Type type, string propertyName)
		{
			InspectorPane = inspectorPane;
			Objects = objects;
			Type = type;
			PropertyName = propertyName;
			TangerineAttribute = PropertyAttributes<TangerineKeyframeColorAttribute>.Get(Type, PropertyName) ?? new TangerineKeyframeColorAttribute(0);
			Group = PropertyAttributes<TangerineGroupAttribute>.Get(Type, PropertyName)?.Name ?? String.Empty;
			PropertyInfo = Type.GetProperty(PropertyName);
			PropertySetter = SetProperty;
			NumericEditBoxFactory = () => new ThemedNumericEditBox();
			DropDownListFactory = () => new ThemedDropDownList();
			EditBoxFactory = () => new ThemedEditBox();
		}

		public PropertyEditorParams(Widget inspectorPane, object obj, string propertyName, string displayName = null)
			: this(inspectorPane, new List<object> { obj }, obj.GetType(), propertyName)
		{
			DisplayName = displayName;
		}

		private void SetProperty(object obj, string propertyName, object value) => PropertyInfo.SetValue(obj, value);
	}

	public class ExpandablePropertyEditor<T> : CommonPropertyEditor<T>, IExpandablePropertyEditor
	{
		private bool expanded;
		public bool Expanded
		{
			get { return expanded; }
			set
			{
				expanded = value;
				ExpandButton.Expanded = value;
				ExpandableContent.Visible = value;
			}
		}
		public Widget ExpandableContent { get; }
		private ThemedExpandButton ExpandButton { get; }

		public ExpandablePropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			ExpandableContent = new ThemedFrame {
				Padding = new Thickness(4),
				Layout = new VBoxLayout(),
				Visible = false
			};
			ExpandButton = new ThemedExpandButton {
				Anchors = Anchors.Left,
				MinMaxSize = Vector2.One * 20f,
			};
			ExpandButton.Clicked += () => Expanded = !Expanded;
			editorParams.InspectorPane.AddNode(ExpandableContent);
			ContainerWidget.Nodes.Insert(0, ExpandButton);
		}
	}

	public class CommonPropertyEditor<T> : IPropertyEditor
	{
		public IPropertyEditorParams EditorParams { get; private set; }
		public Widget ContainerWidget { get; private set; }
		public SimpleText PropertyLabel { get; private set; }

		public CommonPropertyEditor(IPropertyEditorParams editorParams)
		{
			EditorParams = editorParams;
			ContainerWidget = new Widget {
				Layout = new HBoxLayout { IgnoreHidden = false },
				LayoutCell = new LayoutCell { StretchY = 0 },
			};
			editorParams.InspectorPane.AddNode(ContainerWidget);
			if (editorParams.ShowLabel) {
				PropertyLabel = new ThemedSimpleText {
					Text = editorParams.DisplayName ?? editorParams.PropertyName,
					VAlignment = VAlignment.Center,
					LayoutCell = new LayoutCell(Alignment.LeftCenter, stretchX: 0),
					ForceUncutText = false,
					MinWidth = 120,
					OverflowMode = TextOverflowMode.Minify,
					HitTestTarget = true,
					TabTravesable = new TabTraversable()
				};
				PropertyLabel.Tasks.Add(ManageLabelTask());
				ContainerWidget.AddNode(PropertyLabel);
			}
		}

		IEnumerator<object> ManageLabelTask()
		{
			while (true) {
				var popupMenu = PropertyLabel.Input.WasMouseReleased(1);
				if (popupMenu || PropertyLabel.Input.WasMouseReleased(0)) {
					PropertyLabel.SetFocus();
				}
				PropertyLabel.Color = PropertyLabel.IsFocused() ? Theme.Colors.KeyboardFocusBorder : Theme.Colors.BlackText;
				if (popupMenu) {
					// Wait until the label actually change its color.
					yield return null;
					yield return null;
					ShowPropertyContextMenu();
				}
				if (PropertyLabel.IsFocused()) {
					if (Command.Copy.WasIssued()) {
						Command.Copy.Consume();
						Copy();
					}
					if (Command.Paste.WasIssued()) {
						Command.Paste.Consume();
						Paste();
					}
					if (resetToDefault.WasIssued()) {
						resetToDefault.Consume();
						var defaultValue = EditorParams.DefaultValueGetter();
						if (defaultValue != null)
							SetProperty(defaultValue);
					}
				}
				yield return null;
			}
		}

		static Yuzu.Json.JsonSerializer serializer = new Yuzu.Json.JsonSerializer {
			JsonOptions = new Yuzu.Json.JsonSerializeOptions { FieldSeparator = " ", Indent = "", EnumAsString = true }
		};

		static Yuzu.Json.JsonDeserializer deserializer = new Yuzu.Json.JsonDeserializer {
			JsonOptions = new Yuzu.Json.JsonSerializeOptions { EnumAsString = true }
		};

		protected virtual void Copy()
		{
			var v = CoalescedPropertyValue().GetValue();
			Clipboard.Text = Serialize(v);
		}

		protected virtual void Paste()
		{
			try {
				var v = Deserialize(Clipboard.Text);
				SetProperty(v);
			} catch (System.Exception) { }
		}

		protected virtual string Serialize(T value) => serializer.ToString(value);
		protected virtual T Deserialize(string source) => deserializer.FromString<T>(source + ' ');

		protected void DoTransaction(Action block)
		{
			if (EditorParams.History != null) {
				using (EditorParams.History.BeginTransaction()) {
					block();
					EditorParams.History.CommitTransaction();
				}
			} else {
				block();
			}
		}

		private ICommand resetToDefault = new Command("Reset To Default");

		void ShowPropertyContextMenu()
		{
			var menu = new Menu {
				Command.Copy,
				Command.Paste
			};
			if (EditorParams.DefaultValueGetter != null) {
				menu.Insert(0, resetToDefault);
			}
			if (EditorParams.Objects.Count == 1) {
				var owner = EditorParams.Objects.First();
				var value = CoalescedPropertyValue().GetValue();
				var pi = EditorParams.PropertyInfo;
				if (value != null) {
					string path = null;
					if (pi.PropertyType == typeof(ITexture)) {
						path = (value as ITexture).SerializationPath;
					} else if (pi.PropertyType == typeof(SerializableSample)) {
						path = (value as SerializableSample).SerializationPath;
					} else if (pi.PropertyType == typeof(SerializableFont)) {
						var name = (value as SerializableFont).Name;
						if (string.IsNullOrEmpty(name)) {
							name = FontPool.DefaultFontName;
						}
						path = FontPool.DefaultFontDirectory + name;
					} else if (owner is Movie && pi.Name == "Path") {
						path = (owner as Movie).Path;
					} else if (owner is Node && pi.Name == "ContentsPath") {
						path = (owner as Node).ContentsPath;
					}
					if (!string.IsNullOrEmpty(path)) {
						path = Path.Combine(Project.Current.AssetsDirectory, path);
						FilesystemCommands.NavigateTo.UserData = path;
						menu.Insert(0, FilesystemCommands.NavigateTo);
						FilesystemCommands.OpenInSystemFileManager.UserData = path;
						menu.Insert(0, FilesystemCommands.OpenInSystemFileManager);
					}
				}
			}
			menu.Popup();
		}

		public virtual void DropFiles(IEnumerable<string> files) { }

		protected IDataflowProvider<T> CoalescedPropertyValue(T defaultValue = default(T))
		{
			IDataflowProvider<T> provider = null;
			foreach (var o in EditorParams.Objects) {
				var p = new Property<T>(o, EditorParams.PropertyName);
				provider = (provider == null) ? p : provider.SameOrDefault(p, defaultValue);
			}
			return provider;
		}

		protected IDataflowProvider<ComponentType> CoalescedPropertyComponentValue<ComponentType>(Func<T, ComponentType> selector, ComponentType defaultValue = default(ComponentType))
		{
			IDataflowProvider<ComponentType> provider = null;
			foreach (var o in EditorParams.Objects) {
				var p = new Property<T>(o, EditorParams.PropertyName).Select(selector);
				provider = (provider == null) ? p : provider.SameOrDefault(p, defaultValue);
			}
			return provider;
		}

		protected void SetProperty(object value)
		{
			DoTransaction(() => {
				foreach (var o in EditorParams.Objects) {
					EditorParams.PropertySetter(o, EditorParams.PropertyName, value);
				}
			});
		}
	}

	public class Vector2PropertyEditor : CommonPropertyEditor<Vector2>
	{
		private NumericEditBox editorX, editorY;

		public Vector2PropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			ContainerWidget.AddNode(new Widget {
				Layout = new HBoxLayout { CellDefaults = new LayoutCell(Alignment.Center), Spacing = 4 },
				Nodes = {
					(editorX = editorParams.NumericEditBoxFactory()),
					(editorY = editorParams.NumericEditBoxFactory())
				}
			});
			var currentX = CoalescedPropertyComponentValue(v => v.X);
			var currentY = CoalescedPropertyComponentValue(v => v.Y);
			editorX.Submitted += text => SetComponent(editorParams, 0, editorX, currentX.GetValue());
			editorY.Submitted += text => SetComponent(editorParams, 1, editorY, currentY.GetValue());
			editorX.AddChangeWatcher(currentX, v => editorX.Text = v.ToString());
			editorY.AddChangeWatcher(currentY, v => editorY.Text = v.ToString());
		}

		void SetComponent(IPropertyEditorParams editorParams, int component, CommonEditBox editor, float currentValue)
		{
			float newValue;
			if (float.TryParse(editor.Text, out newValue)) {
				DoTransaction(() => {
					foreach (var obj in editorParams.Objects) {
						var current = new Property<Vector2>(obj, editorParams.PropertyName).Value;
						current[component] = newValue;
						editorParams.PropertySetter(obj, editorParams.PropertyName, current);
					}
				});
			} else {
				editor.Text = currentValue.ToString();
			}
		}
	}

	public class Vector3PropertyEditor : CommonPropertyEditor<Vector3>
	{
		private NumericEditBox editorX, editorY, editorZ;

		public Vector3PropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			ContainerWidget.AddNode(new Widget {
				Layout = new HBoxLayout { CellDefaults = new LayoutCell(Alignment.Center), Spacing = 4 },
				Nodes = {
					(editorX = editorParams.NumericEditBoxFactory()),
					(editorY = editorParams.NumericEditBoxFactory()),
					(editorZ = editorParams.NumericEditBoxFactory())
				}
			});
			var currentX = CoalescedPropertyComponentValue(v => v.X);
			var currentY = CoalescedPropertyComponentValue(v => v.Y);
			var currentZ = CoalescedPropertyComponentValue(v => v.Z);
			editorX.Submitted += text => SetComponent(editorParams, 0, editorX, currentX.GetValue());
			editorY.Submitted += text => SetComponent(editorParams, 1, editorY, currentY.GetValue());
			editorZ.Submitted += text => SetComponent(editorParams, 2, editorZ, currentZ.GetValue());
			editorX.AddChangeWatcher(currentX, v => editorX.Text = v.ToString());
			editorY.AddChangeWatcher(currentY, v => editorY.Text = v.ToString());
			editorZ.AddChangeWatcher(currentZ, v => editorZ.Text = v.ToString());
		}

		void SetComponent(IPropertyEditorParams editorParams, int component, NumericEditBox editor, float currentValue)
		{
			float newValue;
			if (float.TryParse(editor.Text, out newValue)) {
				DoTransaction(() => {
					foreach (var obj in editorParams.Objects) {
						var current = new Property<Vector3>(obj, editorParams.PropertyName).Value;
						current[component] = newValue;
						editorParams.PropertySetter(obj, editorParams.PropertyName, current);
					}
				});
			} else {
				editor.Text = currentValue.ToString();
			}
		}
	}

	public class QuaternionPropertyEditor : CommonPropertyEditor<Quaternion>
	{
		private NumericEditBox editorX, editorY, editorZ;

		public QuaternionPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			ContainerWidget.AddNode(new Widget {
				Layout = new HBoxLayout { CellDefaults = new LayoutCell(Alignment.Center), Spacing = 4 },
				Nodes = {
					(editorX = editorParams.NumericEditBoxFactory()),
					(editorY = editorParams.NumericEditBoxFactory()),
					(editorZ = editorParams.NumericEditBoxFactory())
				}
			});
			var current = CoalescedPropertyValue();
			editorX.Submitted += text => SetComponent(editorParams, 0, editorX, current.GetValue());
			editorY.Submitted += text => SetComponent(editorParams, 1, editorY, current.GetValue());
			editorZ.Submitted += text => SetComponent(editorParams, 2, editorZ, current.GetValue());
			editorX.AddChangeWatcher(current, v => {
				var ea = v.ToEulerAngles() * Mathf.RadToDeg;
				editorX.Text = RoundAngle(ea.X).ToString();
				editorY.Text = RoundAngle(ea.Y).ToString();
				editorZ.Text = RoundAngle(ea.Z).ToString();
			});
		}

		float RoundAngle(float value) => (value * 1000f).Round() / 1000f;

		void SetComponent(IPropertyEditorParams editorParams, int component, NumericEditBox editor, Quaternion currentValue)
		{
			float newValue;
			if (float.TryParse(editor.Text, out newValue)) {
				DoTransaction(() => {
					foreach (var obj in editorParams.Objects) {
						var current = new Property<Quaternion>(obj, editorParams.PropertyName).Value.ToEulerAngles();
						current[component] = newValue * Mathf.DegToRad;
						editorParams.PropertySetter(obj, editorParams.PropertyName,
							Quaternion.CreateFromEulerAngles(current));
					}
				});
			} else {
				editor.Text = RoundAngle(currentValue.ToEulerAngles()[component] * Mathf.RadToDeg).ToString();
			}
		}
	}

	public class NumericRangePropertyEditor : CommonPropertyEditor<NumericRange>
	{
		private NumericEditBox medEditor, dispEditor;

		public NumericRangePropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			ContainerWidget.AddNode(new Widget {
				Layout = new HBoxLayout { CellDefaults = new LayoutCell(Alignment.Center), Spacing = 4 },
				Nodes = {
					(medEditor = editorParams.NumericEditBoxFactory()),
					(dispEditor = editorParams.NumericEditBoxFactory()),
				}
			});
			var currentMed = CoalescedPropertyComponentValue(v => v.Median);
			var currentDisp = CoalescedPropertyComponentValue(v => v.Dispersion);
			medEditor.Submitted += text => SetComponent(editorParams, 0, medEditor, currentMed.GetValue());
			dispEditor.Submitted += text => SetComponent(editorParams, 1, dispEditor, currentDisp.GetValue());
			medEditor.AddChangeWatcher(currentMed, v => medEditor.Text = v.ToString());
			dispEditor.AddChangeWatcher(currentDisp, v => dispEditor.Text = v.ToString());
		}

		void SetComponent(IPropertyEditorParams editorParams, int component, NumericEditBox editor, float currentValue)
		{
			float newValue;
			if (float.TryParse(editor.Text, out newValue)) {
				DoTransaction(() => {
					foreach (var obj in editorParams.Objects) {
						var current = new Property<NumericRange>(obj, editorParams.PropertyName).Value;
						if (component == 0) {
							current.Median = newValue;
						} else {
							current.Dispersion = newValue;
						}
						editorParams.PropertySetter(obj, editorParams.PropertyName, current);
					}
				});
			} else {
				editor.Text = currentValue.ToString();
			}
		}
	}

	public class NodeReferencePropertyEditor<T> : CommonPropertyEditor<NodeReference<T>> where T : Node
	{
		private EditBox editor;

		public NodeReferencePropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			var propName = editorParams.PropertyName;
			if (propName.EndsWith("Ref")) {
				PropertyLabel.Text = propName.Substring(0, propName.Length - 3);
			}
			editor = editorParams.EditBoxFactory();
			editor.LayoutCell = new LayoutCell(Alignment.Center);
			ContainerWidget.AddNode(editor);
			editor.Submitted += text => {
				SetProperty(new NodeReference<T>(text));
			};
			editor.AddChangeWatcher(CoalescedPropertyValue(), v => editor.Text = v?.Id);
		}
	}

	public class StringPropertyEditor : CommonPropertyEditor<string>
	{
		const int maxLines = 5;
		private EditBox editor;

		public StringPropertyEditor(IPropertyEditorParams editorParams, bool multiline = false) : base(editorParams)
		{
			editor = editorParams.EditBoxFactory();
			editor.LayoutCell = new LayoutCell(Alignment.Center);
			editor.Editor.EditorParams.MaxLines = multiline ? maxLines : 1;
			editor.MinHeight += multiline ? editor.TextWidget.FontHeight * (maxLines - 1) : 0;
			ContainerWidget.AddNode(editor);
			editor.Submitted += SetProperty;
			editor.AddChangeWatcher(CoalescedPropertyValue(), v => editor.Text = v);
		}
	}

	public class EnumPropertyEditor<T> : CommonPropertyEditor<T>
	{
		public DropDownList Selector { get; }

		public EnumPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			Selector = editorParams.DropDownListFactory();
			Selector.LayoutCell = new LayoutCell(Alignment.Center);
			ContainerWidget.AddNode(Selector);
			var propType = editorParams.PropertyInfo.PropertyType;
			var fields = propType.GetFields(BindingFlags.Public | BindingFlags.Static);
			var allowedFields = fields.Where(f => !Attribute.IsDefined(f, typeof(TangerineIgnoreAttribute)));
			foreach (var field in allowedFields) {
				Selector.Items.Add(new CommonDropDownList.Item(field.Name, field.GetValue(null)));
			}
			Selector.Changed += a => {
				if (a.ChangedByUser)
					SetProperty((T)Selector.Items[a.Index].Value);
			};
			Selector.AddChangeWatcher(CoalescedPropertyValue(), v => Selector.Value = v);
		}
	}

	public class BooleanPropertyEditor : CommonPropertyEditor<bool>
	{
		private CheckBox checkBox;

		public BooleanPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			checkBox = new ThemedCheckBox { LayoutCell = new LayoutCell(Alignment.LeftCenter) };
			ContainerWidget.AddNode(checkBox);
			checkBox.Changed += args => {
				if (args.ChangedByUser)
				{
					SetProperty(args.Value);
				}
			};
			checkBox.AddChangeWatcher(CoalescedPropertyValue(), v => checkBox.Checked = v);
		}
	}

	public class FloatPropertyEditor : CommonPropertyEditor<float>
	{
		private NumericEditBox editor;

		public FloatPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			editor = editorParams.NumericEditBoxFactory();
			ContainerWidget.AddNode(editor);
			var current = CoalescedPropertyValue();
			editor.Submitted += text => {
				float newValue;
				if (float.TryParse(text, out newValue)) {
					SetProperty(newValue);
				}

				editor.Text = current.GetValue().ToString();
			};
			editor.AddChangeWatcher(current, v => editor.Text = v.ToString());
		}
	}

	public class IntPropertyEditor : CommonPropertyEditor<int>
	{
		private EditBox editor;

		public IntPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			editor = editorParams.EditBoxFactory();
			editor.MinMaxWidth = 80;
			editor.LayoutCell = new LayoutCell(Alignment.Center);
			ContainerWidget.AddNode(editor);
			var current = CoalescedPropertyValue();
			editor.Submitted += text => {
				int newValue;
				if (int.TryParse(text, out newValue)) {
					SetProperty(newValue);
				} else {
					editor.Text = current.GetValue().ToString();
				}
			};
			editor.AddChangeWatcher(current, v => editor.Text = v.ToString());
		}
	}

	public class Color4PropertyEditor : ExpandablePropertyEditor<Color4>
	{
		private EditBox editor;

		public Color4PropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			ColorBoxButton colorBox;
			var panel = new ColorPickerPanel();
			var currentColor = CoalescedPropertyValue(Color4.White).DistinctUntilChanged();
			ContainerWidget.AddNode(new Widget {
				Layout = new HBoxLayout { CellDefaults = new LayoutCell(Alignment.Center) },
				Nodes = {
					(editor = editorParams.EditBoxFactory()),
					new HSpacer(4),
					(colorBox = new ColorBoxButton(currentColor)),
					CreatePipetteButton(),
				}
			});
			ExpandableContent.AddNode(panel.Widget);
			panel.Widget.Padding.Right = 12;
			panel.Widget.Tasks.Add(currentColor.Consume(v => panel.Color = v));
			panel.Changed += () => {
				EditorParams.History?.RollbackTransaction();
				SetProperty(panel.Color);
			};
			panel.DragStarted += () => EditorParams.History?.BeginTransaction();
			panel.DragEnded += () => {
				EditorParams.History?.CommitTransaction();
				EditorParams.History?.EndTransaction();
			};
			colorBox.Clicked += () => Expanded = !Expanded;
			var currentColorString = currentColor.Select(i => i.ToString(Color4.StringPresentation.Dec));
			editor.Submitted += text => {
				Color4 newColor;
				if (Color4.TryParse(text, out newColor)) {
					SetProperty(newColor);
				} else {
					editor.Text = currentColorString.GetValue();
				}
			};
			editor.Tasks.Add(currentColorString.Consume(v => editor.Text = v));
		}


		private Node CreatePipetteButton()
		{
			var button = new ToolbarButton {
				Texture = IconPool.GetTexture("Tools.Pipette"),
			};
			button.Tasks.Add(UIProcessors.PickColorProcessor(button, v => SetProperty(v)));
			return button;
		}

		class ColorBoxButton : Button
		{
			public ColorBoxButton(IDataflowProvider<Color4> colorProvider)
			{
				Nodes.Clear();
				Size = MinMaxSize = new Vector2(25, Theme.Metrics.DefaultButtonSize.Y);
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

	public abstract class FilePropertyEditor<T> : CommonPropertyEditor<T>
	{
		protected readonly EditBox editor;
		protected readonly Button button;
		protected static string LastOpenedDirectory = Project.Current.GetSystemDirectory(Document.Current.Path);

		protected FilePropertyEditor(IPropertyEditorParams editorParams, string[] allowedFileTypes) : base(editorParams)
		{
			ContainerWidget.AddNode(new Widget {
				Layout = new HBoxLayout(),
				Nodes = {
					(editor = editorParams.EditBoxFactory()),
					new HSpacer(4),
					(button = new ThemedButton {
						Text = "...",
						MinMaxWidth = 20,
						LayoutCell = new LayoutCell(Alignment.Center)
					})
				}
			});
			editor.LayoutCell = new LayoutCell(Alignment.Center);
			editor.Submitted += text => AssignAsset(AssetPath.CorrectSlashes(text));
			button.Clicked += () => {
				var dlg = new FileDialog {
					AllowedFileTypes = allowedFileTypes,
					Mode = FileDialogMode.Open,
					InitialDirectory = Directory.Exists(LastOpenedDirectory) ? LastOpenedDirectory : Project.Current.GetSystemDirectory(Document.Current.Path),
				};
				if (dlg.RunModal()) {
					SetFilePath(dlg.FileName);
					LastOpenedDirectory = Project.Current.GetSystemDirectory(dlg.FileName);
				}
			};
		}

		private void SetFilePath(string path)
		{
			string asset, type;
			if (Utils.ExtractAssetPathOrShowAlert(path, out asset, out type)) {
				AssignAsset(AssetPath.CorrectSlashes(asset));
			}
		}

		public override void DropFiles(IEnumerable<string> files)
		{
			var nodeUnderMouse = WidgetContext.Current.NodeUnderMouse;
			if (nodeUnderMouse != null && nodeUnderMouse.SameOrDescendantOf(editor) && files.Any()) {
				SetFilePath(files.First());
			}
		}

		protected override void Copy()
		{
			Clipboard.Text = editor.Text;
		}

		protected override void Paste()
		{
			try {
				AssignAsset(AssetPath.CorrectSlashes(Clipboard.Text));
			} catch (System.Exception) { }
		}

		protected abstract void AssignAsset(string path);
	}

	public class TexturePropertyEditor : FilePropertyEditor<ITexture>
	{
		public TexturePropertyEditor(IPropertyEditorParams editorParams) : base(editorParams, new string[] { "png" })
		{
			editor.AddChangeWatcher(CoalescedPropertyValue(), v => editor.Text = v?.SerializationPath ?? "");
		}

		protected override void AssignAsset(string path)
		{
			SetProperty(new SerializableTexture(path));
		}
	}

	public class RenderTexturePropertyEditor : CommonPropertyEditor<ITexture>
	{
		private EditBox editor;

		public RenderTexturePropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			editor = editorParams.EditBoxFactory();
			editor.IsReadOnly = true;
			editor.LayoutCell = new LayoutCell(Alignment.Center);
			ContainerWidget.AddNode(editor);
			editor.AddChangeWatcher(CoalescedPropertyValue(), v =>
				editor.Text = v == null ?
				"RenderTexture (null)" :
				$"RenderTexture ({v.ImageSize.Width}x{v.ImageSize.Height})"
			);
		}
	}

	public class AudioSamplePropertyEditor : FilePropertyEditor<SerializableSample>
	{
		public AudioSamplePropertyEditor(IPropertyEditorParams editorParams) : base(editorParams, new string[] { "ogg" })
		{
			editor.AddChangeWatcher(CoalescedPropertyValue(), v => editor.Text = v?.SerializationPath ?? "");
		}

		protected override void AssignAsset(string path)
		{
			SetProperty(new SerializableSample(path));
		}
	}

	public class ContentsPathPropertyEditor : FilePropertyEditor<string>
	{
		public ContentsPathPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams, Document.AllowedFileTypes)
		{
			editor.AddChangeWatcher(CoalescedPropertyValue(), v => editor.Text = v);
		}

		protected override void AssignAsset(string path)
		{
			SetProperty(path);
			Document.Current.RefreshExternalScenes();
		}
	}

	public class FontPropertyEditor : CommonPropertyEditor<SerializableFont>
	{
		private DropDownList selector;

		public FontPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			selector = editorParams.DropDownListFactory();
			selector.LayoutCell = new LayoutCell(Alignment.Center);
			ContainerWidget.AddNode(selector);
			var propType = editorParams.PropertyInfo.PropertyType;
			var items = AssetBundle.Current.EnumerateFiles("Fonts").
				Where(i => i.EndsWith(".fnt") || i.EndsWith(".tft")).
				Select(i => new DropDownList.Item(Path.ChangeExtension(Path.GetFileName(i), null)));
			foreach (var i in items) {
				selector.Items.Add(i);
			}
			selector.Text = GetFontName(CoalescedPropertyValue().GetValue());
			selector.Changed += a => {
				SetProperty(new SerializableFont((string)a.Value));
			};
			selector.AddChangeWatcher(CoalescedPropertyValue(), i => {
				selector.Text = GetFontName(i);
			});
		}

		private static string GetFontName(SerializableFont i)
		{
			return string.IsNullOrEmpty(i?.Name) ? "Default" : i.Name;
		}
	}

	public class TriggerPropertyEditor : CommonPropertyEditor<string>
	{
		private ComboBox comboBox;

		public TriggerPropertyEditor(IPropertyEditorParams editorParams, bool multiline = false) : base(editorParams)
		{
			comboBox = new ThemedComboBox { LayoutCell = new LayoutCell(Alignment.Center) };
			ContainerWidget.AddNode(comboBox);
			comboBox.Changed += ComboBox_Changed;
			foreach (var obj in editorParams.Objects) {
				var node = (Node)obj;
				foreach (var a in node.Animations) {
					foreach (var m in a.Markers.Where(i => i.Action != MarkerAction.Jump && !string.IsNullOrEmpty(i.Id))) {
						var id = a.Id != null ? m.Id + '@' + a.Id : m.Id;
						if (!comboBox.Items.Any(i => i.Text == id)) {
							comboBox.Items.Add(new DropDownList.Item(id));
						}
					}
				}
			}
			comboBox.AddChangeWatcher(CoalescedPropertyValue(), v => comboBox.Text = v);
		}

		void ComboBox_Changed(DropDownList.ChangedEventArgs args)
		{
			if (!args.ChangedByUser)
				return;
			var newTrigger = (string)args.Value;
			var currentTriggers = CoalescedPropertyValue().GetValue();
			if (string.IsNullOrWhiteSpace(currentTriggers) || args.Index < 0) {
				// Keep existing and remove absent triggers after hand input.
				var availableTriggers = new HashSet<string>(comboBox.Items.Select(item => item.Text));
				var setTrigger = string.Join(
					",",
					newTrigger.
						Split(',').
						Select(el => el.Trim()).
						Where(el => availableTriggers.Contains(el)).
						Distinct(new TriggerStringComparer())
				);

				SetProperty(setTrigger.Length == 0 ? null : setTrigger);
				if (setTrigger != newTrigger) {
					comboBox.Text = setTrigger;
				}
				return;
			}
			var triggers = new List<string>();
			var added = false;
			string newMarker, newAnimation;
			SplitTrigger(newTrigger, out newMarker, out newAnimation);
			foreach (var trigger in currentTriggers.Split(',').Select(i => i.Trim())) {
				string marker, animation;
				SplitTrigger(trigger, out marker, out animation);
				if (animation == newAnimation) {
					if (!added) {
						added = true;
						triggers.Add(newTrigger);
					}
				} else {
					triggers.Add(trigger);
				}
			}
			if (!added) {
				triggers.Add(newTrigger);
			}
			var newValue = string.Join(",", triggers);
			SetProperty(newValue);
			comboBox.Text = newValue;
		}

		private static void SplitTrigger(string trigger, out string markerId, out string animationId)
		{
			if (!trigger.Contains('@')) {
				markerId = trigger;
				animationId = null;
			} else {
				var t = trigger.Split('@');
				markerId = t[0];
				animationId = t[1];
			}
		}

		private class TriggerStringComparer : IEqualityComparer<string>
		{

			public bool Equals(string x, string y)
			{
				string xMarker;
				string yMarker;
				string xAnimation;
				string yAnimation;
				SplitTrigger(x, out xMarker, out xAnimation);
				SplitTrigger(y, out yMarker, out yAnimation);
				return xAnimation == yAnimation;
			}

			public int GetHashCode(string obj)
			{
				string marker;
				string animation;
				SplitTrigger(obj, out marker, out animation);
				return animation == null ? 0 : animation.GetHashCode();
			}
		}

	}

	public class AnchorsPropertyEditor : CommonPropertyEditor<Anchors>
	{
		private ToolbarButton firstButton;
		private Widget group;

		public AnchorsPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			group = new Widget { Layout = new HBoxLayout { CellDefaults = new LayoutCell(Alignment.Center), Spacing = 4 } };
			ContainerWidget.AddNode(group);
			firstButton = AddButton(Anchors.Left, "Anchor to the left");
			AddButton(Anchors.Right, "Anchor to the right");
			AddButton(Anchors.Top, "Anchor to the top");
			AddButton(Anchors.Bottom, "Anchor to the bottom");
			AddButton(Anchors.CenterH, "Anchor to the center horizontally");
			AddButton(Anchors.CenterV, "Anchor to the center vertically");
		}

		ToolbarButton AddButton(Anchors anchor, string tip)
		{
			var tb = new AnchorButton { LayoutCell = new LayoutCell(Alignment.Center), Tip = tip };
			group.AddNode(tb);
			var current = CoalescedPropertyValue();
			tb.CompoundPresenter.Insert(0, new DelegatePresenter<Widget>(w => DrawIcon(w, anchor)));
			tb.Clicked += () => {
				tb.Checked = !tb.Checked;
				SetProperty(tb.Checked ? current.GetValue() | anchor : current.GetValue() & ~anchor);
			};
			tb.AddChangeWatcher(current, v => tb.Checked = (v & anchor) != 0);
			return tb;
		}

		float[] a = { 0, 0, 1, 0, 0, 0, 0, 1, 0.5f, 0, 0, 0.5f };
		float[] b = { 0, 1, 1, 1, 1, 0, 1, 1, 0.5f, 1, 1, 0.5f };

		void DrawIcon(Widget button, Anchors anchor)
		{
			button.PrepareRendererState();
			int t = -1;
			while (anchor != Anchors.None) {
				anchor = (Anchors)((int)anchor >> 1);
				t++;
			}
			var w = button.Width;
			var h = button.Height;
			Renderer.DrawLine(
				Scale(a[t * 2], w), Scale(a[t * 2 + 1], h),
				Scale(b[t * 2], w), Scale(b[t * 2 + 1], h),
				ColorTheme.Current.Basic.BlackText);
		}

		float Scale(float x, float s)
		{
			x *= s;
			if (x == 0) x += 4;
			if (x == s) x -= 4;
			return x;
		}

		class AnchorButton : ToolbarButton
		{
			protected override void GetColors(State state, out Color4 bgColor, out Color4 borderColor)
			{
				base.GetColors(state, out bgColor, out borderColor);
				if (state == State.Default && !Checked) {
					bgColor = ColorTheme.Current.Basic.WhiteBackground;
					borderColor = ColorTheme.Current.Basic.ControlBorder;
				}
			}
		}
	}

	public class SkinningWeightsPropertyEditor : ExpandablePropertyEditor<SkinningWeights>
	{
		private readonly NumericEditBox[] indexEditors;
		private readonly NumericEditBox[] weigthsEditors;
		public SkinningWeightsPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			editorParams.DefaultValueGetter = () => new SkinningWeights();
			indexEditors = new NumericEditBox[4];
			weigthsEditors = new NumericEditBox[4];
			foreach (var o in editorParams.Objects) {
				var prop = new Property<SkinningWeights>(o, editorParams.PropertyName).Value;
			}
			for (var i = 0; i <= 3; i++) {
				indexEditors[i] = editorParams.NumericEditBoxFactory();
				indexEditors[i].Step = 1;
				weigthsEditors[i] = editorParams.NumericEditBoxFactory();
				var wrapper = new Widget {
					Padding = new Thickness { Left = 20 },
					Layout = new HBoxLayout(),
					LayoutCell = new LayoutCell { StretchY = 0 }
				};
				var propertyLabel = new ThemedSimpleText {
					Text = $"Bone { char.ConvertFromUtf32(65 + i) }",
					VAlignment = VAlignment.Center,
					LayoutCell = new LayoutCell(Alignment.LeftCenter, 0),
					ForceUncutText = false,
					MinWidth = 140,
					OverflowMode = TextOverflowMode.Minify,
					HitTestTarget = true,
					TabTravesable = new TabTraversable(),
				};
				wrapper.AddNode(propertyLabel);
				wrapper.AddNode(new Widget {
					Layout = new HBoxLayout { CellDefaults = new LayoutCell(Alignment.Center), Spacing = 4 },
					Nodes = {
						indexEditors[i] ,
						weigthsEditors[i]
					}
				});
				ExpandableContent.AddNode(wrapper);
				SetLink(i, CoalescedPropertyValue(new SkinningWeights()));
			}
		}

		private void SetLink(int idx, IDataflowProvider<SkinningWeights> provider)
		{
			var currentValue = provider.GetValue();
			indexEditors[idx].Submitted += text => SetIndexValue(EditorParams, idx, indexEditors[idx], currentValue);
			weigthsEditors[idx].Submitted += text => SetWeightValue(EditorParams, idx, weigthsEditors[idx], currentValue);
			indexEditors[idx].AddChangeWatcher(provider, v => indexEditors[idx].Text = v[idx].Index.ToString());
			weigthsEditors[idx].AddChangeWatcher(provider, v => weigthsEditors[idx].Text = v[idx].Weight.ToString());
		}

		private void SetIndexValue(IPropertyEditorParams editorParams, int idx, CommonEditBox editor, SkinningWeights sw)
		{
			float newValue;
			if (float.TryParse(editor.Text, out newValue)) {
				DoTransaction(() => {
					foreach (var obj in editorParams.Objects) {
						var prop = new Property<SkinningWeights>(obj, editorParams.PropertyName).Value.Clone();
						prop[idx] = new BoneWeight {
							Index = (int)newValue,
							Weight = prop[idx].Weight
						};
						editorParams.PropertySetter(obj, editorParams.PropertyName, prop);
					}
				});
			} else {
				editor.Text = sw[idx].Index.ToString();
			}
		}

		private void SetWeightValue(IPropertyEditorParams editorParams, int idx, CommonEditBox editor, SkinningWeights sw)
		{
			float newValue;
			if (float.TryParse(editor.Text, out newValue)) {
				DoTransaction(() => {
					foreach (var obj in editorParams.Objects) {
						var prop = new Property<SkinningWeights>(obj, editorParams.PropertyName).Value.Clone();
						prop[idx] = new BoneWeight {
							Index = prop[idx].Index,
							Weight = newValue
						};
						editorParams.PropertySetter(obj, editorParams.PropertyName, prop);
					}
				});
			} else {
				editor.Text = sw[idx].Weight.ToString();
			}
		}
	}

	public class RenderTargetPropertyEditor : EnumPropertyEditor<RenderTarget>
	{
		private const string SmallTexDesc = " (256x256)";
		private const string MiddleTexDesc = " (512x512)";
		private const string LargeTexDesc = " (1024x1024)";

		public RenderTargetPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			Selector.Items[1].Text += SmallTexDesc;
			Selector.Items[2].Text += SmallTexDesc;
			Selector.Items[3].Text += MiddleTexDesc;
			Selector.Items[4].Text += LargeTexDesc;
			Selector.Items[5].Text += LargeTexDesc;
			Selector.Items[6].Text += LargeTexDesc;
			Selector.Items[7].Text += LargeTexDesc;
		}
	}

	public class BlendingPropertyEditor : EnumPropertyEditor<Blending>
	{
		private static readonly Dictionary<string, string> blendingToPhotoshopAnalog = new Dictionary<string, string> {
			{Blending.Alpha.ToString(), "Normal"},
			{Blending.Add.ToString(), "Linear Dodge"},
			{Blending.Glow.ToString(), "Normal with Brightness"},
			{Blending.Modulate.ToString(), "Multiply without Transparency"},
			{Blending.Burn.ToString(), "Multiply"},
			{Blending.Darken.ToString(), "Normal with Darkness"},
			{Blending.Opaque.ToString(), "Normal without Transparency"},
		};

		public BlendingPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			foreach (var item in Selector.Items) {
				string photoshopAnalog;
				if (blendingToPhotoshopAnalog.TryGetValue(item.Text, out photoshopAnalog)) {
					item.Text += $" ({photoshopAnalog})";
				}
			}
		}
	}

	public class ShortcutPropertyEditor : CommonPropertyEditor<Shortcut>
	{
		private EditBox editor;
		private Modifiers modifiers;
		private Key main;

		private WidgetFlatFillPresenter flatFillPresenter;

		public Action PropertyChanged { get; set; }

		private void SetValue(Shortcut value)
		{
			var oldValue = CoalescedPropertyValue().GetValue();
			foreach (var obj in EditorParams.Objects) {
				EditorParams.PropertySetter(obj, EditorParams.PropertyName, value);
			}
			if (value != oldValue) {
				PropertyChanged?.Invoke();
			}
		}

		public ShortcutPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			editor = editorParams.EditBoxFactory();
			editor.Updating += Updating;
			editor.Updated += Updated;
			editor.LayoutCell = new LayoutCell(Alignment.Center);
			editor.AddChangeWatcher(CoalescedPropertyValue(), v => {
				var text = v.ToString();
				editor.Text = v.Main != Key.Unknown ? text : text.Replace("Unknown", "");
			});
			editor.IsReadOnly = true;
			editor.TextWidget.Tasks.Clear();
			editor.TextWidget.Position = new Vector2(0, editor.MinHeight / 2);
			editor.TextWidget.Padding = new Thickness(5, 0);
			editor.Gestures.Add(new ClickGesture(() => editor.SetFocus()));
			editor.Gestures.Add(new ClickGesture(1, () => {
				main = Key.Unknown;
				modifiers = Modifiers.None;
				SetValue(new Shortcut(modifiers, main));
			}));
			editor.AddToNode(ContainerWidget);

			PropertyLabel.Tasks.Clear();
			PropertyLabel.Tasks.Add(ManageLabelFocus());
			ContainerWidget.Tasks.Add(ManageFocusTask());

			var value = CoalescedPropertyValue().GetValue();
			main = value.Main;
			modifiers = value.Modifiers;
			flatFillPresenter = new WidgetFlatFillPresenter(Theme.Colors.GrayBackground);
			ContainerWidget.CompoundPresenter.Add(flatFillPresenter);
		}

		private void PressModifier(Modifiers modifier, Key key)
		{
			var input = editor.Input;
			if (input.IsKeyPressed(key)) {
				modifiers |= modifier;
			}
		}
		
		IEnumerator<object> ManageLabelFocus()
		{
			while (true) {
				if (PropertyLabel.Input.WasMouseReleased()) {
					PropertyLabel.SetFocus();
				}
				yield return null;
			}
		}

		IEnumerator<object> ManageFocusTask()
		{
			while (true) {
				if (PropertyLabel.IsFocused()) {
					editor.SetFocus();
				}
				flatFillPresenter.Color = editor.IsFocused() ? Theme.Colors.SelectedBackground : Theme.Colors.GrayBackground;
				yield return null;
			}
		}

		private void Updating(float dt)
		{
			if (!editor.IsFocused())
				return;
			var input = editor.Input;
			var keys = Key.Enumerate().Where(k => input.WasKeyPressed(k));
			if (!keys.Any())
				return;
			modifiers = Modifiers.None;

			PressModifier(Modifiers.Alt, Key.Alt);
			PressModifier(Modifiers.Shift, Key.Shift);
			PressModifier(Modifiers.Control, Key.Control);
			PressModifier(Modifiers.Win, Key.Win);
			foreach (var key in keys) {
				if (!key.IsModifier() && !key.IsMouseKey() && Shortcut.ValidateMainKey(key)) {
					main = key;
					SetValue(new Shortcut(modifiers, main));
					return;
				}
			}
		}

		private void Updated(float dt)
		{
			if (!editor.IsFocused())
				return;
			var input = editor.Input;
			input.ConsumeKeys(Key.Enumerate().Where(
				k => input.WasKeyRepeated(k) || input.WasKeyPressed(k) || input.WasKeyReleased(k)));
			Command.ConsumeRange(Command.Editing);
		}
	}
}
