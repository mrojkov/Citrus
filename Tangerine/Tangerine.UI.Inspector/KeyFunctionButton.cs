using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Inspector
{
	public class KeyFunctionButton : ToolbarButton
	{
		private ClickGesture rightClickGesture;

		public KeyFunctionButton()
		{
			Awoke += Awake;
		}

		private static void Awake(Node owner)
		{
			var kfb = (KeyFunctionButton)owner;
			kfb.rightClickGesture = new ClickGesture(1);
			kfb.Gestures.Add(kfb.rightClickGesture);
		}

		public bool WasRightClicked() => rightClickGesture?.WasRecognized() ?? false;

		public void SetKeyFunction(KeyFunction function)
		{
			var s = "Timeline.Interpolation." + FunctionToString(function);
			Texture = IconPool.GetTexture(s);
		}

		private static string FunctionToString(KeyFunction function)
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
		readonly IPropertyEditorParams editorParams;
		readonly KeyFunctionButton button;

		public KeyFunctionButtonBinding(IPropertyEditorParams editorParams, KeyFunctionButton button)
		{
			this.editorParams = editorParams;
			this.button = button;
		}

		public IEnumerator<object> Task()
		{
			var provider = KeyframeDataflow.GetProvider(editorParams, i => i?.Function).DistinctUntilChanged();
			var keyFunction = provider.GetDataflow();
			while (true) {
				KeyFunction? v;
				if (keyFunction.Poll(out v)) {
					if ((button.Visible = v.HasValue)) {
						button.SetKeyFunction(v.Value);
					}
				}
				if (button.WasRightClicked()) {
					KeyFunctionContextMenu.Create(this);
				}
				if (button.WasClicked()) {
					Document.Current.History.DoTransaction(() => {
						SetKeyFunction(NextKeyFunction(v.GetValueOrDefault()));
					});
				}
				yield return null;
			}
		}

		static KeyFunction NextKeyFunction(KeyFunction value)
		{
			var count = Enum.GetValues(typeof(KeyFunction)).Length;
			return (KeyFunction)(((int)value + 1) % count);
		}

		internal void SetKeyFunction(KeyFunction value)
		{
			foreach (var animable in editorParams.Objects.OfType<IAnimable>()) {
				IAnimator animator;
				if (animable.Animators.TryFind(editorParams.PropertyName, out animator, Document.Current.AnimationId)) {
					var keyframe = animator.ReadonlyKeys.FirstOrDefault(i => i.Frame == Document.Current.AnimationFrame).Clone();
					keyframe.Function = value;
					Core.Operations.SetKeyframe.Perform(animable, editorParams.PropertyName, Document.Current.AnimationId, keyframe);
				}
			}
		}
	}
}
