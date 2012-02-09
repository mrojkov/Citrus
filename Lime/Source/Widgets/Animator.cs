using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Lime;
using ProtoBuf;

namespace Lime
{
	public enum KeyFunction
	{
		Linear,
		Steep,
		Spline,
		ClosedSpline
	}

	public struct KeyFrame
	{
		public int Frame;
		public KeyFunction Function;
		public Object Value;
	}

	[ProtoContract(SkipConstructor = true)]
	[ProtoInclude(101, typeof(GenericAnimator<string>))]
	[ProtoInclude(102, typeof(GenericAnimator<int>))]
	[ProtoInclude(103, typeof(GenericAnimator<bool>))]
	[ProtoInclude(104, typeof(GenericAnimator<Blending>))]
	[ProtoInclude(105, typeof(GenericAnimator<SerializableTexture>))]
	[ProtoInclude(106, typeof(GenericAnimator<NumericRange>))]
	[ProtoInclude(107, typeof(Vector2Animator))]
	[ProtoInclude(108, typeof(Color4Animator))]
	[ProtoInclude(109, typeof(NumericAnimator))]
	[ProtoInclude(110, typeof(GenericAnimator<EmitterShape>))]
	[ProtoInclude(111, typeof(GenericAnimator<AudioAction>))]
	[ProtoInclude(112, typeof(GenericAnimator<SerializableSample>))]
	public abstract class Animator
	{
		public const int FramesPerSecond = 16;

		static public int MsecsToFrames(int msecs)
		{
			return msecs >> 6;
		}

		static public int FramesToMsecs(int frames)
		{
			return frames << 6;
		}

		protected abstract Array values { get; }

		protected abstract void ResizeValuesArray(int newSize);

		protected Node Owner;
		internal bool IsTriggerable;

		internal abstract void Bind(Node owner);

		[ProtoMember(1)]
		public string TargetProperty;

		[ProtoMember(2)]
		int[] frames = new int[0];

		[ProtoMember(3)]
		KeyFunction[] functions = new KeyFunction[0];

		protected int currentKey = 0;
		
		public void Add(int frame, object value, KeyFunction function = KeyFunction.Linear)
		{
			Add(new KeyFrame {Frame = frame, Value = value, Function = function});
		}

		protected virtual bool IsEvaluable()
		{
			return true;
		}

		protected abstract void ApplyValue(int i);
		
		protected virtual void ApplyValue(float t, int a, int b)
		{
			ApplyValue(a);
		}

		protected virtual void ApplyValue(float t, int a, int b, int c, int d)
		{
			ApplyValue(t, b, c);
		}

		public void Remove(int index)
		{
			//Frames.RemoveAt(index);
			//Functions.RemoveAt(index);
			//Values.RemoveAt(index);
			currentKey = 0;
		}

		public void Clear()
		{
			frames = new int[0];
			functions = new KeyFunction[0];
			ResizeValuesArray(0);
			currentKey = 0;
		}

		public void InvokeTrigger(int intervalBegin, int intervalEnd)
		{
			if (frames.Length > 0) {
				// This function relies on currentKey value. Therefore Apply(time) must be called before.
				int t = FramesToMsecs(frames[currentKey]);
				if (t >= intervalBegin && t < intervalEnd) {
					Owner.OnTrigger(TargetProperty);
				}
			}
		}

		public void Apply(int time)
		{
			int count = frames.Length;
			if (count == 0)
				return;
			int frame = MsecsToFrames(time);
			while (currentKey < count - 1 && frame > frames[currentKey])
				currentKey++;
			while (currentKey >= 0 && frame < frames[currentKey])
				currentKey--;
			if (currentKey < 0) {
				ApplyValue(0);
				currentKey = 0;
			} else if (currentKey == count - 1) {
				ApplyValue(count - 1);
			} else {
				ApplyHelper(time);
			}
		}

		private void ApplyHelper(int time)
		{
			int i = currentKey;
			KeyFunction function = functions[i];
			if (function == KeyFunction.Steep) {
				ApplyValue(i);
			}
			else {
				int t0 = FramesToMsecs(frames[i]);
				int t1 = FramesToMsecs(frames[i + 1]);
				float t = (time - t0) / (float)(t1 - t0);
				switch(function) {
				case KeyFunction.Linear:
					ApplyValue(t, i, i + 1);
					break;
				case KeyFunction.Spline:
					{
						int count = frames.Length;
						int a = i < 1 ? 0 : i - 1;
						int b = i;
						int c = i + 1;
						int d = c + 1 >= count - 1 ? count - 1 : c + 1;
						ApplyValue(t, a, b, c, d);
					}
					break;
				case KeyFunction.ClosedSpline:
					{
						int count = frames.Length;
						int a = i < 1 ? count - 1 : i - 1;
						int b = i;
						int c = i + 1;
						int d = c + 1 >= count - 1 ? 0 : c + 1;
						ApplyValue(t, a, b, c, d);
					}
					break;
				}
			}
		}

		public int Duration {
			get {
				if (frames.Length == 0)
					return 0;
				return frames[frames.Length - 1];
			}
		}

