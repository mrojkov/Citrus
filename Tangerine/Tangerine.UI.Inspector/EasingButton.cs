using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Inspector
{
	public class EasingButton : Button
	{
		private readonly Image image;
		private ClickGesture rightClickGesture;
		private bool @checked;

		private static void Awake(Node owner)
		{
			var button = (EasingButton)owner;
			button.rightClickGesture = new ClickGesture(1);
			button.Gestures.Add(button.rightClickGesture);
		}

		public bool Checked
		{
			get => @checked;
			set
			{
				@checked = value;
				image.Visible = value;
			}
		}

		public bool WasRightClicked() => rightClickGesture?.WasRecognized() ?? false;

		public EasingButton()
		{
			Nodes.Clear();
			Size = MinMaxSize = new Vector2(22, 22);
			image = new Image { Size = Size, Texture = new SerializableTexture(), Visible = false };
			Nodes.Add(image);
			Layout = new StackLayout();
			PostPresenter = new WidgetBoundsPresenter(ColorTheme.Current.Inspector.BorderAroundKeyframeColorbox);
			Awoke += Awake;
		}

		public void SetEasing(EasingFunction function, EasingType type)
		{
			image.Visible = true;
			image.Texture = EasingIcons.Instance.Get(function, type);
		}
	}

	public class EasingButtonBinding : ITaskProvider
	{
		private readonly IPropertyEditorParams editorParams;
		private readonly EasingButton button;

		public EasingButtonBinding(IPropertyEditorParams editorParams, EasingButton button)
		{
			this.editorParams = editorParams;
			this.button = button;
		}

		public IEnumerator<object> Task()
		{
			var easingDataflow = KeyframeDataflow.GetProvider(editorParams, i => (i?.EasingFunction, i?.EasingType)).DistinctUntilChanged().GetDataflow();
			while (true) {
				if (easingDataflow.Poll(out var kf)) {
					if (kf.EasingFunction.HasValue) {
						button.SetEasing(kf.EasingFunction.Value, kf.EasingType.Value);
					}
					button.Checked = kf.EasingFunction.HasValue;
				}
				bool wasClicked = button.WasClicked();
				bool wasRightClicked = button.WasRightClicked();
				if (CoreUserPreferences.Instance.SwapMouseButtonsForKeyframeSwitch) {
					Toolbox.Swap(ref wasClicked, ref wasRightClicked);
				}
				if (wasClicked && kf.EasingFunction.HasValue) {
					var menu = new Menu();
					foreach (var i in Enum.GetNames(typeof(EasingFunction))) {
						var func = (EasingFunction)menu.Count;
						var cmd = new Command { Text = i, Checked = kf.EasingFunction.Value == func };
						cmd.Issued += () => {
							Document.Current.History.DoTransaction(() => SetFunction(func));
						};
						menu.Add(cmd);
					}
					menu.Popup();
				}
				if (wasRightClicked && kf.EasingType.HasValue) {
					var menu = new Menu();
					foreach (var i in Enum.GetNames(typeof(EasingType))) {
						var type = (EasingType)menu.Count;
						var cmd = new Command { Text = i, Checked = kf.EasingType.Value == type };
						cmd.Issued += () => {
							Document.Current.History.DoTransaction(() => SetType(type));
						};
						menu.Add(cmd);
					}
					menu.Popup();
				}
				yield return null;
			}
		}

		internal void SetFunction(EasingFunction value)
		{
			foreach (var animable in editorParams.RootObjects.OfType<IAnimationHost>()) {
				if (animable.Animators.TryFind(editorParams.PropertyPath, out var animator, Document.Current.AnimationId)) {
					var keyframe = animator.ReadonlyKeys.FirstOrDefault(i => i.Frame == Document.Current.AnimationFrame).Clone();
					keyframe.EasingFunction = value;
					Core.Operations.SetKeyframe.Perform(animable, editorParams.PropertyPath, Document.Current.AnimationId, keyframe);
				}
			}
		}

		internal void SetType(EasingType value)
		{
			foreach (var animable in editorParams.RootObjects.OfType<IAnimationHost>()) {
				if (animable.Animators.TryFind(editorParams.PropertyPath, out var animator, Document.Current.AnimationId)) {
					var keyframe = animator.ReadonlyKeys.FirstOrDefault(i => i.Frame == Document.Current.AnimationFrame).Clone();
					keyframe.EasingType = value;
					Core.Operations.SetKeyframe.Perform(animable, editorParams.PropertyPath, Document.Current.AnimationId, keyframe);
				}
			}
		}
	}
}
