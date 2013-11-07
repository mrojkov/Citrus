using System;
using Lime;
using ProtoBuf;
using System.IO;
using System.Collections.Generic;

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
	
	public class KeyEventArgs : EventArgs 
	{
		public bool Consumed;
	}

	[ProtoContract]
	[TangerineClass]
	public class Frame : Widget, IImageCombinerArg
	{
		public Action Rendered;

		// In dialog mode frame acts like a modal dialog, all controls behind the dialog are frozen.
		// If dialog is being shown or hidden then all controls on dialog are frozen either.
		public bool DialogMode { get; set; }

		RenderTarget renderTarget;
		ITexture renderTexture;

		[ProtoMember(1)]
		public RenderTarget RenderTarget {
			get { return renderTarget; }
			set { SetRenderTarget(value); }
		}

		public Frame() {}

		public Frame(Vector2 position)
		{
			this.Position = position;
		}

		public Frame(string path)
		{
			LoadFromBundle(path);
		}

		public bool IsTopDialog()
		{
			return World.Instance.GetTopDialog() == this;
		}

		private void SetRenderTarget(RenderTarget value)
		{
			renderTarget = value;
			RenderedToTexture = value != RenderTarget.None;
			renderTexture = CreateRenderTargetTexture(value);
		}

		private void UpdateForDialogMode(int delta)
		{
			if (!World.Instance.IsTopDialogUpdated) {
				if (GloballyVisible && Input.MouseVisible) {
					if (World.Instance.ActiveWidget != null && !World.Instance.ActiveWidget.ChildOf(this)) {
						// Discard active widget if it's not a child of the topmost dialog.
						World.Instance.ActiveWidget = null;
					}
				}
				if (GloballyVisible && World.Instance.ActiveTextWidget != null && !World.Instance.ActiveTextWidget.ChildOf(this)) {
					// Discard active text widget if it's not a child of the topmost dialog.
					World.Instance.ActiveTextWidget = null;
				}
			}
			if (!IsRunning) {
				base.Update(delta);
			}
			if (GloballyVisible) {
				// Consume all input events and drive mouse out of the screen.
				Input.ConsumeAllKeyEvents(true);
				Input.MouseVisible = false;
				Input.TextInput = null;
			}
			if (IsRunning) {
				base.Update(delta);
			}
			World.Instance.IsTopDialogUpdated = true;
		}

		public override void Update(int delta)
		{
			if (DialogMode) {
				UpdateForDialogMode(delta);
			} else {
				base.Update(delta);
			}
		}

		public override void Render()
		{
			if (renderTexture != null) {
				RenderToTexture(renderTexture);
			}
			if (Rendered != null) {
				Renderer.Transform1 = LocalToWorldTransform;
				Rendered();
			}
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (!GloballyVisible) {
				return;
			}
			if (renderTexture != null) {
				chain.Add(this);
			} else {
				base.AddToRenderChain(chain);
			}
		}

		[ThreadStatic]
		private static HashSet<string> cyclicDependencyTracker;

		private void LoadFromBundle(string path)
		{
			if (cyclicDependencyTracker == null) {
				cyclicDependencyTracker = new HashSet<string>();
			}
			path = Path.ChangeExtension(path, "scene");
			if (cyclicDependencyTracker.Contains(path))
				throw new Lime.Exception("Cyclic dependency of scenes was detected: {0}", path);
			cyclicDependencyTracker.Add(path);
			try {
				using (Stream stream = AssetsBundle.Instance.OpenFileLocalized(path)) {
					Serialization.ReadObject<Frame>(path, stream, this);
				}
				LoadContent();
				Tag = path;
			} finally {
				cyclicDependencyTracker.Remove(path);
			}
		}

		public void LoadContent()
		{
			if (!string.IsNullOrEmpty(ContentsPath)) {
				LoadContentHelper();
			} else {
				foreach (Node node in Nodes.AsArray) {
					if (node is Frame) {
						(node as Frame).LoadContent();
					}
				}
			}
		}

		private void LoadContentHelper()
		{
			Nodes.Clear();
			Markers.Clear();
			var contentsPath = Path.ChangeExtension(ContentsPath, "scene");
			if (!AssetsBundle.Instance.FileExists(contentsPath)) {
				return;
			}
			var content = new Frame(ContentsPath);
			if (content.AsWidget != null && AsWidget != null) {
				content.Update(0);
				content.AsWidget.Size = AsWidget.Size;
				content.Update(0);
			}
			foreach (Marker marker in content.Markers)
				Markers.Add(marker);
			foreach (Node node in content.Nodes.AsArray) {
				node.Unlink();
				Nodes.Add(node);
			}
		}

		public static Frame CreateSubframe(string path)
		{
			var frame = (Frame)(new Frame(path).Nodes[0]);
			frame.Unlink();
			frame.Tag = path;
			return frame;
		}

		private static ITexture CreateRenderTargetTexture(RenderTarget value)
		{
			switch (value) {
				case RenderTarget.A:
					return new SerializableTexture("#a");
				case RenderTarget.B:
					return new SerializableTexture("#b");
				case RenderTarget.C:
					return new SerializableTexture("#c");
				case RenderTarget.D:
					return new SerializableTexture("#d");
				default:
					return null;
			}
		}

		#region IImageCombinerArg
		
		void IImageCombinerArg.SkipRender() { }

		ITexture IImageCombinerArg.GetTexture()
		{
			return renderTexture;
		}

		#endregion
	}
}
