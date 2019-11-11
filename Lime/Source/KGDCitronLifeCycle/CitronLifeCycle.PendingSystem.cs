using System;
using System.Collections.Generic;

namespace Lime.KGDCitronLifeCycle {
	public static partial class CitronLifeCycle
	{
		private class PendingSystem
		{
			public readonly LinkedList<Action<float>>[] PendingCustomUpdates;
			public readonly LinkedList<Node> PendingTasksUpdate = new LinkedList<Node>();
			public readonly LinkedList<Node> PendingAdvanceAnimation = new LinkedList<Node>();

			public PendingSystem(int customUpdatesTypesCount)
			{
				PendingCustomUpdates = new LinkedList<Action<float>>[customUpdatesTypesCount];
				for (int i = 0; i < PendingCustomUpdates.Length; i++) {
					PendingCustomUpdates[i] = new LinkedList<Action<float>>();
				}
			}
		}
	}
}
