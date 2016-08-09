using System;
using ProtoBuf;
using Yuzu;

namespace Lime
{
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
	[ProtoInclude(117, typeof(Animator<ShaderId>))]
	[ProtoInclude(118, typeof(Animator<Vector3>))]
	[ProtoInclude(119, typeof(Animator<Quaternion>))]
	[ProtoInclude(120, typeof(Animator<EmissionType>))]
	public interface IAnimator
	{
		IAnimable Owner { get; }

		void Bind(IAnimable owner);

		IAnimator Clone();

		bool IsTriggerable { get; set; }

		string TargetProperty { get; set; }

		string AnimationId { get; set; }

		int Duration { get; }

		void InvokeTrigger(int frame);

		void Apply(int time);

		IKeyframeCollection ReadonlyKeys { get; }

		IKeyframeCollection Keys { get; }

		object UserData { get; set; }
	}

	/// <summary>
	/// Аниматор. Анимирует значение свойства, основываясь на ключевых кадрах
	/// </summary>
	/// <typeparam name="T">Тип анимируемого свойства</typeparam>
	[ProtoContract]
	[ProtoInclude(151, typeof(NumericAnimator))]
	[ProtoInclude(152, typeof(Vector2Animator))]
	[ProtoInclude(153, typeof(Color4Animator))]
	[ProtoInclude(154, typeof(QuaternionAnimator))]
	[ProtoInclude(155, typeof(Vector3Animator))]
	[ProtoInclude(156, typeof(Matrix44Animator))]
	public class Animator<T> : IAnimator
	{
		public IAnimable Owner { get; private set; }

		private int currentKey = 0;

		public bool IsTriggerable { get; set; }

		/// <summary>
		/// Название анимируемого свойства
		/// </summary>
		[ProtoMember(1)]
		[YuzuMember]
		public string TargetProperty { get; set; }

		/// <summary>
		/// Коллекция ключей анимации
		/// </summary>
		[ProtoMember(2)]
		[YuzuMember]
		public KeyframeCollection<T> ReadonlyKeys = new KeyframeCollection<T>();

		[ProtoMember(3)]
		[YuzuMember]
		public string AnimationId { get; set; }

		public object UserData { get; set; }

