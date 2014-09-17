#if OPENGL
using System;
#if iOS || ANDROID
using OpenTK.Graphics.ES20;
#elif MAC
using MonoMac.OpenGL;
using PrimitiveType = MonoMac.OpenGL.BeginMode;
#elif WIN
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
#endif
using ProtoBuf;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Lime
{
	unsafe static class PlatformRenderer
	{
		public static uint CurrentFramebuffer { get; private set; }
		public static uint DefaultFramebuffer { get; private set; }
		private static Blending blending;
		private static ShaderProgram shaderProgram;
		private static uint indexBuffer;
		private static uint vertexBuffer;
		private static bool premultipliedAlphaMode;
		// First texture pair is used for creation mask effect, second pair - for representing ETC1 alpha channel
		private static readonly uint[] textures = new uint[4];

		public static void CheckErrors()
		{
#if DEBUG
			var errCode = GL.GetError();
			if (errCode == ErrorCode.NoError)
				return;
			string errors = "";
			while (errCode != ErrorCode.NoError) {
				if (errors != "")
					errors += ", ";
				errors += errCode.ToString();
				errCode = GL.GetError();
			}
			throw new Exception("OpenGL error(s): " + errors);
#endif
		}

		public static void ResetShader()
		{
			shaderProgram = null;
		}

		public static void SetShader(ShaderId value)
		{
			int numTextures = textures[1] != 0 ? 2 : (textures[0] != 0 ? 1 : 0);
			var program = ShaderPrograms.GetShaderProgram(value, numTextures);
			if (shaderProgram != program) {
				shaderProgram = program;
				shaderProgram.Use();
				shaderProgram.LoadMatrix(program.ProjectionMatrixUniformId, Renderer.Projection);
			}
#if ANDROID
			shaderProgram.LoadBoolean(shaderProgram.UseAlphaTexture1UniformId, textures[2] != 0); 
			shaderProgram.LoadBoolean(shaderProgram.UseAlphaTexture2UniformId, textures[3] != 0); 
#endif
		}

		static PlatformRenderer()
		{
			DefaultFramebuffer = uint.MaxValue;
		}

		public static void BeginFrame()
		{
			SaveDefaultFramebuffer();
			CurrentFramebuffer = DefaultFramebuffer;
			Texture2D.DeleteScheduledTextures();
			GL.Enable(EnableCap.Blend);
			blending = Blending.None;
			premultipliedAlphaMode = false;
			shaderProgram = null;
			indexBuffer = vertexBuffer = 0;
			SetBlending(Blending.Inherited);
			SetShader(ShaderId.Diffuse);
			ClearRenderTarget(0, 0, 0, 0);
			CheckErrors();
		}

		private static void SaveDefaultFramebuffer()
		{
			if (DefaultFramebuffer == uint.MaxValue) {
				var p = new int[1];
				GL.GetInteger(GetPName.FramebufferBinding, p);
				DefaultFramebuffer = (uint)p[0];
			}
		}

		public static void EndFrame()
		{
		}

		public static void ClearRenderTarget(float r, float g, float b, float a)
		{
			GL.ClearColor(r, g, b, a);
			GL.Clear(ClearBufferMask.ColorBufferBit);
		}

		public static void SetTexture(ITexture texture, int stage)
		{
			var handle = texture != null ? texture.GetHandle() : 0;
			SetTexture(handle, stage);
#if ANDROID
			// Only Android supports ETC1 without embedded alpha channel
			if (texture != null) {
				var alphaTexture = texture.AlphaTexture;
				if (alphaTexture != null) {
					SetTexture(alphaTexture.GetHandle(), stage + 2);
					return;
				}
			}
			SetTexture(0, stage + 2);
#endif
		}

		internal static void SetTexture(uint glTexNum, int stage)
		{
			if (glTexNum == textures[stage])
				return;
			if (stage > 0) {
				GL.ActiveTexture(TextureUnit.Texture0 + stage);
				GL.BindTexture(TextureTarget.Texture2D, glTexNum);
				GL.ActiveTexture(TextureUnit.Texture0);
			} else {
				GL.BindTexture(TextureTarget.Texture2D, glTexNum);
			}
			textures[stage] = glTexNum;
		}

		public static void SetViewport(WindowRect value)
		{
			GL.Viewport(value.X, value.Y, value.Width, value.Height);
		}

		public static void SetScissorRectangle(WindowRect value)
		{
			GL.Scissor(value.X, value.Y, value.Width, value.Height);
		}

		public static void EnableScissorTest(bool value)
		{
			if (value) {
				GL.Enable(EnableCap.ScissorTest);
			} else {
				GL.Disable(EnableCap.ScissorTest);
			}
		}

		public static void SetBlending(Blending value)
		{
			if (value == blending && premultipliedAlphaMode == Renderer.PremultipliedAlphaMode) {
				return;
			}
			premultipliedAlphaMode = Renderer.PremultipliedAlphaMode;
			blending = value;
			switch (blending) {
				case Blending.Inherited:
				case Blending.Alpha:
					var sfactor = Renderer.PremultipliedAlphaMode ? BlendingFactorSrc.One : BlendingFactorSrc.SrcAlpha;
					GL.BlendFunc(sfactor, BlendingFactorDest.OneMinusSrcAlpha);
					break;
				case Blending.Add:
				case Blending.Glow:
					sfactor = Renderer.PremultipliedAlphaMode ? BlendingFactorSrc.One : BlendingFactorSrc.SrcAlpha;
					GL.BlendFunc(sfactor, BlendingFactorDest.One);
					break;
				case Blending.Burn:
					GL.BlendFunc(BlendingFactorSrc.DstColor, BlendingFactorDest.OneMinusSrcAlpha);
					break;
				case Blending.Modulate:
					GL.BlendFunc(BlendingFactorSrc.DstColor, BlendingFactorDest.Zero);
					break;
			}
			CheckErrors();
		}

		public static void BindFramebuffer(uint framebuffer)
		{
			CurrentFramebuffer = framebuffer;
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
		}

		public static void BindIndexBuffer(uint handle)
		{
			if (indexBuffer != handle) {
				indexBuffer = handle;
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, handle);
			}
		}

		public static void BindVertexBuffer(uint handle)
		{
			if (vertexBuffer != handle) {
				vertexBuffer = handle;
				GL.BindBuffer(BufferTarget.ArrayBuffer, handle);
			}
		}
	}
}
#endif