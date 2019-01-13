using System;
using System.Collections.Generic;
using OpenTK.Graphics.ES20;
using GLStencilOp = OpenTK.Graphics.ES20.StencilOp;

namespace Lime.Graphics.Platform.OpenGL
{
	public class PlatformRenderContext : IPlatformRenderContext
	{
		private int glDefaultFramebuffer;
		private Viewport viewport;
		private BlendState blendState;
		private DepthState depthState;
		private StencilState stencilState;
		private ScissorState scissorState;
		private ColorWriteMask colorWriteMask;
		private CullMode cullMode;
		private FrontFace frontFace;
		private PrimitiveTopology primitiveTopology;
		private PlatformShaderProgram shaderProgram;
		private PlatformVertexInputLayout vertexInputLayout;
		private PlatformTexture2D[] textures;
		private PlatformBuffer[] vertexBuffers;
		private int[] vertexOffsets;
		private PlatformBuffer indexBuffer;
		private int indexOffset;
		private IndexFormat indexFormat;
		private PlatformRenderTexture2D renderTarget;

		internal int MaxTextureSlots;
		internal int MaxVertexBufferSlots;
		internal int MaxVertexAttributes;
		internal bool SupportsPackedDepth24Stencil8;
		internal bool SupportsDepth24;
		internal bool ESProfile;

		public PlatformRenderContext(bool esProfile)
		{
			ESProfile = esProfile;
			CheckFeatures();
			textures = new PlatformTexture2D[MaxTextureSlots];
			vertexBuffers = new PlatformBuffer[MaxVertexBufferSlots];
			vertexOffsets = new int[MaxVertexBufferSlots];
		}

		public void Invalidate()
		{
		}

		public void SetDefaultFramebuffer(int glFramebuffer)
		{
			glDefaultFramebuffer = glFramebuffer;
		}

		public void Dispose() { }

		public IPlatformBuffer CreateBuffer(BufferType bufferType, int size, bool dynamic)
		{
			return new PlatformBuffer(this, bufferType, size, dynamic);
		}

		public IPlatformTexture2D CreateTexture2D(Format format, int width, int height, bool mipmaps, TextureParams textureParams)
		{
			return new PlatformTexture2D(this, format, width, height, mipmaps, textureParams);
		}

		public IPlatformRenderTexture2D CreateRenderTexture2D(Format format, int width, int height, TextureParams textureParams)
		{
			return new PlatformRenderTexture2D(this, format, width, height, textureParams);
		}

		public IPlatformShader CreateShader(ShaderStageMask stage, string source)
		{
			return new PlatformShader(this, stage, source);
		}

		public IPlatformShaderProgram CreateShaderProgram(
			IPlatformShader[] shaders,
			ShaderProgram.AttribLocation[] attribLocation,
			ShaderProgram.Sampler[] samplers)
		{
			return new PlatformShaderProgram(this, shaders, attribLocation, samplers);
		}

		public IPlatformVertexInputLayout CreateVertexInputLayout(
			VertexInputLayoutBinding[] bindings,
			VertexInputLayoutAttribute[] attributes)
		{
			return new PlatformVertexInputLayout(this, bindings, attributes);
		}

		private void CheckFeatures()
		{
			var glExtensionsString = GL.GetString(StringName.Extensions);
			GLHelper.CheckGLErrors();
			var glExtensions = new HashSet<string>(glExtensionsString.Split(' '));
			SupportsPackedDepth24Stencil8 = !ESProfile || glExtensions.Contains("GL_OES_packed_depth_stencil");
			SupportsDepth24 = !ESProfile || glExtensions.Contains("GL_OES_depth24");
			GL.GetInteger(GetPName.MaxCombinedTextureImageUnits, out MaxTextureSlots);
			GLHelper.CheckGLErrors();
			GL.GetInteger(GetPName.MaxVertexAttribs, out MaxVertexAttributes);
			GLHelper.CheckGLErrors();
			MaxVertexBufferSlots = MaxVertexAttributes;
		}

