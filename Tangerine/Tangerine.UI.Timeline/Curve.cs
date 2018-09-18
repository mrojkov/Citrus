using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI.Timeline
{
	public class Curve
	{
		public static readonly Dictionary<Type, IAnimatorAdapter> Adapters = new Dictionary<Type, IAnimatorAdapter>();

		public IAnimator Animator { get; private set; }
		public int Component { get; private set; }
		public IAnimatorAdapter Adapter { get; private set; }
		public Color4 Color { get; private set; }
		public readonly List<IKeyframe> SelectedKeys = new List<IKeyframe>();

		public Curve(IAnimator animator, int component, IAnimatorAdapter adapter, Color4 color)
		{
			Animator = animator;
			Component = component;
			Adapter = adapter;
			Color = color;
		}

		static Curve()
		{
			Adapters[typeof(float)] = new NumericAnimatorAdapter();
			Adapters[typeof(Vector2)] = new Vector2AnimatorAdapter();
		}
	}

	public static class AnimatorExtensions
	{
		public static List<Curve> Curves(this IAnimator animator) =>
			(List<Curve>)(animator.UserData ?? (animator.UserData = CreateCurves(animator)));

		static List<Curve> CreateCurves(IAnimator animator)
		{
			var adapter = Curve.Adapters[animator.GetValueType()];
			var curves = new List<Curve>();
			for (int i = 0; i < adapter.ComponentCount; i++) {
				var color = ColorTheme.Current.TimelineCurveEditor.Curves[i];
				var curve = new Curve(animator, i, adapter, color);
				curves.Add(curve);
			}
			return curves;
		}
	}
}