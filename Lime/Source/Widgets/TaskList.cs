using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class TaskResult<T>
	{
		public T Value;
	}

	public class TaskResult<T1, T2>
	{
		public T1 Value1;
		public T2 Value2;
	}

	public class TaskList : List<Task>
	{
		[ThreadStatic]
		private static TaskList current;
		public static TaskList Current { 
			get { return current; } 
			private set { current = value; } 
		}

		public float Time;
		public float Delta;

		public void Stop()
		{
			foreach (var i in ToArray()) {
				i.Dispose();
			}
			Clear();
		}

		public void Stop(Predicate<Task> match)
		{
			foreach (var i in ToArray()) {
				if (match(i)) {
					i.Dispose();
				}
			}
			RemoveAll(match);
		}

		public Task Add(IEnumerator<object> e, object tag = null)
		{
			var task = new Task(e, tag);
			Add(task);
			return task;
		}

		private bool isBeingUpdated;

		public void Update(float delta)
		{
			if (isBeingUpdated) {
				return;
			}
			isBeingUpdated = true;
			TaskList savedCurrent = Current;
			Current = this;
			try {
				Time += delta;
				Delta = delta;
				for (int i = 0; i < Count; ) {
					var task = this[i];
					if (task.Completed) {
						Remove(task);
					} else {
						task.Advance(delta);
						i++;
					}
				}
			} finally {
				isBeingUpdated = false;
				Current = savedCurrent;
			}
		}

		#region Utility methods
		public static IEnumerable<float> SinMotion(float timePeriod, float from, float to)
		{
			for (float t = 0; t < timePeriod; t += Current.Delta) {
				float v = Mathf.Sin(t / timePeriod * Mathf.HalfPi);
				yield return Mathf.Lerp(v, from, to);
			}
			yield return to;
		}

		public static IEnumerable<float> SqrtMotion(float timePeriod, float from, float to)
		{
			for (float t = 0; t < timePeriod; t += Current.Delta) {
				float v = Mathf.Sqrt(t / timePeriod);
				yield return Mathf.Lerp(v, from, to);
			}
			yield return to;
		}

		public static IEnumerable<float> LinearMotion(float timePeriod, float from, float to)
		{
			for (float t = 0; t < timePeriod; t += Current.Delta) {
				yield return Mathf.Lerp(t / timePeriod, from, to);
			}
			yield return to;
		}
		#endregion
	}
}
