using System.Collections.Generic;
using System.IO;

namespace Lime
{
	public interface ITaskProfiler
	{
		void RegisterTask(IEnumerator<object> enumerator);
		void BeforeAdvance(IEnumerator<object> enumerator);
		void AfterAdvance(IEnumerator<object> enumerator);
		void DumpProfile(TextWriter writer);
	}
}
