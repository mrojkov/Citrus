using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orange
{
	[AttributeUsage(AttributeTargets.Method)]
	public class PluginInitializationAttribute : Attribute
	{
	}
}
