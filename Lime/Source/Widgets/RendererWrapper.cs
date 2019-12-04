using System;
using System.Collections.Generic;
using System.Threading;

namespace Lime
{
	public abstract class RendererWrapper
	{
		private static ThreadLocal<RendererWrapper> current = new ThreadLocal<RendererWrapper>(() => ImmediateRendererWrapper.Instance);

		public static RendererWrapper Current
		{
			get => current.Value;
			set => current.Value = value;
		}

		public abstract Blending Blending { set; }
		public abstract ShaderId Shader { set; }
		public abstract Matrix32 Transform1 { set; }
		public abstract Matrix32 Transform2 { set; }
		public abstract ColorWriteMask ColorWriteEnabled { set; }
		public abstract StencilState StencilState { set; }
		public abstract Viewport Viewport { set; }
		public abstract DepthState DepthState { set; }
		public abstract ScissorState ScissorState { set; }
		public abstract Matrix44 World { set; }
		public abstract Matrix44 View { set; }
		public abstract Matrix44 Projection { set; }
		public abstract CullMode CullMode { set; }

		public abstract void BeginFrame();
		public abstract void EndFrame();
		public abstract void Flush();
		public abstract void MultiplyTransform1(Matrix32 transform);
		public abstract void MultiplyTransform2(Matrix32 transform);
		public abstract void DrawRectOutline(Vector2 a, Vector2 b, Color4 color, float thickness = 1);
		public abstract void DrawSprite(ITexture texture1, Color4 color, Vector2 position, Vector2 size, Vector2 uv0, Vector2 uv1);
		public abstract void DrawSprite(ITexture texture1, ITexture texture2, Color4 color, Vector2 position, Vector2 size, Vector2 uv0, Vector2 uv1);
		public abstract void DrawSprite(ITexture texture1, ITexture texture2, Color4 color, Vector2 position, Vector2 size, Vector2 uv0t1, Vector2 uv1t1, Vector2 uv0t2, Vector2 uv1t2);
		public abstract void DrawSprite(ITexture texture1, ITexture texture2, IMaterial material, Color4 color, Vector2 position, Vector2 size, Vector2 uv0t1, Vector2 uv1t1, Vector2 uv0t2, Vector2 uv1t2);

		public abstract void DrawCircle(Vector2 center, float radius, int numSegments, Color4 color);

		public void DrawCircle(float x, float y, float radius, int numSegments, Color4 color)
		{
			DrawCircle(new Vector2(x, y), radius, numSegments, color);
		}

		public abstract void DrawRound(Vector2 center, float radius, int numSegments, Color4 innerColor, Color4 outerColor);

		public void DrawRound(Vector2 center, float radius, int numSegments, Color4 color)
		{
			DrawRound(center, radius, numSegments, color, color);
		}

		public void DrawRound(float x, float y, float radius, int numSegments, Color4 color)
		{
			DrawRound(new Vector2(x, y), radius, numSegments, color, color);
		}
		
		public void DrawTriangleFan(Vertex[] vertices, int numVertices)
		{
			DrawTriangleFan(null, null, vertices, numVertices);
		}

		public void DrawTriangleFan(ITexture texture1, Vertex[] vertices, int numVertices)
		{
			DrawTriangleFan(texture1, null, vertices, numVertices);
		}

		public abstract void DrawTriangleFan(ITexture texture1, ITexture texture2, Vertex[] vertices, int numVertices);

		public void DrawRect(float x0, float y0, float x1, float y1, Color4 color)
		{
			DrawRect(new Vector2(x0, y0), new Vector2(x1, y1), color);
		}

		public abstract void DrawRect(Vector2 a, Vector2 b, Color4 color);

		public void DrawTextLine(float x, float y, string text, float fontHeight, Color4 color, float letterSpacing)
		{
			DrawTextLine(new Vector2(x, y), text, fontHeight, color, letterSpacing);
		}

		public void DrawTextLine(Vector2 position, string text, float fontHeight, Color4 color, float letterSpacing)
		{
			DrawTextLine(FontPool.Instance[null], position, text, fontHeight, color, letterSpacing);
		}

		public void DrawTextLine(IFont font, Vector2 position, string text, float fontHeight, Color4 color, float letterSpacing)
		{
			DrawTextLine(font, position, text, color, fontHeight, 0, text.Length, letterSpacing);
		}

