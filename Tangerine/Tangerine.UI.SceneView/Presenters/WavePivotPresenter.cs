using Lime;
using Tangerine.Core;
using System;
using System.Linq;

namespace Tangerine.UI.SceneView
{
	public class WavePivotPresenter : SyncCustomPresenter<Widget>
	{
		public const float Radius = 5f;
		public const float Thickness = 2f;
		public const int SegmentsCount = 32;

		public WavePivotPresenter(SceneView sceneView)
		{
			sceneView.Frame.CompoundPostPresenter.Add(this);
		}

		protected override void InternalRender(Widget node)
		{
			if (Document.Current.ExpositionMode || Document.Current.PreviewScene) {
				return;
			}

			node.PrepareRendererState();
			foreach (var image in Document.Current.SelectedNodes().OfType<Image>()) {
				// Note that there're no reason for Where(...) to filter by component
				var component = image.Components.Get<WaveComponent>();
				if (component == null) {
					continue;
				}

				var transform = image.CalcTransitionToSpaceOf(node);
				var position = transform.TransformVector(component.Pivot * image.Size);
				Renderer.DrawRound(position, Radius + Thickness, SegmentsCount, Color4.Red);
				Renderer.DrawRound(position, Radius, SegmentsCount, Color4.White);
			}
		}
	}
}
