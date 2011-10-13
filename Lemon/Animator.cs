using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Lime;
using ProtoBuf;

namespace Lemon
{
	public enum KeyFunction
	{
		Linear,
		Steep,
		Spline,
		ClosedSpline
	}
	 
	public struct KeyFrame
	{
		public int Frame;
		public KeyFunction Function;
		public Object Value;
	}
		
	public static partial class WidgetUtils
	{
		public const int FramesPerSecond = 16;
		
		static public int MillisecondsToFrame (int timeMs)
		{
			return timeMs >> 6;
		}

		static public int FrameToMilliseconds (int frame)
		{
			return frame << 6;
		}
		
		static public int RoundMillisecondsToFrame (int timeMs)
		{
			return FrameToMilliseconds (MillisecondsToFrame (timeMs));
		}
	}

	[ProtoContract(SkipConstructor = true)]
	[ProtoInclude(101, typeof(GenericAnimator<string>))]
	[ProtoInclude(102, typeof(GenericAnimator<int>))]
	[ProtoInclude(103, typeof(GenericAnimator<bool>))]
	[ProtoInclude(104, typeof(GenericAnimator<Blending>))]
	[ProtoInclude(105, typeof(GenericAnimator<PersistentTexture>))]
	[ProtoInclude(106, typeof(GenericAnimator<NumericRange>))]
	[ProtoInclude(107, typeof(Vector2Animator))]
	[ProtoInclude(108, typeof(Color4Animator))]
	[ProtoInclude(109, typeof(NumericAnimator))]
	[ProtoInclude(110, typeof(GenericAnimator<EmitterShape>))]
	public abstract class Animator
	{
		[Flags]
		public enum KeyFlags
		{
			Immutable = 1,
		};
			
		public static bool DidKeyFrameLeap;

		protected abstract System.Collections.IList Values { get; }
		
		internal Node Owner;
	
		[ProtoMember(1)]
		public string TargetProperty;
		
		[ProtoMember(2)]
		public readonly List<int> Frames = new List<int> ();

		[ProtoMember(3)]
		public readonly List<KeyFunction> Functions = new List<KeyFunction> ();

		protected int current;
		
		public void Add (int frame, object value, KeyFunction function = KeyFunction.Linear)
		{
			Add (new KeyFrame {Frame = frame, Value = value, Function = function});
		}
		
		protected virtual bool IsEvaluable ()
		{
			return true;
		}

		protected abstract void ApplyValue (int i);
		
		protected virtual void ApplyValue (float t, int a, int b)
		{
			ApplyValue (a);
		}

		protected virtual void ApplyValue (float t, int a, int b, int c, int d)
		{
			ApplyValue (t, b, c);
		}
		
		public void Remove (int index)
		{
			Frames.RemoveAt (index);
			Functions.RemoveAt (index);
			Values.RemoveAt (index);
			current = 0;
		}
		
		public void Clear ()
		{
			Frames.Clear ();
			Functions.Clear ();
			Values.Clear ();
			current = 0;
		}	
			
		public void Apply (int time)
		{
			int count = Frames.Count;
			if (count == 0)
				return;
			int frame = WidgetUtils.MillisecondsToFrame (time);
			while (current < count - 1 && frame > Frames [current])
				current++;
			while (current >= 0 && frame < Frames [current])
				current--;
			if (current >= 0 && frame == Frames [current]) {
				DidKeyFrameLeap = true;
			}
			if (current < 0) {
				ApplyValue (0);
				current = 0;
			} else if (current == count - 1) {
 				ApplyValue (count - 1);
			} else {
				ApplyHelper (time);
			}
			DidKeyFrameLeap = false;
		}

