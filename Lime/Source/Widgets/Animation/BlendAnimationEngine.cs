namespace Lime
{
	public class BlendAnimationEngine : DefaultAnimationEngine
	{
		public static new readonly BlendAnimationEngine Instance = new BlendAnimationEngine();

		public override void AdvanceAnimation(Animation animation, float delta)
		{
			base.AdvanceAnimation(animation, delta);
			Blending(animation, delta);
		}

		public override void ApplyAnimators(Animation animation, bool invokeTriggers, double animationTimeCorrection = 0)
		{
			base.ApplyAnimators(animation, invokeTriggers, animationTimeCorrection);
			Blending(animation);
		}

		public override bool TryRunAnimation(Animation animation, string markerId, double animationTimeCorrection = 0)
		{
			var blender = animation.Owner.Components.Get<AnimationBlender>();
			if (blender == null) {
				return base.TryRunAnimation(animation, markerId, animationTimeCorrection);
			}
			if (markerId != null && animation.Markers.TryFind(markerId) == null) {
				return false;
			}

			blender.Attach(animation, markerId, animation.RunningMarkerId);
			base.TryRunAnimation(animation, markerId, animationTimeCorrection);
			Blending(animation);
			return true;
		}

		protected override void ProcessMarker(Animation animation, Marker marker)
		{
			if ((animation.Owner.TangerineFlags & TangerineFlags.IgnoreMarkers) != 0) {
				return;
			}
			var blender = animation.Owner.Components.Get<AnimationBlender>();
			if (blender == null || marker.Action != MarkerAction.Jump) {
				base.ProcessMarker(animation, marker);
				return;
			}
			var gotoMarker = animation.Markers.TryFind(marker.JumpTo);
			if (gotoMarker != null && gotoMarker != marker) {
				var delta = animation.Time - AnimationUtils.FramesToSeconds(animation.Frame);
				animation.TimeInternal = gotoMarker.Time;
				blender.Attach(animation, gotoMarker.Id, animation.RunningMarkerId);
				AdvanceAnimation(animation, (float)delta);
			}
			marker.CustomAction?.Invoke();
		}

		private static void Blending(Animation animation, float delta = 0f)
		{
			var blender = animation.Owner.Components.Get<AnimationBlender>();
			if (blender == null) {
				return;
			}

			blender.UpdateWantedState(animation);
			blender.Update(animation, delta);
		}
	}
}
