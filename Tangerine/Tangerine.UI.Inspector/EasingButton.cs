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

		public void SetEasing(Mathf.EasingFunction function, Mathf.EasingType type)
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
			var easingDataflow = KeyframeDataflow.GetProvider(editorParams, i => i?.Params).DistinctUntilChanged().GetDataflow();
			while (true) {
				if (easingDataflow.Poll(out var kf)) {
					if (kf.HasValue) {
						button.SetEasing(kf.Value.EasingFunction, kf.Value.EasingType);
					}
					button.Checked = kf.HasValue;
				}
				if (button.WasClicked() && kf.HasValue) {
					var menu = new Menu();
					foreach (var i in Enum.GetNames(typeof(Mathf.EasingType))) {
						var type = (Mathf.EasingType)menu.Count;
						var cmd = new Command { Text = i, Checked = kf.Value.EasingType == type };
						cmd.Issued += () => {
							Document.Current.History.DoTransaction(() => ProcessKeyframe(k => k.EasingType = type));
						};
						menu.Add(cmd);
					}
					menu.Add(Command.MenuSeparator);
					int j = 0;
					foreach (var i in Enum.GetNames(typeof(Mathf.EasingFunction))) {
						var func = (Mathf.EasingFunction)j++;
						var cmd = new Command { Text = i, Checked = kf.Value.EasingFunction == func };
						cmd.Issued += () => {
							Document.Current.History.DoTransaction(() => ProcessKeyframe(k => k.EasingFunction = func));
						};
						menu.Add(cmd);
					}
					menu.Popup();
				}
				yield return null;
			}
		}

		private void ProcessKeyframe(Action<IKeyframe> processor)
		{
			foreach (var animable in editorParams.RootObjects.OfType<IAnimationHost>()) {
				if (animable.Animators.TryFind(editorParams.PropertyPath, out var animator, Document.Current.AnimationId)) {
					var keyframe = animator.ReadonlyKeys.FirstOrDefault(i => i.Frame == Document.Current.AnimationFrame).Clone();
					processor(keyframe);
					Core.Operations.SetKeyframe.Perform(animable, editorParams.PropertyPath, Document.Current.AnimationId, keyframe);
				}
			}
		}
	}
}
