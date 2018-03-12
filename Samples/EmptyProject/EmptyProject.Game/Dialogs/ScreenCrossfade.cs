using System;
using System.Collections.Generic;
using Lime;

namespace EmptyProject.Dialogs
{
	public class ScreenCrossfade: IDisposable
	{
		private const float FadeTime = 0.3f;
		private readonly Image image;

		public ScreenCrossfade()
		{
			image = new Image {
				Layer = Layers.AboveAllElse,
				Shader = ShaderId.Silhuette,
				Color = Color4.Black
			};
		}

		public IEnumerator<object> FadeInTask()
		{
			yield return ChangeOpacity(0, 1);
		}

		public IEnumerator<object> FadeOutTask()
		{
			yield return ChangeOpacity(1, 0);
		}

		private IEnumerator<object> ChangeOpacity(float from, float to)
		{
			foreach (var t in Task.LinearMotion(FadeTime, from, to)) {
				image.Opacity = t;
				yield return null;
			}
		}

		public void Attach()
		{
			Attach(The.World);
		}

		public void Attach(Widget widget)
		{
			image.PushToNode(widget);
			image.ExpandToContainer();
		}

		public void CaptureInput()
		{
			image.Input.RestrictScope();
		}

		public void ReleaseInput()
		{
			image.Input.DerestrictScope();
		}

		public void Detach()
		{
			image.Unlink();
		}

		public void Dispose()
		{
			image.Dispose();
			GC.Collect();
		}
	}
}
