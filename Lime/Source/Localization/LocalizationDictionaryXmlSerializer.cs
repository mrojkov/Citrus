using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Lime
{
	/// <summary>
	/// Serializer that handles xml dictionaries
	/// </summary>
	public class LocalizationDictionaryXmlSerializer : ILocalizationDictionarySerializer
	{
		public string GetFileExtension()
		{
			return ".xml";
		}

		public void Read(LocalizationDictionary dictionary, Stream stream)
		{
			using (var xml = XmlReader.Create(stream)) {
				xml.ReadStartElement("resources");
				while (xml.IsStartElement("string")) {
					var key = xml.GetAttribute("name");
					var isElementEmpty = xml.IsEmptyElement;
					xml.ReadStartElement();
					// Empty elements in English localization mean that source text should be used as-is.
					dictionary.Add(key, new LocalizationEntry() { Text = isElementEmpty ? key : xml.ReadString() });
					if (!isElementEmpty) {
						xml.ReadEndElement();
					}
				}
				xml.ReadEndElement();
			}
		}

		public void Write(LocalizationDictionary dictionary, Stream stream)
		{
			using (var xml = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true })) {
				xml.WriteStartElement("resources");
				foreach (var p in dictionary) {
					xml.WriteStartElement("string");
					xml.WriteAttributeString("name", p.Key);
					if (p.Value.Text.Contains('<')) {
						xml.WriteCData(p.Value.Text);
					} else {
						xml.WriteValue(p.Value.Text);
					}
					xml.WriteEndElement();
				}
				xml.WriteEndElement();
			}
		}
	}
}