using Lime;
using System;
using System.Linq;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Operations
{
	public static class TogglePositionKeyframe
	{
		public static void Perform()
		{
			KeyframeToggle.Perform<Widget>(nameof(Widget.Position), w => w.Position);
			KeyframeToggle.Perform<Bone>(nameof(Bone.Position), b => b.Position);
			KeyframeToggle.Perform<PointObject>(nameof(PointObject.Position), p => p.Position);
		}
	}

	public static class ToggleRotationKeyframe
	{
		public static void Perform()
		{
			KeyframeToggle.Perform<Widget>(nameof(Widget.Rotation), w => w.Rotation);
			KeyframeToggle.Perform<Bone>(nameof(Bone.Rotation), b => b.Rotation);
		}
	}

	public static class ToggleScaleKeyframe
	{
		public static void Perform()
		{
			KeyframeToggle.Perform<Widget>(nameof(Widget.Scale), w => w.Scale);
		}
	}

	public static class KeyframeToggle
	{
		public static void Perform<T>(string propertyName, Func<T, object> valueGetter) where T: IAnimationHost
		{
			Document.Current.History.DoTransaction(() => {
				bool hasKey = false;
				bool hasNoKey = false;
				var nodes = Document.Current.SelectedNodes().Editable().OfType<T>();
				foreach (var node in nodes) {
					if (node.Animators.TryFind(propertyName, out var animator, Document.Current.AnimationId)) {
						bool thisHasKey = animator.ReadonlyKeys.Any(i => i.Frame == Document.Current.AnimationFrame);
						hasKey |= thisHasKey;
						hasNoKey |= !thisHasKey;
					} else {
						hasNoKey = true;
					}
				}
				if (hasKey && !hasNoKey) {
					foreach (var node in nodes) {
						if (node.Animators.TryFind(propertyName, out var animator, Document.Current.AnimationId)) {
							Core.Operations.RemoveKeyframe.Perform(animator, Document.Current.AnimationFrame);
						}
					}
				} else if (!hasKey && hasNoKey) {
					foreach (var node in nodes) {
						Core.Operations.SetAnimableProperty.Perform(
							node,
							propertyName,
							valueGetter(node),
							createAnimatorIfNeeded: true,
							createInitialKeyframeForNewAnimator: false
						);
					}
				}
			});
		}
	}
}
