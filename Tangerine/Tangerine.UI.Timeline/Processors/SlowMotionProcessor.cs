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
				Document.Current.SlowMotion = false;
				if (Application.Input.IsKeyPressed(Key.Tilde) || TimelineUserPreferences.Instance.SlowMotionMode) {
					Document.Current.SlowMotion = true;
				}
				yield return null;
			}
		}
	}
}
