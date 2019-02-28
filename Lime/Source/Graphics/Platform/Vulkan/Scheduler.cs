using System;
using System.Collections.Generic;

namespace Lime.Graphics.Platform.Vulkan
{
	internal class Scheduler
	{
		private PlatformRenderContext context;
		private Queue<Entry> queue = new Queue<Entry>();

		public Scheduler(PlatformRenderContext context)
		{
			this.context = context;
		}

		public void Add(ulong fenceValue, Action action)
		{
			queue.Enqueue(new Entry {
				FenceValue = fenceValue,
				Action = action
			});
		}

		public void Perform()
		{
			while (queue.Count > 0 && context.IsFenceCompleted(queue.Peek().FenceValue)) {
				queue.Dequeue().Action();
			}
		}

		private struct Entry
		{
			public ulong FenceValue;
			public Action Action;
		}
	}
}
