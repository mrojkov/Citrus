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
	
	public class UpdateEventArgs : EventArgs 
	{
		public int Delta;
	}

	public class KeyEventArgs : EventArgs 
	{
		public bool Consumed;
	}

	[ProtoContract]
	public class Frame : Widget, IImageCombinerArg
	{
		RenderTarget renderTarget;
		ITexture renderTexture;

		public EventHandler<UpdateEventArgs> BeforeUpdate;
		public EventHandler<UpdateEventArgs> AfterUpdate;
		public EventHandler<EventArgs> BeforeLateUpdate;
		public EventHandler<EventArgs> AfterLateUpdate;

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
			if (worldShown && Input.MouseVisible) {
				if (RootFrame.Instance.ActiveWidget != null && !RootFrame.Instance.ActiveWidget.ChildOf(this)) {
					// Discard active widget if it's not a child of the topmost dialog.
					RootFrame.Instance.ActiveWidget = null;
				}
			}
			if (worldShown && RootFrame.Instance.ActiveTextWidget != null && !RootFrame.Instance.ActiveTextWidget.ChildOf(this)) {
				// Discard active text widget if it's not a child of the topmost dialog.
				RootFrame.Instance.ActiveTextWidget = null;
			}
			if (!Playing) {
				base.Update(delta);
			}
			if (worldShown) {
				// Cosume all input events and drive mouse out of the screen.
				Input.ConsumeAllKeyEvents(true);
				Input.MouseVisible = false;
				Input.TextInput = null;
			}
			if (Playing) {
				base.Update(delta);
			}
		}

		void LateUpdateHelper(int delta)
		{
			base.LateUpdate(delta);
		}

		public override void LateUpdate(int delta)
		{
			if (BeforeLateUpdate != null)
				BeforeLateUpdate(this, new UpdateEventArgs {Delta = delta});
			if (AnimationSpeed != 1.0f) {
				delta = MultiplyDeltaByAnimationSpeed(delta);
				while (delta > MaxTimeDelta) {
					base.LateUpdate(MaxTimeDelta);
					delta -= MaxTimeDelta;
				}
			}
			base.LateUpdate(delta);
			if (AfterLateUpdate != null)
				AfterLateUpdate(this, new UpdateEventArgs {Delta = delta});
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
			if (BeforeUpdate != null)
				BeforeUpdate(this, new UpdateEventArgs {Delta = delta});
			if (AnimationSpeed != 1.0f) {
				delta = MultiplyDeltaByAnimationSpeed(delta);
				while (delta > MaxTimeDelta) {
					UpdateHelper(MaxTimeDelta);
					delta -= MaxTimeDelta;
				}
			}
			UpdateHelper(delta);
			if (AfterUpdate != null)
				AfterUpdate(this, new UpdateEventArgs {Delta = delta});
		}

		int MultiplyDeltaByAnimationSpeed(int delta)
		{
			if (AnimationSpeed < 0) {
				throw new Lime.Exception("AnimationSpeed can not be negative");
			}
			delta = (int)(delta * AnimationSpeed);
			return delta;
		}

		public override void Render()
		{
			if (renderTexture != null) {
				RenderToTexture(renderTexture);
			}
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (worldShown) {
				if (renderTexture != null)
					chain.Add(this);
				else
					base.AddToRenderChain(chain);
			}
		}
	}
}
