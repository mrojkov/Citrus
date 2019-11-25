using System;
using System.Collections.Generic;

namespace Lime
{
	public class AnimationProcessor : NodeComponentProcessor<AnimationComponent>
	{
		private BucketQueue<Animation> currQueue = new BucketQueue<Animation>(0);
		private BucketQueue<Animation> nextQueue = new BucketQueue<Animation>(0);
		private Stack<BucketQueueNode<Animation>> freeQueueNodes = new Stack<BucketQueueNode<Animation>>();

		private BucketQueueNode<Animation> AcquireQueueNode(Animation animation)
		{
			var node = freeQueueNodes.Count > 0 ? freeQueueNodes.Pop() : new BucketQueueNode<Animation>();
			node.Value = animation;
			return node;
		}

		private void ReleaseQueueNode(BucketQueueNode<Animation> node)
		{
			node.Value = null;
			freeQueueNodes.Push(node);
		}

		protected override void Add(AnimationComponent component, Node owner)
		{
			component.AnimationAdded += OnAnimationAdded;
			component.AnimationRemoved += OnAnimationRemoved;
			component.AnimationRun += OnAnimationRun;
			component.AnimationStopped += OnAnimationStopped;
			component.Depth = GetNodeDepth(owner);
			if (component.Depth >= currQueue.BucketCount) {
				BucketQueue<Animation>.Resize(ref currQueue, component.Depth + 1);
				BucketQueue<Animation>.Resize(ref nextQueue, component.Depth + 1);
			}
			if (owner.GloballyFrozen) {
				return;
			}
			foreach (var a in component.Animations) {
				if (a.IsRunning) {
					Activate(a);
				}
			}
		}

		protected override void Remove(AnimationComponent component, Node owner)
		{
			component.AnimationAdded -= OnAnimationAdded;
			component.AnimationRemoved -= OnAnimationRemoved;
			component.AnimationRun -= OnAnimationRun;
			component.AnimationStopped -= OnAnimationStopped;
			component.Depth = -1;
			foreach (var a in component.Animations) {
				Deactivate(a);
			}
		}

		internal void OnAnimationAdded(AnimationComponent component, Animation animation)
		{
			if (animation.IsRunning) {
				OnAnimationRun(component, animation);
			}
		}

		internal void OnAnimationRemoved(AnimationComponent component, Animation animation)
		{
			OnAnimationStopped(component, animation);
		}

		internal void OnAnimationRun(AnimationComponent component, Animation animation)
		{
			if (!animation.OwnerNode.GloballyFrozen) {
				Activate(animation);
			}
		}

		internal void OnAnimationStopped(AnimationComponent component, Animation animation)
		{
			Deactivate(animation);
		}

		protected override void OnOwnerFrozenChanged(AnimationComponent component, Node owner)
		{
			if (owner.GloballyFrozen) {
				foreach (var a in component.Animations) {
					Deactivate(a);
				}
			} else {
				foreach (var a in component.Animations) {
					if (a.IsRunning) {
						Activate(a);
					}
				}
			}
		}

		private void Activate(Animation animation)
		{
			if (animation.QueueNode == null) {
				animation.QueueNode = AcquireQueueNode(animation);
				currQueue.Enqueue(animation.Owner.Depth, animation.QueueNode);
			}
		}

		private void Deactivate(Animation animation)
		{
			if (animation.QueueNode != null) {
				currQueue.Remove(animation.QueueNode);
				nextQueue.Remove(animation.QueueNode);
				ReleaseQueueNode(animation.QueueNode);
				animation.QueueNode = null;
			}
		}

		protected internal override void Update(float delta)
		{
			while (currQueue.Count > 0) {
				var animation = currQueue.Dequeue().Value;
				nextQueue.Enqueue(animation.Owner.Depth, animation.QueueNode);
				animation.Advance(delta * animation.OwnerNode.EffectiveAnimationSpeed);
			}
			Toolbox.Swap(ref currQueue, ref nextQueue);
		}

		private static int GetNodeDepth(Node node)
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