		public void SetViewport(Viewport vp)
		{
			viewport = vp;
		}

		public void SetBlendState(BlendState state)
		{
			blendState = state;
		}

		public void SetDepthState(DepthState state)
		{
			depthState = state;
		}

		public void SetStencilState(StencilState state)
		{
			stencilState = state;
		}

		public void SetScissorState(ScissorState state)
		{
			scissorState = state;
		}

		public void SetColorWriteMask(ColorWriteMask mask)
		{
			colorWriteMask = mask;
		}

		public void SetCullMode(CullMode mode)
		{
			cullMode = mode;
		}

		public void SetFrontFace(FrontFace face)
		{
			frontFace = face;
		}

		public void SetPrimitiveTopology(PrimitiveTopology topology)
		{
			primitiveTopology = topology;
		}

		public void SetShaderProgram(IPlatformShaderProgram program)
		{
			shaderProgram = (PlatformShaderProgram)program;
		}

		public void SetVertexInputLayout(IPlatformVertexInputLayout layout)
		{
			vertexInputLayout = (PlatformVertexInputLayout)layout;
		}

		public void SetTexture(int slot, IPlatformTexture2D texture)
		{
			textures[slot] = (PlatformTexture2D)texture;
		}

		public void SetVertexBuffer(int slot, IPlatformBuffer buffer, int offset)
		{
			vertexBuffers[slot] = (PlatformBuffer)buffer;
			vertexOffsets[slot] = offset;
		}

		public void SetIndexBuffer(IPlatformBuffer buffer, int offset, IndexFormat format)
		{
			indexBuffer = (PlatformBuffer)buffer;
			indexOffset = offset;
			indexFormat = format;
		}

		public void SetRenderTarget(IPlatformRenderTexture2D texture)
		{
			renderTarget = (PlatformRenderTexture2D)texture;
		}

		public void Clear(ClearOptions options, float r, float g, float b, float a, float depth, byte stencil)
		{
			if (options == ClearOptions.None) {
				return;
			}
			EnsureRenderTarget();
			ClearBufferMask glClearBufferMask = 0;
			if ((options & ClearOptions.ColorBuffer) != 0) {
				glClearBufferMask |= ClearBufferMask.ColorBufferBit;
				GL.ColorMask(true, true, true, true);
				GLHelper.CheckGLErrors();
				GL.ClearColor(r, g, b, a);
				GLHelper.CheckGLErrors();
			}
			if ((options & ClearOptions.DepthBuffer) != 0) {
				glClearBufferMask |= ClearBufferMask.DepthBufferBit;
				GL.DepthMask(true);
				GLHelper.CheckGLErrors();
				GL.ClearDepth(depth);
				GLHelper.CheckGLErrors();
			}
			if ((options & ClearOptions.StencilBuffer) != 0) {
				glClearBufferMask |= ClearBufferMask.StencilBufferBit;
				GL.StencilMask(0xff);
				GLHelper.CheckGLErrors();
				GL.ClearStencil(stencil);
				GLHelper.CheckGLErrors();
			}
			GL.Scissor(viewport.X, viewport.Y, viewport.Width, viewport.Height);
			GLHelper.CheckGLErrors();
			GL.Clear(glClearBufferMask);
			GLHelper.CheckGLErrors();
		}

		public void Draw(int startVertex, int vertexCount)
		{
			PreDraw(0);
			GL.DrawArrays((PrimitiveType)GLHelper.GetGLPrimitiveType(primitiveTopology), startVertex, vertexCount);
			GLHelper.CheckGLErrors();
		}

		public void DrawIndexed(int startIndex, int indexCount, int baseVertex)
		{
			PreDraw(baseVertex);
			var effectiveOffset = indexOffset + startIndex * indexFormat.GetSize();
			GL.DrawElements(
				(PrimitiveType)GLHelper.GetGLPrimitiveType(primitiveTopology), indexCount,
				(DrawElementsType)GLHelper.GetGLDrawElementsType(indexFormat), new IntPtr(effectiveOffset));
			GLHelper.CheckGLErrors();
		}

