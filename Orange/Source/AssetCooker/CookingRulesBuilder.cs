using System;
using System.IO;
using System.Collections.Generic;

namespace Orange
{
	public struct CookingRules
	{
		public string TextureAtlas;
		public bool MipMaps;
		public bool Compression;
		public DateTime LastChangeTime;

		public static CookingRules Default = new CookingRules {
			TextureAtlas = null, MipMaps = false, 
			Compression = true, LastChangeTime = new DateTime(0)};
	}
	
	public class CookingRulesBuilder
	{
		public static Dictionary<string, CookingRules> Build(string assetsDirectory)
		{
			var pathStack = new Stack<string>();
			var rulesStack = new Stack<CookingRules>();
			var map = new Dictionary<string, CookingRules>();
			pathStack.Push("");
			rulesStack.Push(CookingRules.Default);
			using(new DirectoryChanger(assetsDirectory)) {
				var files = Helpers.GetAllFiles(".", "*.*", true);
				foreach (string path in files) {
					if (!path.StartsWith(pathStack.Peek())) {
						rulesStack.Pop();
						pathStack.Pop();
					}
					if (Path.GetFileName(path) == "#CookingRules.txt") {
						pathStack.Push(Path.GetDirectoryName(path));
						rulesStack.Push(ParseCookingRules(rulesStack.Peek(), path));
					} else if (Path.GetExtension(path) != ".txt") {
						var rules = rulesStack.Peek();
						var rulesFile = path + ".txt";
						if (File.Exists(rulesFile)) {
							rules = ParseCookingRules(rulesStack.Peek(), rulesFile);
						}
						if (rules.LastChangeTime > File.GetLastWriteTime(path)) {
							File.SetLastWriteTime(path, rules.LastChangeTime);
						}
						map[path] = rules;
					}
				}
			}
			return map;
		}
		
		static bool ParseBool(string value)
		{
			if (value != "Yes" && value != "No")
				throw new Lime.Exception("Invalid value. Must be either 'Yes' or 'No'");
			return value == "Yes";	
		}	
		
		static CookingRules ParseCookingRules(CookingRules basicRules, string path)
		{
			var rules = basicRules;
			try {
				rules.LastChangeTime = File.GetLastWriteTime(path);
				using(var s = new FileStream(path, FileMode.Open)) {
					TextReader r = new StreamReader(s);
					string line;
	 				while ((line = r.ReadLine()) != null) {
						line = line.Trim();
						if (line == "") {
							continue;
						}
						var words = line.Split(' ');
						if (words.Length != 2) {
							throw new Lime.Exception("Invalid line format");
						}
						switch(words[0])
						{
						case "TextureAtlas":
							if (words[1] == "None")
								rules.TextureAtlas = null;
							else
								rules.TextureAtlas = words[1];
							break;
						case "MipMaps":
							rules.MipMaps = ParseBool(words[1]);
							break;
						case "Compression":
							rules.Compression = ParseBool(words[1]);
							break;
						default:
							throw new Lime.Exception("Unknown attribute {0}", words[0]);
						}
					}
				}
			} catch (Lime.Exception e) {
				throw new Lime.Exception("Syntax error in {0}: {1}", path, e.Message);
			}
			return rules;
		}
	}
}

