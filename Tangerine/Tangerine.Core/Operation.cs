using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine.Core
{
	public interface IOperation
	{
		void Do();
		void Undo();
		DateTime Timestamp { get; set; }
		bool IsChangingDocument { get; }
	}

	public class DelegateOperation : IOperation
	{
		readonly Action @do;
		readonly Action undo;

		public bool IsChangingDocument { get; private set; }
		public DateTime Timestamp { get; set; }

		public static void Perform(Action @do, Action undo, bool hasModifications = false)
		{
			Document.Current.History.Perform(new DelegateOperation(@do, undo, hasModifications));
		}

		private DelegateOperation(Action @do, Action undo, bool hasModifications)
		{
			this.@do = @do;
			this.undo = undo;
			this.IsChangingDocument = hasModifications;
		}

		public void Do() => @do?.Invoke();
		public void Undo() => undo?.Invoke();
	}
}
