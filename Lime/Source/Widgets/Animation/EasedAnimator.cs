using System;

namespace Lime
{
	public interface IEasedAnimator : IAbstractAnimator
	{
		void Initialize(Animation animation, IAbstractAnimator animator);
	}

	internal class EasedAnimator<T> : IAbstractAnimator<T>, IEasedAnimator
	{
		private Animation animation;
		private IAbstractAnimator<T> animator;

		public void Initialize(Animation animation, IAbstractAnimator animator)
		{
			this.animation = animation;
			this.animator = (IAbstractAnimator<T>)animator;
			this.Setter = this.animator.Setter;
		}

		public IAbstractAnimatorSetter<T> Setter { get; private set; }

		public IAnimable Animable => animator.Animable;

		public int TargetPropertyPathComparisonCode => animator.TargetPropertyPathComparisonCode;

		public int Duration => animator.Duration;

		public bool IsTriggerable => animator.IsTriggerable;

		public Type ValueType => typeof(T);

		public void ExecuteTriggersInRange(double minTime, double maxTime, bool executeTriggerAtMaxTime)
		{
			minTime = animation.BezierEasingCalculator.EaseTime(minTime);
			maxTime = animation.BezierEasingCalculator.EaseTime(maxTime);
			animator.ExecuteTriggersInRange(minTime, maxTime, executeTriggerAtMaxTime);
		}

		public void Apply(double time)
		{
			Setter?.SetValue(CalcValue(time));
		}

		public T CalcValue(double time)
		{
			time = animation.BezierEasingCalculator.EaseTime(time);
			return animator.CalcValue(time);
		}
	}
}
