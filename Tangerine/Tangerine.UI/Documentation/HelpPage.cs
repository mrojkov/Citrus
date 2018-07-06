using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.UI
{
	public class HelpPage
	{
		public readonly string PageName;
		public readonly string PageFilepath;
		public readonly string Url;

		public static string DocumentationDirectory { get; set; }
		public static string UrlDirectory => Lime.Environment.GetPathInsideDataDirectory("Tangerine", "DocumentationCache");
		public static string StartPageName { get; set; } = "StartPage";
		public static string ErrorPageName { get; set; } = "ErrorPage";
		
		public HelpPage(string pageName)
		{
			PageName = pageName;
			PageFilepath = Path.Combine(DocumentationDirectory, pageName);

			string hash = GetPageHash();
			string urlDir = UrlDirectory;
			Url = Path.Combine(urlDir, pageName + "_" + hash + ".html");
			if (!File.Exists(Url)) {
				var oldHtml = Directory.GetFiles(urlDir).Where(i => 
					Path.GetFileName(i).Substring(0, pageName.Length) == pageName
				);
				foreach (var file in oldHtml) {
					File.Delete(file);
				}
				using (StreamReader sr = new StreamReader(PageFilepath))
				using (StreamWriter sw = new StreamWriter(Url)) {
					CommonMark.CommonMarkConverter.Convert(sr, sw);
				}
			}
		}

		private string GetPageHash()
		{
			using (var md5 = MD5.Create())
			using (var stream = File.OpenRead(PageFilepath)) {
				var hashed = md5.ComputeHash(stream);
				return BitConverter.ToInt32(hashed, 0).ToString();
			}
		}
	}
}
