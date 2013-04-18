using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XwtPlus
{
	public class CommandManager
	{
		internal static List<Command> Commands = new List<Command>();

		internal static void Add(Command command)
		{
			Commands.Add(command);
		}

		public static void Realize()
		{
			foreach (var c in Commands) {
				c.Realize();
			}
		}
	}
}
