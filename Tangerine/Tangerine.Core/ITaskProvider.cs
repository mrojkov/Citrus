using System;
using Lime;
using System.Collections.Generic;

namespace Tangerine.Core
{
	public interface ITaskProvider
	{
		IEnumerator<object> Task();
	}

	public static class TaskListExtension
	{
		public static void Add(this TaskList taskList, params ITaskProvider[] collection)
		{
			foreach (var i in collection) {
				taskList.Add(i.Task());
			}
		}

		public static void Add(this TaskList taskList, ITaskProvider task)
		{
			taskList.Add(task.Task());
		}
	}

	public static class TaskProviderExtensions
	{
		public static void AddTo(this ITaskProvider task, TaskList tasks)
		{
			tasks.Add(task);
		}

		public static void AddTo(this ITaskProvider task, Widget widget)
		{
			widget.Tasks.Add(task);
		}
	}
}

