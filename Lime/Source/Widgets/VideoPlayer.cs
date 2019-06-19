using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Lime
{
	public enum VideoPlayerStatus
	{
		None,
		Error,
		Playing,
		Paused,
		Finished,
	}

	public class VideoPlayer : Image, IUpdatableNode
	{
		public bool Looped
		{
			get => decoder?.Looped ?? false;
			set {
				if (decoder != null) {
					decoder.Looped = value;
				}
			}
		}
		public bool MuteAudio {
			get => decoder?.MuteAudio ?? false;
			set {
				if (decoder != null) {
					decoder.MuteAudio = value;
				}
			}
		}

		private VideoDecoder decoder;

		public VideoPlayerStatus Status => decoder?.Status ?? VideoPlayerStatus.None;

		public float CurrentPosition
		{
			get => decoder?.CurrentPosition ?? 0f;
			set => decoder.CurrentPosition = value;
		}

		public float Duration => decoder?.Duration ?? 0f;

		public VideoPlayer()
		{
			Anchors = Anchors.LeftRightTopBottom;
			Components.Add(new UpdatableNodeBehavior());
		}

		public VideoPlayer(Widget parentWidget)
		{
			parentWidget.Nodes.Add(this);
			Size = parentWidget.Size;
			Anchors = Anchors.LeftRightTopBottom;
			Components.Add(new UpdatableNodeBehavior());
		}

		public virtual void OnUpdate(float delta)
		{
			if (decoder != null) {
				decoder.Update(delta);
				Texture = decoder.Texture;
			}
		}

		public void InitPlayer(string sourcePath)
		{
			decoder?.Dispose();
			decoder = new VideoDecoder(sourcePath);
		}

		public void Start(Action onStart)
		{
			if (decoder == null) {
				return;
			}
			decoder.OnStart = onStart;
			decoder.Start();
		}

		public void Pause()
		{
			decoder?.Pause();
		}

		public void Stop()
		{
			decoder?.Stop();
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
				Decoder?.UpdateTexture();
				base.Render();
			}

			protected override void OnRelease()
			{
				base.OnRelease();
			}
		}
	}
}
