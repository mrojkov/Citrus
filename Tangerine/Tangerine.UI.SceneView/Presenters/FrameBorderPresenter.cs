using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class FrameBorderPresenter : SyncCustomPresenter<DistortionMesh>
	{
		private readonly SceneView sv;
		private readonly Texture2D DashTexture;

		public FrameBorderPresenter(SceneView sceneView)
		{
			this.sv = sceneView;
			sceneView.Frame.CompoundPostPresenter.Add(new SyncDelegatePresenter<Widget>(Render));
			DashTexture = new Texture2D();
            DashTexture.LoadImage(new Bitmap(new ThemedIconResource("SceneView.Dash", "Tangerine").GetResourceStream()));
			DashTexture.TextureParams = new TextureParams {
				WrapMode = TextureWrapMode.Repeat,
				MinMagFilter = TextureFilter.Nearest
			};
		}

		void Render(Widget canvas)
		{
			canvas.PrepareRendererState();
			if (!Document.Current.PreviewAnimation &&
				!Document.Current.ExpositionMode &&
				SceneUserPreferences.Instance.DrawFrameBorder
			) {
				var frames = Document.Current.Container.Nodes
					.OfType<Frame>()
					.Where(w => w.EditorState().Visibility != NodeVisibility.Hidden)
					.Except(Document.Current.SelectedNodes().OfType<Frame>());
				Quadrangle hull;
				foreach (var frame in frames) {
					hull = frame.CalcHullInSpaceOf(canvas);
					for (var i = 0; i < 4; i++) {
						var a = hull[i];
						var b = hull[(i + 1) % 4];
						Renderer.DrawDashedLine(a, b, Color4.Gray, new Vector2(12, 1));
					}
				}
			}
		}
	}
}
