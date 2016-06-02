using System;
using Lime;
using System.Linq;
using Tangerine.Core;
using System.Collections.Generic;

namespace Tangerine.UI.Inspector
{
	public interface IPropertyEditor
	{
		void Update(float delta);
	}

	public class Inspector
	{
		public delegate IPropertyEditor PropertyEditorBuilder(PropertyEditorContext context);

		public static Inspector Instance { get; private set; }

		public readonly KeyboardFocusController Focus;
		public readonly Widget RootWidget;
		public readonly Widget ContentWidget;
		public readonly List<Node> Nodes;
		public readonly Dictionary<Type, PropertyEditorBuilder> EditorMap;
		public readonly TaskList Tasks = new TaskList();
		public readonly List<IPropertyEditor> Editors;

		public static void Initialize(Widget rootWidget)
		{
			Instance = new Inspector(rootWidget);
		}

		private Inspector(Widget rootWidget)
		{
			RootWidget = rootWidget;
			ContentWidget = new Widget();
			Focus = new KeyboardFocusController(RootWidget);
			Nodes = new List<Node>();
			EditorMap = new Dictionary<Type, PropertyEditorBuilder>();
			Editors = new List<IPropertyEditor>();
			RegisterEditors();
			InitializeWidgets();
			CreateTasks();
			RootWidget.Updating += Update;
		}

		void InitializeWidgets()
		{
			ContentWidget.Layout = new TableLayout { Tag = "InspectorContent", Spacing = 4, ColCount = 4, RowCount = 6 };
			ContentWidget.Padding = new Thickness(4);
			RootWidget.Layout = new StackLayout();
			RootWidget.AddNode(ContentWidget);
		}

		private void RegisterEditors()
		{
			EditorMap.Add(typeof(Vector2), c => new Vector2Editor(c));
		}

		void CreateTasks()
		{
			Tasks.Add(new UpdatePropertyGridTask().Main());
		}

		void Update(float delta)
		{
			Tasks.Update(delta);
			Document.Current.History.Commit();
		}

		public class PropertyEditorContext
		{
			public readonly Widget InspectorPane;
			public readonly Node Node;
			public readonly IAnimable Animable;
			public readonly string Property;
			public readonly string AnimationId;

			public PropertyEditorContext(Widget inspectorPane, Node node, IAnimable animable, string property, string animationId)
			{
				InspectorPane = inspectorPane;
				Node = node;
				Animable = animable;
				Property = property;
				AnimationId = animationId;
			}

			public IAnimator FindAnimator()
			{
				IAnimator animator;
				return Animable.Animators.TryFind(Property, out animator, AnimationId) ? animator : null;
			}

			public IKeyframe FindKeyframe()
			{
				var animation = AnimationId == null ? Node.DefaultAnimation : Node.Animations.Find(AnimationId);
				return FindAnimator()?.ReadonlyKeys.FirstOrDefault(k => k.Frame == animation.Frame);
			}
		}

		class CommonPropertyEditor : IPropertyEditor
		{
			protected readonly KeyframeChangeNotificator KeyframeChangeNotificator;
			protected readonly PropertyEditorContext Context;
			private readonly KeyframeButton keyframeButton;
			private readonly KeyFunctionButton keyFunctionButton;

			public CommonPropertyEditor(PropertyEditorContext context)
			{
				Context = context;
				KeyframeChangeNotificator = new KeyframeChangeNotificator(context);
				context.InspectorPane.AddNode(new SimpleText { Text = context.Property, Padding = new Thickness(8, 0), LayoutCell = new LayoutCell(Alignment.LeftCenter, 0.666f, 0) });
				keyframeButton = new KeyframeButton {
					LayoutCell = new LayoutCell(Alignment.LeftCenter, 0, 0),
				};
				keyFunctionButton = new KeyFunctionButton {
					LayoutCell = new LayoutCell(Alignment.LeftCenter, 0, 0),
				};
				keyframeButton.SetKeyColor(Color4.Red);
				keyFunctionButton.Clicked += KeyFunctionButton_Clicked;
				Context.InspectorPane.AddNode(keyframeButton);
				Context.InspectorPane.AddNode(keyFunctionButton);
			}

			private void KeyFunctionButton_Clicked()
			{
			}

			public virtual void Update(float delta)
			{
				KeyframeChangeNotificator.Update();
				if (KeyframeChangeNotificator.Changed) {
					var k = Context.FindKeyframe();
					keyFunctionButton.Visible = k != null;
					if (k != null) {
						keyFunctionButton.SetKeyFunction(k.Function);
					}
				}
			}

			public class KeyframeButton : Button
			{
				private readonly Image image;

