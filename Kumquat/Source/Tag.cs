using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kumquat
{
	public class Tag : Attribute
	{
		public string Value
		{
			get; protected set;
		}

		public Tag(string value)
		{
			this.Value = value;
		}
	}
}