		/// <summary>
		/// Возвращает коллекцию ключей анимации
		/// </summary>
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
				if (ReadonlyKeys.Shared) {
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

		/// <summary>
		/// Создает клон аниматора
		/// </summary>
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

		/// <summary>
		/// Привязывает аниматор к свойству указанного объекта
		/// </summary>
		/// <param name="owner">Объект, которому будет назначен аниматор</param>
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

		protected virtual void InterpolateAndSet(float t, Keyframe<T> a, Keyframe<T> b)
		{
			Setter(a.Value);
		}

		protected virtual void InterpolateAndSet(float t, Keyframe<T> a, Keyframe<T> b, Keyframe<T> c, Keyframe<T> d)
		{
			InterpolateAndSet(t, b, c);
		}

		/// <summary>
		/// Удаляет все ключи анимации
		/// </summary>
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
					Owner.OnTrigger(TargetProperty);
				}
			}
		}

		/// <summary>
		/// Изменяет значение анимируемого свойства объекта
		/// </summary>
		/// <param name="time">Время в миллисекундах</param>
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
			if (function == KeyFunction.Steep || !IsInterpolable()) {
				Setter(key1.Value);
				return;
			}
			var key2 = ReadonlyKeys[i + 1];
			int t0 = AnimationUtils.FramesToMsecs(key1.Frame);
			int t1 = AnimationUtils.FramesToMsecs(key2.Frame);
			float t = (time - t0) / (float)(t1 - t0);
			if (function == KeyFunction.Linear) {
				InterpolateAndSet(t, key1, key2);
			} else if (function == KeyFunction.Spline) {
				int count = ReadonlyKeys.Count;
				var key0 = ReadonlyKeys[i < 1 ? 0 : i - 1];
				var key3 = ReadonlyKeys[i + 1 >= count - 1 ? count - 1 : i + 1];
				InterpolateAndSet(t, key0, key1, key2, key3);
			} else if (function == KeyFunction.ClosedSpline) {
				int count = ReadonlyKeys.Count;
				var key0 = ReadonlyKeys[i < 1 ? count - 1 : i - 1];
				var key3 = ReadonlyKeys[i + 1 >= count - 1 ? 0 : i + 1];
				InterpolateAndSet(t, key0, key1, key2, key3);
			}
		}

		/// <summary>
		/// Возвращает номер кадра самого последнего ключа или 0, если ключей нет
		/// </summary>
		public int Duration {
			get {
				if (ReadonlyKeys.Count == 0)
					return 0;
				return ReadonlyKeys[ReadonlyKeys.Count - 1].Frame;
			}
		}

		protected virtual bool IsInterpolable()
		{
			return false;
		}
	}

	[ProtoContract]
	public class Vector2Animator : Animator<Vector2>
	{
		protected override bool IsInterpolable()
		{
			return true;
		}

		protected override void InterpolateAndSet(float t, Keyframe<Vector2> a, Keyframe<Vector2> b)
		{
			Setter(Vector2.Lerp(t, a.Value, b.Value));
		}

		protected override void InterpolateAndSet(float t, Keyframe<Vector2> a, Keyframe<Vector2> b, Keyframe<Vector2> c, Keyframe<Vector2> d)
		{
			Setter(Mathf.CatmullRomSpline(t, a.Value, b.Value, c.Value, d.Value));
		}
	}

	[ProtoContract]
	public class Vector3Animator : Animator<Vector3>
	{
		protected override bool IsInterpolable()
		{
			return true;
		}

		protected override void InterpolateAndSet(float t, Keyframe<Vector3> a, Keyframe<Vector3> b)
		{
			Setter(Vector3.Lerp(t, a.Value, b.Value));
		}

		protected override void InterpolateAndSet(float t, Keyframe<Vector3> a, Keyframe<Vector3> b, Keyframe<Vector3> c, Keyframe<Vector3> d)
		{
			Setter(Mathf.CatmullRomSpline(t, a.Value, b.Value, c.Value, d.Value));
		}
	}

	[ProtoContract]
	public class NumericAnimator : Animator<float>
	{
		protected override bool IsInterpolable()
		{
			return true;
		}

		protected override void InterpolateAndSet(float t, Keyframe<float> a, Keyframe<float> b)
		{
			Setter(t * (b.Value - a.Value) + a.Value);
		}

		protected override void InterpolateAndSet(float t, Keyframe<float> a, Keyframe<float> b, Keyframe<float> c, Keyframe<float> d)
		{
			Setter(Mathf.CatmullRomSpline(t, a.Value, b.Value, c.Value, d.Value));
		}
	}

	[ProtoContract]
	public class Color4Animator : Animator<Color4>
	{
		protected override bool IsInterpolable()
		{
			return true;
		}

		protected override void InterpolateAndSet(float t, Keyframe<Color4> a, Keyframe<Color4> b)
		{
			Setter(Color4.Lerp(t, a.Value, b.Value));
		}
	}

	[ProtoContract]
	public class QuaternionAnimator : Animator<Quaternion>
	{
		protected override bool IsInterpolable()
		{
			return true;
		}

		protected override void InterpolateAndSet(float t, Keyframe<Quaternion> a, Keyframe<Quaternion> b)
		{
			Setter(Quaternion.Slerp(a.Value, b.Value, t));
		}
	}

	[ProtoContract]
	public class Matrix44Animator : Animator<Matrix44>
	{
		protected override bool IsInterpolable()
		{
			return true;
		}

		protected override void InterpolateAndSet(float t, Keyframe<Matrix44> a, Keyframe<Matrix44> b)
		{
			Setter(Matrix44.Lerp(a.Value, b.Value, t));
		}

		protected override void InterpolateAndSet(float t, Keyframe<Matrix44> a, Keyframe<Matrix44> b, Keyframe<Matrix44> c, Keyframe<Matrix44> d)
		{
			Setter(Matrix44.Lerp(b.Value, c.Value, t));
		}
	}
}
