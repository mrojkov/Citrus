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
#if !iOS
		void Exit();
#endif
	}

	public class GameApp
	{
		public static string GetDataDirectory(string appName, string appVersion = "1.0")
		{
#if iOS
			string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
#else
			string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
#endif
			path = System.IO.Path.Combine(path, appName, appVersion);
			System.IO.Directory.CreateDirectory(path);
			return path;
		}
		public virtual void OnCreate(IGameWindow gameWindow) {}
		public virtual void OnGLCreate() {}
		public virtual void OnGLDestroy() {}
		public virtual void OnUpdateFrame(double delta) {}
		public virtual void OnRenderFrame() {}
		public virtual void OnDeviceRotated(DeviceOrientation deviceOrientation) {}
		public virtual DeviceOrientation GetSupportedDeviceOrientations() { return DeviceOrientation.LandscapeLeft; }
	}
}