				public KeyframeButton()
				{
					Nodes.Clear();
					Size = MinMaxSize = Metrics.IconSize;
					image = new Image { Size = Size, Shader = ShaderId.Silhuette, Texture = new SerializableTexture() };
					Nodes.Add(image);
					image.PostPresenter = new WidgetBoundsPresenter(Colors.BorderAroundKeyframeColorbox, 1);
				}

				public void SetKeyColor(Color4 color)
				{
					image.Color = color;
				}
			}	

			class KeyFunctionButton : BitmapButton
			{
				public void SetKeyFunction(KeyFunction function)
				{
					var s = "Timeline.Interpolation." + FunctionToString(function);
					HoverTexture = IconPool.GetTexture(s);
					DefaultTexture = IconPool.GetTexture(s + "Grayed");
				}

				string FunctionToString(KeyFunction function)
				{
					switch (function) {
						case KeyFunction.Linear:
							return "Linear";
						case KeyFunction.Steep:
							return "None";
						case KeyFunction.Spline:
							return "Spline";
						case KeyFunction.ClosedSpline:
							return "ClosedSpline";
						default:
							throw new ArgumentException();
					}
				}
			}
		}

		class KeyframeChangeNotificator
		{
			readonly PropertyEditorContext context;

			int animatorCollectionVersion = -1;
			int animatorKeysVersion = -1;
			object keyValue;
			KeyFunction keyFunction;
			IAnimator animator;
			IKeyframe keyframe;

			public bool Changed { get; private set; }

			public KeyframeChangeNotificator(PropertyEditorContext context)
			{
				this.context = context;
			}

			public void Update()
			{
				Changed = false;
				if (animatorCollectionVersion != context.Animable.Animators.Version) {
					Changed = true;
					animator = context.FindAnimator();
				}
				Changed |= animator != null && animator.ReadonlyKeys.Version != animatorKeysVersion;
				if (Changed) {
					keyframe = context.FindKeyframe();
				}
				if (keyframe != null && (keyframe.Function != keyFunction || !keyframe.Value.Equals(keyValue))) {
					Changed = true;
					keyValue = keyframe.Value;
					keyFunction = keyframe.Function;
				}
			}
		}

		class Vector2Editor : CommonPropertyEditor
		{
			Vector2? prevValue;
			readonly System.Reflection.MethodInfo getter;
			readonly EditBox editorX;
			readonly EditBox editorY;

			public Vector2Editor(PropertyEditorContext context) : base(context)
			{
				var prop = Context.Animable.GetType().GetProperty(Context.Property);
				getter = prop.GetGetMethod();
				editorX = new EditBox();
				editorY = new EditBox();
				Context.InspectorPane.AddNode(new Widget {
					LayoutCell = new LayoutCell(Alignment.Center) { StretchY = 0 },
					Layout = new HBoxLayout(),
					Nodes = {
						new SimpleText { Text = "X", Padding = new Thickness(4, 0), LayoutCell = new LayoutCell(Alignment.Center) },
						editorX,
						new SimpleText { Text = "Y", Padding = new Thickness(4, 0), LayoutCell = new LayoutCell(Alignment.Center) },
						editorY,
					}
				});
			}

			public override void Update(float delta)
			{
				base.Update(delta);
				var value = (Vector2)getter.Invoke(Context.Animable, null);
				if (!prevValue.HasValue || value != prevValue) {
					prevValue = value;
					editorX.Text = value.X.ToString();
					editorY.Text = value.Y.ToString();
				}
			}
		}

		class UpdatePropertyGridTask
		{
			Inspector Inspector => Instance;

			public IEnumerator<object> Main()
			{
				var nodes = Inspector.Nodes;
				while (true) {
					var selectedNodes = Document.Current.SelectedNodes;
					if (!nodes.SequenceEqual(selectedNodes)) {
						nodes.Clear();
						nodes.AddRange(selectedNodes);
						RebuildContent();
					}
					foreach (var i in Inspector.Editors) {
						i.Update(Task.Current.Delta);
					}
					yield return null;
				}
			}

			void RebuildContent()
			{
				Inspector.ContentWidget.Nodes.Clear();
				if (Inspector.Nodes.Count > 0) {
					PopulateContent(Inspector.Nodes[0], Inspector.Nodes[0], null);
				}
			}

			void PopulateContent(Node node, IAnimable animable, string animationId)
			{
				Inspector.Editors.Clear();
				foreach (var prop in animable.GetType().GetProperties()) {
					var a = prop.GetCustomAttributes(typeof(TangerineAttribute), false);
					if (a.Length == 0) {
						continue;
					}
					PropertyEditorBuilder editorBuilder;
					if (!Inspector.EditorMap.TryGetValue(prop.PropertyType, out editorBuilder)) {
						continue;
					}
					var context = new PropertyEditorContext(Inspector.ContentWidget, node, animable, prop.Name, animationId);
					var propertyEditor = editorBuilder(context);
					Inspector.Editors.Add(propertyEditor);
				}
			}
		}
	}
}
