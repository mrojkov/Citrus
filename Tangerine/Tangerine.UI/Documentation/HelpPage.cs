using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tangerine.UI
{
	public class HelpPage
	{
		public readonly string PageName;
		public readonly string PageFilepath;
		public readonly string Url;
		
		public HelpPage(string pageName)
		{
			PageName = pageName;
			PageFilepath = Documentation.GetPagePath(pageName);

			string hash = GetPageHash();
			Url = Path.Combine(Documentation.HtmlDocumentationPath, pageName + "_" + hash + ".html");
			if (!File.Exists(Url)) {
				var oldHtml = Directory.GetFiles(Documentation.HtmlDocumentationPath).Where(i => 
					Regex.Match(Path.GetFileName(i), @"(.*)\.html$").Groups[1].Value == pageName
				);
				foreach (var file in oldHtml) {
					File.Delete(file);
				}
				CreateHtml();
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

		private void CreateHtml()
		{
			using (StreamReader sr = new StreamReader(PageFilepath))
			using (StreamWriter sw = new StreamWriter(Url, false, Encoding.UTF8)) {
				sw.WriteLine(
					"<head>" +
					$"<link rel=\"stylesheet\" type=\"text/css\" href=\"{Documentation.StyleSheetPath}\">" +
					"</head>"
				);
				CommonMark.CommonMarkSettings.Default.OutputDelegate =
					(doc, output, settings) =>
					new CustomHtmlFormatter(output, settings).WriteDocument(doc);
				CommonMark.CommonMarkConverter.Convert(sr, sw);
			}
		}
	}
}
