using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Lime
{
	public static class AnimationUtils
	{
		/// <summary>
		///  оличнство кадров в одной секунде
		/// </summary>
		public const int FramesPerSecond = 16;

		/// <summary>
		///  оличество миллисекунд в одном кадре
		/// </summary>
		public const int MsecsPerFrame = 1000 / AnimationUtils.FramesPerSecond - 1;

		/// <summary>
		/// ѕереводит врем€ в миллисекундах во врем€, измер€емое кадрами
		/// </summary>
		public static int MsecsToFrames(int msecs)
		{
			return msecs >> 6;
		}

		/// <summary>
		/// ѕереводит врем€, измер€емое кадрами во врем€ в миллисекунды
		/// </summary>
		public static int FramesToMsecs(int frames)
		{
			return frames << 6;
		}

		internal struct PropertyData
		{
			public Type OwnerType;
			public PropertyInfo Info;
			public bool Triggerable;
		}

		private static Dictionary<string, List<PropertyData>> propertyCache = new Dictionary<string, List<PropertyData>>();

		internal static PropertyData GetProperty(Type ownerType, string propertyName)
		{
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
