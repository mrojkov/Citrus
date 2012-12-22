using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine
{
	public class MemoryLeaksChecker : IDisposable
	{
		long initialAllocated;

		public MemoryLeaksChecker()
		{
			System.GC.GetTotalMemory(true);
			initialAllocated = System.GC.GetTotalMemory(true);
		}

		void IDisposable.Dispose()
		{
			System.GC.GetTotalMemory(true);
			long leaked = System.GC.GetTotalMemory(true) - initialAllocated;
			Console.WriteLine("Leaked {0} bytes", leaked);
		}
	}
}
