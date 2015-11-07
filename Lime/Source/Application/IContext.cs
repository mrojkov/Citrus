using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public struct ContextScope : IDisposable
	{
		public IContext SavedContext;

		public void Dispose()
		{
			SavedContext.MakeCurrent();
		}
	}

	public interface IContext
	{
		ContextScope MakeCurrent();
	}
}
