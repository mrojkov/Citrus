using Lime;
using System;
using System.IO;
using Tangerine.Core.Operations;

namespace Tangerine.Core
{
	public static class DocumentPreview
	{
		const string ScenePreviewSeparator = "{8069CDD4-F02F-4981-A3CB-A0BAD4018D00}";
		const int previewWidth = 150;
		const int previewHeight = 150;

		public static string ReadAsBase64(string filename) => ReadAsBase64Helper(File.ReadAllText(filename));
		public static string ReadAsBase64(Stream stream) => ReadAsBase64Helper((new StreamReader(stream)).ReadToEnd());

		private static string ReadAsBase64Helper(string allText)
		{
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
				texture.LoadImage(Convert.FromBase64String(base64));
			}
			return texture;
		}

		public static void AppendToFile(string filename, string base64)
		{
			if (String.IsNullOrEmpty(base64)) {
				return;
			}
			using(var fs = File.AppendText(filename)) {
				fs.WriteLine(ScenePreviewSeparator);
				fs.WriteLine(base64);
			}
		}

		public static void Generate(CompressionFormat compressionFormat)
		{
			var size = Document.Current.RootNode.AsWidget.Size;
			float newWidth = previewWidth;
			float newHeight = previewHeight;
			if (size.X > size.Y) {
				newHeight *= size.Y / size.X;
			} else {
				newWidth *= size.X / size.Y;
			}
			// Andrey Tyshchenko: Enable animation preview to disable presenters while rendering document preview
			var savedPreviewAnimation = Document.Current.PreviewAnimation;
			Document.Current.PreviewAnimation = true;
			var bitmap = Document.Current.RootNode.AsWidget.ToBitmap().Rescale(newWidth.Round(), newHeight.Round());
			Document.Current.PreviewAnimation = savedPreviewAnimation;
			var stream = new MemoryStream();
			bitmap.SaveTo(stream, compressionFormat);
			SetProperty.Perform(Document.Current, nameof(Document.Preview), Convert.ToBase64String(stream.ToArray()));
		}
	}
}
