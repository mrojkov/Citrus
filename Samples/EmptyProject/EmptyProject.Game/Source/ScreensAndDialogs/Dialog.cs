using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lime;

namespace EmptyProject
{
	public class Dialog : IDisposable
	{
		protected TaskList Tasks = new TaskList();
		protected Frame Root;
		protected bool closed;

		public Action OnAfterShow;
		public bool IsClosed { get { return closed; } }

		public Dialog()
		{
		}

#if ANDROID
		private void HandleAndroidBackButtonForTopDialog(Lime.ActivityDelegate.BackButtonEventArgs args)
		{
			if (IsTopDialog()) {
				args.Handled = HandleAndroidBackButton();
			}
		}
#endif

		protected virtual bool HandleAndroidBackButton()
		{
			Close();
			return true;
		}

		public Dialog(Frame windowProto, string animation = "Show", int layer = Layers.Interface)
			: this()
		{
			LoadFromProto(windowProto, layer);
			Show(animation);
		}

		public Dialog(string scenePath, string animation = "Show", int layer = Layers.Interface)
			: this()
		{
			LoadFromFile(scenePath, layer);
			Show(animation);
		}

		protected void LoadFromProto(Frame windowProto, int layer)
		{
			Root = windowProto.DeepCloneFast<Frame>();
			Root.Layer = layer;
			Root.Tag = "Dialog";
			ApplyLocalization();
		}

		protected void LoadFromFile(string scenePath, int layer)
		{
			Root = new Frame(scenePath) {
				Layer = layer,
			};
			Root.Tag = "Dialog";
			ApplyLocalization();
		}

		protected void Show(string animation)
		{
			The.World.Nodes.Insert(0, Root);
			Root.Size = The.World.Size;
			Root.Input.CaptureAll();
			Root.Updating += Tasks.Update;
			if (animation != null && Root.TryRunAnimation(animation)) {
				Root.AnimationStopped += AfterShow;
			} else {
				Tasks.Add(AfterShowHelper());
			}
#if ANDROID
			Lime.ActivityDelegate.Instance.BackPressed += HandleAndroidBackButtonForTopDialog;
#endif
			The.Application.OrientationChanged += OnOrientationChanged;
			OnOrientationChanged();
		}

		public bool IsTopDialog()
		{
			foreach (Node node in The.World.Nodes) {
				Frame frame = node as Frame;
				if (frame != null && frame.Tag == "Dialog" && frame.GloballyVisible) {
					return (frame == Root);
				}
			}
			return false;
		}

		public static void CalcNumberOfDialogs(out int total, out int visible)
		{
			total = 0;
			visible = 0;
			foreach (Node node in The.World.Nodes) {
				Frame frame = node as Frame;
				if (frame != null && frame.Tag == "Dialog") {
					total++;
					if (frame.GloballyVisible)
						visible++;
				}
			}
		}

		protected bool WasCheatsHotSpotClicked()
		{
			return /*Globals.CheatsEnabled && */
				(Root.Input.WasKeyPressed(Key.F1) || (Root.Input.WasMousePressed() && Root.Input.MousePosition.X < 50 && Root.Input.MousePosition.Y < 50));
		}

		protected virtual void OnOrientationChanged()
		{
			string animationName = The.Application.IsPortraitOrientation ? "@Portrait" : "@Landscape";
			string preferredAnimationNameCustom = GetCustomRotationAnimation();
			foreach (Node node in Root.DescendantsOf<Node>()) {
				if (preferredAnimationNameCustom == null || !node.TryRunAnimation(preferredAnimationNameCustom)) {
					node.TryRunAnimation(animationName);
				}
			}
		}

		protected virtual string GetCustomRotationAnimation()
		{
			return null;
		}

		protected void ApplyLocalization()
		{
			string animationName = string.IsNullOrEmpty(AssetsBundle.CurrentLanguage) ? "@EN" : ("@" + AssetsBundle.CurrentLanguage);
			foreach (Node node in Root.DescendantsOf<Node>()) {
				if (!node.TryRunAnimation(animationName)) {
					node.TryRunAnimation("@other");
				}
			}
		}

		protected void Hide(string animation = "Hide")
		{
#if ANDROID
			Lime.ActivityDelegate.Instance.BackPressed -= HandleAndroidBackButtonForTopDialog;
#endif
			closed = true;
			The.Application.OrientationChanged -= OnOrientationChanged;
			if (animation != null && Root.TryRunAnimation(animation)) {
				Root.AnimationStopped += Root.Unlink;
			} else {
				Root.Unlink();
			}
		}

		private IEnumerator<object> AfterShowHelper()
		{
			yield return 0;
			AfterShow();
		}

		protected virtual void AfterShow()
		{
			OnAfterShow.SafeInvoke();
		}

		public virtual void Close()
		{
			Hide();
		}

		public void Dispose()
		{
			Close();
		}

		public IEnumerator<object> WaitForDisappear()
		{
			while (Root.Parent != null) {
				yield return 0;
			}
			yield return 0;
		}
	}
}
