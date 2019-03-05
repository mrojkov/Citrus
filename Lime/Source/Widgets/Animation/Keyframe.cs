using System;
using Yuzu;

namespace Lime
{
	public enum KeyFunction
	{
		Linear,
		Steep,
		Spline,
		ClosedSpline,
	}

	public interface IKeyframe
	{
		int Frame { get; set; }
		KeyframeParams Params { get; set; }
		KeyFunction Function { get; set; }
		EasingFunction EasingFunction { get; set; }
		EasingType EasingType { get; set; }
		int EasingSoftness { get; set; }
		object Value { get; set; }
		IKeyframe Clone();
	}

	public static class Keyframe
	{
		public static IKeyframe CreateForType(Type type)
		{
			return (IKeyframe)typeof(Keyframe<>).MakeGenericType(type).GetConstructor(new Type[0]).Invoke(new object[0]);
		}

		public static IKeyframe CreateForType(Type type, int frame, object value, KeyFunction function = KeyFunction.Linear)
		{
			var k = CreateForType(type);
			k.Frame = frame;
			k.Value = value;
			k.Function = function;
			return k;
		}
	}

	public struct KeyframeParams
	{
		public int Data;

		public KeyFunction Function
		{
			get => (KeyFunction)(Data & 15);
			set => Data = (Data & ~15) | (int)value;
		}

		public EasingFunction EasingFunction
		{
			get => (EasingFunction)((Data >> 4) & 15);
			set => Data = (Data & ~(15 << 4)) | ((int)value << 4);
		}

		public EasingType EasingType
		{
			get => (EasingType)((Data >> 8) & 3);
			set => Data = (Data & ~(3 << 8)) | ((int)value << 8);
		}

		public int EasingSoftness
		{
			get => (Data >> 10) & 127;
			set
			{
				if (value < 0 || value > 100) {
					throw new ArgumentOutOfRangeException();
				}
				Data = (Data & ~(127 << 10)) | (value << 10);
			}
		}
	}

	[YuzuCompact]
	public class Keyframe<T> : IKeyframe
	{
		private KeyframeParams p;
		public KeyframeParams Params { get => p; set => p = value; }

		[YuzuMember]
		public int Frame { get; set; }

		[YuzuMember]
		public int PackedParams { get => p.Data; set => p.Data = value; }

		[YuzuMember]
		public T Value { get; set; }

		public KeyFunction Function { get => p.Function; set => p.Function = value; }
		public EasingFunction EasingFunction { get => p.EasingFunction; set => p.EasingFunction = value; }
		public EasingType EasingType { get => p.EasingType; set => p.EasingType = value; }
		public int EasingSoftness { get => p.EasingSoftness; set => p.EasingSoftness = value; }

		object IKeyframe.Value
		{
			get { return Value; }
			set { Value = (T)value; }
		}

		public Keyframe() { }

		public Keyframe(int frame, T value, KeyFunction function)
		{
			Frame = frame;
			Value = value;
			Function = function;
		}

		public Keyframe(int frame, T value)
		{
			Frame = frame;
			Value = value;
		}

		public Keyframe<T> Clone()
		{
			return new Keyframe<T> {
				Frame = Frame,
				Function = Function,
				EasingFunction = EasingFunction,
				EasingType = EasingType,
				Value = Value
			};
		}

		IKeyframe IKeyframe.Clone() => Clone();
	}
}
