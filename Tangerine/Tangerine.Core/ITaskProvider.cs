using System;
using Lime;
using System.Collections.Generic;

namespace Tangerine.Core
{
	public interface ITaskProvider
	{
		IEnumerator<object> Loop();
	}

	public static class TaskListExtension
	{
		public static void Add(this TaskList taskList, params ITaskProvider[] collection)
		{
			foreach (var i in collection) {
				taskList.Add(i.Loop());
			}
		}

		public static void Add(this TaskList taskList, ITaskProvider task)
		{
			taskList.Add(task.Loop());
		}
	}
}

