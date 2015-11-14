using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	/// <summary>
	/// Задача (таск). Бывает, что нужно задать какую-нибудь последовательность действий и ждать окончания ее выполнения.
	/// Задачи основаны на перечислителях (IEnumerator<object>) и их операторе yield return
	/// </summary>
	public class Task : IDisposable
	{
		[ThreadStatic]
		private static Task current;
		public static ITaskProfiler Profiler = new NullTaskProfiler();
		public static bool SkipFrameOnTaskCompletion;
		private Stack<IEnumerator<object>> stack = new Stack<IEnumerator<object>>();
		private WaitPredicate waitPredicate;
		private float waitTime;

		/// <summary>
		/// Time delta since last Update of current Task.
		/// </summary>
		[ThreadStatic]
		public static float Delta;

		/// <summary>
		/// Invoked on every Task update. Useful for disposing of the Task on some condition.
		/// </summary>
		public Action Watcher;

		/// <summary>
		/// Total time accumulated via Update.
		/// </summary>
		public float Time;

		public Task(IEnumerator<object> e, object tag = null)
		{
			Tag = tag;
			stack.Push(e);
			Profiler.RegisterTask(e);
		}

		public static Task Current { get { return current; } }

		public object Tag { get; set; }

		public bool Completed { get { return stack.Count == 0; } }

		public override string ToString()
		{
			return stack.Count == 0 ? "Completed" : stack.Peek().GetType().ToString();
		}

		public void Advance(float delta)
		{
			if (Completed) {
				return;
			}
			var savedCurrent = current;
			current = this;
			Delta = delta;
			Time += delta;
			var e = stack.Peek();
			Profiler.BeforeAdvance(e);
			try {
				if (Watcher != null) {
					Watcher();
					if (Completed) {
						return;
					}
				}
				if (waitTime > 0) {
					waitTime -= delta;
					return;
				}
				if (waitPredicate != null) {
					waitPredicate.TotalTime += delta;
					if (waitPredicate.Evaluate()) {
						return;
					}
					waitPredicate = null;
				}
				if (e.MoveNext()) {
					HandleYieldedResult(e.Current);
				} else if (!Completed) {
					stack.Pop();
					if (!SkipFrameOnTaskCompletion && !Completed) {
						Advance(0);
					}
				}
			} finally {
				current = savedCurrent;
				Profiler.AfterAdvance(e);
			}
		}

		public void Dispose()
		{
			while (stack.Count > 0) {
				var e = stack.Pop();
				e.Dispose();
			}
			waitPredicate = null;
			Watcher = null;
		}

		private void HandleYieldedResult(object result)
		{
			if (result == null) {
				waitTime = 0;
			}
			else if (result is int) {
				waitTime = (int)result;
			}
			else if (result is float) {
				waitTime = (float)result;
			}
			else if (result is IEnumerator<object>) {
				stack.Push((IEnumerator<object>) result);
				Advance(0);
			}
			else if (result is WaitPredicate) {
				waitPredicate = (WaitPredicate) result;
			}
			else if (result is Node) {
				waitPredicate = WaitForAnimation((Node) result);
			}
			else if (result is IEnumerable<object>) {
				throw new Exception("Use IEnumerator<object> instead of IEnumerable<object> for " + result);
			}
			else {
				throw new Exception("Invalid object yielded " + result);
			}
		}

		public static WaitPredicate WaitWhile(Func<bool> predicate)
		{
			return new BooleanWaitPredicate() { Predicate = predicate };
		}

		public static WaitPredicate WaitWhile(Func<float, bool> timePredicate)
		{
			return new TimeWaitPredicate() { Predicate = timePredicate };
		}
		
		public static WaitPredicate WaitForAnimation(Node node)
		{
			return new AnimationWaitPredicate() { Node = node };
		}

		/// <summary>
		/// Выполняет задачу асинхронно в другом потоке. Возвращает null до тех пор, пока задача не будет выполнена или отменена
		/// </summary>
		/// <param name="action">Действия, которые должны быть выполнены</param>
		public static IEnumerator<object> ExecuteAsync(Action action)
		{
#if UNITY
			throw new NotImplementedException();
#else
			var t = new System.Threading.Tasks.Task(action);
			t.Start();
			while (!t.IsCompleted && !t.IsCanceled && !t.IsFaulted) {
				yield return null;
			}
#endif
		}

		public static void KillMeIf(Func<bool> pred)
		{
			Current.Watcher = () => {
				if (pred()) {
					Current.Dispose();
				}
			};
		}

		/// <summary>
		/// Returns a sequence of numbers, interpolated as sine in specified time period.
		/// Advances by using Delta.
		/// </summary>
		public static IEnumerable<float> SinMotion(float timePeriod, float from, float to)
		{
			for (float t = 0; t < timePeriod; t += Delta) {
				float v = Mathf.Sin(t / timePeriod * Mathf.HalfPi);
				yield return Mathf.Lerp(v, from, to);
			}
			yield return to;
		}

		/// <summary>
		/// Returns a sequence of numbers, interpolated as square root in specified time period.
		/// Advances by using Delta.
		/// </summary>
		public static IEnumerable<float> SqrtMotion(float timePeriod, float from, float to)
		{
			for (float t = 0; t < timePeriod; t += Delta) {
				float v = Mathf.Sqrt(t / timePeriod);
				yield return Mathf.Lerp(v, from, to);
			}
			yield return to;
		}

		/// <summary>
		/// Returns a sequence of numbers, linear interpolated in specified time period.
		/// Advances by using Delta.
		/// </summary>
		public static IEnumerable<float> LinearMotion(float timePeriod, float from, float to)
		{
			for (float t = 0; t < timePeriod; t += Delta) {
				yield return Mathf.Lerp(t / timePeriod, from, to);
			}
			yield return to;
		}
	}
}
