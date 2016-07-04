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
		public readonly List<object> Objects;
		public readonly Type PropertyType;
		public readonly string PropertyName;
		public readonly IAnimationContext AnimationContext;

		public PropertyEditorContext(Widget inspectorPane, List<object> objects, Type propertyType, string propertyName, IAnimationContext animationContext)
		{
			InspectorPane = inspectorPane;
			Objects = objects;
			PropertyType = propertyType;
			PropertyName = propertyName;
			AnimationContext = animationContext;
		}

		public System.Reflection.PropertyInfo PropertyInfo => PropertyType.GetProperty(PropertyName);

		public TangerineAttribute TangerineAttribute => tangerineAttribute ?? 
		(tangerineAttribute = PropertyRegistry.GetTangerineAttribute(PropertyType, PropertyName) ?? new TangerineAttribute(0));

		//public IAnimator FindAnimator()
		//{
		//	var animable = Object as IAnimable;
		//	if (animable == null) {
		//		return null;
		//	}
		//	IAnimator animator;
		//	return animable.Animators.TryFind(PropertyName, out animator, AnimationContext.AnimationId) ? animator : null;
		//}

		//public IKeyframe FindKeyframe()
		//{
		//	return FindAnimator()?.ReadonlyKeys.FirstOrDefault(k => k.Frame == AnimationContext.AnimationFrame);
		//}
	}

	//class KeyframeDataflow : IDataflow<IKeyframe>
	//{
	//	readonly PropertyEditorContext context;

	//	int animatorCollectionVersion = -1;
	//	int animatorKeysVersion = -1;
	//	object keyValue;
	//	KeyFunction keyFunction;
	//	IAnimator animator;
	//	IKeyframe keyframe;

	//	public IKeyframe Value => keyframe;
	//	public bool GotValue { get; private set; }

	//	public KeyframeDataflow(PropertyEditorContext context)
	//	{
	//		this.context = context;
	//	}

	//	public void Poll()
	//	{
	//		GotValue = false;
	//		var animable = context.Object as IAnimable;
	//		if (animable != null && animatorCollectionVersion != animable.Animators.Version) {
	//			animatorCollectionVersion = animable.Animators.Version;
	//			GotValue = true;
	//			animator = context.FindAnimator();
	//		}
	//		GotValue |= animator != null && animator.ReadonlyKeys.Version != animatorKeysVersion;
	//		if (GotValue) {
	//			if (animator != null) {
	//				animatorKeysVersion = animator.ReadonlyKeys.Version;
	//				keyframe = context.FindKeyframe();
	//			} else {
	//				animatorKeysVersion = -1;
	//				keyframe = null;
	//			}
	//		}
	//		if (Value != null && (keyframe.Function != keyFunction || !keyframe.Value.Equals(keyValue))) {
	//			GotValue = true;
	//			keyValue = keyframe.Value;
	//			keyFunction = keyframe.Function;
	//		}
	//	}
	//}

	class CommonPropertyEditor : IPropertyEditor
	{
		readonly KeyframeButton keyframeButton;
		readonly KeyFunctionButton keyFunctionButton;

		// protected readonly KeyframeDataflow keyframeDataflow;
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
			// keyframeDataflow = new KeyframeDataflow(context);
			containerWidget.AddNode(new SimpleText {
				Text = context.PropertyName,
				LayoutCell = new LayoutCell(Alignment.LeftCenter, stretchX: 0.5f),
				AutoSizeConstraints = false,
			});
			//if (context.Object is IAnimable) {
			//	keyFunctionButton = new KeyFunctionButton {
			//		LayoutCell = new LayoutCell(Alignment.LeftCenter, stretchX: 0),
			//	};
			//	keyframeButton = new KeyframeButton {
			//		LayoutCell = new LayoutCell(Alignment.LeftCenter, stretchX: 0),
			//	};
			//	var keyColor = KeyframePalette.Colors[context.TangerineAttribute.ColorIndex];
			//	keyframeButton.SetKeyColor(keyColor);
			//	keyFunctionButton.Clicked += KeyFunctionButton_Clicked;
			//	containerWidget.AddNode(keyFunctionButton);
			//	containerWidget.AddNode(keyframeButton);
			//}
		}

		private void KeyFunctionButton_Clicked()
		{
		}

		public virtual void Update(float delta)
		{
			//keyframeDataflow.Poll();
			//if (keyframeDataflow.GotValue) {
			//	keyFunctionButton.Visible = (keyframeDataflow.Value != null);
			//	if (keyframeDataflow.Value != null) {
			//		keyFunctionButton.SetKeyFunction(keyframeDataflow.Value.Function);
			//	}
			//}
		}

		protected static IDataflowProvider<string> EditBoxCommittedText(EditBox editor)
		{
			return new Property<bool>(editor.IsFocused).Distinct().Where(i => !i).Select(i => editor.Text);
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

	class Vector2PropertyEditor : CommonPropertyEditor
	{
		public Vector2PropertyEditor(PropertyEditorContext context) : base(context)
		{
			EditBox editorX;
			EditBox editorY;
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
				new EditBoxBinding(editorX, CoalescedPropertyValue<Vector2, float>(context, v => v.X).Distinct().Select(i => i.ToString())),
				new EditBoxBinding(editorY, CoalescedPropertyValue<Vector2, float>(context, v => v.Y).Distinct().Select(i => i.ToString()))
			);
		}

		IDataflowProvider<T2> CoalescedPropertyValue<T1, T2>(PropertyEditorContext context, Func<T1, T2> selector) where T1: IEquatable<T1> where T2: IEquatable<T2>
		{
			IDataflowProvider<T2> provider = null;
			foreach (var o in context.Objects) {
				var p = new Property<T1>(o, context.PropertyName).Select(selector);
				provider = (provider == null) ? p : provider.SameOrDefault(p);
			}
			return provider;
		}

		static bool IsNumericString(string value)
		{
			float temp;
			return float.TryParse(value, out temp);
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
					(editor = new EditBox()),
				}
			});
		}

		public override void Update(float delta)
		{
			base.Update(delta);
		}
	}
}