		public void Flush()
		{
			GL.Flush();
			GLHelper.CheckGLErrors();
		}

		public void Finish()
		{
			GL.Finish();
			GLHelper.CheckGLErrors();
		}

		private void PreDraw(int baseVertex)
		{
			EnsureRenderTarget();
			BindState();
			BindShaderProgram();
			BindTextures();
			BindVertexAttributes(baseVertex);
			BindIndexBuffer();
		}

		private void EnsureRenderTarget()
		{
			var glFramebuffer = renderTarget != null ? renderTarget.GLFramebuffer : glDefaultFramebuffer;
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, glFramebuffer);
			GLHelper.CheckGLErrors();
		}

		private void BindState()
		{
			if (blendState.Enable) {
				GL.Enable(EnableCap.Blend);
				GLHelper.CheckGLErrors();
			} else {
				GL.Disable(EnableCap.Blend);
				GLHelper.CheckGLErrors();
			}
			GL.BlendEquationSeparate(
				(BlendEquationMode)GLHelper.GetGLBlendEquationMode(blendState.ColorBlendFunc),
				(BlendEquationMode)GLHelper.GetGLBlendEquationMode(blendState.AlphaBlendFunc));
			GLHelper.CheckGLErrors();
			GL.BlendFuncSeparate(
				(BlendingFactorSrc)GLHelper.GetGLBlendFactor(blendState.ColorSrcBlend), (BlendingFactorDest)GLHelper.GetGLBlendFactor(blendState.ColorDstBlend),
				(BlendingFactorSrc)GLHelper.GetGLBlendFactor(blendState.AlphaSrcBlend), (BlendingFactorDest)GLHelper.GetGLBlendFactor(blendState.AlphaDstBlend));
			GLHelper.CheckGLErrors();
			GL.BlendColor(
				blendState.BlendFactor.R / 255f,
				blendState.BlendFactor.G / 255f,
				blendState.BlendFactor.B / 255f,
				blendState.BlendFactor.A / 255f);
			GLHelper.CheckGLErrors();
			GL.ColorMask(
				(colorWriteMask & ColorWriteMask.Red) != 0,
				(colorWriteMask & ColorWriteMask.Green) != 0,
				(colorWriteMask & ColorWriteMask.Blue) != 0,
				(colorWriteMask & ColorWriteMask.Alpha) != 0);
			GLHelper.CheckGLErrors();
			if (depthState.Enable) {
				GL.Enable(EnableCap.DepthTest);
				GLHelper.CheckGLErrors();
			} else {
				GL.Disable(EnableCap.DepthTest);
				GLHelper.CheckGLErrors();
			}
			GL.DepthMask(depthState.WriteEnable);
			GLHelper.CheckGLErrors();
			GL.DepthFunc((DepthFunction)GLHelper.GetGLCompareFunc(depthState.Comparison));
			GLHelper.CheckGLErrors();
			if (stencilState.Enable) {
				GL.Enable(EnableCap.StencilTest);
				GLHelper.CheckGLErrors();
			} else {
				GL.Disable(EnableCap.StencilTest);
				GLHelper.CheckGLErrors();
			}
			GL.StencilMask(stencilState.WriteMask);
			GLHelper.CheckGLErrors();
			GL.StencilFuncSeparate(StencilFace.Front,
				(StencilFunction)GLHelper.GetGLCompareFunc(stencilState.FrontFaceComparison), stencilState.ReferenceValue, stencilState.ReadMask);
			GLHelper.CheckGLErrors();
			GL.StencilOpSeparate(StencilFace.Front,
				(GLStencilOp)GLHelper.GetGLStencilOp(stencilState.FrontFaceFail),
				(GLStencilOp)GLHelper.GetGLStencilOp(stencilState.FrontFaceDepthFail),
				(GLStencilOp)GLHelper.GetGLStencilOp(stencilState.FrontFacePass));
			GLHelper.CheckGLErrors();
			GL.StencilFuncSeparate(StencilFace.Back,
				(StencilFunction)GLHelper.GetGLCompareFunc(stencilState.BackFaceComparison), stencilState.ReferenceValue, stencilState.ReadMask);
			GLHelper.CheckGLErrors();
			GL.StencilOpSeparate(StencilFace.Back,
				(GLStencilOp)GLHelper.GetGLStencilOp(stencilState.BackFaceFail),
				(GLStencilOp)GLHelper.GetGLStencilOp(stencilState.BackFaceDepthFail),
				(GLStencilOp)GLHelper.GetGLStencilOp(stencilState.BackFacePass));
			GLHelper.CheckGLErrors();
			if (scissorState.Enable) {
				GL.Enable(EnableCap.ScissorTest);
				GLHelper.CheckGLErrors();
			} else {
				GL.Disable(EnableCap.ScissorTest);
				GLHelper.CheckGLErrors();
			}
			var scissor = scissorState.Bounds;
			GL.Scissor(scissor.X, scissor.Y, scissor.Width, scissor.Height);
			GLHelper.CheckGLErrors();
			if (cullMode != CullMode.None) {
				GL.Enable(EnableCap.CullFace);
				GLHelper.CheckGLErrors();
				GL.CullFace((CullFaceMode)GLHelper.GetGLCullFaceMode(cullMode));
				GLHelper.CheckGLErrors();
			} else {
				GL.Disable(EnableCap.CullFace);
				GLHelper.CheckGLErrors();
			}
			GL.FrontFace((FrontFaceDirection)GLHelper.GetGLFrontFaceDirection(frontFace));
			GLHelper.CheckGLErrors();
			GL.Viewport(viewport.X, viewport.Y, viewport.Width, viewport.Height);
			GLHelper.CheckGLErrors();
			GL.DepthRange(viewport.MinDepth, viewport.MaxDepth);
			GLHelper.CheckGLErrors();
		}