		public abstract void DrawTextLine(IFont font, Vector2 position, string text, Color4 color, float fontHeight, int start, int length, float letterSpacing);
		public abstract void DrawRenderChain(RenderChain renderChain);
		public abstract void DrawRenderObjects(RenderObjectList renderObjects);

		public void DrawLine(float x0, float y0, float x1, float y1, Color4 color, float thickness = 1, LineCap cap = LineCap.Butt)
		{
			DrawLine(new Vector2(x0, y0), new Vector2(x1, y1), color, thickness, cap);
		}

		public abstract void DrawLine(Vector2 a, Vector2 b, Color4 color, float thickness = 1, LineCap cap = LineCap.Butt);

		public void Clear(Color4 color)
		{
			Clear(ClearOptions.All, color);
		}

		public void Clear(ClearOptions options)
		{
			Clear(options, Color4.Black);
		}

		public abstract void Clear(ClearOptions options, Color4 color);
		public abstract void PushState(RenderState mask);
		public abstract void PopState();

		public void SetOrthogonalProjection(Vector2 leftTop, Vector2 rightBottom)
		{
			SetOrthogonalProjection(leftTop.X, leftTop.Y, rightBottom.X, rightBottom.Y);
		}

		public abstract void SetOrthogonalProjection(float left, float top, float right, float bottom);

		public abstract void PushRenderTarget(ITexture texture);
		public abstract void PopRenderTarget();
		public abstract void Callback(Action callback);

		public void RenderToTexture(RenderChain renderChain, ITexture texture, float width, float height, Matrix32 viewMatrix, bool clearRenderTarget = true)
		{
			if (width > 0 && height > 0) {
				PushRenderTarget(texture);
				PushState(
					RenderState.ScissorState |
					RenderState.View |
					RenderState.World |
					RenderState.Viewport |
					RenderState.Projection |
					RenderState.DepthState |
					RenderState.CullMode |
					RenderState.Transform2);
				ScissorState = ScissorState.ScissorDisabled;
				Viewport = new Viewport(0, 0, texture.ImageSize.Width, texture.ImageSize.Height);
				if (clearRenderTarget) {
					Clear(new Color4(0, 0, 0, 0));
				}
				World = Matrix44.Identity;
				View = Matrix44.Identity;
				SetOrthogonalProjection(0, 0, width, height);
				DepthState = DepthState.DepthDisabled;
				CullMode = CullMode.None;
				Transform2 = viewMatrix;
				DrawRenderChain(renderChain);
				PopState();
				PopRenderTarget();
			}
		}

		public void RenderToTexture(Widget widget, ITexture texture, RenderChain renderChain, bool clearRenderTarget = true)
		{
			try {
				for (var node = widget.FirstChild; node != null; node = node.NextSibling) {
					node.RenderChainBuilder?.AddToRenderChain(renderChain);
				}
				RenderToTexture(renderChain, texture, widget.Width, widget.Height, widget.LocalToWorldTransform.CalcInversed(), clearRenderTarget);
			} finally {
				renderChain.Clear();
			}
		}

		public void GetBitmap(Widget widget, Action<Bitmap> callback)
		{
			var pixelScale = Window.Current.PixelScale;
			var scaledWidth = (int)(widget.Width * pixelScale);
			var scaledHeight = (int)(widget.Height * pixelScale);
			var savedScale = widget.Scale;
			var savedPosition = widget.Position;
			var savedPivot = widget.Pivot;
			try {
				widget.Scale = Vector2.One;
				widget.Position = Vector2.Zero;
				widget.Pivot = Vector2.Zero;
				var texture = new RenderTexture(scaledWidth, scaledHeight);
				var renderChain = new RenderChain();
				widget.RenderChainBuilder?.AddToRenderChain(renderChain);
				RenderToTexture(widget, texture, renderChain);
				Callback(() => {
					callback(new Bitmap(texture.GetPixels(), scaledWidth, scaledHeight));
				});
			} finally {
				widget.Scale = savedScale;
				widget.Position = savedPosition;
				widget.Pivot = savedPivot;
			}
		}
	}

