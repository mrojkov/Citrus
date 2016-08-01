using System;
using System.Collections.Generic;
using Lime;

namespace EmptyProject.ScreensAndDialogs
{
	public class ScreenCrossfade
	{
		private const float FadeTime = 0.3f;
		private readonly Image image;
		private readonly Frame frame;
		private readonly Action action;

		public ScreenCrossfade(Action action, bool doFadeIn = true, bool doFadeOut = true)
		{
			this.action = action;
			image = new Image() {
				Size = The.World.Size,
				Layer = Layers.AboveAllElse,
				Shader = ShaderId.Silhuette,
				Color = Color4.Black
			};
			if (doFadeIn) {
				image.Opacity = 0;
			}
			frame = new Frame();
			frame.PushToNode(The.World);
			frame.Input.CaptureAll();
			image.PushToNode(frame);
			var tasks = new TaskList { MainTask(doFadeIn, doFadeOut) };
			image.Updating += tasks.Update;
		}

		private IEnumerator<object> MainTask(bool doFadeIn, bool doFadeOut)
		{
			if (doFadeIn) {
				for (float t = 0; t < FadeTime; t += Task.Current.Delta) {
					image.Opacity = t / FadeTime;
					yield return 0;
				}
			}
			frame.Input.ReleaseAll();
			action.SafeInvoke();
			if (doFadeOut) {
				for (float t = 0; t < FadeTime; t += Task.Current.Delta) {
					image.Opacity = 1 - t / FadeTime;
					yield return 0;
				}
			}
			frame.Unlink();
			GC.Collect();
		}
	}

	internal class ScreenCrossfadeScene
	{
		public ScreenCrossfadeScene(string path, Action action)
		{
			var frame = new Frame(path) {
				Layer = Layers.AboveAllElse,
				Size = The.World.Size
			};
			frame.PushToNode(The.World);
			frame.Input.CaptureAll();
			frame.RunAnimation("Show");
			frame.AnimationStopped += () => {
				frame.Input.ReleaseAll();
				action.SafeInvoke();
				frame.RunAnimation("Hide");
				frame.AnimationStopped += () => {
					frame.Unlink();
					GC.Collect();
				};
			};
		}
	}
}
