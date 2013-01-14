using System;
using MonoTouch;
using MonoTouch.UIKit;

namespace Lime
{
	public static class Environment
	{
#if !iOS
		public static void GenerateSerializationAssembly(string assemblyName, params Type[] types)
		{
			var model = ProtoBuf.Meta.TypeModel.Create();
			model.UseImplicitZeroDefaults = false;
			model.Add(typeof(ITexture), true);
			model.Add(typeof(Node), true);
			model.Add(typeof(TextureAtlasPart), true);
			model.Add(typeof(Font), true);
			foreach (var type in types) {
				model.Add(type, true);
			}
			model.Compile(assemblyName, assemblyName + ".dll");
		}
#endif
		
		public static void OpenBrowser(string url)
		{
#if iOS
			var nsUrl = new MonoTouch.Foundation.NSUrl(url);
			MonoTouch.UIKit.UIApplication.SharedApplication.OpenUrl(nsUrl);
#else
			System.Diagnostics.Process.Start(url);
#endif
		}

		public static string GetDataDirectory(string appName)
		{
			return GetDataDirectory(null, appName, "1.0");
		}

		public static string GetPathInsideDataDirectory(string appName, string path)
		{
			return System.IO.Path.Combine(GetDataDirectory(appName), path);
		}

		public static string GetDataDirectory(string companyName, string appName, string appVersion)
		{
#if iOS || MAC
			string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
#else
			string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData);
#endif
			if (string.IsNullOrEmpty(companyName)) {
				path = System.IO.Path.Combine(path, appName, appVersion);
			} else {
				path = System.IO.Path.Combine(path, companyName, appName, appVersion);
			}
			System.IO.Directory.CreateDirectory(path);
			return path;
		}

		public static Vector2 GetDesktopSize()
		{
#if !iOS
			var device = OpenTK.DisplayDevice.Default;
			return new Vector2(device.Width, device.Height);
#else
			UIScreen screen = UIScreen.MainScreen;
			return new Vector2(screen.Bounds.Width, screen.Bounds.Height); 
#endif
		}


	}
}
