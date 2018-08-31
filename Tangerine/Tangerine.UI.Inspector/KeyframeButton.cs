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
		readonly Image fillImage;
		readonly Image outlintImage;
		private static string[] iconNames = new[] { "Linear", "Step", "Catmullrom", "Loop" };
		private List<ITexture> fillTextures = new List<ITexture>();
		private List<ITexture> outlineTextures = new List<ITexture>();
		private ClickGesture rightClickGesture;
		KeyFunction function;
		bool @checked;

		public Color4 KeyColor { get; set; }
		public bool Checked
		{
			get { return @checked; }
			set
			{
				@checked = value;
				fillImage.Visible = value;
				outlintImage.Visible = value;
				fillImage.Color = KeyColor;
				outlintImage.Color = ColorTheme.Current.Inspector.BorderAroundKeyframeColorbox;
			}
		}

		public void SetKeyFunction(KeyFunction function)
		{
			this.function = function;
			fillImage.Texture = fillTextures[(int)function];
			outlintImage.Texture = outlineTextures[(int)function];
		}

		private static void Awake(Node owner)
		{
			var kfb = (KeyframeButton)owner;
			kfb.rightClickGesture = new ClickGesture(1);
			kfb.Gestures.Add(kfb.rightClickGesture);
		}

		public bool WasRightClicked() => rightClickGesture?.WasRecognized() ?? false;

		public KeyframeButton()
		{
			foreach (var v in Enum.GetValues(typeof(KeyFunction))) {
				fillTextures.Add(IconPool.GetTexture("Inspector." + iconNames[(int)v] + "Fill"));
				outlineTextures.Add(IconPool.GetTexture("Inspector." + iconNames[(int)v] + "Outline"));
			}
			Nodes.Clear();
			Size = MinMaxSize = new Vector2(22, 22);
			image = new Image { Size = Size, Shader = ShaderId.Silhuette, Texture = new SerializableTexture(), Color = Theme.Colors.WhiteBackground };
			fillImage = new Image { Size = Size, Visible = true };
			outlintImage = new Image { Size = Size, Visible = true };
			Nodes.Add(outlintImage);
			Nodes.Add(fillImage);
			Nodes.Add(image);
			Layout = new StackLayout();
			this.PostPresenter = new WidgetBoundsPresenter(ColorTheme.Current.Inspector.BorderAroundKeyframeColorbox);
			Awoke += Awake;
		}
	}

	class KeyframeButtonBinding : ITaskProvider
	{
		readonly IPropertyEditorParams editorParams;
		readonly KeyframeButton button;

		public KeyframeButtonBinding(IPropertyEditorParams editorParams, KeyframeButton button)
		{
			this.editorParams = editorParams;
			this.button = button;
		}

		public IEnumerator<object> Task()
		{
			var keyFunctionFlow = KeyframeDataflow.GetProvider(editorParams, i => i?.Function).DistinctUntilChanged().GetDataflow();
			while (true) {
				KeyFunction? kf;
				keyFunctionFlow.Poll(out kf);
				button.Checked = kf.HasValue;
				if (kf.HasValue) {
					button.SetKeyFunction(kf.Value);
				}
				if (button.WasClicked()) {
					Document.Current.History.DoTransaction(() => {
						SetKeyframe(!kf.HasValue);
					});
				}
				if (button.WasRightClicked()) {
					if (kf.HasValue) {
						var nextKeyFunction = NextKeyFunction(kf.GetValueOrDefault());
						Document.Current.History.DoTransaction(() => {
							SetKeyFunction(nextKeyFunction);
						});
					} else {
						Document.Current.History.DoTransaction(() => {
							SetKeyframe(true);
						});
					}
				}
				yield return null;
			}
		}

		private static KeyFunction[] nextKeyFunction = {
			KeyFunction.Spline, KeyFunction.ClosedSpline,
			KeyFunction.Steep, KeyFunction.Linear
		};

		private static KeyFunction NextKeyFunction(KeyFunction value)
		{
			return nextKeyFunction[(int)value];
		}

		internal void SetKeyFunction(KeyFunction value)
		{
			foreach (var animable in editorParams.RootObjects.OfType<IAnimationHost>()) {
				IAnimator animator;
				if (animable.Animators.TryFind(editorParams.PropertyPath, out animator, Document.Current.AnimationId)) {
					var keyframe = animator.ReadonlyKeys.FirstOrDefault(i => i.Frame == Document.Current.AnimationFrame).Clone();
					keyframe.Function = value;
					Core.Operations.SetKeyframe.Perform(animable, editorParams.PropertyPath, Document.Current.AnimationId, keyframe);
				}
			}
		}

		void SetKeyframe(bool value)
		{
			int currentFrame = Document.Current.AnimationFrame;

			foreach (var (animable, owner) in editorParams.RootObjects.Zip(editorParams.Objects, (ro, o) => (ro as IAnimationHost, o))) {
				bool hasKey = false;
				if (animable.Animators.TryFind(editorParams.PropertyPath, out IAnimator animator, Document.Current.AnimationId)) {
					hasKey = animator.ReadonlyKeys.Any(i => i.Frame == currentFrame);
					if (hasKey && !value) {
						Core.Operations.RemoveKeyframe.Perform(animator, currentFrame);
					}
				}

				if (!hasKey && value) {
					var propValue = new Property(owner, editorParams.PropertyName).Getter();
					var keyFunction = animator?.Keys.LastOrDefault(k => k.Frame <= currentFrame)?.Function ??
						CoreUserPreferences.Instance.DefaultKeyFunction;
					IKeyframe keyframe = Keyframe.CreateForType(editorParams.PropertyInfo.PropertyType, currentFrame, propValue, keyFunction);
					Core.Operations.SetKeyframe.Perform(animable, editorParams.PropertyPath, Document.Current.AnimationId, keyframe);
				}
			}
		}
	}
}
