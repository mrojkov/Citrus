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
			public string TaskId;
			public TimeSpan ConsumedTime;
			public int Updates;
			public float TimePerUpdate => (float)ConsumedTime.Ticks / Updates;
		}

		private static readonly Dictionary<string, UsageData> records = new Dictionary<string,UsageData>();

		public static void Register(Task task, TimeSpan timeSpan)
		{
			var id = task.InitialEnumeratorType.ToString();
			if (!records.ContainsKey(id)) {
				records[id] = new UsageData();
				records[id].TaskId = id;
			}
			records[id].ConsumedTime += timeSpan;
			records[id].Updates += 1;
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

		public static List<UsageData> GetTopByConsumedTime(int count)
		{
			var values = records.Values.ToList();
			values.Sort((a, b) => b.ConsumedTime.CompareTo(a.ConsumedTime));
			return values.Take(count).ToList();
		}

		public static List<UsageData> GetTopByConsumedTimePerUpdate(int count)
		{
			var values = records.Values.ToList();
			values.Sort((a, b) => b.TimePerUpdate.CompareTo(a.TimePerUpdate));
			return values.Take(count).ToList();
		}

	}
}
#endif