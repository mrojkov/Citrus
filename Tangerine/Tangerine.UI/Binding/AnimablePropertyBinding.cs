using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class AnimablePropertyBinding<T> : IProcessor
	{
		readonly object @object;
		readonly string propertyName;
		readonly IDataflowProvider<T> source;

		public AnimablePropertyBinding(object @object, string propertyName, IDataflowProvider<T> source)
		{
			this.@object = @object;
			this.propertyName = propertyName;
			this.source = source;
		}

		public IEnumerator<object> Loop()
		{
			var dataflow = source.GetDataflow();
			while (true) {
				dataflow.Poll();
				if (dataflow.GotValue) {
					var currentValue = new Property(@object, propertyName).Value;
					if (!Equals(currentValue, dataflow.Value)) {
						Document.Current.History.Execute(new Core.Operations.SetAnimableProperty(@object, propertyName, dataflow.Value));
					}
				}
				yield return null;
			}
		}
	}
}