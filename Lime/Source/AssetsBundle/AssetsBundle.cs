using System;
using System.Collections.Generic;
using System.IO;

namespace Lime
{
	[Flags]
	public enum AssetAttributes
	{
		None = 0,

		/// <summary>
		/// Бандл заархивирован
		/// </summary>
		ZippedDeflate = 1 << 0,

		/// <summary>
		/// Бандл содержит текстуры, не равные степени 2
		/// </summary>
		NonPowerOf2Texture = 1 << 1,
		ZippedLZMA = 1 << 2,
		Zipped = ZippedDeflate | ZippedLZMA
	}

	/// <summary>
	/// Бандл. Архив, с игровыми ресурсами
	/// </summary>
	public abstract class AssetsBundle : IDisposable
	{
		private static AssetsBundle instance;

		/// <summary>
		/// Ссылка на текущий бандл
		/// </summary>
		public static AssetsBundle Instance
		{
			get
			{
				if (instance == null) {
					throw new Lime.Exception("AssetsBundle.Instance should be initialized before the usage");
				}
				return instance;
			}
			set
			{
				instance = value;
				// The game could use some of textures from this bundle, and if they are missing
				// we should notify texture pool to search them again.
				TexturePool.Instance.DiscardAllStubTextures();
			}
		}

		/// <summary>
		/// Возвращает true, если Instance не null
		/// </summary>
		public static bool Initialized { get { return instance != null; } }

		public virtual void Dispose()
		{
			if (instance == this) {
				instance = null;
			}
		}

		/// <summary>
		/// Текущий язык (например RU, EN)
		/// </summary>
		public static string CurrentLanguage;

		public abstract Stream OpenFile(string path);

		public byte[] ReadFile(string path)
		{
			using (var stream = OpenFile(path)) {
				using (var memoryStream = new MemoryStream()) {
					stream.CopyTo(memoryStream);
					return memoryStream.ToArray();
				}
			}
		}

		/// <summary>
		/// Возвращает время записи файла (время, когда файл был изменен)
		/// </summary>
		/// <param name="path">Путь к проверяемому файлу в бандле</param>
		public abstract DateTime GetFileLastWriteTime(string path);

		public abstract void DeleteFile(string path);
		public abstract bool FileExists(string path);

		/// <summary>
		/// Импортирует файл в бандл
		/// </summary>
		/// <param name="path">По какому пути разместить файл в бандле. Если такой файл уже есть, он будет удален</param>
		/// <param name="stream">поток импортируемого файла</param>
		/// <param name="reserve">Сколько байт зарезервировать (будет фактически записано 'Длина_Файла + reserve' байт)</param>
		/// <param name="attributes">Атрибуты импортируемого файла</param>
		public abstract void ImportFile(string path, Stream stream, int reserve, string sourceExtension, AssetAttributes attributes = AssetAttributes.None);

		/// <summary>
		/// Перечисляет все файлы, входящие в бандл
		/// </summary>
		public abstract IEnumerable<string> EnumerateFiles();

		/// <summary>
		/// Импортирует файл в бандл
		/// </summary>
		/// <param name="dstPath">По какому пути разместить файл в бандле. Если такой файл уже есть, он будет удален</param>
		/// <param name="srcPath">Импортируемый файл</param>
		/// <param name="reserve">Сколько байт зарезервировать (будет фактически записано 'Длина_Файла + reserve' байт)</param>
		/// <param name="attributes">Атрибуты импортируемого файла</param>
		public void ImportFile(string srcPath, string dstPath, int reserve, string sourceExtension, AssetAttributes attributes = AssetAttributes.None)
		{
			using (var stream = new FileStream(srcPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
				ImportFile(dstPath, stream, reserve, sourceExtension, attributes);
			}
		}

		/// <summary>
		/// Открывает файл, используя локализованный путь (см описание к методу GetLocalizedPath)
		/// </summary>
		public Stream OpenFileLocalized(string path)
		{
			var stream = OpenFile(GetLocalizedPath(path));
			return stream;
		}

		/// <summary>
		/// Возвращает путь с учетом текущего языка (свойство CurrentLanguage). Например при path == "dictionary.txt" вернет dictionary.ru.txt
		/// (при условии, что CurrentLanguage == "ru")
		/// </summary>
		public string GetLocalizedPath(string path)
		{
			if (string.IsNullOrEmpty(CurrentLanguage))
				return path;
			string extension = Path.GetExtension(path);
			string pathWithoutExtension = Path.ChangeExtension(path, null);
			string localizedParth = pathWithoutExtension + "." + CurrentLanguage + extension;
			if (FileExists(localizedParth)) {
				return localizedParth;
			}
			return path;
		}

#if UNITY
		public virtual T LoadUnityAsset<T>(string path) where T : UnityEngine.Object
		{
			throw new NotImplementedException();
		}
#endif

		public virtual AssetAttributes GetAttributes(string path)
		{
			return AssetAttributes.None;
		}

		public virtual void SetAttributes(string path, AssetAttributes attributes)
		{
		}
	}
}
