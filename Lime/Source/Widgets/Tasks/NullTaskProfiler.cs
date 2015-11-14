using System.Collections.Generic;
using System.IO;

namespace Lime
{
	public class NullTaskProfiler: ITaskProfiler
	{
		public void RegisterTask(IEnumerator<object> enumerator) { }

		public void BeforeAdvance(IEnumerator<object> enumerator) { }

		public void AfterAdvance(IEnumerator<object> enumerator) { }

		public void DumpProfile(TextWriter writer) { }

		public bool IsNull
		{
			get { return true; }
		}
	}
}
