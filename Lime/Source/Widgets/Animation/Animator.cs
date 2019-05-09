using System;
using System.Collections;
using System.Collections.Generic;
using Yuzu;

namespace Lime
{
	public interface IAbstractAnimator
	{
		IAnimable Animable { get; }
		int TargetPropertyPathComparisonCode { get; }
		int Duration { get; }
		bool IsTriggerable { get; }
		Type ValueType { get; }
		void Apply(double time);
		void ExecuteTriggersInRange(double minTime, double maxTime, bool executeTriggerAtMaxTime);
	}

	internal interface IAbstractAnimator<T> : IAbstractAnimator
	{
		T CalcValue(double time);
		IAbstractAnimatorSetter<T> Setter { get; }
	}

	internal interface IAbstractAnimatorSetter<T>
	{
		void SetValue(T value);
	}

	public interface IAnimator : IDisposable, IAbstractAnimator
	{
		IAnimationHost Owner { get; set; }
		IAnimator Clone();
		string TargetPropertyPath { get; set; }
		string AnimationId { get; set; }
		bool Enabled { get; set; }
		IKeyframeList ReadonlyKeys { get; }
		IKeyframeList Keys { get; }
		object UserData { get; set; }
		void Unbind();
		bool IsZombie { get; }
		object CalcValue(double time);
		void ExecuteTrigger(int frame, double animationTimeCorrection);
		void ResetCache();
#if TANGERINE
		int Version { get; }
#endif // TANGERINE
	}

	public interface IKeyframeList : IList<IKeyframe>
	{
		IKeyframe CreateKeyframe();
		IKeyframe GetByFrame(int frame);

		void Add(int frame, object value, KeyFunction function = KeyFunction.Linear);
		void AddOrdered(int frame, object value, KeyFunction function = KeyFunction.Linear);
		void AddOrdered(IKeyframe keyframe);
#if TANGERINE
		int Version { get; }
#endif // TANGERINE
	}

	public class Animator<T> : IAnimator, IAbstractAnimator<T>, IAbstractAnimatorSetter<T>
	{
		public IAnimationHost Owner { get; set; }
		public IAnimable Animable
		{
			get
			{
				if (animable == null && !IsZombie) {
					Bind();
				}
				return animable;
			}
		}
		private IAnimable animable;
		private double minTime;
		private double maxTime;
		private KeyframeParams @params;
		private int keyIndex;
		protected T Value1, Value2, Value3, Value4;
		private bool isTriggerable;
		public bool IsTriggerable
		{
			get
			{
				if (animable == null && !IsZombie) {
					Bind();
				}
				return isTriggerable;
			}
		}
		public bool Enabled { get; set; } = true;
		private delegate void SetterDelegate(T value);
		private delegate void IndexedSetterDelegate(int index, T value);
		private SetterDelegate setter;
		private bool isZombie;
		public bool IsZombie
		{
			get => isZombie;
			private set
			{
				isZombie = value;
#if TANGERINE
				version++;
#endif // TANGERINE
			}
		}

#if TANGERINE
		private int version;
		public int Version { get => version + ReadonlyKeys.Version; }
#endif // TANGERINE

		private string targetPropertyPath;
		[YuzuMember("TargetProperty")]
		public string TargetPropertyPath
		{
			get => targetPropertyPath;
			set
			{
				targetPropertyPath = value;
				TargetPropertyPathComparisonCode = Toolbox.StringUniqueCodeGenerator.Generate(value);
			}
		}

		public int TargetPropertyPathComparisonCode { get; private set; }

		public Type ValueType => typeof(T);

		[YuzuMember]
		public TypedKeyframeList<T> ReadonlyKeys { get; private set; }

		[YuzuMember]
		public string AnimationId { get; set; }

		public object UserData { get; set; }

		IAbstractAnimatorSetter<T> IAbstractAnimator<T>.Setter => this;

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
			clone.setter = null;
#if TANGERINE
			clone.animable = null;
#endif // TANGERINE
			clone.IsZombie = false;
			clone.Owner = null;
			clone.boxedKeys = null;
			boxedKeys = null;
			ReadonlyKeys.AddRef();
			return clone;
		}

		public void Unbind()
		{
			IsZombie = false;
			setter = null;
#if TANGERINE
			animable = null;
#endif // TANGERINE
		}

