using System;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class ParticleEmitterPresenter : CustomPresenter<ParticleEmitter>
	{
		protected override void InternalRender(ParticleEmitter emitter)
		{
			if (Document.Current.PreviewAnimation) {
				return;
			}
			SceneView.Instance.Frame.PrepareRendererState();
			if (emitter.Shape == EmitterShape.Custom && Document.Current.Container == emitter) {
				emitter.DrawCustomShape(
					ColorTheme.Current.SceneView.PointObject.Lighten(0.2f).Transparentify(0.5f),
					ColorTheme.Current.SceneView.SelectedWidget.Lighten(0.2f).Transparentify(0.5f),
					emitter.CalcTransitionToSpaceOf(SceneView.Instance.Frame));
			}
		}
	}
}
