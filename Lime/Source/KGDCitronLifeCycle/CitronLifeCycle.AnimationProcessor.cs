namespace Lime.KGDCitronLifeCycle
{
	public static partial class CitronLifeCycle
	{
		private class AnimationProcessor : Lime.AnimationProcessor
		{
			protected override void Add(AnimationComponent component, Node owner)
			{
				foreach (var a in component.Animations) {
					ReplaceAnimationEngine(a);
				}
				component.AnimationAdded += OnAnimationAdded;
				component.AnimationRemoved += OnAnimationRemoved;
				base.Add(component, owner);
			}

			protected override void Remove(AnimationComponent component, Node owner)
			{
				base.Remove(component, owner);
				foreach (var a in component.Animations) {
					RevertAnimationEngine(a);
				}
				component.AnimationAdded -= OnAnimationAdded;
				component.AnimationRemoved -= OnAnimationRemoved;
			}

			private void ReplaceAnimationEngine(Animation animation)
			{
				if (animation.AnimationEngine == Lime.DefaultAnimationEngine.Instance) {
					animation.AnimationEngine = new DefaultAnimationEngine();
				} else if (animation.AnimationEngine == Lime.BlendAnimationEngine.Instance) {
					animation.AnimationEngine = new BlendAnimationEngine();
				}
			}

			private void RevertAnimationEngine(Animation animation)
			{
				if (animation.AnimationEngine is DefaultAnimationEngine) {
					animation.AnimationEngine = Lime.DefaultAnimationEngine.Instance;
				} else if (animation.AnimationEngine is BlendAnimationEngine) {
					animation.AnimationEngine = Lime.BlendAnimationEngine.Instance;
				}
			}

			private void OnAnimationRemoved(AnimationComponent component, Animation animation)
			{
				ReplaceAnimationEngine(animation);
			}

			private void OnAnimationAdded(AnimationComponent component, Animation animation)
			{
				RevertAnimationEngine(animation);
			}

			protected internal override void Update(float delta)
			{
				advanceAnimationActiveCount++;
				pipelinedAdvanceAnimationActiveCount++;
				base.Update(delta);
				pipelinedAdvanceAnimationActiveCount--;
				advanceAnimationActiveCount--;
			}
		}

	}
}
