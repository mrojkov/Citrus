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

		/// <summary>
		/// Currently processing task.
		/// </summary>
		public static TaskList Current { 
			get { return current; } 
			private set { current = value; } 
		}

		/// <summary>
		/// Total time accumulated via Update.
		/// </summary>
		public float Time;

		/// <summary>
		/// Time delta since last Update.
		/// </summary>
		public float Delta;

		/// <summary>
		/// Stops all tasks.
		/// </summary>
		public void Stop()
		{
			foreach (var i in ToArray()) {
				i.Dispose();
			}
			Clear();
		}

		/// <summary>
		/// Stops all tasks that match the conditions defined by the specified predicate.
		/// </summary>
		public void Stop(Predicate<Task> match)
		{
			foreach (var i in ToArray()) {
				if (match(i)) {
					i.Dispose();
					Remove(i);
				}
			}
		}

		/// <summary>
		/// Stops all tasks with specified tag (null is also a tag).
		/// </summary>
		public void StopByTag(object tag)
		{
			Stop(t => t.Tag == tag);
		}

		/// <summary>
		/// Adds task to the end of this list.
		/// </summary>
		public Task Add(IEnumerator<object> e, object tag = null)
		{
			var task = new Task(e, tag);
			Add(task);
			return task;
		}

		/// <summary>
		/// Adds task to the end of this list.
		/// </summary>
		public Task Add(Func<IEnumerator<object>> e, object tag = null)
		{
			return Add(e(), tag);
		}

		/// <summary>
		/// Stops all tasks with specified tag, creates new one with same tag 
		/// and adds it to the end of this list.
		/// </summary>
		public Task Replace(IEnumerator<object> e, object tag)
		{
			StopByTag(tag);
			return Add(e, tag);
		}

		private bool isBeingUpdated;

		/// <summary>
		/// Advances tasks by provided delta and removes completed ones.
		/// </summary>
		/// <param name="delta">Time delta since last Update.</param>
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

		/// <summary>
		/// TODO: Add summary
		/// Задача (таск), изменяющая число в указанном диапазоне в течении указанного периода времени.
		/// Значение изменяется по синусоиде от from к to
		/// </summary>
		/// <param name="timePeriod">Период времени в секундах</param>
		/// <param name="from">Начальное значение</param>
		/// <param name="to">Конечное значение</param>
		public static IEnumerable<float> SinMotion(float timePeriod, float from, float to)
		{
			for (float t = 0; t < timePeriod; t += Current.Delta) {
				float v = Mathf.Sin(t / timePeriod * Mathf.HalfPi);
				yield return Mathf.Lerp(v, from, to);
			}
			yield return to;
		}

		/// <summary>
		/// TODO: Add summary
		/// Задача (таск), изменяющая число в указанном диапазоне в течении указанного периода времени.
		/// Значение изменяется по функции квадратного корня от from к to
		/// </summary>
		/// <param name="timePeriod">Период времени в секундах</param>
		/// <param name="from">Начальное значение</param>
		/// <param name="to">Конечное значение</param>
		public static IEnumerable<float> SqrtMotion(float timePeriod, float from, float to)
		{
			for (float t = 0; t < timePeriod; t += Current.Delta) {
				float v = Mathf.Sqrt(t / timePeriod);
				yield return Mathf.Lerp(v, from, to);
			}
			yield return to;
		}

		/// <summary>
		/// TODO: Add summary
		/// Задача (таск), изменяющая число в указанном диапазоне в течении указанного периода времени.
		/// Значение изменяется от from к to линейно
		/// </summary>
		/// <param name="timePeriod">Период времени в секундах</param>
		/// <param name="from">Начальное значение</param>
		/// <param name="to">Конечное значение</param>
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
