namespace Lime.KGDCitronLifeCycle
{
	public static partial class CitronLifeCycle
	{
		private class DefaultAnimationEngine : Lime.DefaultAnimationEngine
		{
			public override void AdvanceAnimation(Animation animation, float delta)
			{
				automateAdvanceAnimationActiveCount++;
				base.AdvanceAnimation(animation, delta);
				automateAdvanceAnimationActiveCount--;
			}

			public override bool TryRunAnimation(Animation animation, string markerId, double animationTimeCorrection = 0)
			{
				if (!base.TryRunAnimation(animation, markerId, animationTimeCorrection)) {
					return false;
				}
				animation.ProcessAfterRunAnimation();
				return true;
			}

			// ReSharper disable once MemberHidesStaticFromOuterClass
			public override void RaiseStopped(Animation animation)
			{
				CitronLifeCycle.RaiseStopped(animation);
			}
		}

		private class BlendAnimationEngine : Lime.BlendAnimationEngine
		{
			public override void AdvanceAnimation(Animation animation, float delta)
			{
				automateAdvanceAnimationActiveCount++;
				base.AdvanceAnimation(animation, delta);
				automateAdvanceAnimationActiveCount--;
			}

			public override bool TryRunAnimation(Animation animation, string markerId, double animationTimeCorrection = 0)
			{
				if (!base.TryRunAnimation(animation, markerId, animationTimeCorrection)) {
					return false;
				}
				animation.ProcessAfterRunAnimation();
				return true;
			}

			// ReSharper disable once MemberHidesStaticFromOuterClass
			public override void RaiseStopped(Animation animation)
			{
				CitronLifeCycle.RaiseStopped(animation);
			}
		}
	}
}
