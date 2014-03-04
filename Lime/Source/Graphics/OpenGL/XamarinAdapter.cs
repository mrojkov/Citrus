#if iOS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.ES20;

namespace Lime
{
	static class VertexAttribPointerType
	{
		public const All Float = All.Float;
		public const All UnsignedByte = All.UnsignedByte;
	}

	static class BeginMode
	{
		public const All Triangles = All.Triangles;
	}

	static class DrawElementsType
	{
		public const All UnsignedShort = All.UnsignedShort;
	}

	static class GetPName
	{
		public const All FramebufferBinding = All.FramebufferBinding;
	}

	static class EnableCap
	{
		public const All Blend = All.Blend;
		public const All ScissorTest = All.ScissorTest;
	}

	static class ClearBufferMask
	{
		public const int ColorBufferBit = (int)All.ColorBufferBit;
	}

	static class TextureUnit
	{
		public const All Texture0 = All.Texture0;
	}

	static class TextureTarget
	{
		public const All Texture2D = All.Texture2D;
	}

	static class GenerateMipmapTarget
	{
		public const All Texture2D = All.Texture2D;
	}

	static class BlendingFactorSrc
	{
		public const All One = All.One;
		public const All SrcAlpha = All.SrcAlpha;
		public const All DstColor = All.DstColor;
	}

	static class BlendingFactorDest
	{
		public const All OneMinusSrcAlpha = All.OneMinusSrcAlpha;
		public const All One = All.One;
		public const All Zero = All.Zero;
	}

	static class TextureParameterName
	{
		public const All TextureMinFilter = All.TextureMinFilter;
		public const All TextureMagFilter = All.TextureMagFilter;
		public const All TextureWrapS = All.TextureWrapS;
		public const All TextureWrapT = All.TextureWrapT;
	}

	static class PixelInternalFormat
	{
		public const int Rgb = (int)All.Rgb;
		public const int Rgba = (int)All.Rgba;
#if !iOS
		public const All CompressedRgbS3tcDxt1Ext = All.CompressedRgbS3tcDxt1Ext;
		public const All CompressedRgbaS3tcDxt3Ext = All.CompressedRgbaS3tcDxt3Ext;
		public const All CompressedRgbaS3tcDxt5Ext = All.CompressedRgbaS3tcDxt5Ext;
#endif
	}

	static class PixelFormat
	{
		public const All Rgb = All.Rgb;
		public const All Rgba = All.Rgba;
	}

	static class PixelType
	{
		public const All UnsignedByte = All.UnsignedByte;
	}

	static class FramebufferTarget
	{
		public const All Framebuffer = All.Framebuffer;
	}

	static class FramebufferAttachment
	{
		public const All ColorAttachment0 = All.ColorAttachment0;
	}

	static class FramebufferErrorCode
	{
		public const All FramebufferComplete = All.FramebufferComplete;
	}

	static class ShaderType
	{
		public const All FragmentShader = All.FragmentShader;
		public const All VertexShader = All.VertexShader;
	}

	static class ShaderParameter
	{
		public const All CompileStatus = All.CompileStatus;
		public const All InfoLogLength = All.InfoLogLength;
	}

	static class GetProgramParameterName
	{
		public const All LinkStatus = All.LinkStatus;
	}
		
	static class ProgramParameter
	{
		public const All InfoLogLength = All.InfoLogLength;
	}

	static class ErrorCode
	{
		public const All NoError = All.NoError;
	}
}
#endif