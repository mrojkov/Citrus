using Lime;
using System.Collections.Generic;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class SlowMotionProcessor : ITaskProvider
	{
		public IEnumerator<object> Task()
		{
			while (true) {
				if (Document.Current == null) {
					yield return null;
					continue;
				}
				Document.Current.Container.AnimationSpeed = 1;
				if (Window.Current.Input.IsKeyPressed(Key.Tilde) || TimelineUserPreferences.Instance.SlowMotionMode) {
					Document.Current.Container.AnimationSpeed = 0.1f;
				}
				yield return null;
			}
		}
	}
}
