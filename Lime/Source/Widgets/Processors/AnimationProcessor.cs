using System.Collections.Generic;

namespace Lime
{
	public class AnimationProcessor : NodeComponentProcessor<AnimationComponent>
	{
		private List<List<Animation>> runningAnimationsByDepth = new List<List<Animation>>();

		protected override void Add(AnimationComponent component)
		{
			component.AnimationRun += OnAnimationRun;
			component.AnimationStopped += OnAnimationStopped;
			foreach (var a in component.Animations) {
				if (a.IsRunning) {
					OnAnimationRun(component, a);
				}
			}
		}

		protected override void Remove(AnimationComponent component)
		{
			component.AnimationRun -= OnAnimationRun;
			component.AnimationStopped -= OnAnimationStopped;
			foreach (var a in component.Animations) {
				if (a.IsRunning) {
					OnAnimationStopped(component, a);
				}
			}
		}

		internal void OnAnimationRun(AnimationComponent component, Animation animation)
		{
			if (animation.Depth < 0) {
				var depth = GetDepth(animation.OwnerNode);
				var list = GetRunningAnimationList(depth);
				animation.Depth = depth;
				animation.Index = list.Count;
				list.Add(animation);
			}
		}

		internal void OnAnimationStopped(AnimationComponent component, Animation animation)
		{
			if (animation.Depth >= 0) {
				var list = GetRunningAnimationList(animation.Depth);
				list[animation.Index] = null;
				animation.Depth = -1;
				animation.Index = -1;
			}
		}

		private List<Animation> GetRunningAnimationList(int depth)
		{
			while (depth >= runningAnimationsByDepth.Count) {
				runningAnimationsByDepth.Add(new List<Animation>());
			}
			return runningAnimationsByDepth[depth];
		}

		protected internal override void Update(float delta)
		{
			for (var i = 0; i < runningAnimationsByDepth.Count; i++) {
				var runningAnimations = runningAnimationsByDepth[i];
				for (var j = runningAnimations.Count - 1; j >= 0; j--) {
					var a = runningAnimations[j];
					if (a != null) {
						a.Advance(delta * a.OwnerNode.EffectiveAnimationSpeed);
					} else {
						a = runningAnimations[runningAnimations.Count - 1];
						if (a != null) {
							a.Index = j;
						}
						runningAnimations[j] = a;
						runningAnimations.RemoveAt(runningAnimations.Count - 1);
					}
				}
			}
		}

		private int GetDepth(Node node)
		{
			var depth = 0;
			var p = node.Parent;
			while (p != null) {
				depth++;
				p = p.Parent;
			}
			return depth;
		}
	}
}
