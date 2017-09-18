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
				image.Color = value ? KeyColor : ColorTheme.Current.Basic.WhiteBackground;
			}
		}

		public KeyframeButton()
		{
			Nodes.Clear();
			Size = MinMaxSize = new Vector2(16, 16);
			image = new Image { Size = Size, Shader = ShaderId.Silhuette, Texture = new SerializableTexture() };
			Nodes.Add(image);
			image.PostPresenter = new WidgetBoundsPresenter(ColorTheme.Current.Inspector.BorderAroundKeyframeColorbox);
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
			var provider = KeyframeDataflow.GetProvider(editorParams, i => i != null).DistinctUntilChanged();
			var hasKeyframe = provider.GetDataflow();
			while (true) {
				bool @checked;
				if (hasKeyframe.Poll(out @checked)) {
					button.Checked = @checked;
				}
				if (button.WasClicked()) {
					SetKeyframe(!hasKeyframe.Value);
				}
				yield return null;
			}
		}

		void SetKeyframe(bool value)
		{
			foreach (var animable in editorParams.Objects.OfType<IAnimable>()) {
				IKeyframe keyframe = null;
				IAnimator animator;
				var hasKey = false;
				if (animable.Animators.TryFind(editorParams.PropertyName, out animator, Document.Current.AnimationId)) {
					hasKey = animator.ReadonlyKeys.Any(i => i.Frame == Document.Current.AnimationFrame);
					if (hasKey && !value) {
						Core.Operations.RemoveKeyframe.Perform(animator, Document.Current.AnimationFrame);
					}
				}
				if (!hasKey && value) {
					var propValue = new Property(animable, editorParams.PropertyName).Getter();
					keyframe = Keyframe.CreateForType(editorParams.PropertyInfo.PropertyType, Document.Current.AnimationFrame, propValue);
					Core.Operations.SetKeyframe.Perform(animable, editorParams.PropertyName, Document.Current.AnimationId, keyframe);
				}
			}
		}
	}
}