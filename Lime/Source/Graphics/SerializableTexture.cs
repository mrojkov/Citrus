using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class SerializableTexture : CommonTexture, ITexture
	{
		private string path;
		private ITexture texture;

		public SerializableTexture(string path)
		{
			this.path = path;
		}

		public SerializableTexture()
			: this("")
		{
		}

		public SerializableTexture(string format, params object[] args)
			: this(string.Format(format, args))
		{
		}

		public string SerializationPath {
			get { return GetSerializationPath(); }
			set { SetSerializationPath(value); }
		}

		private string GetSerializationPath()
		{
			if (!string.IsNullOrEmpty(path) && path[0] == '#') {
				return path;
			} else {
				return Serialization.ShrinkPath(path);
			}
		}

		private void SetSerializationPath(string value)
		{
			path = value;
			if (!string.IsNullOrEmpty(value) && value[0] != '#') {
				path = Serialization.ExpandPath(value);
			}
			texture = null;
		}

		public bool IsStubTexture
		{
			get 
			{
				if (texture == null) {
					texture = LoadTexture();
				}
				return texture.IsStubTexture;
			}
		}

		public Size ImageSize {
			get 
			{
				if (texture == null) {
					texture = LoadTexture();
				}
				return texture.ImageSize; 
			}
		}

		public Size SurfaceSize {
			get 
			{
				if (texture == null) {
					texture = LoadTexture();
				}
				return texture.SurfaceSize; 
			}
		}

		public Rectangle AtlasUVRect {
			get {
				if (texture == null) {
					texture = LoadTexture();
				}
				return texture.AtlasUVRect; 
			}
		}

		public ITexture AlphaTexture 
		{ 
			get 
			{
				if (texture == null) {
					texture = LoadTexture();
				}
				return texture.AlphaTexture; 
			}
		}

		public void TransformUVCoordinatesToAtlasSpace(ref Vector2 uv0, ref Vector2 uv1)
		{
			if (texture == null) {
				texture = LoadTexture();
			}
			texture.TransformUVCoordinatesToAtlasSpace(ref uv0, ref uv1);
		}

		public uint GetHandle()
		{
			if (texture == null) {
				texture = LoadTexture();
			}
			return texture.GetHandle();
		}

		public void Discard() { }

#if UNITY
		public UnityEngine.Texture GetUnityTexture()
		{
			return texture.GetUnityTexture();
		}
#endif

		public void SetAsRenderTarget()
		{
			if (texture == null) {
				texture = LoadTexture();
			}
			texture.SetAsRenderTarget();
		}

		public void RestoreRenderTarget()
		{
			if (texture == null) {
				texture = LoadTexture();
			}
			texture.RestoreRenderTarget();
		}

		public bool IsTransparentPixel(int x, int y)
		{
			if (texture == null) {
				texture = LoadTexture();
			}
			return texture.IsTransparentPixel(x, y);
		}

		public override string ToString()
		{
			return path;
		}

		private ITexture LoadTexture()
		{
			return TexturePool.Instance.GetTexture(path);
		}
	}
}