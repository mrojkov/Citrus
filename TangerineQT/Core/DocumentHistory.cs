using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine
{
	public class DocumentHistory
	{
		List<Command> Commands = new List<Command>();

		CompoundCommand transaction = new CompoundCommand();

		public void Add(Command command)
		{
			transaction.Add(command);
		}

		public void Commit(string text)
		{
			transaction.Text = text;
			Commands.Add(transaction);
			transaction.Do();
			The.Document.OnChanged();
			transaction = new CompoundCommand();
		}
	}
}
