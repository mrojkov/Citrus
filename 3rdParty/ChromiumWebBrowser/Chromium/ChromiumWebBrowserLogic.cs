using System;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Internals;
using Lime;
using Exception = Lime.Exception;

namespace ChromiumWebBrowser
{
	// TODO: Extract ILimeWebBrowser to support other implementations
	/// <summary>
	///		An offscreen instance of Chromium that you can use to render
	///		webpages or evaluate JavaScript.
	/// </summary>
	public class ChromiumWebBrowserLogic : IRenderWebBrowser
	{
		/// <summary>
		///		Object that contains info about last taken snapshot.
		/// </summary>
		public BitmapInfo BitmapInfo;

		/// <summary>
		///		Chromium binder.
		///		Examples of usage: https://github.com/cefsharp/CefSharp
		/// </summary>
		private ManagedCefBrowserAdapter managedCefBrowserAdapter;

		/// <summary>
		///		Size of the Chromium viewport.
		///		This must be set to something other than 0x0 otherwise Chromium will not render.
		/// </summary>
		private Size size;

		static ChromiumWebBrowserLogic()
		{
			Cef.Initialize();
			Application.Instance.Terminating += () => {
				if (Cef.IsInitialized) {
					Cef.Shutdown();
				}
			};
		}

		/// <summary>
		///		Create a new OffScreen Chromium Browser.
		/// </summary>
		/// <param name="height">Height of browser</param>
		/// <param name="address">Initial address (url) to load</param>
		/// <param name="browserSettings">The browser settings to use. If null, the default settings are used.</param>
		/// <param name="width">Width of browser</param>
		public ChromiumWebBrowserLogic(int width = 1366, int height = 768, string address = "",
			BrowserSettings browserSettings = null)
		{
			if (!Cef.IsInitialized && !Cef.Initialize()) {
				throw new InvalidOperationException("Cef::Initialize() failed");
			}

			size = new Size(width, height);
			ResourceHandlerFactory = new DefaultResourceHandlerFactory();
			BrowserSettings = browserSettings ?? new BrowserSettings();

			Cef.AddDisposable(this);
			Address = address;

			managedCefBrowserAdapter = new ManagedCefBrowserAdapter(this, true);
			managedCefBrowserAdapter.CreateOffscreenBrowser(IntPtr.Zero, BrowserSettings, address);
		}

		public BrowserSettings BrowserSettings { get; private set; }

		/// <summary>
		///     Get/set the size of the Chromium viewport, in pixels.
		///     This also changes the size of the next screenshot.
		/// </summary>
		public Size Size
		{
			get { return size; }
			set
			{
				if (size != value) {
					size = value;
					managedCefBrowserAdapter.WasResized();
				}
			}
		}

		public bool IsBrowserInitialized { get; private set; }
		public bool IsLoading { get; set; }
		public string Title { get; set; }
		public string TooltipText { get; set; }
		public double ZoomLevel { get; set; }
		public bool CanReload { get; private set; }
		public string Address { get; private set; }
		public bool CanGoBack { get; private set; }
		public bool CanGoForward { get; private set; }
		public IJsDialogHandler JsDialogHandler { get; set; }
		public IDialogHandler DialogHandler { get; set; }
		public IDownloadHandler DownloadHandler { get; set; }
		public IKeyboardHandler KeyboardHandler { get; set; }
		public ILifeSpanHandler LifeSpanHandler { get; set; }

		// TODO : Implement context menu
		// At this moment there is no default CefSharp implementation
		// of context menu, so it needs to be implemented manually (with additional Widget, I guess).
		// It can be done via creating new class that implements IMenuHandler and assigning it to
		// this property.
		// https://github.com/cefsharp/CefSharp/issues/943#issuecomment-92788784
		public IMenuHandler MenuHandler { get; set; }

		public IFocusHandler FocusHandler { get; set; }
		public IRequestHandler RequestHandler { get; set; }
		public IDragHandler DragHandler { get; set; }
		public IResourceHandlerFactory ResourceHandlerFactory { get; set; }
		public IGeolocationHandler GeolocationHandler { get; set; }
		public event EventHandler<LoadErrorEventArgs> LoadError;
		public event EventHandler<NavStateChangedEventArgs> NavStateChanged;
		public event EventHandler<FrameLoadStartEventArgs> FrameLoadStart;
		public event EventHandler<FrameLoadEndEventArgs> FrameLoadEnd;
		public event EventHandler<ConsoleMessageEventArgs> ConsoleMessage;
		public event EventHandler<StatusMessageEventArgs> StatusMessage;
		public event EventHandler<AddressChangedEventArgs> AddressChanged;
		public event EventHandler BrowserInitialized;

		/// <summary>
		///     Fired by a separate thread when Chrome has re-rendered.
		/// </summary>
		public event EventHandler NewScreenshot;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}


