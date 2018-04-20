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
				if (SceneView.Instance.InputArea.IsMouseOverThisOrDescendant() && !Docking.WindowDragBehaviour.IsActive) {
					var isMouseOverWindow = false;
					if (!Window.Current.Active) {
						var hoveredWindow = Application.Windows.OfType<Window>().FirstOrDefault(w => w.Active);
						if (hoveredWindow != null) {
							// Don't set focus on SceneView when another Tangerine's window is hovered
							var mousePositionPoint = hoveredWindow.WorldToWindow(hoveredWindow.Input.MousePosition);
							var borderWidth = (hoveredWindow.DecoratedSize.X - hoveredWindow.ClientSize.X) * 0.5f;
							var titleBarHeight = hoveredWindow.DecoratedSize.Y - hoveredWindow.ClientSize.Y - borderWidth * 2f;
							var windowRectangle = new Rectangle(new Vector2(-borderWidth, -titleBarHeight - borderWidth), hoveredWindow.ClientSize + new Vector2(borderWidth));
							isMouseOverWindow = windowRectangle.Contains(mousePositionPoint);

							if (!isMouseOverWindow) {
								Window.Current.Activate();
							}
						}
					}
					if (!isMouseOverWindow) {
						SceneView.Instance.InputArea.SetFocus();
					}
				}
				yield return null;
			}
		}
	}
}
