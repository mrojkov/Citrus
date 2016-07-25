using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class CheckBoxBinding : IProcessor
	{
		readonly CheckBox checkBox;
		readonly IDataflowProvider<bool> source;

		public CheckBoxBinding(CheckBox checkBox, IDataflowProvider<bool> source)
		{
			this.source = source;
			this.checkBox = checkBox;
		}

		public IEnumerator<object> Loop()
		{
			var c = source.Consume(v => checkBox.Checked = v);
			while (true) {
				c.Execute();
				yield return null;
			}
		}
	}
}