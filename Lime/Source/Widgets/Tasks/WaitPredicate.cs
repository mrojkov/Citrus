using System;

namespace Lime
{
	public abstract class WaitPredicate
	{
		public float TotalTime;
		public abstract bool Evaluate();
	}

	class AnimationWaitPredicate : WaitPredicate
	{
		private Animation animation;

		public AnimationWaitPredicate(Animation animation) { this.animation = animation; }
		public override bool Evaluate() { return animation.IsRunning; }
	}

	class InputWaitPredicate : WaitPredicate
	{
		public static readonly InputWaitPredicate Instance = new InputWaitPredicate();

		private InputWaitPredicate() { }
		public override bool Evaluate() { return !Window.Current.Input.Changed; }
	}

	class BooleanWaitPredicate : WaitPredicate
	{
		private Func<bool> predicate;

		public BooleanWaitPredicate(Func<bool> predicate) { this.predicate = predicate; }
		public override bool Evaluate() { return predicate(); }
	}

	class TimeWaitPredicate : WaitPredicate
	{
		private Func<float, bool> predicate;

		public TimeWaitPredicate(Func<float, bool> predicate) { this.predicate = predicate; }
		public override bool Evaluate() { return predicate(TotalTime); }
	}
}