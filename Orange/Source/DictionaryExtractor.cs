using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Lime;

namespace Orange
{
	public class DictionaryExtractor
	{
		private CitrusProject project;

		public DictionaryExtractor(CitrusProject project)
		{
			this.project = project;
		}
		
		public void ExtractDictionary()
		{
			using (new DirectoryChanger(project.AssetsDirectory)) {
				Console.WriteLine("------------- Localization dictionary update started -------------");
				var files = Helpers.GetAllFiles(".", "*.scene", true);
				foreach (string file in files) {
					Console.WriteLine("* " + file);
					ProcessSceneFile(file);
				}
			}
		}

		string TagString(string str)
		{
			var match = Regex.Match(str, @"^\[(\d*)\](.*)$");
			if (match.Success) {
				if (match.Groups[1].Length > 0) {
					// case of "[123]..."
					int tag = int.Parse(match.Groups[1].Value);
					string value = match.Groups[2].Value;
					if (!Locale.Dictionary.Contains(tag)) {
						Locale.Dictionary.Add(tag, value);
					}
					return str;
				} else {
					// case of "[]..."
					int tag = GenerateTag();
					string value = match.Groups[2].Value;
					Locale.Dictionary.Add(tag, value);
					str = string.Format("[{0}]{1}", tag, value);
					return str;
				}
			} else {
				int tag = GenerateTag();
				Locale.Dictionary.Add(tag, str);
				str = string.Format("[{0}]{1}", tag, str);
				return str;
			}
		}

		int GenerateTag()
		{
			string s;
			for (int tag = 1; ; tag++) {
				if (!Locale.Dictionary.TryGetString(tag, out s))
					return tag;
			}
		}

		void ProcessSceneFile(string file)
		{
			var text = File.ReadAllText(file);
			var s = Regex.Replace(text, @"^(\s*Text)\s""([^""]*)""$", 
				(match) => {
					string prefix = match.Groups[1].Value;
					string str = match.Groups[2].Value;
					str = TagString(str);
					string result = string.Format(@"{0} ""{1}""", prefix, str);
					return result;
				}, RegexOptions.Multiline);
			File.WriteAllText(file + "X", s);
		}
	}
}
