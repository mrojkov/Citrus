using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	/// <summary>
	/// Отладочные аргументы командной строки
	/// </summary>
	public static class CommandLineArgs
	{
		/// <summary>
		/// Окно развернуто на весь экран
		/// </summary>
		public static readonly bool MaximizedWindow = CheckFlag("--Maximized");

		/// <summary>
		/// Использовать OpenGL вместо GLES
		/// </summary>
		public static readonly bool OpenGL = CheckFlag("--OpenGL");

		/// <summary>
		/// Ограничить FPS 25 кадрами в секунду
		/// </summary>
		public static readonly bool Limit25FPS = CheckFlag("--Limit25FPS");

		/// <summary>
		/// Стартовать в полноэкранном режиме
		/// </summary>
		public static readonly bool FullscreenMode = CheckFlag("--Fullscreen");

		/// <summary>
		/// Имитировать медленную загрузку файлов из бандла
		/// </summary>
		public static readonly bool SimulateSlowExternalStorage = CheckFlag("--SimulateSlowExternalStorage");

		/// <summary>
		/// Отключить звук полностью
		/// </summary>
		public static readonly bool NoAudio = CheckFlag("--NoAudio");

		/// <summary>
		/// Отключить музыку
		/// </summary>
		public static readonly bool NoMusic = CheckFlag("--NoMusic");

		/// <summary>
		/// Режим отладки
		/// </summary>
		public static readonly bool Debug = CheckFlag("--Debug");

		/// <summary>
		/// Возвращает аргументы командной строки
		/// </summary>
		public static string[] Get()
		{
#if UNITY_WEB
			return new string[] {};
#else
			return System.Environment.GetCommandLineArgs();
#endif
		}

		/// <summary>
		/// Возвращает true, если установлен указанный флаг
		/// </summary>
		public static bool CheckFlag(string name)
		{
			return Array.IndexOf(Get(), name) >= 0;
		}
	}
}
