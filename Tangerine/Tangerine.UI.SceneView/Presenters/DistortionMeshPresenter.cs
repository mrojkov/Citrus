using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class DistortionMeshPresenter : CustomPresenter<DistortionMesh>
	{
		private readonly SceneView sv;
		private readonly VisualHint MeshHint =
			VisualHintsRegistry.Instance.Register("/All/Distortion Mesh Grid", hideRule: VisualHintsRegistry.HideRules.VisibleIfProjectOpened);

		public DistortionMeshPresenter(SceneView sceneView)
		{
			this.sv = sceneView;
			sceneView.Frame.CompoundPostPresenter.Add(new DelegatePresenter<Widget>(Render));
		}

		void Render(Widget canvas)
		{
			if (MeshHint.Enabled && !Document.Current.PreviewAnimation && Document.Current.Container is DistortionMesh) {
				var mesh = Document.Current.Container as DistortionMesh;
				canvas.PrepareRendererState();
				for (int i = 0; i <= mesh.NumRows; i++) {
					for (int j = 0; j <= mesh.NumCols; j++) {
						var p = mesh.GetPoint(i, j).CalcPositionInSpaceOf(canvas);
						if (i + 1 <= mesh.NumRows) {
							Renderer.DrawLine(p, mesh.GetPoint(i + 1, j).CalcPositionInSpaceOf(canvas), ColorTheme.Current.SceneView.DistortionMeshOutline);
						}
						if (j + 1 <= mesh.NumCols) {
							Renderer.DrawLine(p, mesh.GetPoint(i, j + 1).CalcPositionInSpaceOf(canvas), ColorTheme.Current.SceneView.DistortionMeshOutline);
						}
					}
				}
			}
		}
	}
}
