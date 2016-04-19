using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lime;

namespace EmptyProject
{
	class ScreenCrossfade
	{
		const float FadeTime = 0.3f;
		Image image;
		Frame frame;
		TaskList workflow;
		Action action;

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
			workflow = new TaskList();
			workflow.Add(MainTask(doFadeIn, doFadeOut));
			image.Updating += workflow.Update;
		}

		private IEnumerator<object> MainTask(bool doFadeIn, bool doFadeOut)
		{
			if (doFadeIn) {
				for (float t = 0; t < FadeTime; t += TaskList.Current.Delta) {
					image.Opacity = t / FadeTime;
					yield return 0;
				}
			}
			frame.Input.ReleaseAll();
			action.SafeInvoke();
			if (doFadeOut) {
				for (float t = 0; t < FadeTime; t += TaskList.Current.Delta) {
					image.Opacity = 1 - t / FadeTime;
					yield return 0;
				}
			}
			frame.Unlink();
			GC.Collect();
		}
	}

	class ScreenCrossfadeScene
	{
		public ScreenCrossfadeScene(string path, Action action)
		{
			Frame frame = new Frame(path);
			frame.Layer = Layers.AboveAllElse;
			frame.PushToNode(The.World);
			frame.Size = The.World.Size;
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
