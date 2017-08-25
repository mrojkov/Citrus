using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class ActivateOnMouseOverProcessor : ITaskProvider
	{
		public IEnumerator<object> Task()
		{
			while (true) {
				if (SceneView.Instance.InputArea.IsMouseOverThisOrDescendant()) {
					if (!Window.Current.Active && Application.Windows.Any(w => w.Active)) {
						Window.Current.Activate();
					}
					SceneView.Instance.InputArea.SetFocus();
				}
				yield return null;
			}
		}
	}
}