		public int Duration => (ReadonlyKeys.Count == 0) ? 0 : ReadonlyKeys[ReadonlyKeys.Count - 1].Frame;

		protected virtual T InterpolateLinear(float t) => Value2;
		protected virtual T InterpolateSplined(float t) => InterpolateLinear(t);

		public void Clear()
		{
			keyIndex = 0;
			Keys.Clear();
		}

		public void ExecuteTrigger(int frame, double animationTimeCorrection)
		{
			if (!Enabled || IsZombie || !IsTriggerable) {
				return;
			}
			foreach (var key in ReadonlyKeys) {
				if (key.Frame == frame) {
					Owner?.OnTrigger(TargetPropertyPath, key.Value, animationTimeCorrection: animationTimeCorrection);
					break;
				}
			}
		}

		public void ExecuteTriggersInRange(double minTime, double maxTime, bool executeTriggerAtMaxTime)
		{
			if (!Enabled || IsZombie || !IsTriggerable || Owner == null) {
				return;
			}
			int minFrame = AnimationUtils.SecondsToFramesCeiling(minTime);
			int maxFrame = AnimationUtils.SecondsToFramesCeiling(maxTime) + (executeTriggerAtMaxTime ? 1 : 0);
			if (minFrame >= maxFrame) {
				return;
			}
			foreach (var key in ReadonlyKeys) {
				if (key.Frame >= maxFrame) {
					break;
				} else if (key.Frame >= minFrame) {
					var t = minTime - AnimationUtils.FramesToSeconds(key.Frame);
					Owner.OnTrigger(TargetPropertyPath, key.Value, animationTimeCorrection: t);
				}
			}
		}

		public void SetValue(T value)
		{
			if (Enabled && !IsZombie && ReadonlyKeys.Count > 0) {
				if (setter == null) {
					Bind();
					if (IsZombie) {
						return;
					}
				}
				setter(value);
			}
		}

		public void Apply(double time)
		{
			SetValue(CalcValue(time));
		}

		private void Bind()
		{
			var (p, a, index) = AnimationUtils.GetPropertyByPath(Owner, TargetPropertyPath);
			var mi = p.Info?.GetSetMethod();
			IsZombie = a == null || mi == null || p.Info.PropertyType != typeof(T) || a is IList list && index >= list.Count;
			if (IsZombie) {
				return;
			}
#if TANGERINE
			animable = a;
#endif // TANGERINE
			isTriggerable = p.Triggerable;
			if (index == -1) {
				setter = (SetterDelegate)Delegate.CreateDelegate(typeof(SetterDelegate), a, mi);
			} else {
				var indexedSetter = (IndexedSetterDelegate)Delegate.CreateDelegate(typeof(IndexedSetterDelegate), a, mi);
				setter = (v) => {
					indexedSetter(index, v);
				};
			}
		}

		public void ResetCache()
		{
			minTime = maxTime = 0;
			Owner?.OnAnimatorCollectionChanged();
		}

		public T CalcValue(double time)
		{
			if (time < minTime || time >= maxTime) {
				CacheInterpolationParameters(time);
			}
			if (@params.Function == KeyFunction.Steep) {
				return Value2;
			}
			var t = (float)((time - minTime) / (maxTime - minTime));
			if (@params.EasingFunction != Mathf.EasingFunction.Linear) {
				t = Mathf.Easings.Interpolate(t, @params.EasingFunction, @params.EasingType);
			}
			if (@params.Function == KeyFunction.Linear) {
				return InterpolateLinear(t);
			} else {
				return InterpolateSplined(t);
			}
		}

		object IAnimator.CalcValue(double time) => CalcValue(time);

		private static KeyframeParams defaultKeyframeParams = new KeyframeParams {
			Function = KeyFunction.Steep,
			EasingFunction = Mathf.EasingFunction.Linear
		};

