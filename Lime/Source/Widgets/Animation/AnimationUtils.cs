using System;
using System.Collections;
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
		public const double Threshold = 0.000001;

		public static int SecondsToFrames(double seconds)
		{
			return (int)(seconds * FramesPerSecond + Threshold);
		}

		public static int SecondsToFramesCeiling(double seconds)
		{
			return (int)(seconds * FramesPerSecond + (1 - Threshold));
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

		public static (IAnimationHost, int) GetPropertyHost(IAnimationHost host, string propertyPath)
		{
			int prevIndex = 0;
			int index;
			while ((index = propertyPath.IndexOf('/', prevIndex)) >= 0) {
				var id = propertyPath.Substring(prevIndex, index - prevIndex);
				host = ((Node)host).TryFindNode(id);
				if (host == null) {
					return (null, -1);
				}
				prevIndex = index + 1;
			}
			return (host, prevIndex);
		}

		public static (PropertyData PropertyData, IAnimable Animable, int Index) GetPropertyByPath(IAnimationHost host, string propertyPath)
		{
			PropertyData result = PropertyData.Empty;
			int prevIndex;
			(host, prevIndex) = GetPropertyHost(host, propertyPath);
			object o = host;
			if (propertyPath[prevIndex] == '[') {
				var index = IndexOfClosedBracket(prevIndex + 1, propertyPath);
				if (index < 0) {
					return (result, null, -1);
				}
				var	componentTypeName = propertyPath.Substring(prevIndex + 1, index - prevIndex - 1);
				var type = global::Yuzu.Metadata.Meta.GetTypeByReadAlias(componentTypeName, InternalPersistence.Instance.YuzuCommonOptions)
				           ?? global::Yuzu.Util.TypeSerializer.Deserialize(componentTypeName);
				o = host.GetComponent(type);
				if (o == null) {
					return (result, null, -1);
				}
				prevIndex = index + 2;
			}
			while (true) {
				int periodIndex = propertyPath.IndexOf('.', prevIndex);
				bool last = periodIndex == -1;
				int length = last
					? propertyPath.Length - prevIndex
					: periodIndex - prevIndex;
				var p = propertyPath.Substring(prevIndex, length);
				int bracketIndex = p.IndexOf('[');
				int indexInList = -1;
				if (bracketIndex == -1) {
					result = GetProperty(o.GetType(), p);
				} else {
					indexInList = int.Parse(p.Substring(bracketIndex + 1, p.Length - bracketIndex - 2));
					result = GetProperty(o.GetType(), p.Substring(0, bracketIndex));
				}
				if (result.Info == null) {
					return (result, null, -1);
				}
				if (last) {
					return (result, (IAnimable)o, indexInList);
				} else {
					if (indexInList == -1) {
						o = result.Info.GetValue(o);
					} else if (o is IList list && indexInList >= list.Count) {
						return (result, null, -1);
					} else {
						o = result.Info.GetValue(o, new object[] { indexInList });
					}
					if (o == null) {
						return (result, null, -1);
					}
				}
				prevIndex = periodIndex + 1;
			}

			int IndexOfClosedBracket(int startIndex, string value)
			{
				var c = 1;
				for (var i = startIndex; i < value.Length; i++) {
					if (value[i] == '[') {
						c++;
					} else if (value[i] == ']') {
						if (--c == 0) {
							return i;
						}
					}
				}
				return -1;
			}
		}

		private static PropertyData GetProperty(Type ownerType, string propertyName)
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
			p.Triggerable = p.Info?.GetCustomAttributes(typeof(TriggerAttribute), false)?.Length > 0;
			plist.Add(p);
			return p;
		}
	}
}
