using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmptyProject
{
	public static class Toolbox
	{
		private static Random random;

		static Toolbox()
		{
			random = new Random();
		}

		public static int Rand(int min, int max)
		{
			return random.Next(min, max);
		}

		public static IEnumerator<object> WhenDoneTask(Task task, Action action)
		{
			while (!task.Completed) {
				yield return 0;
			}

			action();
		}

		public static IEnumerator<object> WaitTask(float time)
		{
			while (time > 0) {
				time -= Task.Current.Delta;
				yield return 0;
			}
		}
	}
}
