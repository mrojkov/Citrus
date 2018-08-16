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

		public struct PropertyData
		{
			public Type OwnerType;
			public PropertyInfo Info;
			public bool Triggerable;

			public static PropertyData Empty = new PropertyData();
		}

		[ThreadStatic]
		private static Dictionary<string, List<PropertyData>> propertyCache;

		public static (PropertyData, IAnimable) GetPropertyByPath(IAnimationHost owner, string propertyPath)
		{
			PropertyData result = PropertyData.Empty;
			object o = owner;
			if (propertyPath[0] == '[') {
				int index = propertyPath.IndexOf(']');
				var componentTypeName = propertyPath.Substring(1, index - 1);
				var type = Yuzu.Metadata.Meta.GetTypeByReadAlias(componentTypeName, Serialization.DefaultYuzuCommonOptions)
				           ?? Yuzu.Util.TypeSerializer.Deserialize(componentTypeName);
				o = owner.Components.Get(type);
				propertyPath = propertyPath.Substring(index + 2);
			}
			var parts = propertyPath.Split('.');
			for (int i = 0; i < parts.Length; i++) {
				var p = parts[i];
				result = GetProperty(o.GetType(), p);
				if (i < parts.Length - 1) {
					o = result.Info.GetValue(o);
				}
			}
			return (result, (IAnimable)o);
		}

		public static PropertyData GetProperty(Type ownerType, string propertyName)
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
