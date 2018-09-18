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
		private Node node;

		public AnimationWaitPredicate(Node node) { this.node = node; }
		public override bool Evaluate() { return node.IsRunning; }
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