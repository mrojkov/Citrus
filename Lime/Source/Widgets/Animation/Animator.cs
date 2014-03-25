using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Lime;
using ProtoBuf;

namespace Lime
{
	public static class AnimationUtils
	{
		public const int FramesPerSecond = 16;

		public static int MsecsToFrames(int msecs)
		{
			return msecs >> 6;
		}

		public static int FramesToMsecs(int frames)
		{
			return frames << 6;
		}

		public struct PropertyData
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

	[ProtoContract]
	[ProtoInclude(101, typeof(Animator<string>))]
	[ProtoInclude(102, typeof(Animator<int>))]
	[ProtoInclude(103, typeof(Animator<bool>))]
	[ProtoInclude(104, typeof(Animator<Blending>))]
	[ProtoInclude(105, typeof(Animator<ITexture>))]
	[ProtoInclude(106, typeof(Animator<NumericRange>))]
	[ProtoInclude(107, typeof(Animator<Vector2>))]
	[ProtoInclude(108, typeof(Animator<Color4>))]
	[ProtoInclude(109, typeof(Animator<float>))]
	[ProtoInclude(110, typeof(Animator<EmitterShape>))]
	[ProtoInclude(111, typeof(Animator<AudioAction>))]
	[ProtoInclude(112, typeof(Animator<SerializableSample>))]
	[ProtoInclude(113, typeof(Animator<HAlignment>))]
	[ProtoInclude(114, typeof(Animator<VAlignment>))]
	[ProtoInclude(115, typeof(Animator<SerializableSample>))]
	[ProtoInclude(116, typeof(Animator<MovieAction>))]
	public interface IAnimator
	{
		void Bind(Node owner);

		IAnimator Clone();

		bool IsTriggerable { get; set; }

		string TargetProperty { get; set; }

		bool IsEvaluable();

		int Duration { get; }

		void InvokeTrigger(int frame);

		void Apply(int time);

		IKeyframeCollection Keys { get; }
	}

	[ProtoContract]
	[ProtoInclude(101, typeof(NumericAnimator))]
	[ProtoInclude(102, typeof(Vector2Animator))]
	[ProtoInclude(103, typeof(Color4Animator))]
	public class Animator<T> : IAnimator
	{
		private Node owner;
		private int currentKey = 0;

		public bool IsTriggerable { get; set; }

		[ProtoMember(1)]
		public string TargetProperty { get; set; }

		[ProtoMember(2)]
		public KeyframeCollection<T> ReadonlyKeys = new KeyframeCollection<T>();

		public KeyframeCollection<T> Keys
		{
			get {
				if (ReadonlyKeys.Shared) {
					ReadonlyKeys = ReadonlyKeys.Clone();
				}
				return ReadonlyKeys;
			}
		}
			
		IKeyframeCollection proxyKeys;
		IKeyframeCollection IAnimator.Keys {
			get {
				if (proxyKeys == null) {
					proxyKeys = new KeyframeCollectionProxy<T>(Keys);
				}
				return proxyKeys;
			}
		}
			
		public IAnimator Clone()
		{
			var clone = (Animator<T>)MemberwiseClone();
			clone.proxyKeys = null;
			proxyKeys = null;
			ReadonlyKeys.Shared = true;
			return clone;
		}

		protected delegate void SetterDelegate(T value);

		protected SetterDelegate Setter;

		public void Bind(Node owner)
		{
			this.owner = owner;
			var p = AnimationUtils.GetProperty(owner.GetType(), TargetProperty);
			IsTriggerable = p.Triggerable;
			var mi = p.Info.GetSetMethod();
			if (mi == null) {
				throw new Lime.Exception("Property '{0}' (class '{1}') is readonly", TargetProperty, owner.GetType());
			}
			Setter = (SetterDelegate)Delegate.CreateDelegate(typeof(SetterDelegate), owner, mi);
		}
		
		protected virtual void InterpolateValue(float t, T a, T b)
		{
			Setter(a);
		}

		protected virtual void InterpolateValue(float t, T a, T b, T c, T d)
		{
			InterpolateValue(t, b, c);
		}

