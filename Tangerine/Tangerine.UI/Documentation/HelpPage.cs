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

		public static string MarkdownDocumentationPath { get; set; } =
			Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, "Documentation");
		public static string HtmlDocumentationPath { get; set; } =
			Lime.Environment.GetPathInsideDataDirectory("Tangerine", "DocumentationCache");
		public static string StartPageName { get; set; } = "StartPage.md";
		public static string ErrorPageName { get; set; } = "ErrorPage.md";
		public static string StyleSheetPath { get; set; } = Path.Combine(MarkdownDocumentationPath, "stylesheet.css");
		
		public HelpPage(string pageName)
		{
			PageName = pageName;
			PageFilepath = Path.Combine(MarkdownDocumentationPath, pageName);

			string hash = GetPageHash();
			Url = Path.Combine(HtmlDocumentationPath, pageName + "_" + hash + ".html");
			if (!File.Exists(Url)) {
				var oldHtml = Directory.GetFiles(HtmlDocumentationPath).Where(i => 
					Path.GetFileName(i).Substring(0, pageName.Length) == pageName
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
					$"<link rel=\"stylesheet\" type=\"text/css\" href=\"{StyleSheetPath}\">" +
					"</head>"
				);
				CommonMark.CommonMarkConverter.Convert(sr, sw);
			}
		}
	}
}
