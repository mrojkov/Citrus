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
		public readonly object Object;
		public readonly string PropertyName;
		public readonly IAnimationContext AnimationContext;

		public PropertyEditorContext(Widget inspectorPane, object @object, string propertyName, IAnimationContext animationContext)
		{
			InspectorPane = inspectorPane;
			Object = @object;
			PropertyName = propertyName;
			AnimationContext = animationContext;
		}

		public System.Reflection.PropertyInfo PropertyInfo => Object.GetType().GetProperty(PropertyName);

		public TangerineAttribute TangerineAttribute => tangerineAttribute ?? 
		(tangerineAttribute = PropertyRegistry.GetTangerineAttribute(Object.GetType(), PropertyName) ?? new TangerineAttribute(0));

		public IAnimator FindAnimator()
		{
			var animable = Object as IAnimable;
			if (animable == null) {
				return null;
			}
			IAnimator animator;
			return animable.Animators.TryFind(PropertyName, out animator, AnimationContext.AnimationId) ? animator : null;
		}

		public IKeyframe FindKeyframe()
		{
			return FindAnimator()?.ReadonlyKeys.FirstOrDefault(k => k.Frame == AnimationContext.AnimationFrame);
		}
	}

	class CommonPropertyEditor : IPropertyEditor
	{
		readonly KeyframeButton keyframeButton;
		readonly KeyFunctionButton keyFunctionButton;

		protected readonly KeyframeObserver keyframeObserver;
		protected readonly PropertyObserver propertyObserver;
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
			keyframeObserver = new KeyframeObserver(context);
			propertyObserver = new PropertyObserver(context);
			containerWidget.AddNode(new SimpleText {
				Text = context.PropertyName,
				LayoutCell = new LayoutCell(Alignment.LeftCenter, stretchX: 0.5f),
				AutoSizeConstraints = false,
			});
			if (context.Object is IAnimable) {
				keyFunctionButton = new KeyFunctionButton {
					LayoutCell = new LayoutCell(Alignment.LeftCenter, stretchX: 0),
				};
				keyframeButton = new KeyframeButton {
					LayoutCell = new LayoutCell(Alignment.LeftCenter, stretchX: 0),
				};
				var keyColor = KeyframePalette.Colors[context.TangerineAttribute.ColorIndex];
				keyframeButton.SetKeyColor(keyColor);
				keyFunctionButton.Clicked += KeyFunctionButton_Clicked;
				containerWidget.AddNode(keyFunctionButton);
				containerWidget.AddNode(keyframeButton);
			}
		}

		private void KeyFunctionButton_Clicked()
		{
		}

		public virtual void Update(float delta)
		{
			keyframeObserver.Observe();
			propertyObserver.Observe();
			if (keyframeObserver.Changed) {
				var k = context.FindKeyframe();
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
				image.PostPresenter = new WidgetBoundsPresenter(Colors.Inspector.BorderAroundKeyframeColorbox, 1);
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

	interface IDataObserver
	{
		void Observe();
		bool Changed { get; }
	}

	class KeyframeObserver : IDataObserver
	{
		readonly PropertyEditorContext context;

		int animatorCollectionVersion = -1;
		int animatorKeysVersion = -1;
		object keyValue;
		KeyFunction keyFunction;
		IAnimator animator;
		IKeyframe keyframe;

		public bool Changed { get; private set; }

		public KeyframeObserver(PropertyEditorContext context)
		{
			this.context = context;
		}

		public void Observe()
		{
			Changed = false;
			var animable = context.Object as IAnimable;
			if (animable != null && animatorCollectionVersion != animable.Animators.Version) {
				animatorCollectionVersion = animable.Animators.Version;
				Changed = true;
				animator = context.FindAnimator();
			}
			Changed |= animator != null && animator.ReadonlyKeys.Version != animatorKeysVersion;
			if (Changed) {
				if (animator != null) {
					animatorKeysVersion = animator.ReadonlyKeys.Version;
					keyframe = context.FindKeyframe();
				} else {
					animatorKeysVersion = -1;
					keyframe = null;
				}
			}
			if (keyframe != null && (keyframe.Function != keyFunction || !keyframe.Value.Equals(keyValue))) {
				Changed = true;
				keyValue = keyframe.Value;
				keyFunction = keyframe.Function;
			}
		}
	}

	class PropertyObserver : IDataObserver
	{
		readonly PropertyEditorContext context;
		readonly System.Reflection.MethodInfo getter;
		public object Value { get; private set; }
		public bool Changed { get; private set; }

		public PropertyObserver(PropertyEditorContext context)
		{
			this.context = context;
			getter = context.PropertyInfo.GetGetMethod();
		}

		public void Observe()
		{
			var prevValue = Value;
			Value = getter.Invoke(context.Object, null);
			Changed = ((prevValue == null) ^ (Value == null)) || (Value != null && !Value.Equals(prevValue));
		}
	}

	class Vector2PropertyEditor : CommonPropertyEditor
	{
		readonly EditBox editorX;
		readonly EditBox editorY;

		public Vector2PropertyEditor(PropertyEditorContext context) : base(context)
		{
			containerWidget.AddNode(new Widget {
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
			if (propertyObserver.Changed) {
				var value = (Vector2)propertyObserver.Value;
				editorX.Text = value.X.ToString();
					editorY.Text = value.Y.ToString();
			}
		}
	}

	class StringPropertyEditor : CommonPropertyEditor
	{
		readonly EditBox editor;

		public StringPropertyEditor(PropertyEditorContext context) : base(context)
		{
			containerWidget.AddNode(new Widget {
				Layout = new HBoxLayout(),
				Padding = new Thickness { Left = 4 },
				Nodes = {
					(editor = new EditBox()),
				}
			});
			editor.Focusable.FocusLost += () => {
				if (!editor.Text.Equals(propertyObserver.Value)) {
					Document.Current.History.Execute(new Core.Operations.SetAnimableProperty(context.Object, context.PropertyName, editor.Text));
				}
			};
		}

		public override void Update(float delta)
		{
			base.Update(delta);
			if (propertyObserver.Changed) {
				editor.Text = (string)propertyObserver.Value;
			}
		}
	}
}