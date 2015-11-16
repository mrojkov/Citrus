using System;
using System.Collections.Generic;
using System.IO;
#if iOS
using MonoTouch;
using UIKit;
#elif ANDROID
using Android.Content;
using Android.App;
#elif MAC
using AppKit;
#endif

namespace Lime
{
	/// <summary>
	/// Предоставляет функции для работы с переменными окружения
	/// </summary>
	public static class Environment
	{
#if !iOS && !UNITY
		/// <summary>
		/// Создает Serializer.dll. Этот метод не должен вызываться в игровом цикле.
		/// Он должен вызываться, только если приложение запущено с параметром --GenerateSerializationAssembly.
		/// После вызова этого метода приложение должно завершить работу.
		/// </summary>
		/// <param name="assemblyName">Должно быть "Serializer" (имя сборки)</param>
		/// <param name="types">Типы, которые нужно включить в Serizlizer.dll</param>
		public static void GenerateSerializationAssembly(string assemblyName, params Type[] types)
		{
			GenerateSerializationAssembly(assemblyName, types, null);
		}

		/// <summary>
		/// Создает Serializer.dll. Этот метод не должен вызываться в игровом цикле.
		/// Он должен вызываться, только если приложение запущено с параметром --GenerateSerializationAssembly.
		/// После вызова этого метода приложение должно завершить работу.
		/// </summary>
		/// <param name="assemblyName">Должно быть "Serializer" (имя сборки)</param>
		/// <param name="types">Типы, которые нужно включить в Serizlizer.dll</param>
		/// <param name="subTypes">(Нужно разобраться что это и написать документацию)</param>
		public static void GenerateSerializationAssembly(string assemblyName, Type[] types, Dictionary<Type, List<KeyValuePair<int, Type>>> subTypes)
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
			if (subTypes != null) {
				foreach (var type in subTypes.Keys) {
					foreach(var subTypePair in subTypes[type]) {
						model[type].AddSubType(subTypePair.Key, subTypePair.Value);
					}
				}
			}
			model.Compile(assemblyName, assemblyName + ".dll");
		}

#endif
		/// <summary>
		/// Открывает веб-браузер как отдельное приложение
		/// </summary>
		public static void OpenBrowser(string url)
		{
#if iOS
			var nsUrl = new Foundation.NSUrl(url);
			UIKit.UIApplication.SharedApplication.OpenUrl(nsUrl);
#elif ANDROID
			var uri = Android.Net.Uri.Parse(url);
			var intent = new Intent(Intent.ActionView, uri);
			ActivityDelegate.Instance.Activity.StartActivity(intent);
#elif UNITY_WEBPLAYER
			throw new NotImplementedException();
#else
			System.Diagnostics.Process.Start(url);
#endif
		}

		/// <summary>
		/// Возвращает папку, где приложение может хранить свою служебную информацию (например для Windows это ApplicationData + имя приложения)
		/// </summary>
		/// <param name="appName">Название приложения (влияет только на название папки)</param>
		public static string GetDataDirectory(string appName)
		{
			return GetDataDirectory(null, appName, "1.0");
		}

		public static string GetPathInsideDataDirectory(string appName, string path)
		{
			return Path.Combine(GetDataDirectory(appName), path);
		}

		/// <summary>
		/// Возвращает папку, где приложение может хранить свою служебную информацию (например для Windows это ApplicationData + имя приложения)
		/// Параметры функции влияют только на название папки
		/// </summary>
		/// <param name="companyName">Название компании-разработчика (можно null)</param>
		/// <param name="appName">Название приложения</param>
		/// <param name="appVersion">Версия приложения</param>
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

		/// <summary>
		/// Возвращает папку, в которую сохраняется контент, загруженный из интернета
		/// </summary>
		/// <param name="appName">Название приложения (влияет только на имя папки)</param>
		public static string GetDownloadableContentDirectory(string appName)
		{
			return GetDownloadableContentDirectory(null, appName, "1.0");
		}

		/// <summary>
		/// Возвращает папку, в которую сохраняется контент, загруженный из интернета.
		/// Параметры функции влияют только на название папки
		/// </summary>
		/// <param name="companyName">Название компании-разработчика (можно null)</param>
		/// <param name="appName">Название приложения</param>
		/// <param name="appVersion">Версия приложения</param>
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

		/// <summary>
		/// Возвращает размер рабочего стола (для десктопа), либо размер экрана (для мобильных устройств)
		/// </summary>
		public static Vector2 GetDesktopSize()
		{
#if iOS
			UIScreen screen = UIScreen.MainScreen;
			return new Vector2((float)screen.Bounds.Width, (float)screen.Bounds.Height); 
#elif UNITY
			var r = UnityEngine.Screen.currentResolution;
			return new Vector2(r.width, r.height);
#elif ANDROID
			var s = ActivityDelegate.Instance.GameView.Size;
			return new Vector2(s.Width, s.Height);
#else
			var device = Lime.Platform.DisplayDevice.Default;
			return new Vector2(device.Width, device.Height);
#endif
		}
	}
}
