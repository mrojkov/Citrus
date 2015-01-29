using ProtoBuf;

namespace Lime
{
	public enum KeyFunction
	{
		Linear,
		Steep,
		Spline,
		ClosedSpline
	}

	public interface IKeyframe
	{
		int Frame { get; set; }
		KeyFunction Function { get; set; }
		object Value { get; set; }
	}


	[ProtoContract]
	public class Keyframe<T> : IKeyframe
	{
		[ProtoMember(1)]
		public int Frame { get; set; }

		[ProtoMember(2)]
		public KeyFunction Function { get; set; }

		[ProtoMember(3)]
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
	}
}