		// TODO: Implement better way to pass url if !IsBrowserInitialized
		public void Load(string url)
		{
			Address = url;
			if (IsBrowserInitialized) {
				managedCefBrowserAdapter.LoadUrl(url);
			}
			else {
				BrowserInitialized += (sender, args) => { Load(url); };
			}
		}

		// TODO: Implement better way to pass uri if !IsBrowserInitialized
		public void LoadHtml(string html, string url)
		{
			Address = url;
			if (IsBrowserInitialized) {
				managedCefBrowserAdapter.LoadHtml(html, url);
			}
			else {
				BrowserInitialized += (sender, args) => { LoadHtml(html, url); };
			}
		}

		// TODO: Implement better way to pass url if !IsBrowserInitialized
		public void LoadHtml(string html, string url, Encoding encoding)
		{
			Address = url;
			if (IsBrowserInitialized)
			{
				managedCefBrowserAdapter.LoadHtml(string.Format(html, encoding), url);
			}
			else
			{
				BrowserInitialized += (sender, args) => { LoadHtml(html, url, encoding); };
			}
		}

		public void SendMouseWheelEvent(int x, int y, int deltaX, int deltaY)
		{
			managedCefBrowserAdapter.OnMouseWheel(x, y, deltaX, deltaY);
		}

		public void SendMouseClick(int x, int y, int button, bool mouseUp, CefEventFlags modifiers = CefEventFlags.None, int clickCount = 1)
		{
			managedCefBrowserAdapter.OnMouseButton(x, y, button, mouseUp, clickCount, modifiers);
			managedCefBrowserAdapter.SendFocusEvent(true);
		}

		public void SendMouseMove(int x, int y, bool mouseLeave, CefEventFlags modifiers = CefEventFlags.None)
		{
			managedCefBrowserAdapter.OnMouseMove(x, y, mouseLeave, modifiers);
		}

		public void SendKeyPress(int message, int wParam, CefEventFlags modifiers = CefEventFlags.None)
		{
			managedCefBrowserAdapter.SendKeyEvent(message, wParam, (int)modifiers);
		}

		public void RegisterJsObject(string name, object objectToBind, bool camelCaseJavascriptNames = true)
		{
			if (IsBrowserInitialized) {
				throw new Exception("Browser is already initialized. RegisterJsObject must be" +
									"called before the underlying CEF browser is created.");
			}
			managedCefBrowserAdapter.RegisterJsObject(name, objectToBind, camelCaseJavascriptNames);
		}

		public void ExecuteScriptAsync(string script)
		{
			managedCefBrowserAdapter.ExecuteScriptAsync(script);
		}

		public Task<JavascriptResponse> EvaluateScriptAsync(string script, TimeSpan? timeout = null)
		{
			return managedCefBrowserAdapter.EvaluateScriptAsync(script, timeout);
		}

		public void Find(int identifier, string searchText, bool forward, bool matchCase, bool findNext)
		{
			managedCefBrowserAdapter.Find(identifier, searchText, forward, matchCase, findNext);
		}

		public void StopFinding(bool clearSelection)
		{
			managedCefBrowserAdapter.StopFinding(clearSelection);
		}

		public void Back()
		{
			managedCefBrowserAdapter.GoBack();
		}

		public void Forward()
		{
			managedCefBrowserAdapter.GoForward();
		}

		public void Stop()
		{
			managedCefBrowserAdapter.Stop();
		}

		public Task<string> GetSourceAsync()
		{
			throw new NotImplementedException();
		}

		public Task<string> GetTextAsync()
		{
			throw new NotImplementedException();
		}

		public void ViewSource()
		{
			managedCefBrowserAdapter.ViewSource();
		}

		bool IWebBrowser.Focus()
		{
			return true;
		}

		public void Reload()
		{
			managedCefBrowserAdapter.Reload();
		}

		public void Reload(bool ignoreCache)
		{
			managedCefBrowserAdapter.Reload(ignoreCache);
		}

		public void Print()
		{
			managedCefBrowserAdapter.Print();
		}

		public void ShowDevTools()
		{
			managedCefBrowserAdapter.ShowDevTools();
		}

		public void CloseDevTools()
		{
			managedCefBrowserAdapter.CloseDevTools();
		}

		public void ReplaceMisspelling(string word)
		{
			managedCefBrowserAdapter.ReplaceMisspelling(word);
		}

		public void AddWordToDictionary(string word)
		{
			managedCefBrowserAdapter.AddWordToDictionary(word);
		}

		ScreenInfo IRenderWebBrowser.GetScreenInfo()
		{
			return new ScreenInfo {
				Width = size.Width,
				Height = size.Height,
				ScaleFactor = 1.0F
			};
		}

		BitmapInfo IRenderWebBrowser.CreateBitmapInfo(bool isPopup)
		{
			return new GdiBitmapInfo {IsPopup = isPopup};
		}