		private void CacheInterpolationParameters(double time)
		{
			int count = ReadonlyKeys.Count;
			if (count == 0) {
				Value2 = default(T);
				minTime = -double.MaxValue;
				maxTime = double.MaxValue;
				@params = defaultKeyframeParams;
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
			if (i < 0) {
				keyIndex = 0;
				maxTime = ReadonlyKeys[0].Frame * AnimationUtils.SecondsPerFrame;
				minTime = double.MinValue;
				Value2 = ReadonlyKeys[0].Value;
				@params = defaultKeyframeParams;
			} else if (i == count - 1) {
				minTime = ReadonlyKeys[i].Frame * AnimationUtils.SecondsPerFrame;
				maxTime = double.MaxValue;
				Value2 = ReadonlyKeys[i].Value;
				@params = defaultKeyframeParams;
			} else {
				var key1 = ReadonlyKeys[i];
				var key2 = ReadonlyKeys[i + 1];
				minTime = key1.Frame * AnimationUtils.SecondsPerFrame;
				maxTime = key2.Frame * AnimationUtils.SecondsPerFrame;
				Value2 = key1.Value;
				Value3 = key2.Value;
				@params = key1.Params;
				if (@params.Function == KeyFunction.Spline) {
					Value1 = ReadonlyKeys[i < 1 ? 0 : i - 1].Value;
					Value4 = ReadonlyKeys[i + 1 >= count - 1 ? count - 1 : i + 2].Value;
				} else if (@params.Function == KeyFunction.ClosedSpline) {
					Value1 = ReadonlyKeys[i < 1 ? count - 2 : i - 1].Value;
					Value4 = ReadonlyKeys[i + 1 >= count - 1 ? 1 : i + 2].Value;
				}
			}
		}
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

	public class IntAnimator : Animator<int>
	{
		protected override int InterpolateLinear(float t)
		{
			return (t * (Value3 - Value2) + Value2).Round();
		}

		protected override int InterpolateSplined(float t)
		{
			return Mathf.CatmullRomSpline(t, Value1, Value2, Value3, Value4).Round();
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
			var a = 1.0f - t;
			var b = Quaternion.Dot(Value2, Value3) > 0.0f ? t : -t;
			Quaternion q;
			q.X = a * Value2.X + b * Value3.X;
			q.Y = a * Value2.Y + b * Value3.Y;
			q.Z = a * Value2.Z + b * Value3.Z;
			q.W = a * Value2.W + b * Value3.W;
			var invl = Mathf.FastInverseSqrt(q.LengthSquared());
			q.X *= invl;
			q.Y *= invl;
			q.Z *= invl;
			q.W *= invl;
			return q;
		}
	}

	public class Matrix44Animator : Animator<Matrix44>
	{
		protected override Matrix44 InterpolateLinear(float t)
		{
			return Matrix44.Lerp(Value2, Value3, t);
		}
	}

	public class ThicknessAnimator : Animator<Thickness>
	{
		protected override Thickness InterpolateLinear(float t)
		{
			Thickness r;
			r.Left = Value2.Left + (Value3.Left - Value2.Left) * t;
			r.Right = Value2.Right + (Value3.Right - Value2.Right) * t;
			r.Top = Value2.Top + (Value3.Top - Value2.Top) * t;
			r.Bottom = Value2.Bottom + (Value3.Bottom - Value2.Bottom) * t;
			return r;
		}

		protected override Thickness InterpolateSplined(float t)
		{
			return new Thickness(
				Mathf.CatmullRomSpline(t, Value1.Left, Value2.Left, Value3.Left, Value4.Left),
				Mathf.CatmullRomSpline(t, Value1.Right, Value2.Right, Value3.Right, Value4.Right),
				Mathf.CatmullRomSpline(t, Value1.Top, Value2.Top, Value3.Top, Value4.Top),
				Mathf.CatmullRomSpline(t, Value1.Bottom, Value2.Bottom, Value3.Bottom, Value4.Bottom)
			);
		}
	}

	public class NumericRangeAnimator : Animator<NumericRange>
	{
		protected override NumericRange InterpolateLinear(float t)
		{
			NumericRange r;
			r.Median = Value2.Median + (Value3.Median - Value2.Median) * t;
			r.Dispersion = Value2.Dispersion + (Value3.Dispersion - Value2.Dispersion) * t;
			return r;
		}

		protected override NumericRange InterpolateSplined(float t)
		{
			return new NumericRange(
				Mathf.CatmullRomSpline(t, Value1.Median, Value2.Median, Value3.Median, Value4.Median),
				Mathf.CatmullRomSpline(t, Value1.Dispersion, Value2.Dispersion, Value3.Dispersion, Value4.Dispersion)
			);
		}
	}
}
