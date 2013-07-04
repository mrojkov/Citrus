using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orange
{
	[AttributeUsage(AttributeTargets.Method)]
	public class MenuItemAttribute : Attribute
	{
		public string Label;

		public int Priority;

		public MenuItemAttribute(string label, int priority = 100)
		{
			Label = label;
			Priority = priority;
		}
	}
}
