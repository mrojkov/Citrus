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
				if (
					SceneView.Instance.InputArea.IsMouseOverThisOrDescendant() && 
					!Docking.WindowDragBehaviour.IsActive && 
					!(Widget.Focused is CommonEditBox))
				{
					var isMouseOverWindow = false;
					if (!Window.Current.Active) {
						var activeWindow = Application.Windows.FirstOrDefault(w => w.Active);
						if (activeWindow != null) {
							// Don't give focus to SceneView if mouse is upon another Tangerine's window.
							var activeWindowRect = CalcWidowRectangleWithTitleAndBorder(activeWindow);
							var mousePosition = ((Window)activeWindow).WorldToWindow(activeWindow.Input.MousePosition);
							isMouseOverWindow = activeWindowRect.Contains(mousePosition);
							if (!isMouseOverWindow)
								Window.Current.Activate();
						}
					}
					if (!isMouseOverWindow) {
						SceneView.Instance.InputArea.SetFocus();
					}
				}
				yield return null;
			}
		}

		private static Rectangle CalcWidowRectangleWithTitleAndBorder(IWindow window)
		{
			var borderWidth = (window.DecoratedSize.X - window.ClientSize.X) * 0.5f;
			var titleBarHeight = window.DecoratedSize.Y - window.ClientSize.Y - borderWidth * 2f;
			return new Rectangle(new Vector2(-borderWidth, -titleBarHeight - borderWidth), window.ClientSize + new Vector2(borderWidth));
		}
	}
}