	public class ImmediateRendererWrapper : RendererWrapper
	{
		public static readonly ImmediateRendererWrapper Instance = new ImmediateRendererWrapper();

		private ImmediateRendererWrapper() { }

		public override Blending Blending { set => Renderer.Blending = value; }
		public override ShaderId Shader { set => Renderer.Shader = value; }
		public override Matrix32 Transform1 { set => Renderer.Transform1 = value; }
		public override Matrix32 Transform2 { set => Renderer.Transform2 = value; }
		public override ColorWriteMask ColorWriteEnabled { set => Renderer.ColorWriteEnabled = value; }
		public override StencilState StencilState { set => Renderer.StencilState = value; }
		public override Viewport Viewport { set => Renderer.Viewport = value; }
		public override DepthState DepthState { set => Renderer.DepthState = value; }
		public override ScissorState ScissorState { set => Renderer.ScissorState = value; }
		public override Matrix44 World { set => Renderer.World = value; }
		public override Matrix44 View { set => Renderer.View = value; }
		public override Matrix44 Projection { set => Renderer.Projection = value; }
		public override CullMode CullMode { set => Renderer.CullMode = value; }

		public override void BeginFrame()
		{
			Renderer.BeginFrame();
		}

		public override void EndFrame()
		{
			Renderer.EndFrame();
		}

		public override void Flush()
		{
			Renderer.Flush();
		}

		public override void MultiplyTransform1(Matrix32 transform)
		{
			Renderer.MultiplyTransform1(transform);
		}

		public override void MultiplyTransform2(Matrix32 transform)
		{
			Renderer.MultiplyTransform2(transform);
		}

		public override void DrawCircle(Vector2 center, float radius, int numSegments, Color4 color)
		{
			Renderer.DrawCircle(center, radius, numSegments, color);
		}

		public override void DrawRound(Vector2 center, float radius, int numSegments, Color4 innerColor, Color4 outerColor)
		{
			Renderer.DrawRound(center, radius, numSegments, innerColor, outerColor);
		}

		public override void DrawRectOutline(Vector2 a, Vector2 b, Color4 color, float thickness = 1)
		{
			Renderer.DrawRectOutline(a, b, color, thickness);
		}

		public override void DrawSprite(ITexture texture1, Color4 color, Vector2 position, Vector2 size, Vector2 uv0, Vector2 uv1)
		{
			Renderer.DrawSprite(texture1, color, position, size, uv0, uv1);
		}

		public override void DrawSprite(ITexture texture1, ITexture texture2, Color4 color, Vector2 position, Vector2 size, Vector2 uv0, Vector2 uv1)
		{
			Renderer.DrawSprite(texture1, texture2, color, position, size, uv0, uv1);
		}

		public override void DrawSprite(ITexture texture1, ITexture texture2, Color4 color, Vector2 position, Vector2 size, Vector2 uv0t1, Vector2 uv1t1, Vector2 uv0t2, Vector2 uv1t2)
		{
			Renderer.DrawSprite(texture1, texture2, color, position, size, uv0t1, uv1t1, uv0t2, uv1t2);
		}

		public override void DrawSprite(ITexture texture1, ITexture texture2, IMaterial material, Color4 color, Vector2 position, Vector2 size, Vector2 uv0t1, Vector2 uv1t1, Vector2 uv0t2, Vector2 uv1t2)
		{
			Renderer.DrawSprite(texture1, texture2, material, color, position, size, uv0t1, uv1t1, uv0t2, uv1t2);
		}

		public override void DrawTriangleFan(ITexture texture1, ITexture texture2, Vertex[] vertices, int numVertices)
		{
			Renderer.DrawTriangleFan(texture1, texture2, vertices, numVertices);
		}

		public override void DrawRect(Vector2 a, Vector2 b, Color4 color)
		{
			Renderer.DrawRect(a, b, color);
		}

		public override void DrawTextLine(IFont font, Vector2 position, string text, Color4 color, float fontHeight, int start, int length, float letterSpacing)
		{
			Renderer.DrawTextLine(font, position, text, color, fontHeight, start, length, letterSpacing);
		}

		public override void DrawLine(Vector2 a, Vector2 b, Color4 color, float thickness = 1, LineCap cap = LineCap.Butt)
		{
			Renderer.DrawLine(a, b, color, thickness, cap);
		}

