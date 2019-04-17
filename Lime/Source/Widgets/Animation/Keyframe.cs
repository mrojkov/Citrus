using System;
using System.Runtime.InteropServices;
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
		Mathf.EasingFunction EasingFunction { get; set; }
		Mathf.EasingType EasingType { get; set; }
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

	[StructLayout(LayoutKind.Explicit)]
	public struct KeyframeParams
	{
		[FieldOffset(0)]
		public int Packed;

		// Yuzu doesn't allow serialize byte enums, so hack it.
		public KeyFunction Function { get => (KeyFunction)(Packed & 255); set { Packed = (Packed & ~255) | (int)value; } }

		[FieldOffset(1)]
		public Mathf.EasingFunction EasingFunction;

		[FieldOffset(2)]
		public Mathf.EasingType EasingType;
	}

	[YuzuCompact]
	public class Keyframe<T> : IKeyframe
	{
		private KeyframeParams p;
		public KeyframeParams Params { get => p; set => p = value; }

		[YuzuMember]
		public int Frame { get; set; }

		[YuzuMember]
		public int PackedParams {
			get => p.Packed;
			set {
				// Protect us is someone has used previous version of easings
				if ((value & 255) >= (int)KeyFunction.ClosedSpline) {
					value &= 7;
				}
				p.Packed = value;
			}
		}

		[YuzuMember]
		public T Value { get; set; }

		public KeyFunction Function { get => p.Function; set => p.Function = value; }
		public Mathf.EasingFunction EasingFunction { get => p.EasingFunction; set => p.EasingFunction = value; }
		public Mathf.EasingType EasingType { get => p.EasingType; set => p.EasingType = value; }

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
				Params = Params,
				Value = Value
			};
		}

		IKeyframe IKeyframe.Clone() => Clone();
	}
}
