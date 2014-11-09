using System;
using System.IO;
#if iOS
using MonoTouch;
using MonoTouch.UIKit;
#endif

namespace Lime
{
	public static class Environment
	{
#if !iOS && !UNITY
		public static void GenerateSerializationAssembly(string assemblyName, params Type[] types)
		{
			var model = ProtoBuf.Meta.TypeModel.Create();
			model.UseImplicitZeroDefaults = false;
			model.Add(typeof(ITexture), true);
			model.Add(typeof(Node), true);
			model.Add(typeof(TextureAtlasElement.Params), true);
			model.Add(typeof(Font), true);
			model.Add(typeof(Bitmap), true);
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
#elif UNITY_WEBPLAYER
			throw new NotImplementedException();
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
			return Path.Combine(GetDataDirectory(appName), path);
		}

		public static string GetDataDirectory(string companyName, string appName, string appVersion)
		{
#if UNITY
			return UnityEngine.Application.persistentDataPath;
#else
#if iOS
			string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
			return path;
#elif MAC
			string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
#else
			string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
#endif
#if !iOS
			if (string.IsNullOrEmpty(companyName)) {
				path = Path.Combine(path, appName, appVersion);
			} else {
				path = Path.Combine(path, companyName, appName, appVersion);
			}
			Directory.CreateDirectory(path);
			return path;
#endif
#endif
		}

		public static string GetDownloadableContentDirectory(string appName)
		{
			return GetDownloadableContentDirectory(null, appName, "1.0");
		}

		public static string GetDownloadableContentDirectory(string companyName, string appName, string appVersion)
		{
#if UNITY
			return UnityEngine.Application.persistentDataPath;
#else
#if iOS
			string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
			path = Path.Combine(Path.GetDirectoryName(path), "Library", "DLC");
#else
			string path = GetDataDirectory(companyName, appName, appVersion);
			path = Path.Combine(path, "DLC");
#endif
			Directory.CreateDirectory(path);
			return path;
#endif
		}

		public static Vector2 GetDesktopSize()
		{
#if iOS
			UIScreen screen = UIScreen.MainScreen;
			return new Vector2(screen.Bounds.Width, screen.Bounds.Height); 
#elif UNITY
			var r = UnityEngine.Screen.currentResolution;
			return new Vector2(r.width, r.height);
#elif ANDROID
			var s = GameView.Instance.Size;
			return new Vector2(s.Width, s.Height);
#else
			var device = OpenTK.DisplayDevice.Default;
			return new Vector2(device.Width, device.Height);
#endif
		}


	}
}
