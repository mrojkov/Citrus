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
			var dataflow = source.GetDataflow();
			while (true) {
				dataflow.Poll();
				if (dataflow.GotValue) {
					dropDownList.Value = dataflow.Value;
				}
				yield return null;
			}
		}
	}
}