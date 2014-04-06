using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	using EnumType = IEnumerator<object>;

	public class Task : IDisposable
	{
		public object Tag { get; set; }
		public bool Completed { get { return stack.Count == 0; } }
		private float sleepTime;
		private Node animationToWait;
		private Stack<EnumType> stack = new Stack<EnumType>();

		public Task(EnumType e, object tag = null)
		{
			Tag = tag;
			stack.Push(e);
		}

		public void Advance(float delta)
		{
			if (sleepTime > 0) {
				sleepTime -= delta;
				return;
			}
			if (animationToWait != null && animationToWait.IsRunning) {
				return;
			}
			var e = stack.Peek();
			if (e.MoveNext()) {
				HandleYieldedResult(e.Current);
			} else if (!Completed) {
				stack.Pop();
			}
		}

		public void Dispose()
		{
			while (stack.Count > 0) {
				var e = stack.Pop();
				e.Dispose();
			}
		}

		private void HandleYieldedResult(object result)
		{
			if (result is int) {
				sleepTime = (int)result;
			} else if (result is float) {
				sleepTime = (float)result;
			} else if (result is IEnumerator<object>) {
				stack.Push(result as IEnumerator<object>);
				Advance(0);
			} else if (result is Lime.Node) {
				animationToWait = result as Lime.Node;
			} else if (result is IEnumerable<object>) {
				throw new Lime.Exception("Use IEnumerator<object> instead of IEnumerable<object> for " + result);
			} else {
				throw new Lime.Exception("Invalid object yielded " + result);
			}
		}
	}
}
