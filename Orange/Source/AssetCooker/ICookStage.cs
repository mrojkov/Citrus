using System;
using System.Collections.Generic;

namespace Orange
{
	public interface ICookStage
	{
		void Action();
		IEnumerable<string> ImportedExtensions { get; }
		IEnumerable<string> BundleExtensions { get; }
	}
}
