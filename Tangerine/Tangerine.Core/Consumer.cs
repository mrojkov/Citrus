using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine.Core
{
	public interface IConsumer
	{
		void Consume();
	}

	public class Consumer<T> : IConsumer
	{
		private readonly IDataflow<T> dataflow;
		private readonly Action<T> action;

		public Consumer(IDataflow<T> dataflow, Action<T> action)
		{
			this.dataflow = dataflow;
			this.action = action;
		}
		public void Consume()
		{
			dataflow.Poll();
			if (dataflow.GotValue) {
				action(dataflow.Value);
			}
		}
	}

	[NodeComponentDontSerialize]
	public class ConsumeBehaviour : BehaviorComponent
	{
		private readonly List<IConsumer> consumers = new List<IConsumer>();

		public ConsumeBehaviour() { }

		public void Add(IConsumer consumer) => consumers.Add(consumer);

		protected override void Update(float delta)
		{
			foreach (var i in consumers) {
				i.Consume();
			}
		}
	}

	[NodeComponentDontSerialize]
	[UpdateStage(typeof(EarlyUpdateStage))]
	public class EarlyConsumeBehaviour : ConsumeBehaviour
	{
	}

	[UpdateStage(typeof(LateUpdateStage))]
	[NodeComponentDontSerialize]
	public class LateConsumeBehaviour : ConsumeBehaviour
	{
	}
}
