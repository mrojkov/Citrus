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
		private bool renderTargetDirty = false;

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
		internal bool SupportsDxt1;
		internal bool SupportsDxt3;
		internal bool SupportsDxt5;
		internal bool SupportsPvrtc;
		internal bool SupportsEtc1;
		internal bool SupportsEtc2;
		internal int GLMajorVersion;
		internal int GLMinorVersion;
		internal bool ESProfile;

		public PlatformRenderContext()
		{
			CheckFeatures();
			textures = new PlatformTexture2D[MaxTextureSlots];
			vertexBuffers = new PlatformBuffer[MaxVertexBufferSlots];
			vertexOffsets = new int[MaxVertexBufferSlots];
		}

		public void Dispose() { }

		public void Begin(int glFramebuffer)
		{
			glDefaultFramebuffer = glFramebuffer;
			renderTargetDirty = true;
		}

		internal void InvalidateRenderTargetBinding()
		{
			renderTargetDirty = true;
		}

		public void End()
		{
		}

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
			GLHelper.ParseGLVersion(GL.GetString(StringName.Version), out GLMajorVersion, out GLMinorVersion, out ESProfile);
			var glExtensions = new HashSet<string>(GL.GetString(StringName.Extensions).Split(' '));
			SupportsPackedDepth24Stencil8 = !ESProfile || glExtensions.Contains("GL_OES_packed_depth_stencil");
			SupportsDepth24 = !ESProfile || glExtensions.Contains("GL_OES_depth24");
			var supportsS3tc = glExtensions.Contains("GL_EXT_texture_compression_s3tc");
			SupportsDxt1 = supportsS3tc || glExtensions.Contains("GL_EXT_texture_compression_dxt1");
			SupportsDxt3 = supportsS3tc || glExtensions.Contains("GL_ANGLE_texture_compression_dxt3");
			SupportsDxt5 = supportsS3tc || glExtensions.Contains("GL_ANGLE_texture_compression_dxt5");
			SupportsPvrtc = glExtensions.Contains("GL_IMG_texture_compression_pvrtc");
			SupportsEtc1 = glExtensions.Contains("GL_OES_compressed_ETC1_RGB8_texture");
			SupportsEtc2 = (ESProfile && GLMajorVersion >= 3) || glExtensions.Contains("GL_ARB_ES3_compatibility");
			GL.GetInteger(GetPName.MaxCombinedTextureImageUnits, out MaxTextureSlots);
			GL.GetInteger(GetPName.MaxVertexAttribs, out MaxVertexAttributes);
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
			if (renderTarget != texture) {
				renderTarget = (PlatformRenderTexture2D)texture;
				renderTargetDirty = true;
			}
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
			if (renderTargetDirty) {
				var glFramebuffer = renderTarget != null ? renderTarget.GLFramebuffer : glDefaultFramebuffer;
				GL.BindFramebuffer(FramebufferTarget.Framebuffer, glFramebuffer);
				GLHelper.CheckGLErrors();
				renderTargetDirty = false;
			}
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
			foreach (var slot in shaderProgram.TextureSlots) {
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
			switch (format) {
				case Format.R8_SInt:
				case Format.R8_SNorm:
				case Format.R8_UInt:
				case Format.R8_UNorm:
				case Format.R8G8_SInt:
				case Format.R8G8_SNorm:
				case Format.R8G8_UInt:
				case Format.R8G8_UNorm:
				case Format.R8G8B8_SInt:
				case Format.R8G8B8_SNorm:
				case Format.R8G8B8_UInt:
				case Format.R8G8B8A8_SInt:
				case Format.R8G8B8A8_SNorm:
				case Format.R8G8B8A8_UInt:
				case Format.R32_SFloat:
				case Format.R32G32_SFloat:
				case Format.R32G32B32_SFloat:
				case Format.R32G32B32A32_SFloat:
					return FormatFeatures.VertexBuffer;
				case Format.R8G8B8_UNorm:
				case Format.R8G8B8A8_UNorm:
				case Format.R5G6B5_UNorm_Pack16:
				case Format.R5G5B5A1_UNorm_Pack16:
				case Format.R4G4B4A4_UNorm_Pack16:
					return FormatFeatures.Sample | FormatFeatures.RenderTarget | FormatFeatures.VertexBuffer;
				case Format.BC1_RGB_UNorm_Block:
				case Format.BC1_RGBA_UNorm_Block:
					return SupportsDxt1 ? FormatFeatures.Sample : FormatFeatures.None;
				case Format.BC2_UNorm_Block:
					return SupportsDxt3 ? FormatFeatures.Sample : FormatFeatures.None;
				case Format.BC3_UNorm_Block:
					return SupportsDxt5 ? FormatFeatures.Sample : FormatFeatures.None;
				case Format.ETC1_R8G8B8_UNorm_Block:
					return SupportsEtc1 ? FormatFeatures.Sample : FormatFeatures.None;
				case Format.ETC2_R8G8B8_UNorm_Block:
				case Format.ETC2_R8G8B8A1_UNorm_Block:
				case Format.ETC2_R8G8B8A8_UNorm_Block:
					return SupportsEtc2 ? FormatFeatures.Sample : FormatFeatures.None;
				case Format.PVRTC1_2Bpp_UNorm_Block:
				case Format.PVRTC1_4Bpp_UNorm_Block:
				case Format.PVRTC2_2Bpp_UNorm_Block:
				case Format.PVRTC2_4Bpp_UNorm_Block:
					return SupportsPvrtc ? FormatFeatures.Sample : FormatFeatures.None;
				default:
					return FormatFeatures.None;
			}
		}
	}
}
