#if ANDROID
using System;
using Android.Content;
using Android;

namespace Lime
{
	public class ClipboardImplementation : IClipboardImplementation
	{
		public string Text
		{
			get
			{
				var clipboard = (Android.Content.ClipboardManager)ActivityDelegate.Instance.Activity.
					GetSystemService(Android.Content.Context.ClipboardService);
				if ((clipboard.HasPrimaryClip) && (clipboard.PrimaryClipDescription.HasMimeType(ClipDescription.MimetypeTextPlain))) {
					return clipboard.PrimaryClip.GetItemAt(0).Text;
				} else {
					return "";
				}
			}
			set
			{
				var clipboard = (Android.Content.ClipboardManager)ActivityDelegate.Instance.Activity.
					GetSystemService(Android.Content.Context.ClipboardService);
				var clip = Android.Content.ClipData.NewPlainText("", value);				
			}	
		}
	}
}
#endif
