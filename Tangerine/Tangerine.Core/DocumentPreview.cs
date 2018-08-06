using Lime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.Core
{
	public static class DocumentPreview
	{
		static readonly string ScenePreviewSeparator = "{8069CDD4-F02F-4981-A3CB-A0BAD4018D00}";

		public static string ReadAsBase64(string filename)
		{
			var allText = File.ReadAllText(filename);
			var index = allText.IndexOf(ScenePreviewSeparator);
			if (index <= 0) {
				return null;
			}
			var endOfBase64Index = allText.Length - 1;
			while (allText[endOfBase64Index] == 0 || allText[endOfBase64Index] == '\n' || allText[endOfBase64Index] == '\r') {
				endOfBase64Index--;
			}
			int startOfBase64Index = index + ScenePreviewSeparator.Length;
			return allText.Substring(startOfBase64Index, endOfBase64Index - startOfBase64Index + 1);
		}
		public static Texture2D ReadAsTexture2D(string filename)
		{
			var texture = new Texture2D();
			string base64 = ReadAsBase64(filename);
			if (!String.IsNullOrEmpty(base64)) {
				texture.LoadImage(System.Convert.FromBase64String(base64));
			}
			return texture;
		}

		public static void AppendToFile(string filename, string base64)
		{
			if (String.IsNullOrEmpty(base64)) {
				return;
			}
			using(var fs = File.AppendText(filename)) {
				fs.Write(ScenePreviewSeparator);
				fs.Write(base64);
			}
		}
	}
}
