using System;
using System.Collections.Generic;
using System.Linq;

namespace Lime
{
	internal class AtomicFlagAttribute : Attribute
	{
	}

	internal static class EnumExtensions
	{
		public static TEnum[] GetAtomicFlags<TEnum>()
		{
			return AtomicFlagsCache<TEnum>.Values;
		}
	}

	internal static class AtomicFlagsCache<TEnum>
	{
		public static readonly TEnum[] Values = GetTargetMembers().ToArray();

		private static IEnumerable<TEnum> GetTargetMembers()
		{
			var type = typeof(TEnum);
			foreach (var member in (TEnum[])Enum.GetValues(type)) {
				var info = type.GetMember(Enum.GetName(type, member)).Single();
				if (info.IsDefined(typeof(AtomicFlagAttribute), false)) {
					yield return member;
				}
			}
		}
	}
}