using System;

namespace Lime
{
	public static class Cloner
	{
		public static T Clone<T>(T obj) => (T)Clone((object)obj);

		public static object Clone(object obj)
		{
			return obj is ICloneable cloneable ? cloneable.Clone() : Serialization.Clone(obj);
		}
	}
}
