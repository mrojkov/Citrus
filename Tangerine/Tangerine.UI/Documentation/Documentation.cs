using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public static class Documentation
	{
		public static bool IsHelpModeOn { get; set; } = false;

		public static string MarkdownDocumentationPath { get; set; } =
			Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, "Documentation");
		public static string HtmlDocumentationPath { get; set; } =
			Lime.Environment.GetPathInsideDataDirectory("Tangerine", "DocumentationCache");
		public static string StyleSheetPath { get; set; } = Path.Combine(MarkdownDocumentationPath, "stylesheet.css");

		public static string PageExtension { get; set; } = ".md";
		public static string StartPageName { get; set; } = "StartPage";
		public static string ErrorPageName { get; set; } = "ErrorPage";

		public static HelpPage StartPage => new HelpPage(StartPageName);
		public static HelpPage ErrorPage => new HelpPage(ErrorPageName);

		public static string GetPagePath(string pageName)
		{
			string path = Path.Combine(MarkdownDocumentationPath, Path.Combine(pageName.Split('.')));
			return path + PageExtension;
		}

		public static void Init()
		{
			if (!Directory.Exists(HtmlDocumentationPath)) {
				Directory.CreateDirectory(HtmlDocumentationPath);
			}
			if (!Directory.Exists(MarkdownDocumentationPath)) {
				Directory.CreateDirectory(MarkdownDocumentationPath);
			}
			string startPagePath = GetPagePath(StartPageName);
			if (!File.Exists(startPagePath)) {
				File.WriteAllText(startPagePath, "# Start page #");
			}
			string errorPagePath = GetPagePath(ErrorPageName);
			if (!File.Exists(errorPagePath)) {
				File.WriteAllText(errorPagePath, "# Error #\nThis is error page");
			}
		}
	}
}
