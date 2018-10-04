using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Lime
{
	public class VideoPlayer : Image
	{
		public bool Looped { get { return decoder.Looped; } set { decoder.Looped = value; } }

		private VideoDecoder decoder;

		public VideoPlayer()
		{
			Anchors = Anchors.LeftRight | Anchors.TopBottom;
		}

		public VideoPlayer(Widget parentWidget)
		{
			parentWidget.Nodes.Add(this);
			Size = parentWidget.Size;
			Anchors = Anchors.LeftRight | Anchors.TopBottom;
		}

		public override void Update(float delta)
		{
			base.Update(delta);
			decoder.Update(delta);
			Texture = decoder.Texture;
		}

		public void InitPlayer(string sourcePath)
		{
			decoder = new VideoDecoder(sourcePath);
		}

		public IEnumerator<object> Start(Action onStart)
		{
			decoder.OnStart = onStart;
			yield return decoder.Start();
		}

		public void Pause()
		{
			decoder.Pause();
		}

		public void Stop()
		{
			decoder.Stop();
		}

		public override void Dispose()
		{
			if (decoder != null) {
				decoder.Dispose();
				decoder = null;
			}
			Texture = null;
			base.Dispose();
		}

		protected internal override Lime.RenderObject GetRenderObject()
		{
			var renderObject = (RenderObject)GetRenderObject<RenderObject>();
			renderObject.Decoder = decoder;
			return renderObject;
		}

		private class RenderObject : Image.RenderObject
		{
			public VideoDecoder Decoder;

			public override void Render()
			{
				Decoder.UpdateTexture();
				base.Render();
			}

			protected override void OnRelease()
			{
				base.OnRelease();
			}
		}
	}
}
