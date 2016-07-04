using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class EditBoxBinding : IProcessor
	{
		readonly EditBox editBox;
		readonly IDataflowProvider<string> source;

		public EditBoxBinding(EditBox editBox, IDataflowProvider<string> source)
		{
			this.source = source;
			this.editBox = editBox;
		}

		public IEnumerator<object> MainLoop()
		{
			var dataflow = source.GetDataflow();
			var wasFocused = false;
			while (true) {
				dataflow.Poll();
				var focused = editBox.IsFocused();
				var lostFocus = wasFocused && !focused;
				if (!focused && dataflow.GotValue || lostFocus) {
					editBox.Text = dataflow.Value;
				}
				wasFocused = focused;
				yield return null;
			}
		}
	}	
}