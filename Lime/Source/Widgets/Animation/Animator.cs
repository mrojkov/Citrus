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

		private int keyIndex;
		private KeyFunction function;
		private int frame1, frame2;
		private T value1, value2, value3, value4;

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

		protected virtual T Interpolate(float t, T a, T b)
		{
			return a;
		}

		protected virtual T Interpolate(float t, T a, T b, T c, T d)
		{
			return Interpolate(t, b, c);
		}

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
			if (Enabled && ReadonlyKeys.Count > 0) {
				Setter(CalcValue(time));
			}
		}

		public T CalcValue(double time)
		{
			int frame = AnimationUtils.SecondsToFrames(time);
			if (frame < frame1 || frame >= frame2) {
				CacheInterpolationParameters(frame);
			}
			if (function == KeyFunction.Steep) {
				return value2;
			}
			var t = (float)(time * AnimationUtils.FramesPerSecond - frame1) / (frame2 - frame1);
			if (function == KeyFunction.Linear) {
				return Interpolate(t, value2, value3);
			} else {
				return Interpolate(t, value1, value2, value3, value4);
			}
		}

		private void CacheInterpolationParameters(int frame)
		{
			int count = ReadonlyKeys.Count;
			var i = keyIndex;
			// find rightmost key on the left from the given frame
			while (i < count - 1 && frame > ReadonlyKeys[i].Frame) {
				i++;
			}
			while (i >= 0 && frame < ReadonlyKeys[i].Frame) {
				i--;
			}
			keyIndex = i;
			if (i < 0) {
				keyIndex = 0;
				frame1 = int.MinValue;
				frame2 = ReadonlyKeys[0].Frame;
				value2 = ReadonlyKeys[0].Value;
				function = KeyFunction.Steep;
			} else if (i == count - 1) {
				frame1 = ReadonlyKeys[i].Frame;
				frame2 = int.MaxValue;
				value2 = ReadonlyKeys[i].Value;
				function = KeyFunction.Steep;
			} else {
				var key1 = ReadonlyKeys[i];
				var key2 = ReadonlyKeys[i + 1];
				frame1 = key1.Frame;
				frame2 = key2.Frame;
				value2 = key1.Value;
				value3 = key2.Value;
				function = IsInterpolable() ? key1.Function : KeyFunction.Steep;
				if (function == KeyFunction.Spline) {
					value1 = ReadonlyKeys[i < 1 ? 0 : i - 1].Value;
					value4 = ReadonlyKeys[i + 1 >= count - 1 ? count - 1 : i + 2].Value;
				} else if (function == KeyFunction.ClosedSpline) {
					value1 = ReadonlyKeys[i < 1 ? count - 2 : i - 1].Value;
					value4 = ReadonlyKeys[i + 1 >= count - 1 ? 1 : i + 2].Value;
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

		protected virtual bool IsInterpolable() => false;
	}

	public class Vector2Animator : Animator<Vector2>
	{
		protected override bool IsInterpolable() => true;

		protected override Vector2 Interpolate(float t, Vector2 a, Vector2 b)
		{
			Vector2 r;
			r.X = a.X + (b.X - a.X) * t;
			r.Y = a.Y + (b.Y - a.Y) * t;
			return r;
		}

		protected override Vector2 Interpolate(float t, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
		{
			return new Vector2(
				Mathf.CatmullRomSpline(t, a.X, b.X, c.X, d.X),
				Mathf.CatmullRomSpline(t, a.Y, b.Y, c.Y, d.Y)
			);
		}
	}

	public class Vector3Animator : Animator<Vector3>
	{
		protected override bool IsInterpolable() => true;

		protected override Vector3 Interpolate(float t, Vector3 a, Vector3 b)
		{
			return Vector3.Lerp(t, a, b);
		}

		protected override Vector3 Interpolate(float t, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
		{
			return Mathf.CatmullRomSpline(t, a, b, c, d);
		}
	}

	public class NumericAnimator : Animator<float>
	{
		protected override bool IsInterpolable() => true;

		protected override float Interpolate(float t, float a, float b)
		{
			return t * (b - a) + a;
		}

		protected override float Interpolate(float t, float a, float b, float c, float d)
		{
			return Mathf.CatmullRomSpline(t, a, b, c, d);
		}
	}

	public class Color4Animator : Animator<Color4>
	{
		protected override bool IsInterpolable() => true;

		protected override Color4 Interpolate(float t, Color4 a, Color4 b)
		{
			return Color4.Lerp(t, a, b);
		}
	}

	public class QuaternionAnimator : Animator<Quaternion>
	{
		protected override bool IsInterpolable() => true;

		protected override Quaternion Interpolate(float t, Quaternion a, Quaternion b)
		{
			return Quaternion.Slerp(a, b, t);
		}
	}

	public class Matrix44Animator : Animator<Matrix44>
	{
		protected override bool IsInterpolable() => true;

		protected override Matrix44 Interpolate(float t, Matrix44 a, Matrix44 b)
		{
			return Matrix44.Lerp(a, b, t);
		}

		protected override Matrix44 Interpolate(float t, Matrix44 a, Matrix44 b, Matrix44 c, Matrix44 d)
		{
			return Matrix44.Lerp(b, c, t);
		}
	}
}
