using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Lime
{
	internal class ReferenceEqualityComparer : IEqualityComparer<object>
	{
		public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

		private ReferenceEqualityComparer() { }

		public new bool Equals(object x, object y) => x == y;

		public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
	}
}
