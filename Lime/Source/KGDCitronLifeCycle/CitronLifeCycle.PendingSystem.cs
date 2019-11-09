using System;
using System.Collections.Generic;

namespace Lime.KGDCitronLifeCycle {
	public static partial class CitronLifeCycle
	{
		private class PendingSystem
		{
			public readonly LinkedList<Action<float>> PendingCustomUpdates = new LinkedList<Action<float>>();
			public readonly LinkedList<Node> PendingTasksUpdate = new LinkedList<Node>();
			public readonly LinkedList<Node> PendingAdvanceAnimation = new LinkedList<Node>();
		}
	}
}