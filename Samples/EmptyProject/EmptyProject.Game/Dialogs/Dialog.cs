using System;
using System.Collections.Generic;
using System.Linq;
using EmptyProject.Application;
using Lime;

namespace EmptyProject.Dialogs
{
	public class Dialog : IDisposable
	{
		private const string DialogTag = "Dialog";

		public static List<Dialog> ActiveDialogs = new List<Dialog>();
		public static Dialog Top { get { return ActiveDialogs.FirstOrDefault(); } }

		protected readonly TaskList Tasks = new TaskList();
		protected virtual string HideAnimationName { get { return "Hide"; } }
		protected virtual string CustomRotationAnimation { get { return null; } }
		protected Widget Root;

		public bool IsClosed { get { return Root.Parent == null; } }
		public bool IsTopDialog { get { return Top == this; } }
		public DialogState State { get; protected set; }

		public Action BeforeHide;
		public Action AfterHide;
		public Action BeforeShow;
		public Action AfterShow;

		public Dialog(Widget windowProto, string animation = "Show", int layer = Layers.Interface)
		{
			Root = windowProto.Clone<Widget>();
			Initialize(animation, layer);
		}

		public Dialog(string scenePath, string animation = "Show", int layer = Layers.Interface)
		{
			Root = new Frame(scenePath);
			Initialize(animation, layer);
		}

		private void Initialize(string animation, int layer)
		{
			ActiveDialogs.Add(this);
			Root.Layer = layer;
			Root.Tag = DialogTag;
			Root.PushToNode(The.World);
			Root.ExpandToContainer();
			Root.Update(0);
			Root.Updating += Tasks.Update;
			Lime.Application.InvokeOnMainThread(() => {
				Root.Input.CaptureAll();
			});
			DisplayInfo.BeforeOrientationOrResolutionChanged += OnBeforeOrientationOrResolutionChanged;
			DisplayInfo.OrientationOrResolutionChanged += OnOrientationOrResolutionChanged;
			ApplyLocalization();
			Tasks.Add(ShowTask(animation));
		}

		private IEnumerator<object> ShowTask(string animation)
		{
			BeforeShow.SafeInvoke();
			Orientate();
			State = DialogState.Showing;
			if (animation != null && Root.TryRunAnimation(animation)) {
				yield return Root;
			}
			State = DialogState.Shown;
			AfterShow.SafeInvoke();
		}

		private IEnumerator<object> HideTask(string animation)
		{
			BeforeHide.SafeInvoke();
			State = DialogState.Closing;
			if (animation != null && Root.TryRunAnimation(animation)) {
				yield return Root;
			}
			State = DialogState.Closed;
			UnlinkAndDispose();
			AfterHide.SafeInvoke();
		}

		protected virtual void OnBeforeOrientationOrResolutionChanged()
		{
			Root.ExpandToContainer();
			Orientate();
		}

		protected virtual void OnOrientationOrResolutionChanged()
		{
		}

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

		private void ApplyLocalization()
		{
			var animationName = string.IsNullOrEmpty(AssetsBundle.CurrentLanguage) ? "@EN" : ("@" + AssetsBundle.CurrentLanguage);
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

		public virtual void Close()
		{
			ActiveDialogs.Remove(this);
			Tasks.Add(HideTask(HideAnimationName));
		}

		public void CloseImmediately()
		{
			ActiveDialogs.Remove(this);
			UnlinkAndDispose();
			State = DialogState.Closed;
		}

		public void Dispose()
		{
			Close();
		}

		public IEnumerator<object> WaitForDisappear()
		{
			yield return Task.WaitWhile(() => Root.Parent != null);
		}

		protected virtual bool HandleAndroidBackButton()
		{
			if (!IsTopDialog)
				return false;
			Close();
			return true;
		}

		public virtual void FillDebugMenuItems(RainbowDash.Menu menu)
		{
		}
	}

	public enum DialogState
	{
		Showing,
		Shown,
		Closing,
		Closed
	}
}
