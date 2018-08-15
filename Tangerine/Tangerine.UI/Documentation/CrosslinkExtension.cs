using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax.Inlines;

namespace Tangerine.UI
{
	public class CrosslinkExtension : IMarkdownExtension
	{
		public void Setup(MarkdownPipelineBuilder pipeline) { }

		public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer) {
			var htmlRenderer = renderer as HtmlRenderer;
			if (htmlRenderer != null) {
				var inlineRenderer = htmlRenderer.ObjectRenderers.FindExact<LinkInlineRenderer>();
				if (inlineRenderer != null) {
					inlineRenderer.TryWriters.Remove(TryLinkInlineRenderer);
					inlineRenderer.TryWriters.Add(TryLinkInlineRenderer);
				}
			}
		}

		private bool TryLinkInlineRenderer(HtmlRenderer renderer, LinkInline linkInline)
		{
			if (
				linkInline.IsImage ||
				linkInline.IsAutoLink ||
				linkInline.Url == null ||
				!linkInline.Url.StartsWith("@")
			) {
				return false;
			}
			linkInline.Url = Documentation.GetDocPath(linkInline.Url.TrimStart('@'));
			renderer.Write(linkInline);
			return true;
		}
	}
}
