using System;
using Lime;
using System.Collections.Generic;

namespace Tangerine.Core
{
	public interface IProcessor
	{
		IEnumerator<object> MainLoop();
	}

	public static class TaskListExtension
	{
		public static void AddRange(this TaskList taskList, IEnumerable<IProcessor> collection)
		{
			foreach (var i in collection) {
				taskList.Add(i.MainLoop());
			}
		}

		public static void Add(this TaskList taskList, IProcessor processor)
		{
			taskList.Add(processor.MainLoop());
		}
	}
}