		private void ApplyHelper (int time)
		{
			int i = current;
			KeyFunction function = Functions [i];
			if (function == KeyFunction.Steep) {
				ApplyValue (i);
			}
			else {			
				int t0 = WidgetUtils.FrameToMilliseconds (Frames [i]);
				int t1 = WidgetUtils.FrameToMilliseconds (Frames [i + 1]);
				float t = (time - t0) / (float)(t1 - t0);
				switch (function) {
				case KeyFunction.Linear:
					ApplyValue (t, i, i + 1);
					break;
				case KeyFunction.Spline:
					{
						int count = Frames.Count;
						int a = i < 1 ? 0 : i - 2;
						int b = i;
						int c = i + 1;
						int d = c + 1 >= count - 1 ? count - 1 : c + 1;
						ApplyValue (t, a, b, c, d);
					}
					break;
				case KeyFunction.ClosedSpline:
					{
						int count = Frames.Count;
						int a = i < 1 ? count - 1 : i - 1;
						int b = i;
						int c = i + 1;
						int d = c + 1 >= count - 1 ? 0 : c + 1;
						ApplyValue (t, a, b, c, d);
					}
					break;
				}
			}
		}
		
		public int Duration {
			get {
				if (Frames.Count == 0)
					return 0;
				return Frames [Frames.Count - 1];
			}
		}
			
		public void Add (KeyFrame key)
		{
			if (!IsEvaluable ()) {
				key.Function = KeyFunction.Steep;
			}
			Frames.Add (key.Frame);
			Functions.Add (key.Function);
			Values.Add (key.Value);
		}
		
		public KeyFrame this [int index] { 
			get {
				return new KeyFrame { 
					Frame = Frames [index], 
					Function = Functions [index], 
					Value = Values [index] };
			} 
			set {
				if (!IsEvaluable ()) {
					value.Function = KeyFunction.Steep;
				}
				Frames [index] = value.Frame;
				Functions [index] = value.Function;
				Values [index] = value.Value;
			}
		}
	}
	
	[ProtoContract]
	public abstract class AnimatorHelper<T> : Animator
	{
		protected delegate void Setter (T value);
		
		private Setter setter;

		protected Setter SetProperty { 
			get {
				if (setter != null)
					return setter;
				PropertyInfo pi = Owner.GetType ().GetProperty (TargetProperty);
				if (pi == null)
					throw new InternalError ("Property '{0}' doesn't exist for class '{1}'", TargetProperty, Owner.GetType ());

				MethodInfo mi = pi.GetSetMethod ();
				if (mi == null)
					throw new InternalError ("Property '{0}' (class '{1}') is readonly", TargetProperty, Owner.GetType ());

				setter = (Setter)Delegate.CreateDelegate (typeof(Setter), Owner, mi);
				return setter;
			}
		}
	}

	[ProtoContract]
	public class GenericAnimator<T> : AnimatorHelper<T>
	{
		[ProtoMember(1)]
		public readonly List<T> V = new List<T> ();

		protected override bool IsEvaluable ()
		{
			return false;
		}

		protected override System.Collections.IList Values { get { return V; } }

		protected override void ApplyValue (int i)
		{
			SetProperty (V [i]);	
		}
	}
	
	[ProtoContract]
	public class Vector2Animator : AnimatorHelper<Vector2>
	{
		[ProtoMember(1)]
		public readonly List<Vector2> V = new List<Vector2> ();
		
		protected override System.Collections.IList Values { get { return V; } }

		protected override void ApplyValue (int i)
		{
			SetProperty (V [i]);	
		}
		
		protected override void ApplyValue (float t, int a, int b)
		{
			SetProperty (Vector2.Lerp (V [a], V [b], t));	
		}
	}
	
	[ProtoContract]
	public class NumericAnimator : AnimatorHelper<float>
	{
		[ProtoMember(1)]
		public readonly List<float> V = new List<float> ();
		
		protected override System.Collections.IList Values { get { return V; } }

		protected override void ApplyValue (int i)
		{
			SetProperty (V [i]);	
		}

		protected override void ApplyValue (float t, int a, int b)
		{
			float va = V [a];
			float vb = V [b];
			SetProperty (t * (vb - va) + va);
		}
	}
	
	[ProtoContract]
	public class Color4Animator : AnimatorHelper<Color4>
	{
		[ProtoMember(1)]
		public readonly List<Color4> V = new List<Color4> ();
		
		protected override System.Collections.IList Values { get { return V; } }

		protected override void ApplyValue (int i)
		{
			SetProperty (V [i]);	
		}

		protected override void ApplyValue (float t, int a, int b)
		{
			SetProperty (Color4.Lerp (V [a], V [b], t));	
		}
	}
}
