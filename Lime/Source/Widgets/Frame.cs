using System;
using System.Linq;
using Lime;
using ProtoBuf;
using System.IO;
using System.Collections.Generic;

namespace Lime
{
	/// <summary>
	/// Перечень текстур для рендеринга графики в них
	/// </summary>
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
		[ProtoEnum]
		E,
		[ProtoEnum]
		F,
		[ProtoEnum]
		G
	}
	
	/// <summary>
	/// Способы отсечения поверхности, которая не должна отрисовываться
	/// </summary>
	[ProtoContract]
	public enum ClipMethod
	{
		[ProtoEnum]
		None,
		[ProtoEnum]
		ScissorTest,
		[ProtoEnum]
		StencilBuffer,
		[ProtoEnum]
		NoRender,
	}

	/// <summary>
	/// Сцена, загружаемая из бандла
	/// </summary>
	[ProtoContract]
	public class Frame : Widget, IImageCombinerArg
	{
		public event Action Rendered;

		public ClipMethod ClipChildren { get; set; }

		public Widget ClipByWidget { get; set; }

		RenderTarget renderTarget;
		ITexture renderTexture;

		/// <summary>
		/// Текстура, в которую будет отрисовываться фрейм
		/// </summary>
		[ProtoMember(1)]
		public RenderTarget RenderTarget {
			get { return renderTarget; }
			set { SetRenderTarget(value); }
		}

		public Frame() { }

		public Frame(Vector2 position)
		{
			this.Position = position;
		}

		/// <summary>
		/// Загружает фрейм из бандла
		/// </summary>
		/// <param name="path">Путь к файлу сцены в бандле без расширения, относительно папки Data (например Scenes/Maps/MyScene)</param>
		public Frame(string path) : this()
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
			if (Rendered != null) {
				Renderer.Transform1 = LocalToWorldTransform;
				Rendered();
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
				if (renderChain == null) {
					renderChain = new RenderChain();
				}
				foreach (var node in Nodes) {
					node.AddToRenderChain(renderChain);
				}
				renderChain.RenderAndClear();
			} finally {
				Renderer.ScissorTestEnabled = savedScissorTest;
				Renderer.ScissorRectangle = savedScissorRect;
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
