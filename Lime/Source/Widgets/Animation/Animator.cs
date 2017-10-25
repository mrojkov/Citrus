using System;
using Yuzu;

namespace Lime
{
	public interface IAnimator : IDisposable
	{
		IAnimable Owner { get; }

		void Bind(IAnimable owner);

		IAnimator Clone();

		bool IsTriggerable { get; set; }

		string TargetProperty { get; set; }

		string AnimationId { get; set; }

		bool Enabled { get; set; }

		int Duration { get; }

		void InvokeTrigger(int frame, double animationTimeCorrection = 0);

		void Apply(double time);

		IKeyframeCollection ReadonlyKeys { get; }

		IKeyframeCollection Keys { get; }

		object UserData { get; set; }

		Type GetValueType();
	}

	public class Animator<T> : IAnimator
	{
		public IAnimable Owner { get; private set; }

		private int currentKey = 0;

		public bool IsTriggerable { get; set; }

		public bool Enabled { get; set; } = true;

		[YuzuMember]
		public string TargetProperty { get; set; }

		public Type GetValueType() { return typeof(T); }

		[YuzuMember]
		public KeyframeCollection<T> ReadonlyKeys { get; private set; }

		[YuzuMember]
		public string AnimationId { get; set; }

		public object UserData { get; set; }

		public Animator()
		{
			ReadonlyKeys = new KeyframeCollection<T>();
			ReadonlyKeys.AddRef();
		}

		public void Dispose()
		{
			ReadonlyKeys.Release();
		}

		public KeyframeCollection<T> Keys
		{
			get {
				if (ReadonlyKeys.RefCount > 1) {
					ReadonlyKeys.Release();
					ReadonlyKeys = ReadonlyKeys.Clone();
					ReadonlyKeys.AddRef();
				}
				return ReadonlyKeys;
			}
		}

		IKeyframeCollection proxyKeys;
		IKeyframeCollection IAnimator.Keys {
			get {
				if (ReadonlyKeys.RefCount > 1) {
					proxyKeys = null;
				}
				if (proxyKeys == null) {
					proxyKeys = new KeyframeCollectionProxy<T>(Keys);
				}
				return proxyKeys;
			}
		}

		IKeyframeCollection IAnimator.ReadonlyKeys {
			get {
				if (proxyKeys == null) {
					proxyKeys = new KeyframeCollectionProxy<T>(ReadonlyKeys);
				}
				return proxyKeys;
			}
		}

		public IAnimator Clone()
		{
			var clone = (Animator<T>)MemberwiseClone();
			clone.proxyKeys = null;
			proxyKeys = null;
			ReadonlyKeys.AddRef();
			return clone;
		}

		protected delegate void SetterDelegate(T value);

		protected SetterDelegate Setter;

		public void Bind(IAnimable owner)
		{
			this.Owner = owner;
			var p = AnimationUtils.GetProperty(owner.GetType(), TargetProperty);
			IsTriggerable = p.Triggerable;
			var mi = p.Info.GetSetMethod();
			if (mi == null) {
				throw new Lime.Exception("Property '{0}' (class '{1}') is readonly", TargetProperty, owner.GetType());
			}
			Setter = (SetterDelegate)Delegate.CreateDelegate(typeof(SetterDelegate), owner, mi);
		}

		protected virtual T Interpolate(float t, Keyframe<T> a, Keyframe<T> b)
		{
			return a.Value;
		}

		protected virtual T Interpolate(float t, Keyframe<T> a, Keyframe<T> b, Keyframe<T> c, Keyframe<T> d)
		{
			return Interpolate(t, b, c);
		}

		public void Clear()
		{
			currentKey = 0;
			Keys.Clear();
		}

		public void InvokeTrigger(int frame, double animationTimeCorrection = 0)
		{
			if (ReadonlyKeys.Count > 0 && Enabled) {
				// This function relies on currentKey value. Therefore Apply(time) must be called before.
				if (ReadonlyKeys[currentKey].Frame == frame) {
					Owner.OnTrigger(TargetProperty, animationTimeCorrection);
				}
			}
		}

		public void Apply(double time)
		{
			if (Enabled && ReadonlyKeys.Count > 0) {
				Setter(CalcValue(time));
			}
		}

