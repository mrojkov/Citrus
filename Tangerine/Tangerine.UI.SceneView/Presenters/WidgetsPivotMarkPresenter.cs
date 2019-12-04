using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class WidgetsPivotMarkPresenter
	{
		private SceneView sceneView;

		public WidgetsPivotMarkPresenter(SceneView sceneView)
		{
			this.sceneView = sceneView;
			sceneView.Frame.CompoundPostPresenter.Add(new SyncDelegatePresenter<Widget>(RenderWidgetsPivotMark));
		}

		private void RenderWidgetsPivotMark(Widget canvas)
		{
			if (Document.Current.PreviewScene) {
				return;
			}
			var widgets = WidgetsWithDisplayedPivot().ToList();
			if (widgets.Count == 0) {
				return;
			}
			canvas.PrepareRendererState();
			var iconSize = new Vector2(16, 16);
			foreach (var widget in widgets) {
				var t = NodeIconPool.GetTexture(widget.GetType());
				var p = sceneView.CalcTransitionFromSceneSpace(sceneView.Frame).TransformVector(widget.GlobalPivotPosition);
				var position = p - iconSize / 2;
				position.X = (float)System.Math.Truncate(position.X);
				position.Y = (float)System.Math.Truncate(position.Y);
				Renderer.DrawSprite(t, Color4.White, position, iconSize, Vector2.Zero, Vector2.One);
			}
		}

		public static IEnumerable<Widget> WidgetsWithDisplayedPivot() =>
			Core.Document.Current.Container.Nodes.Editable().OfType<Widget>()
			.Where(w => VisualHintsRegistry.Instance.DisplayCondition(w));
	}
}
