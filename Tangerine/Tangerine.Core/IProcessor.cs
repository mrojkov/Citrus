using System;
using Lime;
using System.Collections.Generic;

namespace Tangerine.Core
{
	public interface IProcessor
	{
		IEnumerator<object> Loop();
	}

	public static class TaskListExtension
	{
		public static void Add(this TaskList taskList, params IProcessor[] collection)
		{
			foreach (var i in collection) {
				taskList.Add(i.Loop());
			}
		}

		public static void Add(this TaskList taskList, IProcessor processor)
		{
			taskList.Add(processor.Loop());
		}
	}
}

