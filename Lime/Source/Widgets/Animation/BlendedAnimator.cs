using System;
using System.Collections.Generic;

namespace Lime
{
	public interface IBlendedAnimator : IAbstractAnimator
	{
		void Add(AnimationTrack track, IAbstractAnimator animator);
	}

	internal class BlendedAnimator<T> : IAbstractAnimator<T>, IBlendedAnimator
	{
		private class Item
		{
			public AnimationTrack Track;
			public IAbstractAnimator<T> Animator;
			public Item Next;
		}

		private Item listHead;

		public IAbstractAnimatorSetter<T> Setter { get; private set; }

		public IAnimable Animable => listHead.Animator.Animable;

		public int TargetPropertyPathComparisonCode => listHead.Animator.TargetPropertyPathComparisonCode;

		public bool IsTriggerable => listHead.Animator.IsTriggerable;

		public int Duration { get; private set; }

		public Type ValueType => typeof(T);

		public void Apply(double time)
		{
			Setter?.SetValue(CalcValue(time));
		}

		public void Add(AnimationTrack track, IAbstractAnimator animator)
		{
			if (listHead == null) {
				Setter = ((IAbstractAnimator<T>)animator).Setter;
			}
			listHead = new Item { Track = track, Animator = (IAbstractAnimator<T>)animator, Next = listHead };
			Duration = Math.Max(Duration, animator.Duration);
		}

		public T CalcValue(double time)
		{
			var totalWeight = 0f;
			var value = default(T);
			for (var item = listHead; item != null; item = item.Next) {
				var trackWeight = item.Track.CalcWeight(time);
				if (trackWeight == 0 && item.Next != null) {
					continue;
				}
				if (totalWeight == 0) {
					totalWeight = trackWeight;
					value = item.Animator.CalcValue(time);
				} else {
					totalWeight += trackWeight;
					Blend(ref value, item.Animator.CalcValue(time), trackWeight / totalWeight);
				}
			}
			return value;
		}

		protected virtual void Blend(ref T value1, T value2, float blendFactor)
		{
			if (blendFactor >= 0.5f) {
				value1 = value2;
			}
		}

		public void ExecuteTriggersInRange(double minTime, double maxTime, bool executeTriggerAtMaxTime)
		{
			for (var item = listHead; item != null; item = item.Next) {
				item.Animator.ExecuteTriggersInRange(minTime, maxTime, executeTriggerAtMaxTime);
			}
		}
	}

	internal class Vector2BlendedAnimator : BlendedAnimator<Vector2>
	{
		protected override void Blend(ref Vector2 value1, Vector2 value2, float blendFactor)
		{
			value1 = Vector2.Lerp(blendFactor, value1, value2);
		}
	}

	internal class Vector3BlendedAnimator : BlendedAnimator<Vector3>
	{
		protected override void Blend(ref Vector3 value1, Vector3 value2, float blendFactor)
		{
			value1 = Vector3.Lerp(blendFactor, value1, value2);
		}
	}

	internal class NumericBlendedAnimator : BlendedAnimator<float>
	{
		protected override void Blend(ref float value1, float value2, float blendFactor)
		{
			value1 = Mathf.Lerp(blendFactor, value1, value2);
		}
	}

	internal class IntBlendedAnimator : BlendedAnimator<int>
	{
		protected override void Blend(ref int value1, int value2, float blendFactor)
		{
			value1 = Mathf.Lerp(blendFactor, value1, value2).Round();
		}
	}

	internal class SByteBlendedAnimator : BlendedAnimator<sbyte>
	{
		protected override void Blend(ref sbyte value1, sbyte value2, float blendFactor)
		{
			value1 = (sbyte)Mathf.Lerp(blendFactor, value1, value2).Round();
		}
	}

	internal class Color4BlendedAnimator : BlendedAnimator<Color4>
	{
		protected override void Blend(ref Color4 value1, Color4 value2, float blendFactor)
		{
			value1 = Color4.Lerp(blendFactor, value1, value2);
		}
	}

	internal class QuaternionBlendedAnimator : BlendedAnimator<Quaternion>
	{
		protected override void Blend(ref Quaternion value1, Quaternion value2, float blendFactor)
		{
			value1 = Quaternion.Slerp(value1, value2, blendFactor);
		}
	}

	internal class Matrix44BlendedAnimator : BlendedAnimator<Matrix44>
	{
		protected override void Blend(ref Matrix44 value1, Matrix44 value2, float blendFactor)
		{
			value1 = Matrix44.Lerp(value1, value2, blendFactor);
		}
	}

	internal class ThicknessBlendedAnimator : BlendedAnimator<Thickness>
	{
		protected override void Blend(ref Thickness value1, Thickness value2, float blendFactor)
		{
			value1 = new Thickness(
				Mathf.Lerp(blendFactor, value1.Left, value2.Left),
				Mathf.Lerp(blendFactor, value1.Right, value2.Right),
				Mathf.Lerp(blendFactor, value1.Top, value2.Top),
				Mathf.Lerp(blendFactor, value1.Bottom, value2.Bottom)
			);
		}
	}

	internal class NumericRangeBlendedAnimator : BlendedAnimator<NumericRange>
	{
		protected override void Blend(ref NumericRange value1, NumericRange value2, float blendFactor)
		{
			value1 = new NumericRange(
				Mathf.Lerp(blendFactor, value1.Median, value2.Median),
				Mathf.Lerp(blendFactor, value1.Dispersion, value2.Dispersion)
			);
		}
	}
}
