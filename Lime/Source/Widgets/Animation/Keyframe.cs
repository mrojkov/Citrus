using System;
using ProtoBuf;
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

	[ProtoContract]
	public class Keyframe<T> : IKeyframe
	{
		[ProtoMember(1)]
		[YuzuMember]
		public int Frame { get; set; }

		[ProtoMember(2)]
		[YuzuMember]
		public KeyFunction Function { get; set; }

		[ProtoMember(3)]
		[YuzuMember]
		public T Value;

		object IKeyframe.Value
		{
			get { return (object)this.Value; }
			set { this.Value = (T)value; }
		}

		public Keyframe() { }

		public Keyframe(int frame, T value, KeyFunction function)
		{
			this.Frame = frame;
			this.Value = value;
			this.Function = function;
		}

		public Keyframe(int frame, T value)
		{
			this.Frame = frame;
			this.Value = value;
		}

		public Keyframe<T> Clone()
		{
			return new Keyframe<T>() {
				Frame = Frame,
				Function = Function,
				Value = Value
			};
		}

		IKeyframe IKeyframe.Clone()
		{
			return Clone();
		}

		public override bool Equals(object obj)
		{
			var other = (IKeyframe)obj;
			return Frame == other.Frame && Function == other.Function && Equals(Value, other.Value);
		}

		public override int GetHashCode()
		{
			return Frame.GetHashCode() ^ Function.GetHashCode() ^ Value.GetHashCode();
		}
	}
}
