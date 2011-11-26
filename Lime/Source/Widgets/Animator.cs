using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Lime;
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

	public struct KeyFrame
	{
		public int Frame;
		public KeyFunction Function;
		public Object Value;
	}

	[ProtoContract(SkipConstructor = true)]
	[ProtoInclude(101, typeof(GenericAnimator<string>))]
	[ProtoInclude(102, typeof(GenericAnimator<int>))]
	[ProtoInclude(103, typeof(GenericAnimator<bool>))]
	[ProtoInclude(104, typeof(GenericAnimator<Blending>))]
	[ProtoInclude(105, typeof(GenericAnimator<SerializableTexture>))]
	[ProtoInclude(106, typeof(GenericAnimator<NumericRange>))]
	[ProtoInclude(107, typeof(Vector2Animator))]
	[ProtoInclude(108, typeof(Color4Animator))]
	[ProtoInclude(109, typeof(NumericAnimator))]
	[ProtoInclude(110, typeof(GenericAnimator<EmitterShape>))]
	[ProtoInclude(111, typeof(GenericAnimator<AudioAction>))]
	[ProtoInclude(112, typeof(GenericAnimator<SerializableSound>))]
	public abstract class Animator
	{
		public const int FramesPerSecond = 16;

		static public int MsecsToFrames (int msecs)
		{
			return msecs >> 6;
		}

		static public int FramesToMsecs (int frames)
		{
			return frames << 6;
		}
		
		public abstract System.Collections.IList Values { get; }

		protected Node Owner;
		internal bool IsTriggerable;

		internal abstract void Bind (Node owner);

		[ProtoMember(1)]
		public string TargetProperty;

		[ProtoMember(2)]
		public readonly List<int> Frames = new List<int> ();

		[ProtoMember(3)]
		public readonly List<KeyFunction> Functions = new List<KeyFunction> ();

		protected int currentKey = 0;
		
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
			currentKey = 0;
		}

		public void Clear ()
		{
			Frames.Clear ();
			Functions.Clear ();
			Values.Clear ();
			currentKey = 0;
		}

		public void InvokeTrigger (int intervalBegin, int intervalEnd)
		{
			// This function relies on currentKey value. Therefore Apply (time) must be called before.
			int t = FramesToMsecs (Frames [currentKey]);
			if (t >= intervalBegin && t < intervalEnd) {
				Owner.OnTrigger (TargetProperty);
			}
		}

		public void Apply (int time)
		{
			int count = Frames.Count;
			if (count == 0)
				return;
			int frame = MsecsToFrames (time);
			while (currentKey < count - 1 && frame > Frames [currentKey])
				currentKey++;
			while (currentKey >= 0 && frame < Frames [currentKey])
				currentKey--;
			if (currentKey < 0) {
				ApplyValue (0);
				currentKey = 0;
			} else if (currentKey == count - 1) {
				ApplyValue (count - 1);
			} else {
				ApplyHelper (time);
			}
		}

		private void ApplyHelper (int time)
		{
			int i = currentKey;
			KeyFunction function = Functions [i];
			if (function == KeyFunction.Steep) {
				ApplyValue (i);
			}
			else {
				int t0 = FramesToMsecs (Frames [i]);
				int t1 = FramesToMsecs (Frames [i + 1]);
				float t = (time - t0) / (float)(t1 - t0);
				switch (function) {
				case KeyFunction.Linear:
					ApplyValue (t, i, i + 1);
					break;
				case KeyFunction.Spline:
					{
						int count = Frames.Count;
						int a = i < 1 ? 0 : i - 1;
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
		
		protected struct Property
		{
			public PropertyInfo Info;
			public bool Triggerable;
		}
		
		static Dictionary<string, List<Property>> propertyCache = new Dictionary<string, List<Property>> ();
		
		protected static Property GetProperty (Type ownerType, string propertyName)
		{
			List<Property> plist;
			if (!propertyCache.TryGetValue (propertyName, out plist)) {
				plist = new List<Property> ();
				propertyCache [propertyName] = plist;
			}
			foreach (Property i in plist) {
				if (ownerType.IsSubclassOf (i.Info.DeclaringType))
					return i;
			}
			var p = new Property ();
			p.Info = ownerType.GetProperty (propertyName);
			if (p.Info == null)
				throw new Lime.Exception ("Property '{0}' doesn't exist for class '{1}'", propertyName, ownerType);
			p.Triggerable = p.Info.GetCustomAttributes (typeof (TriggerAttribute), false).Length > 0;
			plist.Add (p);
			return p;
		}
	}
	
	[ProtoContract]
	public abstract class AnimatorHelper<T> : Animator
	{
		protected delegate void SetterDelegate (T value);
		
		protected SetterDelegate Setter;

		internal override void Bind (Node owner)
		{
			Owner = owner;
			Property p = GetProperty (owner.GetType (), TargetProperty);
			IsTriggerable = p.Triggerable;
			var mi = p.Info.GetSetMethod ();
			if (mi == null)
				throw new Lime.Exception ("Property '{0}' (class '{1}') is readonly", TargetProperty, owner.GetType ());
			Setter = (SetterDelegate)Delegate.CreateDelegate (typeof(SetterDelegate), owner, mi);
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

		public override System.Collections.IList Values { get { return V; } }

		protected override void ApplyValue (int i)
		{
			Setter (V [i]);
		}
	}

	[ProtoContract]
	public class Vector2Animator : AnimatorHelper<Vector2>
	{
		[ProtoMember(1)]
		public readonly List<Vector2> V = new List<Vector2> ();
		
		public override System.Collections.IList Values { get { return V; } }

		protected override void ApplyValue (int i)
		{
			Setter (V [i]);
		}
		
		protected override void ApplyValue (float t, int a, int b)
		{
			Setter (Vector2.Lerp (V [a], V [b], t));
		}

		protected override void ApplyValue (float t, int a, int b, int c, int d)
		{
			Setter (Utils.CatmullRomSpline (t, V [a], V [b], V [c], V [d]));
		}
	}

	[ProtoContract]
	public class NumericAnimator : AnimatorHelper<float>
	{
		[ProtoMember(1)]
		public readonly List<float> V = new List<float> ();
		
		public override System.Collections.IList Values { get { return V; } }

		protected override void ApplyValue (int i)
		{
			Setter (V [i]);
		}

		protected override void ApplyValue (float t, int a, int b)
		{
			float va = V [a];
			float vb = V [b];
			Setter (t * (vb - va) + va);
		}

		protected override void ApplyValue (float t, int a, int b, int c, int d)
		{
			Setter (Utils.CatmullRomSpline (t, V [a], V [b], V [c], V [d]));
		}
	}
	
	[ProtoContract]
	public class Color4Animator : AnimatorHelper<Color4>
	{
		[ProtoMember(1)]
		public readonly List<Color4> V = new List<Color4> ();
		
		public override System.Collections.IList Values { get { return V; } }

		protected override void ApplyValue (int i)
		{
			Setter (V [i]);
		}

		protected override void ApplyValue (float t, int a, int b)
		{
			Setter (Color4.Lerp (V [a], V [b], t));
		}
	}
}
