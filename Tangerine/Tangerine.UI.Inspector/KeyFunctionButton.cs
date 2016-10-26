using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Inspector
{
	class KeyFunctionButton : ToolbarButton
	{
		public void SetKeyFunction(KeyFunction function)
		{
			var s = "Timeline.Interpolation." + FunctionToString(function);
			Texture = IconPool.GetTexture(s);
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

	class KeyFunctionButtonBinding : ITaskProvider
	{
		readonly PropertyEditorContext context;
		readonly KeyFunctionButton button;

		public KeyFunctionButtonBinding(PropertyEditorContext context, KeyFunctionButton button)
		{
			this.context = context;
			this.button = button;
		}

		public IEnumerator<object> Task()
		{
			var provider = KeyframeDataflow.GetProvider(context, i => i?.Function).DistinctUntilChanged();
			var keyFunction = provider.GetDataflow();
			while (true) {
				KeyFunction? v; 
				if (keyFunction.Poll(out v)) {
					if ((button.Visible = v.HasValue)) {
						button.SetKeyFunction(v.Value);
					}
				}
				if (button.WasClicked()) {
					SetKeyFunction(NextKeyFunction(v.GetValueOrDefault()));
				}
				yield return null;
			}
		}

		static KeyFunction NextKeyFunction(KeyFunction value)
		{
			var count = Enum.GetValues(typeof(KeyFunction)).Length;
			return (KeyFunction)(((int)value + 1) % count);
		}

		void SetKeyFunction(KeyFunction value)
		{
			foreach (var animable in context.Objects.OfType<IAnimable>()) {
				IAnimator animator;
				if (animable.Animators.TryFind(context.PropertyName, out animator, Document.Current.AnimationId)) {
					var keyframe = animator.ReadonlyKeys.FirstOrDefault(i => i.Frame == Document.Current.AnimationFrame).Clone();
					keyframe.Function = value;
					Core.Operations.SetKeyframe.Perform(animable, context.PropertyName, Document.Current.AnimationId, keyframe); 
				}
			}
		}
	}
}