		private void BindTextures()
		{
			for (var slot = 0; slot < textures.Length; slot++) {
				var texture = textures[slot];
				GL.ActiveTexture(TextureUnit.Texture0 + slot);
				GLHelper.CheckGLErrors();
				GL.BindTexture(TextureTarget.Texture2D, texture != null ? texture.GLTexture : 0);
				GLHelper.CheckGLErrors();
			}
		}

		private void BindShaderProgram()
		{
			GL.UseProgram(shaderProgram.GLProgram);
			GLHelper.CheckGLErrors();
			shaderProgram.SyncUniforms();
		}

		private List<int> enabledVertexAttribs = new List<int>();

		private void BindVertexAttributes(int baseVertex)
		{
			foreach (var attribIndex in enabledVertexAttribs) {
				GL.DisableVertexAttribArray(attribIndex);
			}
			enabledVertexAttribs.Clear();
			foreach (var binding in vertexInputLayout.GLBindings) {
				var buffer = vertexBuffers[binding.Slot];
				if (buffer == null) {
					continue;
				}
				var offset = vertexOffsets[binding.Slot] + baseVertex * binding.Stride;
				GL.BindBuffer(BufferTarget.ArrayBuffer, buffer.GLBuffer);
				GLHelper.CheckGLErrors();
				foreach (var attrib in binding.Attributes) {
					var effectiveOffset = offset + attrib.Offset;
					GL.EnableVertexAttribArray(attrib.Index);
					GLHelper.CheckGLErrors();
					GL.VertexAttribPointer(
						attrib.Index, attrib.Size, (VertexAttribPointerType)attrib.Type,
						attrib.Normalized, binding.Stride, effectiveOffset);
					GLHelper.CheckGLErrors();
					enabledVertexAttribs.Add(attrib.Index);
				}
			}
		}

		private void BindIndexBuffer()
		{
			if (indexBuffer != null) {
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer.GLBuffer);
				GLHelper.CheckGLErrors();
			}
		}

		public FormatFeatures GetFormatFeatures(Format format)
		{
			throw new NotImplementedException();
		}
	}
}
