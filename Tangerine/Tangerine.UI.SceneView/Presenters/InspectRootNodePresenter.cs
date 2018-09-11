using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class InspectRootNodePresenter
	{
		private readonly Texture2D inspectingRootTexture;
		private readonly Vector2 inspectingRootTextureSize;

		public InspectRootNodePresenter(SceneView sceneView)
		{
			sceneView.Frame.CompoundPostPresenter.Add(new SyncDelegatePresenter<Widget>(Render));
			inspectingRootTexture = new Texture2D();
			inspectingRootTexture.LoadImage(new Bitmap(new ThemedIconResource("SceneView.InspectingRoot", "Tangerine").GetResourceStream()));
			inspectingRootTextureSize = (Vector2)inspectingRootTexture.ImageSize;
		}

		private void Render(Widget canvas)
		{
			if (!Document.Current.InspectRootNode || Document.Current.ExpositionMode) {
				return;
			}

			canvas.PrepareRendererState();
			var p = Vector2.Right * (canvas.Width - inspectingRootTextureSize.X);
			if (ProjectUserPreferences.Instance.RulerVisible) {
				p.Y += RulersWidget.RulerHeight;
			}
			Renderer.DrawSprite(inspectingRootTexture, Color4.White, p, inspectingRootTextureSize, Vector2.Zero, Vector2.One);
		}
	}
}
