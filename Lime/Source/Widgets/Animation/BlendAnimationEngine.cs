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

		public override void ApplyAnimators(Animation animation, bool invokeTriggers)
		{
			base.ApplyAnimators(animation, invokeTriggers);
			Blending(animation);
		}

		public override bool TryRunAnimation(Animation animation, string markerId)
		{
			var blender = animation.Owner.Components.Get<AnimationBlender>();
			if (blender == null) {
				return base.TryRunAnimation(animation, markerId);
			}
			if (markerId != null && animation.Markers.TryFind(markerId) == null) {
				return false;
			}

			blender.Attach(animation, markerId, animation.RunningMarkerId);
			base.TryRunAnimation(animation, markerId);
			Blending(animation);
			return true;
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
