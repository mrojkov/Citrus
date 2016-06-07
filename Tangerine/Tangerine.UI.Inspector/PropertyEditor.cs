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

	public class PropertyEditorContext
	{
		TangerineAttribute tangerineAttribute;

		public readonly Widget InspectorPane;
		public readonly Node Node;
		public readonly IAnimable Animable;
		public readonly string PropertyName;
		public readonly string AnimationId;

		public PropertyEditorContext(Widget inspectorPane, Node node, IAnimable animable, string propertyName, string animationId)
		{
			InspectorPane = inspectorPane;
			Node = node;
			Animable = animable;
			PropertyName = propertyName;
			AnimationId = animationId;
		}

		public System.Reflection.PropertyInfo PropertyInfo => Animable.GetType().GetProperty(PropertyName);

		public TangerineAttribute TangerineAttribute => tangerineAttribute ?? 
		(tangerineAttribute = PropertyRegistry.GetTangerineAttribute(Animable.GetType(), PropertyName) ?? new TangerineAttribute(0));

		public IAnimator FindAnimator()
		{
			IAnimator animator;
			return Animable.Animators.TryFind(PropertyName, out animator, AnimationId) ? animator : null;
		}

		public IKeyframe FindKeyframe()
		{
			var animation = AnimationId == null ? Node.DefaultAnimation : Node.Animations.Find(AnimationId);
			return FindAnimator()?.ReadonlyKeys.FirstOrDefault(k => k.Frame == animation.Frame);
		}
	}

	class CommonPropertyEditor : IPropertyEditor
	{
		readonly KeyframeButton keyframeButton;
		readonly KeyFunctionButton keyFunctionButton;

		protected readonly KeyframeChangeNotificator KeyframeChangeNotificator;
		protected readonly PropertyEditorContext Context;
		protected readonly Widget ContainerWidget;

		public CommonPropertyEditor(PropertyEditorContext context)
		{
			Context = context;
			ContainerWidget = new Widget {
				Layout = new HBoxLayout { IgnoreHidden = false },
				LayoutCell = new LayoutCell { StretchY = 0 },
			};
			context.InspectorPane.AddNode(ContainerWidget);
			KeyframeChangeNotificator = new KeyframeChangeNotificator(context);
			ContainerWidget.AddNode(new SimpleText {
				Text = context.PropertyName,
				LayoutCell = new LayoutCell(Alignment.LeftCenter, stretchX: 0.5f),
				AutoSizeConstraints = false,
			});
			keyFunctionButton = new KeyFunctionButton {
				LayoutCell = new LayoutCell(Alignment.LeftCenter, stretchX: 0),
			};
			keyframeButton = new KeyframeButton {
				LayoutCell = new LayoutCell(Alignment.LeftCenter, stretchX: 0),
			};
			var keyColor = KeyframePalette.Colors[Context.TangerineAttribute.ColorIndex];
			keyframeButton.SetKeyColor(keyColor);
			keyFunctionButton.Clicked += KeyFunctionButton_Clicked;
			ContainerWidget.AddNode(keyFunctionButton);
			ContainerWidget.AddNode(keyframeButton);
		}

		private void KeyFunctionButton_Clicked()
		{
		}

		public virtual void Update(float delta)
		{
			KeyframeChangeNotificator.Update();
			if (KeyframeChangeNotificator.Changed) {
				var k = Context.FindKeyframe();
				keyFunctionButton.Visible = (k != null);
				if (k != null) {
					keyFunctionButton.SetKeyFunction(k.Function);
				}
			}
		}

		public class KeyframeButton : Button
		{
			readonly Image image;

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

	class Vector2PropertyEditor : CommonPropertyEditor
	{
		Vector2? prevValue;
		readonly System.Reflection.MethodInfo getter;
		readonly EditBox editorX;
		readonly EditBox editorY;

		public Vector2PropertyEditor(PropertyEditorContext context) : base(context)
		{
			getter = context.PropertyInfo.GetGetMethod();
			ContainerWidget.AddNode(new Widget {
				Layout = new HBoxLayout(),
				Nodes = {
					new SimpleText { Text = "X", Padding = new Thickness(4, 0), LayoutCell = new LayoutCell(Alignment.Center) },
					(editorX = new EditBox()),
					 new SimpleText { Text = "Y", Padding = new Thickness(4, 0), LayoutCell = new LayoutCell(Alignment.Center) },
					(editorY = new EditBox()),
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

	class StringPropertyEditor : CommonPropertyEditor
	{
		string prevValue;
		readonly System.Reflection.MethodInfo getter;
		readonly EditBox editor;

		public StringPropertyEditor(PropertyEditorContext context) : base(context)
		{
			getter = Context.PropertyInfo.GetGetMethod();
			ContainerWidget.AddNode(new Widget {
				Layout = new HBoxLayout(),
				Padding = new Thickness { Left = 4 },
				Nodes = {
					(editor = new EditBox()),
				}
			});
		}

		public override void Update(float delta)
		{
			base.Update(delta);
			var value = (string)getter.Invoke(Context.Animable, null);
			if (value != prevValue) {
				prevValue = value;
				editor.Text = value;
			}
		}
	}
}