		public override void DrawRenderChain(RenderChain renderChain)
		{
			throw new NotSupportedException();
		}

		public override void DrawRenderObjects(RenderObjectList renderObjects)
		{
			renderObjects.Render();
		}

		public override void Clear(ClearOptions options, Color4 color)
		{
			Renderer.Clear(options, color);
		}

		public override void PushState(RenderState mask)
		{
			Renderer.PushState(mask);
		}

		public override void PopState()
		{
			Renderer.PopState();
		}

		public override void SetOrthogonalProjection(float left, float top, float right, float bottom)
		{
			Renderer.SetOrthogonalProjection(left, top, right, bottom);
		}

		private Stack<ITexture> renderTargetStack = new Stack<ITexture>();

		public override void PushRenderTarget(ITexture texture)
		{
			texture.SetAsRenderTarget();
			renderTargetStack.Push(texture);
		}

		public override void PopRenderTarget()
		{
			renderTargetStack.Pop().RestoreRenderTarget();
		}

		public override void Callback(Action callback) => callback();
	}

	public class DeferredRendererWrapper : RendererWrapper
	{
		private List<Command> commands = new List<Command>();

		public override Blending Blending
		{
			set {
				var cmd = AddCommand<SetBlendingCommand>();
				cmd.Value = value;
			}
		}

		public override ShaderId Shader
		{
			set {
				var cmd = AddCommand<SetShaderCommand>();
				cmd.Value = value;
			}
		}

		public override Matrix32 Transform1
		{
			set {
				var cmd = AddCommand<SetTransform1Command>();
				cmd.Value = value;
			}
		}

		public override Matrix32 Transform2
		{
			set {
				var cmd = AddCommand<SetTransform2Command>();
				cmd.Value = value;
			}
		}

		public override ColorWriteMask ColorWriteEnabled
		{
			set {
				var cmd = AddCommand<SetColorWriteEnabledCommand>();
				cmd.Value = value;
			}
		}

		public override StencilState StencilState
		{
			set {
				var cmd = AddCommand<SetStencilStateCommand>();
				cmd.Value = value;
			}
		}

		public override Viewport Viewport
		{
			set {
				var cmd = AddCommand<SetViewportCommand>();
				cmd.Value = value;
			}
		}

		public override DepthState DepthState
		{
			set {
				var cmd = AddCommand<SetDepthStateCommand>();
				cmd.Value = value;
			}
		}

		public override ScissorState ScissorState
		{
			set {
				var cmd = AddCommand<SetScissorStateCommand>();
				cmd.Value = value;
			}
		}

		public override Matrix44 World
		{
			set {
				var cmd = AddCommand<SetWorldCommand>();
				cmd.Value = value;
			}
		}

		public override Matrix44 View
		{
			set {
				var cmd = AddCommand<SetViewCommand>();
				cmd.Value = value;
			}
		}

		public override Matrix44 Projection
		{
			set {
				var cmd = AddCommand<SetProjectionCommand>();
				cmd.Value = value;
			}
		}

		public override CullMode CullMode
		{
			set {
				var cmd = AddCommand<SetCullModeCommand>();
				cmd.Value = value;
			}
		}

		public override void BeginFrame()
		{
			throw new NotSupportedException();
		}

		public override void EndFrame()
		{
			throw new NotSupportedException();
		}

		public override void Flush()
		{
			AddCommand<FlushCommand>();
		}

		public override void MultiplyTransform1(Matrix32 transform)
		{
			var cmd = AddCommand<MultiplyTransform1Command>();
			cmd.Value = transform;
		}

		public override void MultiplyTransform2(Matrix32 transform)
		{
			var cmd = AddCommand<MultiplyTransform2Command>();
			cmd.Value = transform;
		}

		public override void DrawCircle(Vector2 center, float radius, int numSegments, Color4 color)
		{
			var cmd = AddCommand<DrawCircleCommand>();
			cmd.Center = center;
			cmd.Radius = radius;
			cmd.NumSegments = numSegments;
			cmd.Color = color;
		}

		public override void DrawRound(Vector2 center, float radius, int numSegments, Color4 innerColor, Color4 outerColor)
		{
			var cmd = AddCommand<DrawRoundCommand>();
			cmd.Center = center;
			cmd.Radius = radius;
			cmd.NumSegments = numSegments;
			cmd.InnerColor = innerColor;
			cmd.OuterColor = outerColor;
		}

