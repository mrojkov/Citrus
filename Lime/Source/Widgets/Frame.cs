using System;
using Lime;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public enum RenderTarget
	{
		[ProtoEnum]
		None,
		[ProtoEnum]
		A,
		[ProtoEnum]
		B,
		[ProtoEnum]
		C,
		[ProtoEnum]
		D,
	}
	
	public delegate void UpdateDelegate(float delta);

	public class KeyEventArgs : EventArgs 
	{
		public bool Consumed;
	}

	[ProtoContract]
	public class Frame : Widget, IImageCombinerArg
	{
		RenderTarget renderTarget;
		ITexture renderTexture;

		public UpdateDelegate BeforeUpdate;
		public UpdateDelegate AfterUpdate;
		public UpdateDelegate BeforeLateUpdate;
		public UpdateDelegate AfterLateUpdate;
		public Event OnRender;

		[ProtoMember(1)]
		public RenderTarget RenderTarget {
			get { return renderTarget; }
			set {
				renderTarget = value;
				renderedToTexture = value != RenderTarget.None;
				switch(value) {
				case RenderTarget.A:
					renderTexture = new SerializableTexture("#a");
					break;
				case RenderTarget.B:
					renderTexture = new SerializableTexture("#b");
					break;
				case RenderTarget.C:
					renderTexture = new SerializableTexture("#c");
					break;
				case RenderTarget.D:
					renderTexture = new SerializableTexture("#d");
					break;
				default:
					renderTexture = null;
					break;
				}
			}
		}

		public float AnimationSpeed = 1.0f;

		// In dialog mode frame acts like a modal dialog, all controls behind the dialog are frozen.
		// If dialog is being shown or hidden then all controls on dialog are frozen either.
		public bool DialogMode;

		void IImageCombinerArg.BypassRendering() {}

		ITexture IImageCombinerArg.GetTexture()
		{
			return renderTexture;
		}

		void UpdateForDialogMode(int delta)
		{
			if (globallyVisible && Input.MouseVisible) {
				if (RootFrame.Instance.ActiveWidget != null && !RootFrame.Instance.ActiveWidget.ChildOf(this)) {
					// Discard active widget if it's not a child of the topmost dialog.
					RootFrame.Instance.ActiveWidget = null;
				}
			}
			if (globallyVisible && RootFrame.Instance.ActiveTextWidget != null && !RootFrame.Instance.ActiveTextWidget.ChildOf(this)) {
				// Discard active text widget if it's not a child of the topmost dialog.
				RootFrame.Instance.ActiveTextWidget = null;
			}
			if (!Playing) {
				base.Update(delta);
			}
			if (globallyVisible) {
				// Cosume all input events and drive mouse out of the screen.
				Input.ConsumeAllKeyEvents(true);
				Input.MouseVisible = false;
				Input.TextInput = null;
			}
			if (Playing) {
				base.Update(delta);
			}
		}

		public override void LateUpdate(int delta)
		{
			if (AnimationSpeed != 1.0f) {
				delta = MultiplyDeltaByAnimationSpeed(delta);
			}
			if (BeforeLateUpdate != null) {
				BeforeLateUpdate(delta * 0.001f);
			}
			while (delta > MaxTimeDelta) {
				base.LateUpdate(MaxTimeDelta);
				delta -= MaxTimeDelta;
			}
			base.LateUpdate(delta);
			if (AfterLateUpdate != null) {
				AfterLateUpdate(delta * 0.001f);
			}
		}

		void UpdateHelper(int delta)
		{
			if (DialogMode) {
				UpdateForDialogMode(delta);
			} else {
				base.Update(delta);
			}
		}

		public override void Update(int delta)
		{
			if (AnimationSpeed != 1.0f) {
				delta = MultiplyDeltaByAnimationSpeed(delta);
			}
			if (BeforeUpdate != null) {
				BeforeUpdate(delta * 0.001f);
			}
			while (delta > MaxTimeDelta) {
				UpdateHelper(MaxTimeDelta);
				delta -= MaxTimeDelta;
			}
			UpdateHelper(delta);
			if (AfterUpdate != null) {
				AfterUpdate(delta * 0.001f);
			}
		}

		int MultiplyDeltaByAnimationSpeed(int delta)
		{
			delta = (int)(delta * AnimationSpeed);
			if (delta < 0) {
				throw new System.ArgumentOutOfRangeException("delta");
			}
			return delta;
		}

		public override void Render()
		{
			if (renderTexture != null) {
				RenderToTexture(renderTexture);
			}
			if (OnRender != null) {
				Renderer.Transform1 = GlobalMatrix;
				OnRender();
			}
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (globallyVisible) {
				if (renderTexture != null)
					chain.Add(this);
				else
					base.AddToRenderChain(chain);
			}
		}

		public new static Frame Create(string path)
		{
			return Node.Create(path) as Frame;
		}

		public static Frame CreateAndPlay(Node parent, string path, string marker)
		{
			Frame frame = Frame.Create(path);
			frame.PlayAnimation(marker);
			if (parent != null) {
				parent.Nodes.Insert(0, frame);
			}
			return frame;
		}
	}
}
