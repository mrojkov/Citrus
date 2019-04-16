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

	public abstract class CookStage: ICookStage
	{
		public abstract IEnumerable<string> ImportedExtensions { get; }
		public abstract IEnumerable<string> BundleExtensions { get; }
		public abstract void Action();
	}
}