		public T CalcValue(double time)
		{
			int count = ReadonlyKeys.Count;
			if (currentKey >= count) {
				currentKey = count - 1;
			}
			int frame = AnimationUtils.SecondsToFrames(time);
			// find rightmost key on the left from given frame
			while (currentKey < count - 1 && frame > ReadonlyKeys[currentKey].Frame)
				currentKey++;
			while (currentKey >= 0 && frame < ReadonlyKeys[currentKey].Frame)
				currentKey--;
			if (currentKey < 0) {
				currentKey = 0;
				return ReadonlyKeys[0].Value;
			}
			if (currentKey == count - 1) {
				return ReadonlyKeys[count - 1].Value;
			}
			int i = currentKey;
			var key1 = ReadonlyKeys[i];
			var function = key1.Function;
			if (function == KeyFunction.Steep || !IsInterpolable()) {
				return key1.Value;
			}
			var key2 = ReadonlyKeys[i + 1];
			var t = (float)(time * AnimationUtils.FramesPerSecond - key1.Frame) / (key2.Frame - key1.Frame);
			if (function == KeyFunction.Linear) {
				return Interpolate(t, key1, key2);
			} else if (function == KeyFunction.Spline) {
				var key0 = ReadonlyKeys[i < 1 ? 0 : i - 1];
				var key3 = ReadonlyKeys[i + 1 >= count - 1 ? count - 1 : i + 2];
				return Interpolate(t, key0, key1, key2, key3);
			} else { // KeyFunction.ClosedSpline
				var key0 = ReadonlyKeys[i < 1 ? count - 2 : i - 1];
				var key3 = ReadonlyKeys[i + 1 >= count - 1 ? 1 : i + 2];
				return Interpolate(t, key0, key1, key2, key3);
			}
		}

		public int Duration {
			get {
				if (ReadonlyKeys.Count == 0)
					return 0;
				return ReadonlyKeys[ReadonlyKeys.Count - 1].Frame;
			}
		}

		protected virtual bool IsInterpolable() => false;
	}

	public class Vector2Animator : Animator<Vector2>
	{
		protected override bool IsInterpolable() => true;

		protected override Vector2 Interpolate(float t, Keyframe<Vector2> a, Keyframe<Vector2> b)
		{
			Vector2 r;
			r.X = a.Value.X + (b.Value.X - a.Value.X) * t;
			r.Y = a.Value.Y + (b.Value.Y - a.Value.Y) * t;
			return r;
		}

		protected override Vector2 Interpolate(float t, Keyframe<Vector2> a, Keyframe<Vector2> b, Keyframe<Vector2> c, Keyframe<Vector2> d)
		{
			return Mathf.CatmullRomSpline(t, a.Value, b.Value, c.Value, d.Value);
		}
	}

	public class Vector3Animator : Animator<Vector3>
	{
		protected override bool IsInterpolable() => true;

		protected override Vector3 Interpolate(float t, Keyframe<Vector3> a, Keyframe<Vector3> b)
		{
			return Vector3.Lerp(t, a.Value, b.Value);
		}

		protected override Vector3 Interpolate(float t, Keyframe<Vector3> a, Keyframe<Vector3> b, Keyframe<Vector3> c, Keyframe<Vector3> d)
		{
			return Mathf.CatmullRomSpline(t, a.Value, b.Value, c.Value, d.Value);
		}
	}

	public class NumericAnimator : Animator<float>
	{
		protected override bool IsInterpolable() => true;

		protected override float Interpolate(float t, Keyframe<float> a, Keyframe<float> b)
		{
			return t * (b.Value - a.Value) + a.Value;
		}

		protected override float Interpolate(float t, Keyframe<float> a, Keyframe<float> b, Keyframe<float> c, Keyframe<float> d)
		{
			return Mathf.CatmullRomSpline(t, a.Value, b.Value, c.Value, d.Value);
		}
	}

	public class Color4Animator : Animator<Color4>
	{
		protected override bool IsInterpolable() => true;

		protected override Color4 Interpolate(float t, Keyframe<Color4> a, Keyframe<Color4> b)
		{
			return Color4.Lerp(t, a.Value, b.Value);
		}
	}

	public class QuaternionAnimator : Animator<Quaternion>
	{
		protected override bool IsInterpolable() => true;

		protected override Quaternion Interpolate(float t, Keyframe<Quaternion> a, Keyframe<Quaternion> b)
		{
			return Quaternion.Slerp(a.Value, b.Value, t);
		}
	}

	public class Matrix44Animator : Animator<Matrix44>
	{
		protected override bool IsInterpolable() => true;

		protected override Matrix44 Interpolate(float t, Keyframe<Matrix44> a, Keyframe<Matrix44> b)
		{
			return Matrix44.Lerp(a.Value, b.Value, t);
		}

		protected override Matrix44 Interpolate(float t, Keyframe<Matrix44> a, Keyframe<Matrix44> b, Keyframe<Matrix44> c, Keyframe<Matrix44> d)
		{
			return Matrix44.Lerp(b.Value, c.Value, t);
		}
	}
}
