using System;

namespace Lime.Graphics.Platform
{
	internal interface IPlatformRenderContext : IDisposable
	{
		IPlatformBuffer CreateBuffer(BufferType bufferType, int size, bool dynamic);
		IPlatformTexture2D CreateTexture2D(Format format, int width, int height, bool mipmaps, TextureParams textureParams);
		IPlatformRenderTexture2D CreateRenderTexture2D(Format format, int width, int height, TextureParams textureParams);
		IPlatformShader CreateShader(ShaderStageMask stage, string source);

		IPlatformShaderProgram CreateShaderProgram(
			IPlatformShader[] shaders,
			ShaderProgram.AttribLocation[] attribLocation,
			ShaderProgram.Sampler[] samplers);

		IPlatformVertexInputLayout CreateVertexInputLayout(
			VertexInputLayoutBinding[] bindings,
			VertexInputLayoutAttribute[] attributes);

		FormatFeatures GetFormatFeatures(Format format);
		void SetRenderTarget(IPlatformRenderTexture2D texture);
		void Clear(ClearOptions options, float r, float g, float b, float a, float depth, byte stencil);
		void Flush();
		void SetViewport(Viewport vp);
		void SetBlendState(BlendState state);
		void SetDepthState(DepthState state);
		void SetStencilState(StencilState state);
		void SetScissorState(ScissorState state);
		void SetColorWriteMask(ColorWriteMask mask);
		void SetCullMode(CullMode mode);
		void SetFrontFace(FrontFace face);
		void SetShaderProgram(IPlatformShaderProgram program);
		void SetVertexInputLayout(IPlatformVertexInputLayout layout);
		void SetPrimitiveTopology(PrimitiveTopology topology);
		void SetTexture(int slot, IPlatformTexture2D texture);
		void SetVertexBuffer(int slot, IPlatformBuffer buffer, int offset);
		void SetIndexBuffer(IPlatformBuffer buffer, int offset, IndexFormat format);
		void Draw(int startVertex, int vertexCount);
		void DrawIndexed(int startIndex, int indexCount, int baseVertex);
	}
}