		public override void DrawRectOutline(Vector2 a, Vector2 b, Color4 color, float thickness = 1)
		{
			var cmd = AddCommand<DrawRectOutlineCommand>();
			cmd.A = a;
			cmd.B = b;
			cmd.Color = color;
			cmd.Thickness = thickness;
		}

		public override void DrawSprite(ITexture texture1, Color4 color, Vector2 position, Vector2 size, Vector2 uv0, Vector2 uv1)
		{
			var cmd = AddCommand<DrawSpriteCommand>();
			cmd.Texture1 = texture1;
			cmd.Texture2 = null;
			cmd.Material = null;
			cmd.Color = color;
			cmd.Position = position;
			cmd.Size = size;
			cmd.UV0T1 = uv0;
			cmd.UV0T2 = uv0;
			cmd.UV1T1 = uv1;
			cmd.UV1T2 = uv1;
		}

		public override void DrawSprite(ITexture texture1, ITexture texture2, Color4 color, Vector2 position, Vector2 size, Vector2 uv0, Vector2 uv1)
		{
			var cmd = AddCommand<DrawSpriteCommand>();
			cmd.Texture1 = texture1;
			cmd.Texture2 = texture2;
			cmd.Material = null;
			cmd.Color = color;
			cmd.Position = position;
			cmd.Size = size;
			cmd.UV0T1 = uv0;
			cmd.UV0T2 = uv0;
			cmd.UV1T1 = uv1;
			cmd.UV1T2 = uv1;
		}

		public override void DrawSprite(ITexture texture1, ITexture texture2, Color4 color, Vector2 position, Vector2 size, Vector2 uv0t1, Vector2 uv1t1, Vector2 uv0t2, Vector2 uv1t2)
		{
			var cmd = AddCommand<DrawSpriteCommand>();
			cmd.Texture1 = texture1;
			cmd.Texture2 = texture2;
			cmd.Material = null;
			cmd.Color = color;
			cmd.Position = position;
			cmd.Size = size;
			cmd.UV0T1 = uv0t1;
			cmd.UV0T2 = uv0t2;
			cmd.UV1T1 = uv1t1;
			cmd.UV1T2 = uv1t2;
		}

		public override void DrawSprite(ITexture texture1, ITexture texture2, IMaterial material, Color4 color, Vector2 position, Vector2 size, Vector2 uv0t1, Vector2 uv1t1, Vector2 uv0t2, Vector2 uv1t2)
		{
			var cmd = AddCommand<DrawSpriteCommand>();
			cmd.Texture1 = texture1;
			cmd.Texture2 = texture2;
			cmd.Material = material;
			cmd.Color = color;
			cmd.Position = position;
			cmd.Size = size;
			cmd.UV0T1 = uv0t1;
			cmd.UV0T2 = uv0t2;
			cmd.UV1T1 = uv1t1;
			cmd.UV1T2 = uv1t2;
		}

		public override void DrawTriangleFan(ITexture texture1, ITexture texture2, Vertex[] vertices, int numVertices)
		{
			var cmd = AddCommand<DrawTriangleFanCommand>();
			cmd.Texture1 = texture1;
			cmd.Texture2 = texture2;
			cmd.SetVertices(vertices, numVertices);
		}

		public override void DrawRect(Vector2 a, Vector2 b, Color4 color)
		{
			var cmd = AddCommand<DrawRectCommand>();
			cmd.A = a;
			cmd.B = b;
			cmd.Color = color;
		}

		public override void DrawTextLine(IFont font, Vector2 position, string text, Color4 color, float fontHeight, int start, int length, float letterSpacing)
		{
			var cmd = AddCommand<DrawTextLineCommand>();
			cmd.Font = font;
			cmd.Position = position;
			cmd.Text = text;
			cmd.FontHeight = fontHeight;
			cmd.Color = color;
			cmd.Start = start;
			cmd.Length = length;
			cmd.LetterSpacing = letterSpacing;
		}

