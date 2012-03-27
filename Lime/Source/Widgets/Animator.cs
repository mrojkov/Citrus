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

		public static int MsecsToFrames(int msecs)
		{
			return msecs >> 6;
		}

		public static int FramesToMsecs(int frames)
		{
			return frames << 6;
		}

		protected Node Owner;
		internal bool IsTriggerable;

		internal abstract void Bind(Node owner);

		internal Animator SharedClone()
		{
			return (Animator)MemberwiseClone();
		}

		[ProtoMember(1)]
		public string TargetProperty;

		[ProtoMember(2)]
		public int[] Frames = emptyFrames;

		[ProtoMember(3)]
		public KeyFunction[] Functions = emptyFunctions;

		static int[] emptyFrames = new int[0];
		static KeyFunction[] emptyFunctions = new KeyFunction[0];

		protected abstract Array Values { get; }

		protected abstract void ResizeValuesArray(int newSize);

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

		public void Clear()
		{
			Frames = emptyFrames;
			Functions = emptyFunctions;
			ResizeValuesArray(0);
			currentKey = 0;
		}

		public void InvokeTrigger(int frame)
		{
			if (Frames.Length > 0) {
				// This function relies on currentKey value. Therefore Apply(time) must be called before.
				if (Frames[currentKey] == frame) {
					Owner.OnTrigger(TargetProperty);
				}
			}
		}

		public void Apply(int time)
		{
			int count = Frames.Length;
			if (count == 0)
				return;
			int frame = MsecsToFrames(time);
			while (currentKey < count - 1 && frame > Frames[currentKey])
				currentKey++;
			while (currentKey >= 0 && frame < Frames[currentKey])
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
			KeyFunction function = Functions[i];
			if (function == KeyFunction.Steep) {
				ApplyValue(i);
			} else {
				int t0 = FramesToMsecs(Frames[i]);
				int t1 = FramesToMsecs(Frames[i + 1]);
				float t = (time - t0) / (float)(t1 - t0);
				switch (function) {
				case KeyFunction.Linear:
					ApplyValue(t, i, i + 1);
					break;
				case KeyFunction.Spline:
					{
						int count = Frames.Length;
						int a = i < 1 ? 0 : i - 1;
						int b = i;
						int c = i + 1;
						int d = c + 1 >= count - 1 ? count - 1 : c + 1;
						ApplyValue(t, a, b, c, d);
					}
					break;
				case KeyFunction.ClosedSpline:
					{
						int count = Frames.Length;
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
				if (Frames.Length == 0)
					return 0;
				return Frames[Frames.Length - 1];
			}
		}

		public void Add(KeyFrame key)
		{
			if (!IsEvaluable()) {
				key.Function = KeyFunction.Steep;
			}
			int c = Frames.Length;
			if (c > 0 && Frames[c - 1] >= key.Frame) {
				throw new Lime.Exception("Key frames must be added in ascendant order");
			}
			Array.Resize<int>(ref Frames, c + 1);
			Array.Resize<KeyFunction>(ref Functions, c + 1);
			ResizeValuesArray(c + 1);
			Frames[c] = key.Frame;
			Functions[c] = key.Function;
			Values.SetValue(key.Value, c);
		}

		public KeyFrame this[int index] { 
			get {
				return new KeyFrame { 
					Frame = Frames[index], 
					Function = Functions[index], 
					Value = Values.GetValue(index) };
			} 
			set {
				if (!IsEvaluable()) {
					value.Function = KeyFunction.Steep;
				}
				Frames[index] = value.Frame;
				Functions[index] = value.Function;
				Values.SetValue(value.Value, index);
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
		static T[] emptyValues = new T[0];
		
		[ProtoMember(1)]
		public T[] values = emptyValues;

		protected override bool IsEvaluable()
		{
			return false;
		}

		protected override void ResizeValuesArray(int newSize)
		{
			Array.Resize<T>(ref values, newSize);
		}

		protected override Array Values { get { return values; } }

		protected override void ApplyValue(int i)
		{
			Setter(values[i]);
		}
	}

	[ProtoContract]
	public class Vector2Animator : AnimatorHelper<Vector2>
	{
		static Vector2[] emptyValues = new Vector2[0];
		
		[ProtoMember(1)]
		public Vector2[] values = emptyValues;

		protected override Array Values { get { return values; } }

		protected override void ResizeValuesArray(int newSize)
		{
			Array.Resize<Vector2>(ref values, newSize);
		}

		protected override void ApplyValue(int i)
		{
			Setter(values[i]);
		}
		
		protected override void ApplyValue(float t, int a, int b)
		{
			Setter(Vector2.Lerp(values[a], values[b], t));
		}

		protected override void ApplyValue(float t, int a, int b, int c, int d)
		{
			Setter(MathLib.CatmullRomSpline(t, values[a], values[b], values[c], values[d]));
		}
	}

	[ProtoContract]
	public class NumericAnimator : AnimatorHelper<float>
	{
		static float[] emptyValues = new float[0];
		
		[ProtoMember(1)]
		public float[] values = emptyValues;

		protected override Array Values { get { return values; } }

		protected override void ResizeValuesArray(int newSize)
		{
			Array.Resize<float>(ref values, newSize);
		}

		protected override void ApplyValue(int i)
		{
			Setter(values[i]);
		}

		protected override void ApplyValue(float t, int a, int b)
		{
			float va = values[a];
			float vb = values[b];
			Setter(t * (vb - va) + va);
		}

		protected override void ApplyValue(float t, int a, int b, int c, int d)
		{
			Setter(MathLib.CatmullRomSpline(t, values[a], values[b], values[c], values[d]));
		}
	}
	
	[ProtoContract]
	public class Color4Animator : AnimatorHelper<Color4>
	{
		static Color4[] emptyValues = new Color4[0];
		
		[ProtoMember(1)]
		public Color4[] values = emptyValues;

		protected override Array Values { get { return values; } }

		protected override void ResizeValuesArray(int newSize)
		{
			Array.Resize<Color4>(ref values, newSize);
		}

		protected override void ApplyValue(int i)
		{
			Setter(values[i]);
		}

		protected override void ApplyValue(float t, int a, int b)
		{
			Setter(Color4.Lerp(values[a], values[b], t));
		}
	}
}
