namespace Lime
{
	public class BlendAnimationEngine : DefaultAnimationEngine
	{
		public new static readonly BlendAnimationEngine Instance = new BlendAnimationEngine();

		public override void AdvanceAnimation(Animation animation, float delta)
		{
			base.AdvanceAnimation(animation, delta);
			Blending(animation, delta);
		}

		public override bool TryRunAnimation(Animation animation, string markerId, double animationTimeCorrection = 0)
		{
			var blender = animation.OwnerNode.Components.Get<AnimationBlender>();
			if (blender == null || !animation.Markers.TryFind(markerId, out var marker)) {
				return base.TryRunAnimation(animation, markerId, animationTimeCorrection);
			}

			if (marker.Action != MarkerAction.Stop) {
				blender.Attach(animation, markerId, animation.RunningMarkerId);
			}
			base.TryRunAnimation(animation, markerId, animationTimeCorrection);
			Blending(animation);
			return true;
		}

		protected override void ProcessMarker(Animation animation, Marker marker, double previousTime, double currentTime)
		{
			if ((animation.OwnerNode.TangerineFlags & TangerineFlags.IgnoreMarkers) != 0) {
				return;
			}
			var blender = animation.OwnerNode.Components.Get<AnimationBlender>();
			if (blender == null || marker.Action != MarkerAction.Jump) {
				base.ProcessMarker(animation, marker, previousTime, currentTime);
				return;
			}
			if (animation.Markers.TryFind(marker.JumpTo, out var gotoMarker) && gotoMarker != marker) {
				var delta = animation.Time - AnimationUtils.FramesToSeconds(animation.Frame);
				animation.TimeInternal = gotoMarker.Time;
				if (JumpAffectsRunningMarkerId) {
					animation.RunningMarkerId = gotoMarker.Id;
				}
				if (gotoMarker.Action != MarkerAction.Stop) {
					blender.Attach(animation, gotoMarker.Id, animation.RunningMarkerId);
				}
				AdvanceAnimation(animation, (float)delta);
			}
			marker.CustomAction?.Invoke();
		}

		private static void Blending(Animation animation, float delta = 0f)
		{
			var blender = animation.OwnerNode.Components.Get<AnimationBlender>();
			if (blender == null) {
				return;
			}

			blender.UpdateWantedState(animation);
			blender.Update(animation, delta);
		}
	}
}