		public override void DrawLine(Vector2 a, Vector2 b, Color4 color, float thickness = 1, LineCap cap = LineCap.Butt)
		{
			var cmd = AddCommand<DrawLineCommand>();
			cmd.A = a;
			cmd.B = b;
			cmd.Color = color;
			cmd.Thickness = thickness;
			cmd.Cap = cap;
		}

		public override void DrawRenderChain(RenderChain renderChain)
		{
			var cmd = AddCommand<DrawRenderChainCommand>();
			cmd.SetRenderChain(renderChain);
		}

		public override void DrawRenderObjects(RenderObjectList renderObjects)
		{
			throw new NotSupportedException();
		}

		public override void Clear(ClearOptions options, Color4 color)
		{
			var cmd = AddCommand<ClearCommand>();
			cmd.Options = options;
			cmd.Color = color;
		}

		public override void PushState(RenderState mask)
		{
			var cmd = AddCommand<PushStateCommand>();
			cmd.Mask = mask;
		}

		public override void PopState()
		{
			AddCommand<PopStateCommand>();
		}

		public override void SetOrthogonalProjection(float left, float top, float right, float bottom)
		{
			var cmd = AddCommand<SetOrthogonalProjectionCommand>();
			cmd.Left = left;
			cmd.Top = top;
			cmd.Right = right;
			cmd.Bottom = bottom;
		}

		public override void PushRenderTarget(ITexture texture)
		{
			var cmd = AddCommand<PushRenderTargetCommand>();
			cmd.Texture = texture;
		}

		public override void PopRenderTarget()
		{
			AddCommand<PopRenderTargetCommand>();
		}

		public override void Callback(Action callback)
		{
			var cmd = AddCommand<CallbackCommand>();
			cmd.Callback = callback;
		}

		public void ClearCommands()
		{
			foreach (var cmd in commands) {
				cmd.Release();
			}
			commands.Clear();
		}

		public void ExecuteCommands(RendererWrapper renderer)
		{
			foreach (var cmd in commands) {
				cmd.Execute(renderer);
			}
		}

		private T AddCommand<T>() where T : Command, new()
		{
			var cmd = AcquireCommand<T>();
			commands.Add(cmd);
			return cmd;
		}

		private abstract class Command
		{
			public abstract void Execute(RendererWrapper renderer);
			public abstract void Release();
		}

		private class SetBlendingCommand : Command
		{
			public Blending Value;

			public override void Execute(RendererWrapper renderer)
			{
				renderer.Blending = Value;
			}

			public override void Release() => ReleaseCommand(this);
		}

		private class SetShaderCommand : Command
		{
			public ShaderId Value;

			public override void Execute(RendererWrapper renderer)
			{
				renderer.Shader = Value;
			}

			public override void Release() => ReleaseCommand(this);
		}

		private class SetTransform1Command : Command
		{
			public Matrix32 Value;

			public override void Execute(RendererWrapper renderer)
			{
				renderer.Transform1 = Value;
			}

			public override void Release() => ReleaseCommand(this);
		}

		private class SetTransform2Command : Command
		{
			public Matrix32 Value;

			public override void Execute(RendererWrapper renderer)
			{
				renderer.Transform2 = Value;
			}

			public override void Release() => ReleaseCommand(this);
		}

		private class SetColorWriteEnabledCommand : Command
		{
			public ColorWriteMask Value;

			public override void Execute(RendererWrapper renderer)
			{
				renderer.ColorWriteEnabled = Value;
			}

			public override void Release() => ReleaseCommand(this);
		}

		private class SetStencilStateCommand : Command
		{
			public StencilState Value;

			public override void Execute(RendererWrapper renderer)
			{
				renderer.StencilState = Value;
			}

			public override void Release() => ReleaseCommand(this);
		}

		private class SetViewportCommand : Command
		{
			public Viewport Value;

			public override void Execute(RendererWrapper renderer)
			{
				renderer.Viewport = Value;
			}

			public override void Release() => ReleaseCommand(this);
		}

		private class SetDepthStateCommand : Command
		{
			public DepthState Value;

			public override void Execute(RendererWrapper renderer)
			{
				renderer.DepthState = Value;
			}

			public override void Release() => ReleaseCommand(this);
		}

		private class SetScissorStateCommand : Command
		{
			public ScissorState Value;

			public override void Execute(RendererWrapper renderer)
			{
				renderer.ScissorState = Value;
			}

