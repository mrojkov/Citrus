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

	[YuzuSpecializeWith(typeof(Alignment))]
	[YuzuSpecializeWith(typeof(AudioAction))]
	[YuzuSpecializeWith(typeof(Blending))]
	[YuzuSpecializeWith(typeof(bool))]
	[YuzuSpecializeWith(typeof(Color4))]
	[YuzuSpecializeWith(typeof(EmissionType))]
	[YuzuSpecializeWith(typeof(EmitterShape))]
	[YuzuSpecializeWith(typeof(float))]
	[YuzuSpecializeWith(typeof(HAlignment))]
	[YuzuSpecializeWith(typeof(int))]
	[YuzuSpecializeWith(typeof(ITexture))]
	[YuzuSpecializeWith(typeof(Matrix44))]
	[YuzuSpecializeWith(typeof(MovieAction))]
	[YuzuSpecializeWith(typeof(NumericRange))]
	[YuzuSpecializeWith(typeof(Quaternion))]
	[YuzuSpecializeWith(typeof(SerializableSample))]
	[YuzuSpecializeWith(typeof(ShaderId))]
	[YuzuSpecializeWith(typeof(string))]
	[YuzuSpecializeWith(typeof(Thickness))]
	[YuzuSpecializeWith(typeof(VAlignment))]
	[YuzuSpecializeWith(typeof(Vector2))]
	[YuzuSpecializeWith(typeof(Vector3))]
	[YuzuCompact]
	public class Keyframe<T> : IKeyframe
	{
		[YuzuMember]
		public int Frame { get; set; }

		[YuzuMember]
		public KeyFunction Function { get; set; }

		[YuzuMember]
		// Field, not property makes deserialization faster.
		public T Value;

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
				Value = Value
			};
		}

		IKeyframe IKeyframe.Clone() => Clone();
	}
}
