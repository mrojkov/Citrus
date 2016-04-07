using System;
using Foundation;
using AppKit;
using Lime;
using Tangerine.Core;
using System.Collections.Generic;

namespace Tangerine
{
	public class AppDelegate : NSApplicationDelegate
	{
		public override void DidFinishLaunching(NSNotification notification)
		{
			CreateWindow();
		}
		
		static private void LoadFont()
		{
			var fontData = new Tangerine.UI.EmbeddedResource("Tangerine.Resources.SegoeUIRegular.ttf", "Tangerine").GetResourceBytes();
			var font = new DynamicFont(fontData);
			FontPool.Instance.AddFont("Default", font);
		}
		
		static void CreateWindow()
		{
			Theme.Current = new Lime.DesktopTheme();
			LoadFont();
			var doc = new Document();
			doc.AddSomeNodes();
			Document.Current = doc;
			var timeline = new Tangerine.UI.Timeline.Timeline();
			doc.History.OnCommit += Window.Current.Invalidate;
			timeline.RegisterDocument(doc);
		}
	}
}