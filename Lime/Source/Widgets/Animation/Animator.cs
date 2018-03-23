using System;
using System.Collections.Generic;
using Yuzu;

namespace Lime
{
	public interface IAnimator : IDisposable
	{
		IAnimable Owner { get; set; }
		IAnimator Next { get; set; }

		void Bind(IAnimable owner);

		IAnimator Clone();

		bool IsTriggerable { get; set; }

		string TargetProperty { get; set; }

		string AnimationId { get; set; }

		bool Enabled { get; set; }

		int Duration { get; }

		void InvokeTrigger(int frame, double animationTimeCorrection = 0);

		void Apply(double time);

		void ResetCache();

		IKeyframeList ReadonlyKeys { get; }

		IKeyframeList Keys { get; }

		object UserData { get; set; }

		Type GetValueType();
	}

	public interface IKeyframeList : IList<IKeyframe>
	{
		IKeyframe CreateKeyframe();

		void Add(int frame, object value, KeyFunction function = KeyFunction.Linear);
		void AddOrdered(int frame, object value, KeyFunction function = KeyFunction.Linear);
		void AddOrdered(IKeyframe keyframe);

		int Version { get; }
	}

	public class Animator<T> : IAnimator
	{
		public IAnimable Owner { get; set; }
		public IAnimator Next { get; set; }

		private double minTime;
		private double maxTime;
		private KeyFunction function;
		private int keyIndex;
		protected T Value1, Value2, Value3, Value4;

		public bool IsTriggerable { get; set; }
		public bool Enabled { get; set; } = true;

		[YuzuMember]
		public string TargetProperty { get; set; }

		public Type GetValueType() { return typeof(T); }

		[YuzuMember]
		public TypedKeyframeList<T> ReadonlyKeys { get; private set; }

		[YuzuMember]
		public string AnimationId { get; set; }

		public object UserData { get; set; }

		public Animator()
		{
			ReadonlyKeys = new TypedKeyframeList<T>();
			ReadonlyKeys.AddRef();
		}

		public void Dispose()
		{
			ReadonlyKeys.Release();
		}

		public TypedKeyframeList<T> Keys
		{
			get
			{
				if (ReadonlyKeys.RefCount > 1) {
					ReadonlyKeys.Release();
					ReadonlyKeys = ReadonlyKeys.Clone();
					ReadonlyKeys.AddRef();
				}
				return ReadonlyKeys;
			}
		}

		IKeyframeList boxedKeys;
		IKeyframeList IAnimator.Keys
		{
			get
			{
				if (ReadonlyKeys.RefCount > 1) {
					boxedKeys = null;
				}
				if (boxedKeys == null) {
					boxedKeys = new BoxedKeyframeList<T>(Keys);
				}
				return boxedKeys;
			}
		}

		IKeyframeList IAnimator.ReadonlyKeys
		{
			get
			{
				if (boxedKeys == null) {
					boxedKeys = new BoxedKeyframeList<T>(ReadonlyKeys);
				}
				return boxedKeys;
			}
		}

		public IAnimator Clone()
		{
			var clone = (Animator<T>)MemberwiseClone();
			clone.Owner = null;
			clone.Next = null;
			clone.boxedKeys = null;
			boxedKeys = null;
			ReadonlyKeys.AddRef();
			return clone;
		}

		protected delegate void SetterDelegate(T value);

		protected SetterDelegate Setter;

		public void Bind(IAnimable owner)
		{
			Owner = owner;
			var p = AnimationUtils.GetProperty(owner.GetType(), TargetProperty);
			IsTriggerable = p.Triggerable;
			var mi = p.Info.GetSetMethod();
			if (mi == null) {
				throw new Lime.Exception("Property '{0}' (class '{1}') is readonly", TargetProperty, owner.GetType());
			}
			Setter = (SetterDelegate)Delegate.CreateDelegate(typeof(SetterDelegate), owner, mi);
		}

		protected virtual T InterpolateLinear(float t) => Value2;
		protected virtual T InterpolateSplined(float t) => InterpolateLinear(t);

		public void Clear()
		{
			keyIndex = 0;
			Keys.Clear();
		}

		public void InvokeTrigger(int frame, double animationTimeCorrection = 0)
		{
			if (ReadonlyKeys.Count > 0 && Enabled) {
				// This function relies on currentKey value. Therefore Apply(time) must be called before.
				if (ReadonlyKeys[keyIndex].Frame == frame) {
					Owner.OnTrigger(TargetProperty, animationTimeCorrection);
				}
			}
		}

		public void Apply(double time)
		{
			// Apply only if Animator has Keys, because otherwise it sets value to 0.
			if (Enabled && Keys.Count > 0) {
				Setter(CalcValue(time));
			}
		}

