using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Lime
{
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
					xml.ReadStartElement();
					var value = xml.ReadString();
					dictionary.Add(key, new LocalizationEntry() { Text = value });
					xml.ReadEndElement();
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
