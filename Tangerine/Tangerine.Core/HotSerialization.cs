using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Lime;
using Orange;

namespace Tangerine.Core
{
	public class EditorStateNodeThumbnailProvider : Orange.INodeThumbnailProvider
	{
		public string GetThumbnail(Node node)
		{
			return node.EditorState().ThumbnailData;
		}

		public void SetThumbnail(Node node, string thumbnailData)
		{
			node.EditorState().ThumbnailData = thumbnailData;
		}
	}

	public class HotSceneDeserializer : Yuzu.Deserializer.AbstractReaderDeserializer
	{
		Stream stream;

		public HotSceneDeserializer(Stream stream)
		{
			this.stream = stream;
		}

		public override object FromReaderInt()
		{
			return new HotSceneImporter(true).Import(stream, new Frame(), new EditorStateNodeThumbnailProvider());
		}

		public override object FromReaderInt(object obj)
		{
			return new HotSceneImporter(true).Import(stream, obj as Node, new EditorStateNodeThumbnailProvider());
		}

		public override T FromReaderInt<T>()
		{
			return (T)(object)new HotSceneImporter(true).Import(stream, null, new EditorStateNodeThumbnailProvider());
		}
	}

	public class HotSceneSerializer : Yuzu.AbstractSerializer
	{
		public override void ToWriter(object obj, BinaryWriter writer)
		{
			throw new NotImplementedException();
		}

		public override string ToString(object obj)
		{
			throw new NotImplementedException();
		}

		public override byte[] ToBytes(object obj)
		{
			throw new NotImplementedException();
		}

		public override void ToStream(object obj, Stream target)
		{
			HotSceneExporter hotSceneExporter = new HotSceneExporter();
			try {
				hotSceneExporter.Export(target, (Node)obj, new EditorStateNodeThumbnailProvider());
			} catch (System.Exception e) {
				FieldInfo fieldInfo = e.GetType().GetField("_message", BindingFlags.Instance | BindingFlags.NonPublic);
				if (fieldInfo != null) {
					fieldInfo.SetValue(e, e.Message + " for node " +
						string.Join(
							" in ",
							hotSceneExporter.EnumerateWriteNodesStack()
								.Select(node => "'" + (string.IsNullOrEmpty(node.Id) ? "Root" : node.Id) + "'")
						)
					);
				}
				ExceptionDispatchInfo exceptionDispatchInfo = ExceptionDispatchInfo.Capture(e);
				exceptionDispatchInfo.Throw();
			}
		}
	}

	public class HotFontDeserializer : Yuzu.Deserializer.AbstractReaderDeserializer
	{
		Stream stream;

		public HotFontDeserializer(Stream stream)
		{
			this.stream = stream;
		}

		public override object FromReaderInt()
		{
			return new HotFontImporter(true).ParseFont(stream);
		}

		public override object FromReaderInt(object obj)
		{
			return new HotFontImporter(true).ParseFont(stream);
		}

		public override T FromReaderInt<T>()
		{
			return (T)(object)new HotFontImporter(true).ParseFont(stream);
		}
	}
}
