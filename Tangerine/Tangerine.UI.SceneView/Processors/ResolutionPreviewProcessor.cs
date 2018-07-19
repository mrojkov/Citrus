using System.Collections.Generic;
using Lime;

namespace Tangerine.UI.SceneView
{
	public class ResolutionPreviewProcessor : Core.ITaskProvider
	{
		private static SceneView SceneView => SceneView.Instance;

		public IEnumerator<object> Task()
		{
			while (true) {
				if (Core.Document.Current.ResolutionPreview.Enabled && SceneView.Input.ConsumeKeyPress(Key.Escape)) {
					ResolutionPreviewHandler.Execute(Core.Document.Current, false);
				}
				yield return null;
			}
		}
	}
}