		/// <summary>
		///     Invoked from CefRenderHandler.OnPaint
		///     Locking provided by OnPaint as this method is called in it's lock scope
		/// </summary>
		/// <param name="bitmapInfo">information about the bitmap to be rendered</param>
		void IRenderWebBrowser.InvokeRenderAsync(BitmapInfo bitmapInfo)
		{
			BitmapInfo = bitmapInfo;

			var handler = NewScreenshot;
			if (handler != null) {
				handler(this, EventArgs.Empty);
			}
		}

		// TODO: Deal with it
		// There are two ways to deal with it:
		// 1. Find a way to handle with the handle (haha, get it?)
		// Tip: it has type of HCURSOR in WinAPI
		// 2. Implement other cursor types (maybe a bad idea)
		void IRenderWebBrowser.SetCursor(IntPtr handle, CefCursorType type)
		{
			switch (type) {
				case CefCursorType.Pointer: {
					GameView.Instance.SetDefaultCursor();
					break;
				}
				case CefCursorType.IBeam: {
					GameView.Instance.SetCursor("Cursors.IBeam.png", new IntVector2(6, 8), "ChromiumWebBrowser");
					break;
				}
				case CefCursorType.Hand: {
					GameView.Instance.SetCursor("Cursors.Hand.png", new IntVector2(7, 2), "ChromiumWebBrowser");
					break;
				}
			}
		}

		void IRenderWebBrowser.SetPopupIsOpen(bool show)
		{
			throw new NotImplementedException();
		}

		void IRenderWebBrowser.SetPopupSizeAndPosition(int width, int height, int x, int y)
		{
			throw new NotImplementedException();
		}

		public void OnFrameLoadEnd(string url, bool isMainFrame, int httpStatusCode)
		{
			var handler = FrameLoadEnd;
			if (handler != null) {
				handler(this, new FrameLoadEndEventArgs(url, isMainFrame, httpStatusCode));
			}
		}

		void IWebBrowserInternal.OnConsoleMessage(string message, string source, int line)
		{
			var handler = ConsoleMessage;
			if (handler != null) {
				handler(this, new ConsoleMessageEventArgs(message, source, line));
			}
		}

		void IWebBrowserInternal.OnStatusMessage(string value)
		{
			var handler = StatusMessage;
			if (handler != null) {
				handler(this, new StatusMessageEventArgs(value));
			}
		}

		public void OnLoadError(string url, CefErrorCode errorCode, string errorText)
		{
			var handler = LoadError;
			if (handler != null) {
				handler(this, new LoadErrorEventArgs(url, errorCode, errorText));
			}
		}

		public void OnInitialized()
		{
			IsBrowserInitialized = true;
			var handler = BrowserInitialized;
			if (handler != null) {
				handler(this, new EventArgs());
			}
		}

		void IWebBrowserInternal.SetAddress(string address)
		{
			Address = address;
			var handler = AddressChanged;
			if (handler != null) {
				handler(this, new AddressChangedEventArgs(address));
			}
		}

		void IWebBrowserInternal.SetLoadingStateChange(bool canGoBack, bool canGoForward, bool isLoading)
		{
			CanGoBack = canGoBack;
			CanGoForward = canGoForward;
			CanReload = !isLoading;
			IsLoading = isLoading;
		}

		void IWebBrowserInternal.SetTitle(string title)
		{
			Title = title;
		}

		void IWebBrowserInternal.SetTooltipText(string tooltipText)
		{
			TooltipText = tooltipText;
		}

		public void OnFrameLoadStart(string url, bool isMainFrame)
		{
			var handler = FrameLoadStart;
			if (handler != null) {
				handler(this, new FrameLoadStartEventArgs(url, isMainFrame));
			}
		}

		~ChromiumWebBrowserLogic()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			ClearHandlers();
			ClearEventListeners();

			Cef.RemoveDisposable(this);

			if (!disposing) {
				return;
			}

			IsBrowserInitialized = false;

			if (BrowserSettings != null) {
				BrowserSettings.Dispose();
				BrowserSettings = null;
			}

			if (managedCefBrowserAdapter != null) {
				if (!managedCefBrowserAdapter.IsDisposed) {
					managedCefBrowserAdapter.Dispose();
				}
				managedCefBrowserAdapter = null;
			}
		}

		private void ClearEventListeners()
		{
			LoadError = null;
			FrameLoadStart = null;
			FrameLoadEnd = null;
			ConsoleMessage = null;
			BrowserInitialized = null;
			StatusMessage = null;
			AddressChanged = null;
		}

		private void ClearHandlers()
		{
			ResourceHandlerFactory = null;
			JsDialogHandler = null;
			DialogHandler = null;
			DownloadHandler = null;
			KeyboardHandler = null;
			LifeSpanHandler = null;
			MenuHandler = null;
			FocusHandler = null;
			RequestHandler = null;
			DragHandler = null;
			GeolocationHandler = null;
		}
	}
}