		public void Add(KeyFrame key)
		{
			if (!IsEvaluable()) {
				key.Function = KeyFunction.Steep;
			}
			int c = frames.Length;
			Array.Resize<int>(ref frames, c + 1);
			Array.Resize<KeyFunction>(ref functions, c + 1);
			ResizeValuesArray(c + 1);
			frames[c] = key.Frame;
			functions[c] = key.Function;
			values.SetValue(key.Value, c);
		}

		public KeyFrame this[int index] { 
			get {
				return new KeyFrame { 
					Frame = frames[index], 
					Function = functions[index], 
					Value = values.GetValue(index) };
			} 
			set {
				if (!IsEvaluable()) {
					value.Function = KeyFunction.Steep;
				}
				frames[index] = value.Frame;
				functions[index] = value.Function;
				values.SetValue(value.Value, index);
			}
		}
		
		protected struct PropertyData
		{
			public PropertyInfo Info;
			public bool Triggerable;
		}
		
		static Dictionary<string, List<PropertyData>> propertyCache = new Dictionary<string, List<PropertyData>>();
		
		protected static PropertyData GetProperty(Type ownerType, string propertyName)
		{
			List<PropertyData> plist;
			if (!propertyCache.TryGetValue(propertyName, out plist)) {
				plist = new List<PropertyData>();
				propertyCache[propertyName] = plist;
			}
			foreach (PropertyData i in plist) {
				if (ownerType.IsSubclassOf(i.Info.DeclaringType))
					return i;
			}
			var p = new PropertyData();
			p.Info = ownerType.GetProperty(propertyName);
			if (p.Info == null)
				throw new Lime.Exception("Property '{0}' doesn't exist for class '{1}'", propertyName, ownerType);
			p.Triggerable = p.Info.GetCustomAttributes(typeof(TriggerAttribute), false).Length > 0;
			plist.Add(p);
			return p;
		}
	}
	
	[ProtoContract]
	public abstract class AnimatorHelper<T> : Animator
	{
		protected delegate void SetterDelegate(T value);
		
		protected SetterDelegate Setter;

		internal override void Bind(Node owner)
		{
			Owner = owner;
			PropertyData p = GetProperty(owner.GetType(), TargetProperty);
			IsTriggerable = p.Triggerable;
			var mi = p.Info.GetSetMethod();
			if (mi == null)
				throw new Lime.Exception("Property '{0}' (class '{1}') is readonly", TargetProperty, owner.GetType());
			Setter = (SetterDelegate)Delegate.CreateDelegate(typeof(SetterDelegate), owner, mi);
		}
	}

	[ProtoContract]
	public class GenericAnimator<T> : AnimatorHelper<T>
	{
		[ProtoMember(1)]
		T[] v = new T[0];

		protected override bool IsEvaluable()
		{
			return false;
		}

		protected override void ResizeValuesArray(int newSize)
		{
			Array.Resize<T>(ref v, newSize);
		}

		protected override Array values { get { return v; } }

		protected override void ApplyValue(int i)
		{
			Setter(v[i]);
		}
	}

	[ProtoContract]
	public class Vector2Animator : AnimatorHelper<Vector2>
	{
		[ProtoMember(1)]
		Vector2[] v = new Vector2[0];

		protected override Array values { get { return v; } }

		protected override void ResizeValuesArray(int newSize)
		{
			Array.Resize<Vector2>(ref v, newSize);
		}

		protected override void ApplyValue(int i)
		{
			Setter(v[i]);
		}
		
		protected override void ApplyValue(float t, int a, int b)
		{
			Setter(Vector2.Lerp(v[a], v[b], t));
		}

		protected override void ApplyValue(float t, int a, int b, int c, int d)
		{
			Setter(Utils.CatmullRomSpline(t, v[a], v[b], v[c], v[d]));
		}
	}

	[ProtoContract]
	public class NumericAnimator : AnimatorHelper<float>
	{
		[ProtoMember(1)]
		float[] v = new float[0];

		protected override Array values { get { return v; } }

		protected override void ResizeValuesArray(int newSize)
		{
			Array.Resize<float>(ref v, newSize);
		}

		protected override void ApplyValue(int i)
		{
			Setter(v[i]);
		}

		protected override void ApplyValue(float t, int a, int b)
		{
			float va = v[a];
			float vb = v[b];
			Setter(t * (vb - va) + va);
		}

		protected override void ApplyValue(float t, int a, int b, int c, int d)
		{
			Setter(Utils.CatmullRomSpline(t, v[a], v[b], v[c], v[d]));
		}
	}
	
	[ProtoContract]
	public class Color4Animator : AnimatorHelper<Color4>
	{
		[ProtoMember(1)]
		Color4[] v = new Color4[0];

		protected override Array values { get { return v; } }

		protected override void ResizeValuesArray(int newSize)
		{
			Array.Resize<Color4>(ref v, newSize);
		}

		protected override void ApplyValue(int i)
		{
			Setter(v[i]);
		}

		protected override void ApplyValue(float t, int a, int b)
		{
			Setter(Color4.Lerp(v[a], v[b], t));
		}
	}
}
