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
					!SceneView.Instance.InputArea.IsFocused() &&
					SceneView.Instance.InputArea.IsMouseOverThisOrDescendant() &&
					!Docking.WindowDragBehaviour.IsActive &&
					!(Widget.Focused is CommonEditBox))
				{
					var ww = SceneView.Instance.InputArea.GetRoot() as WindowWidget;
					ww.Window.Activate();
					SceneView.Instance.InputArea.SetFocus();
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
