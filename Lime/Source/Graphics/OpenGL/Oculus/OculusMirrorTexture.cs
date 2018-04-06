#if WIN
using OpenTK.Graphics.OpenGL;
using FramebufferSlot = OpenTK.Graphics.OpenGL.FramebufferAttachment;
using OculusWrap;
using System;

namespace Lime.Oculus
{
	public class OculusMirrorTexture : IDisposable
	{
		private readonly MirrorTexture mirrorTexture;
		private readonly uint fbo;
		private readonly Size size;

		public OculusMirrorTexture(Size size)
		{
			this.size = size;
			uint oldFramebuffer = PlatformRenderer.CurrentFramebuffer;
			OVRTypes.MirrorTextureDesc mirrorTextureDescription = new OVRTypes.MirrorTextureDesc();
			mirrorTextureDescription.Format = OVRTypes.TextureFormat.R8G8B8A8_UNORM_SRGB;
			mirrorTextureDescription.Width = size.Width;
			mirrorTextureDescription.Height = size.Height;
			mirrorTextureDescription.MiscFlags = OVRTypes.TextureMiscFlags.None;

			// Create the texture used to display the rendered result on the computer monitor.
			OVRTypes.Result result;
			result = OvrProvider.Instance.Hmd.CreateMirrorTextureGL(mirrorTextureDescription, out mirrorTexture);
			OvrProvider.Instance.WriteErrorDetails(result, "Failed to create mirror texture.");

			uint texId;
			result = mirrorTexture.GetBufferGL(out texId);
			OvrProvider.Instance.WriteErrorDetails(result, "Failed to retrieve the texture from the created mirror texture buffer.");

			var mirrorFbo = new uint[1];
			GL.GenFramebuffers(1, mirrorFbo);
			fbo = mirrorFbo[0];
			GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, fbo);
			GL.FramebufferTexture2D(FramebufferTarget.ReadFramebuffer, FramebufferSlot.ColorAttachment0, TextureTarget.Texture2D, (int)texId, 0);
			GL.FramebufferRenderbuffer(FramebufferTarget.ReadFramebuffer, FramebufferSlot.DepthAttachment, RenderbufferTarget.Renderbuffer, 0);
			GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
			PlatformRenderer.CheckErrors();
			PlatformRenderer.BindFramebuffer(oldFramebuffer);
			PlatformRenderer.CheckErrors();
		}


		public void CopyTo(uint framebufferId, int width, int height)
		{
			// Copy mirror data from mirror texture provided by OVR to the target framebuffer.
			var oldFramebuffer = PlatformRenderer.CurrentFramebuffer;
			GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, fbo);
			GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, PlatformRenderer.DefaultFramebuffer);

			GL.BlitFramebuffer(
				0, size.Height, size.Width, 0,
				0, 0, width, height,
				ClearBufferMask.ColorBufferBit,
				BlitFramebufferFilter.Nearest);
			PlatformRenderer.BindFramebuffer(oldFramebuffer);
			PlatformRenderer.CheckErrors();
		}

		public void Dispose()
		{
			var handle = fbo;
			Window.Current.InvokeOnRendering(() => {
				GL.DeleteFramebuffers(1, new uint[] { handle });
				PlatformRenderer.CheckErrors();
			});
			mirrorTexture.Dispose();
		}
	}
}
#endif