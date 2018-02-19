using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class FrameBorderPresenter : CustomPresenter<DistortionMesh>
	{
		private readonly SceneView sv;
		private const string DashTexturePath = "Tangerine.Resources.Icons.SceneView.Dash.png";
		private readonly Texture2D DashTexture;

		public FrameBorderPresenter(SceneView sceneView)
		{
			this.sv = sceneView;
			sceneView.Frame.CompoundPostPresenter.Add(new DelegatePresenter<Widget>(Render));
			DashTexture = new Texture2D();
			DashTexture.LoadImage(new Bitmap(new EmbeddedResource(DashTexturePath, "Tangerine").GetResourceStream()));
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
				Core.UserPreferences.Instance.Get<UserPreferences>().DrawFrameBorder
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
						Renderer.DrawDashedLine(DashTexture, a, b, Color4.Gray, 6f);
					}
				}
			}
		}
	}
}
