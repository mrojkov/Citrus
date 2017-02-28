using System;
using System.Collections.Generic;
using EmptyProject.Application;
using Lime;
using EmptyProject.Debug;
using EmptyProject.Scenes;

namespace EmptyProject.Dialogs
{
	public enum DialogState
	{
		Showing,
		Shown,
		Closing,
		Closed
	}

	public class Dialog<T> : Dialog where T: ParsedWidget, new()
	{
		public Dialog(): base(new T()) { }

		protected new T Scene => (T) base.Scene;
	}

	public class Dialog : IDisposable
	{
		private const string DialogTag = "Dialog";

		protected readonly TaskList Tasks = new TaskList();
		protected virtual string HideAnimationName { get { return "Hide"; } }
		protected virtual string CustomRotationAnimation { get { return null; } }
		protected ParsedWidget Scene { get; }
		protected Widget Root;

		public bool IsClosed { get { return Root.Parent == null; } }
		public bool IsTopDialog { get { return DialogContext.Top == this; } }
		public DialogState State { get; protected set; }

		public Dialog(ParsedWidget scene, string animation = "Show", int layer = Layers.Interface)
		{
			Scene = scene;
			Root = scene.Widget;
			Initialize(animation, layer);
		}

		private void Initialize(string animation, int layer)
		{
			Root.Layer = layer;
			Root.Tag = DialogTag;
			Root.PushToNode(World);
			Root.ExpandToContainer();
			Root.Update(0);
			Root.Updating += Tasks.Update;
			Root.Updating += Update;
			Lime.Application.InvokeOnMainThread(() => {
				Root.Input.RestrictScope();
			});

			DisplayInfo.BeforeOrientationOrResolutionChanged += OnBeforeOrientationOrResolutionChanged;
			DisplayInfo.OrientationOrResolutionChanged += OnOrientationOrResolutionChanged;

			ApplyLocalization();

			Tasks.Add(ShowTask(animation));
		}

		protected virtual void Update(float delta) { }

		private void Show(string animation)
		{
			State = DialogState.Showing;
			Tasks.Add(ShowTask(animation, () => {
				State = DialogState.Shown;
			}));
		}

		private IEnumerator<object> ShowTask(string animation, Action whenDone = null)
		{
			Orientate();
			if (animation != null && Root.TryRunAnimation(animation)) {
				yield return Root;
			}

			whenDone.SafeInvoke();
		}

		private IEnumerator<object> HideTask(string animation, Action whenDone = null)
		{
			if (animation != null && Root.TryRunAnimation(animation)) {
				yield return Root;
			}

			UnlinkAndDispose();
			whenDone.SafeInvoke();
		}

		protected virtual void Closing() { }

		protected virtual void OnBeforeOrientationOrResolutionChanged()
		{
			Root.ExpandToContainer();
			Orientate();
		}

		protected virtual void OnOrientationOrResolutionChanged() { }

		protected virtual void Orientate()
		{
			string animationName = DisplayInfo.IsLandscapeOrientation() ? "@Landscape" : "@Portrait";
			string preferredAnimationNameCustom = CustomRotationAnimation;
			foreach (var node in Root.Descendants) {
				if (preferredAnimationNameCustom == null || !node.TryRunAnimation(preferredAnimationNameCustom)) {
					node.TryRunAnimation(animationName);
				}
			}
		}

		protected void Open<T>() where T : Dialog, new()
		{
			DialogContext.Open<T>();
		}

		protected void Open<T>(T dialog) where T : Dialog, new()
		{
			DialogContext.Open(dialog);
		}

		protected void CrossfadeInto<T>() where T : Dialog, new()
		{
			CrossfadeInto<T>(true, true);
		}

		protected void CrossfadeInto<T>(bool fadeIn, bool fadeOut) where T : Dialog, new()
		{
			Tasks.Add(CrossfadeIntoTask<T>(fadeIn, fadeOut));
		}

		private IEnumerator<object> CrossfadeIntoTask<T>(bool fadeIn, bool fadeOut) where T : Dialog, new()
		{
			var crossfade = new ScreenCrossfade();
			crossfade.Attach();
			crossfade.CaptureInput();
			if (fadeIn)
				yield return crossfade.FadeInTask();
			Open<T>();
			crossfade.ReleaseInput();
			if (fadeOut)
				yield return crossfade.FadeOutTask();
			crossfade.Detach();
			crossfade.Dispose();
		}

		private void ApplyLocalization()
		{
			var animationName = string.IsNullOrEmpty(AssetBundle.CurrentLanguage) ? "@EN" : ("@" + AssetBundle.CurrentLanguage);
			foreach (var node in Root.Descendants) {
				if (!node.TryRunAnimation(animationName)) {
					node.TryRunAnimation("@other");
				}
			}
		}

		private void UnlinkAndDispose()
		{
			DisplayInfo.BeforeOrientationOrResolutionChanged -= OnBeforeOrientationOrResolutionChanged;
			DisplayInfo.OrientationOrResolutionChanged -= OnOrientationOrResolutionChanged;
			Root.UnlinkAndDispose();
		}

		public void Close()
		{
			State = DialogState.Closing;

			Closing();

			DialogContext.ActiveDialogs.Remove(this);
			Tasks.Add(HideTask(HideAnimationName, () => {
				State = DialogState.Closed;
			}));
		}

		public void CloseImmediately()
		{
			State = DialogState.Closing;

			Closing();

			DialogContext.ActiveDialogs.Remove(this);
			UnlinkAndDispose();

			State = DialogState.Closed;
		}

		public IEnumerator<object> WaitForDisappear()
		{
			yield return Task.WaitWhile(() => Root.Parent != null);
		}

		protected virtual bool HandleAndroidBackButton()
		{
			if (!IsTopDialog) {
				return false;
			}

			Close();
			return true;
		}

		public virtual void FillDebugMenuItems(RainbowDash.Menu menu) { }

		public void Dispose()
		{
			CloseImmediately();
		}

		protected Application.Application App => Application.Application.Instance;
		protected WindowWidget World => App.World;
		protected IWindow Window => World.Window;
		protected SoundManager SoundManager => SoundManager.Instance;
		protected AppData AppData => AppData.Instance;
		protected Profile Profile => Profile.Instance;
		protected DialogContext DialogContext => DialogContext.Instance;
		protected Logger Log => Logger.Instance;
	}
}
