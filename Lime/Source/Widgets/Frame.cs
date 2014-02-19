using System;
using System.Linq;
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
	public enum ClipMethod
	{
		[ProtoEnum]
		None,
		[ProtoEnum]
		ScissorTest,
		[ProtoEnum]
		StencilBuffer,
	}

	[ProtoContract]
	[TangerineClass]
	public class Frame : Widget, IImageCombinerArg
	{
		public Action Rendered;

		// In dialog mode frame acts like a modal dialog, all controls behind the dialog are frozen.
		// If dialog is being shown or hidden then all controls on dialog are frozen either.
		public bool DialogMode { get; set; }

		public ClipMethod ClipChildren { get; set; }

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
			bool savedMouseVisibility = false;
			if (ClipChildren != ClipMethod.None) {
				savedMouseVisibility = Input.MouseVisible;
				HideMouseOutsideFrameRect();
			}
			if (DialogMode) {
				UpdateForDialogMode(delta);
			} else {
				base.Update(delta);
			}
			if (ClipChildren != ClipMethod.None) {
				Input.MouseVisible = savedMouseVisibility;
			}
		}

		private void HideMouseOutsideFrameRect()
		{
			if (!HitTest(Input.MousePosition, HitTestMethod.BoundingRect)) {
				Input.MouseVisible = false;
			}
		}

		public override void Render()
		{
			if (renderTexture != null) {
				RenderToTexture(renderTexture);
			} else if (ClipChildren == ClipMethod.ScissorTest) {
				RenderWithScissorTest();
			}
			if (Rendered != null) {
				Renderer.Transform1 = LocalToWorldTransform;
				Rendered();
			}
		}

		private void RenderWithScissorTest()
		{
			Renderer.ScissorTestEnabled = true;
			Renderer.ScissorRectangle = CalculateScissorRectangle();
			try {
				var chain = new RenderChain();
				foreach (var node in Nodes) {
					node.AddToRenderChain(chain);
				}
				chain.RenderAndClear();
			} finally {
				Renderer.ScissorTestEnabled = false;
			}
		}

		private WindowRect CalculateScissorRectangle()
		{
			var aabb = CalcAABBInSpaceOf(World.Instance);
			// Get the projected AABB coordinates in the normalized OpenGL space
			Matrix44 proj = Renderer.Projection;
			aabb.A = proj.TransformVector(aabb.A);
			aabb.B = proj.TransformVector(aabb.B);
			// Transform to 0,0 - 1,1 coordinate space
			aabb.Left = (1 + aabb.Left) / 2;
			aabb.Right = (1 + aabb.Right) / 2;
			aabb.Top = (1 + aabb.Top) / 2;
			aabb.Bottom = (1 + aabb.Bottom) / 2;
			// Transform to window coordinates
			var viewport = Renderer.Viewport;
			var result = new WindowRect();
			var min = new Vector2(viewport.X, viewport.Y);
			var max = new Vector2(viewport.X + viewport.Width, viewport.Y + viewport.Height);
			result.X = Mathf.Lerp(aabb.Left, min.X, max.X).Round();
			result.Width = Mathf.Lerp(aabb.Right, min.X, max.X).Round() - result.X;
			result.Y = Mathf.Lerp(aabb.Bottom, min.Y, max.Y).Round();
			result.Height = Mathf.Lerp(aabb.Top, min.Y, max.Y).Round() - result.Y;
			return result;
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (!GloballyVisible) {
				return;
			}
			if (renderTexture != null || ClipChildren == ClipMethod.ScissorTest) {
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
