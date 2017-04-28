using System;
using System.Collections.Generic;
using EmptyProject.Application;
using Lime;
using EmptyProject.Scenes;

namespace EmptyProject.Dialogs
{
	public class Dialog : IDisposable
	{
		private const string DialogTag = "Dialog";

		protected ParsedNode Scene { get; }
		protected Widget Root => (Widget) Scene.Node;
		protected TaskList Tasks { get; } = new TaskList();
		public DialogState State { get; protected set; }

		protected Dialog(ParsedNode scene)
		{
			Scene = scene;
			Root.Tag = DialogTag;
			Root.Updating += Tasks.Update;
			Root.Updating += Update;
			ApplyLocalization();
		}

		public void Attach(Widget widget)
		{
			Root.Layer = Layer;
			Root.PushToNode(widget);
			Root.ExpandToContainer();
			Lime.Application.InvokeOnMainThread(Root.Input.RestrictScope);

			DisplayInfo.BeforeOrientationOrResolutionChanged += OnBeforeOrientationOrResolutionChanged;
			DisplayInfo.OrientationOrResolutionChanged += OnOrientationOrResolutionChanged;

			Show(ShowAnimationName);
			Root.Update(0);
		}

		protected virtual int Layer => Layers.Interface;

		protected virtual string ShowAnimationName => "Show";

		protected virtual string HideAnimationName => "Hide";

		protected virtual string CustomRotationAnimation => null;

		protected virtual void Update(float delta) { }

		private void Show(string animation)
		{
			Tasks.Add(ShowTask(animation));
		}

		private IEnumerator<object> ShowTask(string animation)
		{
			State = DialogState.Showing;
			Orientate();
			if (animation != null && Root.TryRunAnimation(animation)) {
				yield return Root;
			}
			State = DialogState.Shown;
		}

		protected virtual void OnBeforeOrientationOrResolutionChanged()
		{
			Root.ExpandToContainer();
			Orientate();
		}

		protected virtual void OnOrientationOrResolutionChanged() { }

		protected virtual void Orientate()
		{
			var animationName = DisplayInfo.IsLandscapeOrientation() ? "@Landscape" : "@Portrait";
			var preferredAnimationNameCustom = CustomRotationAnimation;
			foreach (var node in Root.Descendants) {
				if (preferredAnimationNameCustom == null || !node.TryRunAnimation(preferredAnimationNameCustom)) {
					node.TryRunAnimation(animationName);
				}
			}
		}

		protected void Open<T>() where T : Dialog, new()
		{
			DialogManager.Open<T>();
		}

		protected void Open<T>(T dialog) where T : Dialog
		{
			DialogManager.Open(dialog);
		}

		protected void CrossfadeInto<T>() where T : Dialog, new()
		{
			CrossfadeInto<T>(true, true);
		}

		protected void CrossfadeInto<T>(bool fadeIn, bool fadeOut) where T : Dialog, new()
		{
			World.Tasks.Add(CrossfadeIntoTask<T>(fadeIn, fadeOut));
		}

		private IEnumerator<object> CrossfadeIntoTask<T>(bool fadeIn, bool fadeOut) where T : Dialog, new()
		{
			var crossfade = new ScreenCrossfade();
			crossfade.Attach();
			crossfade.CaptureInput();
			if (fadeIn)
				yield return crossfade.FadeInTask();
			CloseImmediately();
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
			Root.UnlinkAndDispose();
			DisplayInfo.BeforeOrientationOrResolutionChanged -= OnBeforeOrientationOrResolutionChanged;
			DisplayInfo.OrientationOrResolutionChanged -= OnOrientationOrResolutionChanged;
		}

		public void Close()
		{
			BeginClose();
			Tasks.Add(CloseTask(HideAnimationName));
		}

		public void CloseImmediately()
		{
			BeginClose();
			UnlinkAndDispose();
			State = DialogState.Closed;
		}

		private void BeginClose()
		{
			State = DialogState.Closing;
			Closing();
			DialogManager.Remove(this);
		}

		protected virtual void Closing() { }

		private IEnumerator<object> CloseTask(string animation)
		{
			State = DialogState.Closing;
			if (animation != null && Root.TryRunAnimation(animation))
			{
				yield return Root;
			}
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

		public void Dispose()
		{
			CloseImmediately();
		}

		public bool IsClosed => State == DialogState.Closed;

		public bool IsTopDialog => DialogManager.Top == this;

		public virtual void FillDebugMenuItems(RainbowDash.Menu menu) { }

		protected Application.Application App => The.App;
		protected WindowWidget World => The.World;
		protected IWindow Window => The.Window;
		protected Input Input => Window.Input;
		protected SoundManager SoundManager => The.SoundManager;
		protected AppData AppData => The.AppData;
		protected Profile Profile => The.Profile;
		protected DialogManager DialogManager => The.DialogManager;
		protected Logger Log => The.Log;
	}

	public class Dialog<T> : Dialog where T: ParsedNode, new()
	{
		protected Dialog(): base(new T()) { }

		protected new T Scene => (T) base.Scene;
	}

	public enum DialogState
	{
		Showing,
		Shown,
		Closing,
		Closed
	}
}
