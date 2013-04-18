using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine.Commands
{
	public class ChangeNodeProperty : Command
	{
		object value;
		object savedValue;
		Lime.Node node;
		string property;

		public ChangeNodeProperty(Lime.Node node, string property, object value)
		{
			this.node = node;
			this.property = property;
			this.value = value;
		}

		public override void Do()
		{
			var propInfo = node.GetType().GetProperty(property);
			savedValue = propInfo.GetValue(node, null);
			propInfo.SetValue(node, value, null);
		}

		public override void Undo()
		{
			var propInfo = node.GetType().GetProperty(property);
			propInfo.SetValue(node, savedValue, null);
		}
	}
}
