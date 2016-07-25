using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class DropDownListBinding<T> : IProcessor
	{
		readonly DropDownList dropDownList;
		readonly IDataflowProvider<T> source;

		public DropDownListBinding(DropDownList dropDownList, IDataflowProvider<T> source)
		{
			this.source = source;
			this.dropDownList = dropDownList;
		}

		public IEnumerator<object> Loop()
		{
			var c = source.Consume(v => dropDownList.Value = v);
			while (true) {
				c.Execute();
				yield return null;
			}
		}
	}
}