		public void ResetCache()
		{
			minTime = maxTime = 0;
		}

		public T CalcValue(double time)
		{
			if (time < minTime || time >= maxTime) {
				CacheInterpolationParameters(time);
			}
			if (function == KeyFunction.Steep) {
				return Value2;
			}
			var t = (float)((time - minTime) / (maxTime - minTime));
			if (function == KeyFunction.Linear) {
				return InterpolateLinear(t);
			} else {
				return InterpolateSplined(t);
			}
		}

		private void CacheInterpolationParameters(double time)
		{
			int count = ReadonlyKeys.Count;
			if (count == 0) {
				Value2 = default(T);
				minTime = -float.MaxValue;
				maxTime = float.MaxValue;
				function = KeyFunction.Steep;
				return;
			}
			var i = keyIndex;
			if (i >= count) {
				i = count - 1;
			}
			int frame = AnimationUtils.SecondsToFrames(time);
			// find rightmost key on the left from the given frame
			while (i < count - 1 && frame > ReadonlyKeys[i].Frame) {
				i++;
			}
			while (i >= 0 && frame < ReadonlyKeys[i].Frame) {
				i--;
			}
			keyIndex = i;
			int minFrame, maxFrame;
			if (i < 0) {
				keyIndex = 0;
				maxFrame = ReadonlyKeys[0].Frame;
				minFrame = int.MinValue;
				Value2 = ReadonlyKeys[0].Value;
				function = KeyFunction.Steep;
			} else if (i == count - 1) {
				minFrame = ReadonlyKeys[i].Frame;
				maxFrame = int.MaxValue;
				Value2 = ReadonlyKeys[i].Value;
				function = KeyFunction.Steep;
			} else {
				var key1 = ReadonlyKeys[i];
				var key2 = ReadonlyKeys[i + 1];
				minFrame = key1.Frame;
				maxFrame = key2.Frame;
				Value2 = key1.Value;
				Value3 = key2.Value;
				function = key1.Function;
				if (function == KeyFunction.Spline) {
					Value1 = ReadonlyKeys[i < 1 ? 0 : i - 1].Value;
					Value4 = ReadonlyKeys[i + 1 >= count - 1 ? count - 1 : i + 2].Value;
				} else if (function == KeyFunction.ClosedSpline) {
					Value1 = ReadonlyKeys[i < 1 ? count - 2 : i - 1].Value;
					Value4 = ReadonlyKeys[i + 1 >= count - 1 ? 1 : i + 2].Value;
				}
			}
			minTime = minFrame * AnimationUtils.SecondsPerFrame;
			maxTime = maxFrame * AnimationUtils.SecondsPerFrame;
		}

		public int Duration => (ReadonlyKeys.Count == 0) ? 0 : ReadonlyKeys[ReadonlyKeys.Count - 1].Frame;
	}

	public class Vector2Animator : Animator<Vector2>
	{
		protected override Vector2 InterpolateLinear(float t)
		{
			Vector2 r;
			r.X = Value2.X + (Value3.X - Value2.X) * t;
			r.Y = Value2.Y + (Value3.Y - Value2.Y) * t;
			return r;
		}

		protected override Vector2 InterpolateSplined(float t)
		{
			return new Vector2(
				Mathf.CatmullRomSpline(t, Value1.X, Value2.X, Value3.X, Value4.X),
				Mathf.CatmullRomSpline(t, Value1.Y, Value2.Y, Value3.Y, Value4.Y)
			);
		}
	}

	public class Vector3Animator : Animator<Vector3>
	{
		protected override Vector3 InterpolateLinear(float t)
		{
			return Vector3.Lerp(t, Value2, Value3);
		}

		protected override Vector3 InterpolateSplined(float t)
		{
			return Mathf.CatmullRomSpline(t, Value1, Value2, Value3, Value4);
		}
	}

	public class NumericAnimator : Animator<float>
	{
		protected override float InterpolateLinear(float t)
		{
			return t * (Value3 - Value2) + Value2;
		}

		protected override float InterpolateSplined(float t)
		{
			return Mathf.CatmullRomSpline(t, Value1, Value2, Value3, Value4);
		}
	}

	public class Color4Animator : Animator<Color4>
	{
		protected override Color4 InterpolateLinear(float t)
		{
			return Color4.Lerp(t, Value2, Value3);
		}
	}

	public class QuaternionAnimator : Animator<Quaternion>
	{
		protected override Quaternion InterpolateLinear(float t)
		{
			return Quaternion.Slerp(Value2, Value3, t);
		}
	}

	public class Matrix44Animator : Animator<Matrix44>
	{
		protected override Matrix44 InterpolateLinear(float t)
		{
			return Matrix44.Lerp(Value2, Value3, t);
		}
	}
}
