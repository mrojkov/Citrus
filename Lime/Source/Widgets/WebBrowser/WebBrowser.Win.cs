#if WIN
using System;

namespace Lime
{
	[YuzuDontGenerateDeserializer]
	public class WebBrowser : Widget, IUpdatableNode
	{
		public static Func<Widget, IWebBrowserImplementation> BrowserFactory = widget => new WinFormsWebBrowser(widget);
		private IWebBrowserImplementation implementation;

		public Uri Url
		{
			get { return implementation.Url; }
			set { implementation.Url = value; }
		}

		public WebBrowser()
		{
			Presenter = DefaultPresenter.Instance;
			implementation = BrowserFactory(this);
			Components.Add(new UpdatableNodeBehaviour());
		}

		public WebBrowser(Widget parentWidget)
			: this()
		{
			AddToWidget(parentWidget);
		}

		public void AddToWidget(Widget parentWidget)
		{
			parentWidget.AddNode(this);
			Anchors = Anchors.LeftRightTopBottom;
			Size = parentWidget.Size;
		}

		protected override void OnSizeChanged(Vector2 sizeDelta)
		{
			// OnSizeChanged() is called before WebBrowser constructor, where normal
			// web browser implementation is constructed
			if (implementation != null)
				implementation.OnSizeChanged(sizeDelta);
		}

		public virtual void OnUpdate(float delta)
		{
			implementation.Update(delta);
			implementation.Render();
		}

		public override void Dispose()
		{
			base.Dispose();
			implementation.Dispose();
		}
	}
}
#endif
