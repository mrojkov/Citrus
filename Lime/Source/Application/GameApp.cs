using System;

namespace Lime
{
	[Flags]
	public enum DeviceOrientation
	{
		Portrait = 1,
		PortraitUpsideDown = 2,
		LandscapeLeft = 4,
		LandscapeRight = 8,
	}
	
	public interface IGameWindow
	{
		Size WindowSize { get; set; }
		bool FullScreen { get; set; }
		float FrameRate { get; }
		DeviceOrientation CurrentDeviceOrientation { get; }
		void Exit();
	}

	public class GameApp
	{
		public virtual void OnCreate(IGameWindow gameWindow) {}
		public virtual void OnGLCreate() {}
		public virtual void OnGLDestroy() {}
		public virtual void OnUpdateFrame(double delta) {}
		public virtual void OnRenderFrame() {}
		public virtual void OnDeviceRotated(DeviceOrientation deviceOrientation) {}
		public virtual DeviceOrientation GetSupportedDeviceOrientations() { return DeviceOrientation.LandscapeLeft; }
	}
}