		public void Clear()
		{
			currentKey = 0;
			ReadonlyKeys.Clear();
		}

		public void InvokeTrigger(int frame)
		{
			if (ReadonlyKeys.Count > 0) {
				// This function relies on currentKey value. Therefore Apply(time) must be called before.
				if (ReadonlyKeys[currentKey].Frame == frame) {
					owner.OnTrigger(TargetProperty);
				}
			}
		}

		public void Apply(int time)
		{
			int count = ReadonlyKeys.Count;
			if (count == 0)
				return;
			int frame = AnimationUtils.MsecsToFrames(time);
			while (currentKey < count - 1 && frame > ReadonlyKeys[currentKey].Frame)
				currentKey++;
			while (currentKey >= 0 && frame < ReadonlyKeys[currentKey].Frame)
				currentKey--;
			if (currentKey < 0) {
				Setter(ReadonlyKeys[0].Value);
				currentKey = 0;
			} else if (currentKey == count - 1) {
				Setter(ReadonlyKeys[count - 1].Value);
			} else {
				ApplyHelper(time);
			}
		}

		private void ApplyHelper(int time)
		{
			int i = currentKey;
			var key1 = ReadonlyKeys[i];
			KeyFunction function = key1.Function;
			if (function == KeyFunction.Steep) {
				Setter(key1.Value);
			} else {
				var key2 = ReadonlyKeys[i + 1];
				int t0 = AnimationUtils.FramesToMsecs(key1.Frame);
				int t1 = AnimationUtils.FramesToMsecs(key2.Frame);
				float t = (time - t0) / (float)(t1 - t0);
				switch (function) {
				case KeyFunction.Linear:
					InterpolateValue(t, key1.Value, key2.Value);
					break;
				case KeyFunction.Spline:
					{
						int count = ReadonlyKeys.Count;
						var key0 = ReadonlyKeys[i < 1 ? 0 : i - 1];
						var key3 = ReadonlyKeys[i + 1 >= count - 1 ? count - 1 : i + 1];
						InterpolateValue(t, key0.Value, key1.Value, key2.Value, key3.Value);
					}
					break;
				case KeyFunction.ClosedSpline:
					{
						int count = ReadonlyKeys.Count;
						var key0 = ReadonlyKeys[i < 1 ? count - 1 : i - 1];
						var key3 = ReadonlyKeys[i + 1 >= count - 1 ? 0 : i + 1];
						InterpolateValue(t, key0.Value, key1.Value, key2.Value, key3.Value);
					}
					break;
				}
			}
		}

		public int Duration {
			get {
				if (ReadonlyKeys.Count == 0)
					return 0;
				return ReadonlyKeys[ReadonlyKeys.Count - 1].Frame;
			}
		}

		public void Add(Keyframe<T> key)
		{
			if (!IsEvaluable()) {
				key.Function = KeyFunction.Steep;
			}
			int c = ReadonlyKeys.Count;
			if (c > 0 && ReadonlyKeys[c - 1].Frame >= key.Frame) {
				throw new Lime.Exception("Key frames must be added in ascendant order");
			}
			ReadonlyKeys.Add(key);
		}

		public bool IsEvaluable()
		{
			return true;
		}
	}

	[ProtoContract]
	public class Vector2Animator : Animator<Vector2>
	{		
		protected override void InterpolateValue(float t, Vector2 a, Vector2 b)
		{
			Setter(Vector2.Lerp(t, a, b));
		}

		protected override void InterpolateValue(float t, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
		{
			Setter(Mathf.CatmullRomSpline(t, a, b, c, d));
		}
	}

	[ProtoContract]
	public class NumericAnimator : Animator<float>
	{
		protected override void InterpolateValue(float t, float a, float b)
		{
			Setter(t * (b - a) + a);
		}

		protected override void InterpolateValue(float t, float a, float b, float c, float d)
		{
			Setter(Mathf.CatmullRomSpline(t, a, b, c, d));
		}
	}

	[ProtoContract]
	public class Color4Animator : Animator<Color4>
	{
		protected override void InterpolateValue(float t, Color4 a, Color4 b)
		{
			Setter(Color4.Lerp(t, a, b));
		}
	}
}
