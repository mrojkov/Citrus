using System;
using System.Collections.Generic;

namespace Lime
{
	public interface IChainedAnimator : IAbstractAnimator
	{
		void Add(AnimationClip clip, IAbstractAnimator animator);
	}

	internal class ChainedAnimator<T> : IAbstractAnimator<T>, IChainedAnimator
	{
		private class Item
		{
			public AnimationClip Clip;
			public IAbstractAnimator<T> Animator;
			public Item Next;
		}

		private Item listHead;
		private Item currentItem;
		private double currentClipBegin;
		private double nextClipBegin;

		public IAnimable Animable => listHead.Animator.Animable;

		public int TargetPropertyPathComparisonCode => listHead.Animator.TargetPropertyPathComparisonCode;

		public bool IsTriggerable => listHead.Animator.IsTriggerable;

		public IAbstractAnimatorSetter<T> Setter { get; private set; }

		public int Duration { get; private set; }

		public Type ValueType => typeof(T);

		public void Apply(double time)
		{
			Setter?.SetValue(CalcValue(time));
		}

		public void Add(AnimationClip clip, IAbstractAnimator animator)
		{
			if (listHead == null) {
				Setter = ((IAbstractAnimator<T>)animator).Setter;
			}
			var item = new Item { Clip = clip, Animator = (IAbstractAnimator<T>)animator };
			if (listHead == null) {
				listHead = item;
			} else {
				var tail = listHead;
				while (tail.Next != null) {
					tail = tail.Next;
				}
				tail.Next = item;
			}
			Duration = Math.Max(Duration, clip.EndFrame);
		}

		public T CalcValue(double time)
		{
			if (time < currentClipBegin || time >= nextClipBegin) {
				currentItem = null;
				for (var i = listHead; i != null; i = i.Next) {
					if (i.Clip.BeginTime - AnimationUtils.Threshold > time) {
						break;
					}
					currentItem = i;
				}
				if (currentItem != null) {
					currentClipBegin = currentItem.Clip.BeginTime - AnimationUtils.Threshold;
					nextClipBegin = currentItem.Next?.Clip.BeginTime - AnimationUtils.Threshold ?? double.PositiveInfinity;
				} else {
					currentItem = listHead;
					currentClipBegin = 0;
					nextClipBegin = listHead.Clip.BeginTime - AnimationUtils.Threshold;
				}
			}
			time = currentItem.Clip.ToLocalTime(time);
			return currentItem.Animator.CalcValue(time);
		}

		public void ExecuteTriggersInRange(double minTime, double maxTime, bool executeTriggerAtMaxTime)
		{
			for (var i = listHead; i != null; i = i.Next) {
				var nextClipBegin = i.Next?.Clip.BeginTime ?? double.PositiveInfinity;
				if (nextClipBegin + AnimationUtils.Threshold < minTime) {
					continue;
				}
				var clip = i.Clip;
				if (clip.BeginTime - AnimationUtils.Threshold > maxTime) {
					break;
				}
				var a = clip.ToLocalTime(minTime);
				var b = clip.ToLocalTime(maxTime);
				if (a <= b) {
					i.Animator.ExecuteTriggersInRange(a, b, executeTriggerAtMaxTime);
				} else if (clip.PostExtrapolation == AnimationClipExtrapolation.Repeat && !clip.Reversed) {
					i.Animator.ExecuteTriggersInRange(a, clip.DurationInSeconds, true);
					i.Animator.ExecuteTriggersInRange(0, b, executeTriggerAtMaxTime);
				} else if (clip.PostExtrapolation == AnimationClipExtrapolation.PingPong) {
					// No idea how to process triggers for the pingpong extrapolation...
				}
			}
		}
	}
}
