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

		public ClipMethod ClipChildren { get; set; }

		public Widget ClipByWidget { get; set; }

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

		protected override Widget GetEffectiveClipperWidget()
		{
			if (ClipChildren != ClipMethod.None) {
				return ClipByWidget ?? this;
			} else {
				return base.GetEffectiveClipperWidget();
			}
		}

		private void SetRenderTarget(RenderTarget value)
		{
			renderTarget = value;
			RenderedToTexture = value != RenderTarget.None;
			renderTexture = CreateRenderTargetTexture(value);
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
			var savedScissorTest = Renderer.ScissorTestEnabled;
			var savedScissorRect = Renderer.ScissorRectangle;
			Renderer.ScissorTestEnabled = true;
			Renderer.ScissorRectangle = CalculateScissorRectangle(ClipByWidget ?? this);
			try {
				var chain = new RenderChain();
				foreach (var node in Nodes) {
					node.AddToRenderChain(chain);
				}
				chain.RenderAndClear();
			} finally {
				Renderer.ScissorTestEnabled = savedScissorTest;
				Renderer.ScissorRectangle = savedScissorRect;
			}
		}

		private WindowRect CalculateScissorRectangle(Widget widget)
		{
			var aabb = widget.CalcAABBInSpaceOf(World.Instance);
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
            AudioSystem.Update();
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
