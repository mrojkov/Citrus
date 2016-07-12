using System;

namespace Lime
{
	public abstract class WaitPredicate
	{
		public float TotalTime;
		public abstract bool Evaluate();
	}

	public class AnimationWaitPredicate : WaitPredicate
	{
		public Node Node;

		public override bool Evaluate() { return Node.IsRunning; }
	}

	public class InputWaitPredicate : WaitPredicate
	{
		public static readonly InputWaitPredicate Instance = new InputWaitPredicate();

		private InputWaitPredicate() { }
		public override bool Evaluate() { return !Window.Current.Input.Changed; }
	}

	public class BooleanWaitPredicate : WaitPredicate
	{
		public Func<bool> Predicate;

		public override bool Evaluate() { return Predicate(); }
	}

	public class TimeWaitPredicate : WaitPredicate
	{
		public Func<float, bool> Predicate;

		public override bool Evaluate() { return Predicate(TotalTime); }
	}
}