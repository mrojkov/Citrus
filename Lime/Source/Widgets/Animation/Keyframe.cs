using System;
using System.Collections.Generic;
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

		public override bool Equals(object obj)
		{
			return obj is Keyframe<T> keyframe &&
				   Frame == keyframe.Frame &&
				   Function == keyframe.Function &&
				   EqualityComparer<T>.Default.Equals(Value, keyframe.Value);
		}

		public override int GetHashCode()
		{
			var hashCode = 384731617;
			hashCode = hashCode * -1521134295 + Frame.GetHashCode();
			hashCode = hashCode * -1521134295 + Function.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(Value);
			return hashCode;
		}
	}
}
