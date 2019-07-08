using System;
using System.Collections.Generic;

namespace Orange
{
	public interface ICookStage
	{
		void Action(Target target);
		IEnumerable<string> ImportedExtensions { get; }
		IEnumerable<string> BundleExtensions { get; }
		int GetOperationsCount();
	}
}
