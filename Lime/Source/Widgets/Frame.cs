using System;
using System.IO;
using System.Collections.Generic;
using Yuzu;

namespace Lime
{
	public enum RenderTarget
	{
		None,
		A,
		B,
		C,
		D,
		E,
		F,
		G
	}

	public enum ClipMethod
	{
		None,
		ScissorTest,
		StencilBuffer,
		NoRender,
	}

	[TangerineClass(allowChildren: true)]
	public class Frame : Widget, IImageCombinerArg
	{
		public ClipMethod ClipChildren { get; set; }

		public Widget ClipByWidget { get; set; }

		RenderTarget renderTarget;
		ITexture renderTexture;

		[YuzuMember]
		public RenderTarget RenderTarget {
			get { return renderTarget; }
			set { SetRenderTarget(value); }
		}

		public Frame() { }

		public Frame(Vector2 position)
		{
			this.Position = position;
		}

		public Frame(string path) : this()
		{
			LoadFromBundle(path);
		}

		private void SetRenderTarget(RenderTarget value)
		{
			renderTarget = value;
			renderTexture = CreateRenderTargetTexture(value);
			PropagateDirtyFlags();
		}

		internal protected override bool IsRenderedToTexture()
		{
			return base.IsRenderedToTexture() || renderTarget != RenderTarget.None;
		}

		public override void Render()
		{
			if (renderTexture != null) {
				EnsureRenderChain();
				RenderToTexture(renderTexture, renderChain);
			} else if (ClipChildren == ClipMethod.ScissorTest) {
				RenderWithScissorTest();
			}
		}

		private RenderChain renderChain;

		private void RenderWithScissorTest()
		{
			var savedScissorTest = Renderer.ScissorTestEnabled;
			var savedScissorRect = Renderer.ScissorRectangle;
			var rect = CalculateScissorRectangle(ClipByWidget ?? this);
			if (savedScissorTest) {
				if (!IntersectRectangles(rect, savedScissorRect, out rect)) {
					return;
				}
			}
			Renderer.ScissorTestEnabled = true;
			Renderer.ScissorRectangle = rect;
			try {
				EnsureRenderChain();
				foreach (var node in Nodes) {
					node.AddToRenderChain(renderChain);
				}
				renderChain.RenderAndClear();
			} finally {
				Renderer.ScissorTestEnabled = savedScissorTest;
				Renderer.ScissorRectangle = savedScissorRect;
			}
		}

		internal protected override bool PartialHitTest(ref HitTestArgs args)
		{
			if (ClipChildren == ClipMethod.ScissorTest) {
				EnsureRenderChain();
				var savedClipperWidget = args.ClipperWidget;
				try {
					args.ClipperWidget = ClipByWidget ?? this;
					foreach (var node in Nodes) {
						node.AddToRenderChain(renderChain);
					}
					if (renderChain.HitTest(ref args)) {
						return true;
					}
					return base.PartialHitTest(ref args);
				} finally {
					renderChain.Clear();
					args.ClipperWidget = savedClipperWidget;
				}
			} else {
				return base.PartialHitTest(ref args);
			}
		}

		private void EnsureRenderChain()
		{
			if (renderChain == null) {
				renderChain = new RenderChain();
			}
		}

		private bool IntersectRectangles(WindowRect a, WindowRect b, out WindowRect r)
		{
			r = (WindowRect)IntRectangle.Intersect((IntRectangle)a, (IntRectangle)b);
			return r.Width > 0 && r.Height > 0;
		}

		private WindowRect CalculateScissorRectangle(Widget widget)
		{
			var aabb = widget.CalcAABBInViewportSpace();
			return (WindowRect)(IntRectangle)aabb;
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (!GloballyVisible || ClipChildren == ClipMethod.NoRender) {
				return;
			}
			if (renderTexture != null || ClipChildren == ClipMethod.ScissorTest) {
				AddSelfToRenderChain(chain);
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
			path = ResolveScenePath(path);
			if (path == null) {
				return;
			}
			if (cyclicDependencyTracker.Contains(path)) {
				throw new Lime.Exception("Cyclic dependency of scenes was detected: {0}", path);
			}
			cyclicDependencyTracker.Add(path);
			try {
				using (Stream stream = AssetsBundle.Instance.OpenFileLocalized(path)) {
					Serialization.ReadObject<Frame>(path, stream, this);
				}
				LoadContent();
				if (!Application.IsTangerine) {
					Tag = path;
				}
			} finally {
				cyclicDependencyTracker.Remove(path);
			}
		}

		public static Frame CreateSubframe(string path)
		{
			var frame = (Frame)(new Frame(path).Nodes[0]);
			frame.Unlink();
			if (!Application.IsTangerine) {
				frame.Tag = path;
			}
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
				case RenderTarget.E:
					return new SerializableTexture("#e");
				case RenderTarget.F:
					return new SerializableTexture("#f");
				case RenderTarget.G:
					return new SerializableTexture("#g");
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

		Matrix32 IImageCombinerArg.UVTransform { get { return Matrix32.Identity; } }

		#endregion
	}
}
