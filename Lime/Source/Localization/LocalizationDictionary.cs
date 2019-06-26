using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lime
{
	/// <summary>
	/// Запись словаря локализации
	/// </summary>
	public class LocalizationEntry
	{
		/// <summary>
		/// Текст перевода
		/// </summary>
		public string Text;

		/// <summary>
		/// Контекст. Аналог комментария для переводчика
		/// </summary>
		public string Context;
	}

	/// <summary>
	/// Интерфейс сериалайзера, предоставляющего функции чтения и записи словаря в файл
	/// </summary>
	public interface ILocalizationDictionarySerializer
	{
		string GetFileExtension();
		void Read(LocalizationDictionary dictionary, Stream stream);
		void Write(LocalizationDictionary dictionary, Stream stream);
	}

	/// <summary>
	/// Словарь локализации. Используется для перевода текста на другой язык.
	/// Содержит пары ключ-значение. Строка, заданная в HotStudio является ключом,
	/// если начинается с квадратных скобок []. Словарь подменяет ее на фразу для конкретного языка
	/// </summary>
	public class LocalizationDictionary : IEnumerable<KeyValuePair<string, LocalizationEntry>>
	{
		/// <summary>
		/// Счётчик добавленных комментариев. Нужен чтобы заносить в словарь каждый комментарий с уникальным айди
		/// </summary>
		private int commentsCounter;
		private Dictionary<string, LocalizationEntry> dictionary = new Dictionary<string, LocalizationEntry>();

		/// <summary>
		/// Префикс ключа для комментариев
		/// </summary>
		private const string commentKeyPrefix = "_COMMENT";

		/// <summary>
		/// Получить значение по ключу
		/// </summary>
		public LocalizationEntry GetEntry(string key)
		{
			key = key.Trim();
			LocalizationEntry e;
			if (dictionary.TryGetValue(key, out e)) {
				return e;
			} else {
				e = new LocalizationEntry();
				dictionary.Add(key, e);
				return e;
			}
		}

		/// <summary>
		/// Добавляет новую запись в словарь. Если запись с таким ключом уже есть, заменяет ее
		/// </summary>
		/// <param name="key">Ключ, по которому можно будет получить запись</param>
		/// <param name="text">Текст</param>
		/// <param name="context">Контекст. Аналог комментария для переводчика</param>
		public void Add(string key, string text, string context)
		{
			var e = GetEntry(key);
			e.Text = text;
			e.Context = context;
		}

		/// <summary>
		/// Проверяет не является ли ключ специальным ключом для комментариев
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static bool IsComment(string key)
		{
			return key.StartsWith(commentKeyPrefix);
		}

		/// <summary>
		/// Добавляет в словарь запись комментария
		/// </summary>
		/// <param name="comment">Текст комментария</param>
		public void AddComment(string comment)
		{
			var e = GetEntry(commentKeyPrefix + commentsCounter.ToString());
			e.Context = comment;

			commentsCounter += 1;
		}

		/// <summary>
		/// Получает текст перевода по ключу. Возвращает true в случае успешной операции
		/// </summary>
		/// <param name="key">Ключ</param>
		/// <param name="value">Переменная, в которую будет записан результат</param>
		public bool TryGetText(string key, out string value)
		{
			key = key.Trim();
			value = null;
			LocalizationEntry e;
			if (dictionary.TryGetValue(key, out e)) {
				value = e.Text;
			}
			return value != null;
		}

		public bool ContainsKey(string key) => dictionary.ContainsKey(key.Trim());

		/// <summary>
		/// Загружает словарь из потока
		/// </summary>
		public void ReadFromStream(Stream stream)
		{
			new LocalizationDictionaryTextSerializer().Read(this, stream);
		}

		/// <summary>
		/// Записывает словарь в поток
		/// </summary>
		public void WriteToStream(Stream stream)
		{
			new LocalizationDictionaryTextSerializer().Write(this, stream);
		}

		/// <summary>
		/// Загружает словарь из потока
		/// </summary>
		/// <param name="serializer">Сериалайзер, предоставляющий функции чтения и записи словаря в файл</param>
		public void ReadFromStream(ILocalizationDictionarySerializer serializer, Stream stream)
		{
			serializer.Read(this, stream);
		}

		/// <summary>
		/// Записывает словарь в поток
		/// </summary>
		/// <param name="serializer">Сериалайзер, предоставляющий функции чтения и записи словаря в файл</param>
		public void WriteToStream(ILocalizationDictionarySerializer serializer, Stream stream)
		{
			serializer.Write(this, stream);
		}

		public Dictionary<string, LocalizationEntry>.Enumerator GetEnumerator() => dictionary.GetEnumerator();

		IEnumerator<KeyValuePair<string, LocalizationEntry>> IEnumerable<KeyValuePair<string, LocalizationEntry>>.GetEnumerator() => GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
