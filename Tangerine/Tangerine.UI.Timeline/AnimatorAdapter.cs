using Lime;

namespace Tangerine.UI.Timeline
{
	public interface IAnimatorAdapter
	{
		int ComponentCount { get; }
		string GetComponentName(int component);
		float GetComponentValue(IAnimator animator, double time, int component);
	}

	public class NumericAnimatorAdapter : IAnimatorAdapter
	{
		public int ComponentCount => 1;
		public string GetComponentName(int component) => null;

		public float GetComponentValue(IAnimator animator, double time, int component)
		{	 
			return ((NumericAnimator)animator).CalcValue(time);
		}
	}

	public class Vector2AnimatorAdapter : IAnimatorAdapter
	{
		static string[] names = { "X", "Y" };
		public int ComponentCount => 2;
		public string GetComponentName(int component) => names[component];

		public float GetComponentValue(IAnimator animator, double time, int component)
		{	
			return ((Vector2Animator)animator).CalcValue(time)[component];
		}
	}
}