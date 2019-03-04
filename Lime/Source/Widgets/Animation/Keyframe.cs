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
		KeyFunction Function { get; set; }
		EasingFunction EasingFunction { get; set; }
		EasingType EasingType { get; set; }
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

	[YuzuCompact]
	public class Keyframe<T> : IKeyframe
	{
		[YuzuMember]
		public int Frame { get; set; }

		[YuzuMember]
		public int PackedParams
		{
			get => (int)Function | ((int)EasingFunction << 4) | ((int)EasingType << 8);
			set
			{
				Function = (KeyFunction)(value & 15);
				EasingFunction = (EasingFunction)((value >> 4) & 15);
				EasingType = (EasingType)((value >> 8) & 15);
			}
		}

		public KeyFunction Function { get; set; }
		public EasingFunction EasingFunction { get; set; }
		public EasingType EasingType { get; set; }

		[YuzuMember]
		public T Value { get; set; }

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
