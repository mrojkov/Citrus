using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lime
{
	public class LocalizationEntry
	{
		public string Text;
		public string Context;
	}

	public interface ILocalizationDictionarySerializer
	{
		string GetFileExtension();
		void Read(LocalizationDictionary dictionary, Stream stream);
		void Write(LocalizationDictionary dictionary, Stream stream);
	}

	public class LocalizationDictionary : Dictionary<string, LocalizationEntry>
	{
		public LocalizationEntry GetEntry(string key)
		{
			LocalizationEntry e;
			if (TryGetValue(key, out e)) {
				return e;
			} else {
				e = new LocalizationEntry();
				Add(key, e);
				return e;
			}
		}

		public void Add(string key, string text, string context)
		{
			var e = GetEntry(key);
			e.Text = text;
			e.Context = context;
		}

		public bool TryGetText(string key, out string value)
		{
			value = null;
			LocalizationEntry e;
			if (TryGetValue(key, out e)) {
				value = e.Text;
			}
			return value != null;
		}

		public void ReadFromStream(Stream stream)
		{
			new LocalizationDictionaryTextSerializer().Read(this, stream);
		}

		public void WriteToStream(Stream stream)
		{
			new LocalizationDictionaryTextSerializer().Write(this, stream);
		}

		public void ReadFromStream(ILocalizationDictionarySerializer serializer, Stream stream)
		{
			serializer.Read(this, stream);
		}

		public void WriteToStream(ILocalizationDictionarySerializer serializer, Stream stream)
		{
			serializer.Write(this, stream);
		}
	}
}
