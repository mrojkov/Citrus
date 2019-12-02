using Lime;

namespace Tangerine.Core
{
	public interface IAnimationPositioner
	{
		void SetAnimationFrame(Animation animation, int frame, bool animationMode);
		bool CacheAnimationsStates { get; set; }
	}
}