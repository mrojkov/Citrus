#if !ANDROID && !iOS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Lime
{
	public class VideoPlayer : Image
	{
		public bool Looped { get; set; }
		public VideoPlayer (Widget parentWidget)
		{
			parentWidget.Nodes.Add (this);
			Size = parentWidget.Size;
			Anchors = Anchors.LeftRight | Anchors.TopBottom;
			Texture = new Texture2D();
		}

		public override void Update (float delta)
		{
			base.Update (delta);
		}

		public void InitPlayer (string sourcePath)
		{
		}

		public void Start ()
		{
		}

		public void Pause ()
		{
		}

		public void Stop ()
		{
		}

		public override void Dispose ()
		{
			base.Dispose ();
		}
	}
}
#endif