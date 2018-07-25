using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonMark;
using CommonMark.Syntax;

namespace Tangerine.UI
{
	public class CustomHtmlFormatter : CommonMark.Formatters.HtmlFormatter
	{
		private readonly HashSet<string> videoExtensions = new HashSet<string> { ".mp4", ".webm" };
		private readonly HashSet<string> audioExtensions = new HashSet<string> { ".mp3", ".wav" };

		public CustomHtmlFormatter(TextWriter target, CommonMarkSettings settings) : base(target, settings)
		{
		}

		protected override void WriteInline(Inline inline, bool isOpening, bool isClosing, out bool ignoreChildNodes)
		{
			if (inline.Tag == InlineTag.Image && !this.RenderPlainTextInlines.Peek()) {
				ignoreChildNodes = true;
				string videoExtension = videoExtensions.FirstOrDefault((e) => inline.TargetUrl.EndsWith(e));
				string audioExtension = audioExtensions.FirstOrDefault((e) => inline.TargetUrl.EndsWith(e));
				if (videoExtension != default(string)) {
					if (isOpening) {
						Write("<video controls>");
						Write($"<source src=\"{inline.TargetUrl}\" type=\"video/{videoExtension.TrimStart('.')}\"");
						Write("Your browser does not support the video tag.");
						Write("</video>");
					}
				} else if (audioExtension != default(string)) {
					if (isOpening) {
						Write("<audio controls>");
						Write($"<source src=\"{inline.TargetUrl}\" type=\"audio/{audioExtension.TrimStart('.')}\"");
						Write("Your browser does not support the audio tag.");
						Write("</audio>");
					}
				} else {
					base.WriteInline(inline, isOpening, isClosing, out ignoreChildNodes);
				}
			} else {
				base.WriteInline(inline, isOpening, isClosing, out ignoreChildNodes);
			}
		}
	}
}
