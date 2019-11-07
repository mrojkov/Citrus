namespace Lime.KGDCitronLifeCycle
{
	public static partial class CitronLifeCycle
	{
		private class AnimationProcessor : Lime.AnimationProcessor
		{
			protected internal override void Update(float delta)
			{
				afterAdvanceAnimationSuppressedCount++;
				base.Update(delta);
				afterAdvanceAnimationSuppressedCount--;
			}
		}

	}
}
