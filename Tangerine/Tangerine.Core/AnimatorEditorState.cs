using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Lime;

namespace Tangerine
{
	public class AnimatorEditorState
	{
		public readonly IAnimator Animator;
		public bool CurvesShown { get; set; }
		public bool Selected { get; set; }
		public Uid Uid { get; private set; } = Uid.Generate();

		public AnimatorEditorState(IAnimator animator)
		{
			Animator = animator;
			var propertyType = animator.Owner.GetType().GetProperty(animator.TargetProperty).PropertyType;
			if (propertyType == typeof(Vector2)) {
				Curves.Add(new CurveEditorState("X"));
				Curves.Add(new CurveEditorState("Y"));
			} else if (propertyType == typeof(float)) {
				Curves.Add(new CurveEditorState(null));
			}
		}

		public List<CurveEditorState> Curves = new List<CurveEditorState>();
	}

	public class CurveEditorState
	{
		public readonly string Component;

		public Uid Uid { get; private set; } = Uid.Generate();
		public float RowHeight { get; set; } = 30;
		public bool Selected { get; set; }

		public CurveEditorState(string component)
		{
			Component = component;
		}
	}

	public static class AnimatorExtensions
	{
		public static AnimatorEditorState EditorState(this IAnimator animator)
		{
			if (animator.UserData == null) {
				animator.UserData = new AnimatorEditorState(animator);
			}
			return (AnimatorEditorState)animator.UserData;
		}
	}
}