using System.Collections.Generic;
using Lime;

namespace Tangerine.UI.SceneView
{
	public class PreviewAnimationProcessor : Core.ITaskProvider
	{
		public IEnumerator<object> Task()
		{
			var doc = Core.Document.Current;
			while (true) {
				if (doc.PreviewAnimation) {
					Application.InvalidateWindows();
				}
				yield return null;
			}
		}
	}
}
