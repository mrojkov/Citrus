using System;
using System.Collections;
using System.Collections.Generic;
using Lime;

namespace Orange
{
	internal class KeyframeReducer
	{
		private IInterpolationDetectorProvider detectorProvider;

		public KeyframeReducer(IInterpolationDetectorProvider detectorProvider)
		{
			this.detectorProvider = detectorProvider;
		}

		public IList<Keyframe<T>> Reduce<T>(IList<Keyframe<T>> keys)
		{
			keys = RemoveOverlappedKeys(keys);
			keys = CleanupInterpolations(keys);
			return keys;
		}

		private IList<Keyframe<T>> RemoveOverlappedKeys<T>(IList<Keyframe<T>> keys)
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

		private IList<Keyframe<T>> CleanupInterpolations<T>(IList<Keyframe<T>> keys)
		{
			if (keys.Count <= 2) {
				return keys;
			}
			var outputKeys = new List<Keyframe<T>>();
			var detector = detectorProvider.GetDetector<T>();
			outputKeys.Add(keys[0]);
			for (var i = 2; i < keys.Count; i++) {
				var k1 = keys[i - 2];
				var k2 = keys[i - 1];
				var k3 = keys[i];
				if (!detector.Detect(k1, k2, k3)) {
					outputKeys.Add(k2);
				}
			}
			outputKeys.Add(keys[keys.Count - 1]);
			return outputKeys;
		}
	}


	internal interface IInterpolationDetectorProvider
	{
		InterpolationDetector<T> GetDetector<T>();
	}

	internal class CommonInterpolationDetectorProvider : IInterpolationDetectorProvider
	{
		private Dictionary<Type, object> detectors = new Dictionary<Type, object>();

		public CommonInterpolationDetectorProvider(float tolerance)
		{
			detectors[typeof(Vector3)] = new Vector3InterpolationDetector(new Vector3Comparer(tolerance));
			detectors[typeof(Quaternion)] = new QuaternionInterpolationDetector(new QuaternionComparer(tolerance));
		}

		public InterpolationDetector<T> GetDetector<T>()
		{
			object detector;
			if (!detectors.TryGetValue(typeof(T), out detector)) {
				throw new NotSupportedException();
			}
			return (InterpolationDetector<T>)detector;
		}
	}

	internal abstract class InterpolationDetector<T>
	{
		public IEqualityComparer<T> Comparer { get; private set; }

		public InterpolationDetector(IEqualityComparer<T> comparer)
		{
			Comparer = comparer;
		}

		public bool Detect(Keyframe<T> k1, Keyframe<T> k2, Keyframe<T> k3)
		{
			var t2 = (float)(k2.Frame - k1.Frame) / (k3.Frame - k1.Frame);
			return Comparer.Equals(k2.Value, Interpolate(t2, k1.Value, k3.Value));
		}

		public abstract T Interpolate(float t, T a, T b);
	}

	internal class Vector3InterpolationDetector : InterpolationDetector<Vector3>
	{
		public Vector3InterpolationDetector(IEqualityComparer<Vector3> comparer) : base(comparer) { }

		public override Vector3 Interpolate(float t, Vector3 a, Vector3 b)
		{
			return Vector3.Lerp(t, a, b);
		}
	}

	internal class QuaternionInterpolationDetector : InterpolationDetector<Quaternion>
	{
		public QuaternionInterpolationDetector(IEqualityComparer<Quaternion> comparer) : base(comparer) { }

		public override Quaternion Interpolate(float t, Quaternion a, Quaternion b)
		{
			return Quaternion.Slerp(a, b, t);
		}
	}

	internal class Vector3Comparer : EqualityComparer<Vector3>
	{
		private float tolerance;

		public Vector3Comparer(float tolerance)
		{
			this.tolerance = tolerance;
		}

		public override bool Equals(Vector3 a, Vector3 b)
		{
			return (b - a).SqrLength < tolerance;
		}

		public override int GetHashCode(Vector3 obj)
		{
			throw new NotSupportedException();
		}
	}

	internal class QuaternionComparer : EqualityComparer<Quaternion>
	{
		private float tolerance;

		public QuaternionComparer(float tolerance)
		{
			this.tolerance = tolerance;
		}

		public override bool Equals(Quaternion a, Quaternion b)
		{
			a = Quaternion.Normalize(a);
			b = Quaternion.Normalize(b);
			return 1 - Mathf.Abs(Quaternion.Dot(a, b)) < tolerance;
		}

		public override int GetHashCode(Quaternion obj)
		{
			throw new NotSupportedException();
		}
	}
}