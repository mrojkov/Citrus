using System;
using System.Collections.Generic;
using Lime;

namespace Orange
{
	public abstract class KeyReducer<T>
	{
		private float tolerance;

		protected KeyReducer(float tolerance)
		{
			this.tolerance = tolerance;
		}

		public IList<Keyframe<T>> Reduce(IList<Keyframe<T>> keys)
		{
			keys = RemoveOverlappedKeys(keys);
			keys = CleanLerps(keys);
			return keys;
		}

		private IList<Keyframe<T>> RemoveOverlappedKeys(IList<Keyframe<T>> keys)
		{
			var outputKeys = new List<Keyframe<T>>();
			foreach (var k in keys) {
				var i = outputKeys.Count;
				if (i > 0 && k.Frame == outputKeys[i - 1].Frame) {
					outputKeys[i - 1] = k;
				} else {
					outputKeys.Add(k);
				}
			}
			return outputKeys;
		}

		private IList<Keyframe<T>> CleanLerps(IList<Keyframe<T>> keys)
		{
			if (keys.Count <= 2) {
				return keys;
			}
			var outputKeys = new List<Keyframe<T>>();
			var totalError = 0f;
			outputKeys.Add(keys[0]);
			for (int i = 1; i < keys.Count - 1; i++) {
				var k1 = outputKeys[outputKeys.Count - 1];
				var k2 = keys[i];
				var k3 = keys[i + 1];
				var t = (float)(k2.Frame - k1.Frame) / (k3.Frame - k1.Frame);
				var error = GetError(k2.Value, Lerp(t, k1.Value, k3.Value));
				totalError += error;
				if (totalError > tolerance) {
					totalError = 0f;
					outputKeys.Add(k2);
				}
			}
			outputKeys.Add(keys[keys.Count - 1]);
			return outputKeys;
		}

		protected abstract T Lerp(float t, T a, T b);
		protected abstract float GetError(T a, T b);
	}

	public class Vector3KeyReducer : KeyReducer<Vector3>
	{
		public static readonly Vector3KeyReducer Default = new Vector3KeyReducer(1e-3f);

		public Vector3KeyReducer(float tolerance) : base(tolerance) { }

		protected override Vector3 Lerp(float t, Vector3 a, Vector3 b)
		{
			return Vector3.Lerp(t, a, b);
		}

		protected override float GetError(Vector3 a, Vector3 b)
		{
			return (a - b).Length;
		}
	}

	public class QuaternionKeyReducer : KeyReducer<Quaternion>
	{
		public static readonly QuaternionKeyReducer Default = new QuaternionKeyReducer(3.8077177e-7f);

		public QuaternionKeyReducer(float tolerance) : base(tolerance) { }

		protected override Quaternion Lerp(float t, Quaternion a, Quaternion b)
		{
			return Quaternion.Slerp(a, b, t);
		}

		protected override float GetError(Quaternion a, Quaternion b)
		{
			return Mathf.Abs(1f - Quaternion.Dot(a, b) / (a.Length() * b.Length()));
		}
	}
}