			public override void Release() => ReleaseCommand(this);
		}

		private class SetWorldCommand : Command
		{
			public Matrix44 Value;

			public override void Execute(RendererWrapper renderer)
			{
				renderer.World = Value;
			}

			public override void Release() => ReleaseCommand(this);
		}

		private class SetViewCommand : Command
		{
			public Matrix44 Value;

			public override void Execute(RendererWrapper renderer)
			{
				renderer.World = Value;
			}

			public override void Release() => ReleaseCommand(this);
		}

		private class SetProjectionCommand : Command
		{
			public Matrix44 Value;

			public override void Execute(RendererWrapper renderer)
			{
				renderer.World = Value;
			}

			public override void Release() => ReleaseCommand(this);
		}

		private class SetCullModeCommand : Command
		{
			public CullMode Value;

			public override void Execute(RendererWrapper renderer)
			{
				renderer.CullMode = Value;
			}

			public override void Release() => ReleaseCommand(this);
		}

		private class FlushCommand : Command
		{
			public override void Execute(RendererWrapper renderer)
			{
				renderer.Flush();
			}

			public override void Release() => ReleaseCommand(this);
		}

		private class MultiplyTransform1Command : Command
		{
			public Matrix32 Value;

			public override void Execute(RendererWrapper renderer)
			{
				renderer.MultiplyTransform1(Value);
			}

			public override void Release() => ReleaseCommand(this);
		}

		private class MultiplyTransform2Command : Command
		{
			public Matrix32 Value;

			public override void Execute(RendererWrapper renderer)
			{
				renderer.MultiplyTransform2(Value);
			}

			public override void Release() => ReleaseCommand(this);
		}

		private class DrawCircleCommand : Command
		{
			public Vector2 Center;
			public float Radius;
			public int NumSegments;
			public Color4 Color;

			public override void Execute(RendererWrapper renderer)
			{
				renderer.DrawCircle(Center, Radius, NumSegments, Color);
			}

			public override void Release() => ReleaseCommand(this);
		}

		private class DrawRoundCommand : Command
		{
			public Vector2 Center;
			public float Radius;
			public int NumSegments;
			public Color4 InnerColor;
			public Color4 OuterColor;

			public override void Execute(RendererWrapper renderer)
			{
				renderer.DrawRound(Center, Radius, NumSegments, InnerColor, OuterColor);
			}

			public override void Release() => ReleaseCommand(this);
		}

		private class DrawRectOutlineCommand : Command
		{
			public Vector2 A;
			public Vector2 B;
			public Color4 Color;
			public float Thickness;

			public override void Execute(RendererWrapper renderer)
			{
				renderer.DrawRectOutline(A, B, Color, Thickness);
			}

			public override void Release() => ReleaseCommand(this);
		}

		private class DrawSpriteCommand : Command
		{
			public ITexture Texture1;
			public ITexture Texture2;
			public IMaterial Material;
			public Color4 Color;
			public Vector2 Position;
			public Vector2 Size;
			public Vector2 UV0T1;
			public Vector2 UV1T1;
			public Vector2 UV0T2;
			public Vector2 UV1T2;

			public override void Execute(RendererWrapper renderer)
			{
				if (Material != null) {
					renderer.DrawSprite(Texture1, Texture2, Material, Color, Position, Size, UV0T1, UV1T1, UV0T2, UV1T2);
				} else {
					renderer.DrawSprite(Texture1, Texture2, Color, Position, Size, UV0T1, UV1T1, UV0T2, UV1T2);
				}
			}

			public override void Release()
			{
				Texture1 = null;
				Texture2 = null;
				Material = null;
				ReleaseCommand(this);
			}
		}

		private class DrawTriangleFanCommand : Command
		{
			private Vertex[] vertices;
			private int numVertices;

			public ITexture Texture1;
			public ITexture Texture2;

			public void SetVertices(Vertex[] array, int count)
			{
				if (vertices == null || vertices.Length < count) {
					Array.Resize(ref vertices, count);
				}
				Array.Copy(array, vertices, count);
				numVertices = count;
			}

			public override void Execute(RendererWrapper renderer)
			{
				renderer.DrawTriangleFan(Texture1, Texture2, vertices, numVertices);
			}

