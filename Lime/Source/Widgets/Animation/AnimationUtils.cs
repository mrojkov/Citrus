using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Lime
{
	public static class AnimationUtils
	{
		public const double FramesPerSecond = 30.0;
		public const double SecondsPerFrame = 1 / FramesPerSecond;

		public static int SecondsToFrames(double seconds)
		{
			return (int)(seconds * FramesPerSecond + 0.000001);
		}

		public static double FramesToSeconds(int frame)
		{
			return frame * SecondsPerFrame;
		}

		internal struct PropertyData
		{
			public Type OwnerType;
			public PropertyInfo Info;
			public bool Triggerable;
		}

		[ThreadStatic]
		private static Dictionary<string, List<PropertyData>> propertyCache;

		internal static PropertyData GetProperty(Type ownerType, string propertyName)
		{
			if (propertyCache == null) {
				propertyCache = new Dictionary<string, List<PropertyData>>();
			}
			List<PropertyData> plist;
			if (!propertyCache.TryGetValue(propertyName, out plist)) {
				plist = new List<PropertyData>();
				propertyCache[propertyName] = plist;
			}
			foreach (PropertyData i in plist) {
				if (ownerType == i.OwnerType) {
					return i;
				}
			}
			var p = new PropertyData();
			p.Info = ownerType.GetProperty(propertyName);
			p.OwnerType = ownerType;
			if (p.Info == null) {
				throw new Lime.Exception("Property '{0}' doesn't exist for class '{1}'", propertyName, ownerType);
			}
			p.Triggerable = p.Info.GetCustomAttributes(typeof(TriggerAttribute), false).Length > 0;
			plist.Add(p);
			return p;
		}
	}
}
