using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class AnimablePropertyBinding<T> : IProcessor
	{
		readonly object obj;
		readonly string propertyName;
		readonly IDataflowProvider<T> values;

		public AnimablePropertyBinding(object obj, string propertyName, IDataflowProvider<T> values)
		{
			this.obj = obj;
			this.propertyName = propertyName;
			this.values = values;
		}

		public IEnumerator<object> Loop()
		{
			var c = values.Consume(v =>
				Document.Current.History.Execute(new Core.Operations.SetAnimableProperty(obj, propertyName, v)));
			while (true) { 
				c.Execute();
				yield return null;
			}
		}
	}
}