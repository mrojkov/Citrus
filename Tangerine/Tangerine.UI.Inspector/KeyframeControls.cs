using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Inspector
{
	public class KeyframeButton : Button
	{
		readonly Image image;
		bool @checked;

		public Color4 KeyColor { get; set; }
		public bool Checked
		{
			get { return @checked; }
			set
			{
				@checked = value;
				image.Color = value ? KeyColor : Colors.WhiteBackground;
			}
		}

		public KeyframeButton()
		{
			Nodes.Clear();
			Size = MinMaxSize = Metrics.IconSize;
			image = new Image { Size = Size, Shader = ShaderId.Silhuette, Texture = new SerializableTexture() };
			Nodes.Add(image);
			image.PostPresenter = new WidgetBoundsPresenter(Colors.Inspector.BorderAroundKeyframeColorbox);
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

	class KeyframeControlsBinding : IProcessor
	{
		readonly KeyframeButton keyframeButton;
		readonly KeyFunctionButton keyFunctionButton;
		readonly PropertyEditorContext context;

		public KeyframeControlsBinding(KeyframeButton keyframeButton, KeyFunctionButton keyFunctionButton, PropertyEditorContext context)
		{
			this.keyframeButton = keyframeButton;
			this.keyFunctionButton = keyFunctionButton;
			this.context = context;
		}

		public IEnumerator<object> Loop()
		{
			var provider = KeyframeProvider(context).DistinctUntilChanged();
			var keyframe = provider.GetDataflow();
			while (true) {
				keyframe.Poll();
				if (keyframe.GotValue) {
					keyframeButton.Checked = keyframe.Value != null;
					keyFunctionButton.Visible = keyframe.Value != null;
					if (keyframe.Value != null) {
						keyFunctionButton.SetKeyFunction(keyframe.Value.Function);
					}
				}
				if (keyframeButton.WasClicked()) {
					SetKeyframe(keyframe.Value == null);
				}
				yield return null;
			}
		}

		void SetKeyframe(bool value)
		{
			foreach (var obj in context.Objects) {
				var animable = obj as IAnimable;
				if (animable != null) {
					IKeyframe keyframe = null;
					IAnimator animator;
					var hasKey = false;
					if (animable.Animators.TryFind(context.PropertyName, out animator, Document.Current.AnimationId)) {
						hasKey = animator.ReadonlyKeys.Any(i => i.Frame == Document.Current.AnimationFrame);
						if (hasKey && !value) {
							Document.Current.History.Add(new Core.Operations.RemoveKeyframe(animator, Document.Current.AnimationFrame)); 
						}
					}
					if (!hasKey && value) {
						var propValue = new Property(animable, context.PropertyName).Getter();
						keyframe = Keyframe.CreateForType(context.PropertyInfo.PropertyType, Document.Current.AnimationFrame, propValue);
						Document.Current.History.Add(new Core.Operations.SetKeyframe(animable, context.PropertyName, Document.Current.AnimationId, keyframe)); 
					}
				}
			}
			Document.Current.History.Commit();
		}

		static IDataflowProvider<IKeyframe> KeyframeProvider(PropertyEditorContext context)
		{
			IDataflowProvider<IKeyframe> provider = null;
			foreach (var obj in context.Objects) {
				var p = new DataflowProvider<IKeyframe>(() => new KeyframeDataflow(obj, context.PropertyName));
				provider = (provider == null) ? p : provider.SameOrDefault(p);
			}
			return provider;
		}

		class KeyframeDataflow : IDataflow<IKeyframe>
		{
			readonly object obj;
			readonly string propertyName;

			int animatorCollectionVersion = int.MinValue;
			int animatorVersion = int.MinValue;
			int animationFrame = int.MinValue;
			string animationId;
			IAnimator animator;

			public IKeyframe Value { get; private set; }
			public bool GotValue { get; private set; }

			public KeyframeDataflow(object obj, string propertyName)
			{
				this.obj = obj;
				this.propertyName = propertyName;
			}

			public void Poll()
			{
				GotValue = false;
				var animable = obj as IAnimable;
				if (animable == null) {
					return;
				}
				if ((GotValue |= Document.Current.AnimationFrame != animationFrame)) {
					animationFrame = Document.Current.AnimationFrame;
				}
				if ((GotValue |= Document.Current.AnimationId != animationId)) {
					animationId = Document.Current.AnimationId;
					animatorCollectionVersion = int.MinValue;
				}
				if ((GotValue |= animatorCollectionVersion != animable.Animators.Version)) {
					animatorCollectionVersion = animable.Animators.Version;
					animator = FindAnimator();
				}
				if ((GotValue |= animator != null && animator.ReadonlyKeys.Version != animatorVersion)) {
					if (animator != null) {
						animatorVersion = animator.ReadonlyKeys.Version;
						Value = FindKeyframe();
					} else {
						animatorVersion = int.MinValue;
						Value = null;
					}
				}
			}

			IAnimator FindAnimator()
			{
				IAnimator animator;
				return (obj as IAnimable).Animators.TryFind(propertyName, out animator, Document.Current.AnimationId) ? animator : null;
			}

			IKeyframe FindKeyframe()
			{
				return FindAnimator()?.ReadonlyKeys.FirstOrDefault(k => k.Frame == Document.Current.AnimationFrame);
			}
		}
	}
}