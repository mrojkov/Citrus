using System;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class ParticleEmitterPresenter : SyncCustomPresenter<ParticleEmitter>
	{
		protected override void InternalRender(ParticleEmitter emitter)
		{
			if (Document.Current.PreviewScene) {
				return;
			}
			var sv = SceneView.Instance;
			sv.Frame.PrepareRendererState();
			if (emitter.Shape == EmitterShape.Custom && Document.Current.Container == emitter) {
				emitter.DrawCustomShape(
					ColorTheme.Current.SceneView.EmitterCustomShape,
					ColorTheme.Current.SceneView.EmitterCustomShapeLine,
					emitter.LocalToWorldTransform * sv.CalcTransitionFromSceneSpace(sv.Frame));
			}
		}
	}
}
