#if PROFILE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public static class TasksProfiler
	{
		public class UsageData
		{
			public readonly string TaskId;
			public long ConsumedTicks;
			public int Updates;
			public float TicksPerUpdate => (float)ConsumedTicks / Updates;

			public UsageData(string taskId)
			{
				TaskId = taskId;
			}
		}

		private static readonly Dictionary<string, UsageData> records = new Dictionary<string,UsageData>();

		public static void Register(Task task, long ticks)
		{
			var id = task.InitialEnumeratorType.ToString();
			UsageData ud;
			if (records.ContainsKey(id)) {
				ud = records[id];
			} else {
				ud = new UsageData(id);
				records[id] = ud;
			}
			ud.ConsumedTicks += ticks;
			ud.Updates += 1;
		}

		public static void Reset()
		{
			records.Clear();
		}

		public static List<UsageData> GetTopByUsage(int count)
		{
			var values = records.Values.ToList();
			values.Sort((a, b) => b.Updates.CompareTo(a.Updates));
			return values.Take(count).ToList();
		}

		public static List<UsageData> GetTopByConsumedTicks(int count)
		{
			var values = records.Values.ToList();
			values.Sort((a, b) => b.ConsumedTicks.CompareTo(a.ConsumedTicks));
			return values.Take(count).ToList();
		}

		public static List<UsageData> GetTopByConsumedTicksPerUpdate(int count)
		{
			var values = records.Values.ToList();
			values.Sort((a, b) => b.TicksPerUpdate.CompareTo(a.TicksPerUpdate));
			return values.Take(count).ToList();
		}

	}
}
#endif