			public override void Release()
			{
				Texture1 = null;
				Texture2 = null;
				ReleaseCommand(this);
			}
		}

		private class DrawTextLineCommand : Command
		{
			public IFont Font;
			public Vector2 Position;
			public string Text;
			public float FontHeight;
			public Color4 Color;
			public int Start;
			public int Length;
			public float LetterSpacing;

			public override void Execute(RendererWrapper renderer)
			{
				renderer.DrawTextLine(Font, Position, Text, Color, FontHeight, Start, Length, LetterSpacing);
			}

			public override void Release()
			{
				Font = null;
				ReleaseCommand(this);
			}
		}

		private class ClearCommand : Command
		{
			public ClearOptions Options;
			public Color4 Color;

			public override void Execute(RendererWrapper renderer)
			{
				renderer.Clear(Options, Color);
			}

			public override void Release() => ReleaseCommand(this);
		}

		private class DrawRectCommand : Command
		{
			public Vector2 A;
			public Vector2 B;
			public Color4 Color;

			public override void Execute(RendererWrapper renderer)
			{
				renderer.DrawRect(A, B, Color);
			}

			public override void Release() => ReleaseCommand(this);
		}

		private class DrawLineCommand : Command
		{
			public Vector2 A;
			public Vector2 B;
			public Color4 Color;
			public float Thickness;
			public LineCap Cap;

			public override void Execute(RendererWrapper renderer)
			{
				renderer.DrawLine(A, B, Color, Thickness, Cap);
			}

			public override void Release() => ReleaseCommand(this);
		}

		private class DrawRenderChainCommand : Command
		{
			private RenderObjectList renderObjects = new RenderObjectList();

			public void SetRenderChain(RenderChain renderChain)
			{
				renderObjects.Clear();
				renderChain.GetRenderObjects(renderObjects);
			}

			public override void Execute(RendererWrapper renderer)
			{
				renderer.DrawRenderObjects(renderObjects);
			}

			public override void Release()
			{
				renderObjects.Clear();
				ReleaseCommand(this);
			}
		}

		private class PushStateCommand : Command
		{
			public RenderState Mask;

			public override void Execute(RendererWrapper renderer)
			{
				renderer.PushState(Mask);
			}

			public override void Release() => ReleaseCommand(this);
		}

		private class PopStateCommand : Command
		{
			public override void Execute(RendererWrapper renderer)
			{
				renderer.PopState();
			}

			public override void Release() => ReleaseCommand(this);
		}

		private class SetOrthogonalProjectionCommand : Command
		{
			public float Left;
			public float Right;
			public float Top;
			public float Bottom;

			public override void Execute(RendererWrapper renderer)
			{
				renderer.SetOrthogonalProjection(Left, Top, Right, Bottom);
			}

			public override void Release() => ReleaseCommand(this);
		}

		private class PushRenderTargetCommand : Command
		{
			public ITexture Texture;

			public override void Execute(RendererWrapper renderer)
			{
				renderer.PushRenderTarget(Texture);
			}

			public override void Release()
			{
				Texture = null;
				ReleaseCommand(this);
			}
		}

		private class PopRenderTargetCommand : Command
		{
			public override void Execute(RendererWrapper renderer)
			{
				renderer.PopRenderTarget();
			}

			public override void Release() => ReleaseCommand(this);
		}

		private class CallbackCommand : Command
		{
			public Action Callback;

			public override void Execute(RendererWrapper renderer)
			{
				renderer.Callback(Callback);
			}

			public override void Release()
			{
				Callback = null;
				ReleaseCommand(this);
			}
		}

		private static T AcquireCommand<T>() where T : Command, new()
		{
			return CommandPool<T>.Acquire();
		}

		private static void ReleaseCommand<T>(T command) where T : Command, new()
		{
			CommandPool<T>.Release(command);
		}

		private static class CommandPool<T> where T : Command, new()
		{
			private static Stack<T> freeCommands = new Stack<T>();

			public static T Acquire()
			{
				if (freeCommands.Count > 0) {
					return freeCommands.Pop();
				} else {
					return new T();
				}
			}

			public static void Release(T command)
			{
				System.Diagnostics.Debug.Assert(command.GetType() == typeof(T));
				freeCommands.Push(command);
			}
		}
	}
}
