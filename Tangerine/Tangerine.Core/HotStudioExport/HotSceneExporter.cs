using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Lime;

namespace Orange
{
	public class HotSceneExporter : IDisposable
	{
		public class Serializer : Yuzu.AbstractSerializer
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
				var e = new HotSceneExporter(target);
				e.Write((Node)obj);
			}
		}

		class Writer : IDisposable
		{
			readonly TextWriter tw;

			public int Indent { get; set; }

			public Writer(Stream stream)
			{
				tw = new StreamWriter(stream);
			}

			public void Dispose()
			{
				tw.Dispose();
			}

			public void WriteProperty(string name, string value)
			{
				if (!string.IsNullOrEmpty(value))
					WriteLine($"{name} \"{value}\"");
			}

			public void WriteProperty(string name, int value, int def = 0)
			{
				if (value != def)
					WriteLine($"{name} {value}");
			}

			public void WriteProperty(string name, float value, float def = 0)
			{
				if (value != def)
					WriteLine($"{name} {value:0.00000}");
			}

			public void WriteProperty(string name, Vector2 value, Vector2? def = null)
			{
				if (value != (def.HasValue ? def.Value : Vector2.Zero))
					WriteLine($"{name} [ {value.X:0.00000} {value.Y:0.00000} ]");
			}

			public void WriteProperty(string name, SkinningWeights value)
			{
				if (value != null) {
					WriteLine($"{name} [" +
						$"{value.Bone0.Index} {value.Bone0.Weight:0.00000} " +
						$"{value.Bone1.Index} {value.Bone1.Weight:0.00000} " +
						$"{value.Bone2.Index} {value.Bone2.Weight:0.00000} " +
						$"{value.Bone3.Index} {value.Bone3.Weight:0.00000} ]");
				}
			}


			public void WriteProperty(string name, bool value, bool def = false)
			{
				if (value != def)
					WriteLine($"{name} " + (value ? "true" : "false"));
			}

			public void WriteProperty(string name, Color4 value)
			{
				if (value != Color4.White)
					WriteLine($"{name} 0x{value.A:X2}{value.R:X2}{value.G:X2}{value.B:X2}");
			}

			public void WriteLine(string value)
			{
				for (int i = 0; i < Indent; i++) {
					tw.Write('\t');
				}
				tw.WriteLine(value);
			}
		}

		Writer writer;

		class NodeWriter
		{
			public string ActorClass;
			public Action<Node> Writer;
		}

		Dictionary<Type, NodeWriter> nodeWriters;

		public HotSceneExporter(Stream stream)
		{
			writer = new Writer(stream);
			nodeWriters = new Dictionary<Type, NodeWriter> {
				{ typeof(Node), new NodeWriter { ActorClass = "Hot::Actor", Writer = WriteActorProperties } },
				{ typeof(Widget), new NodeWriter { ActorClass = "Hot::Graphic", Writer = n => WriteGraphicProperties((Widget)n) } },
				{ typeof(Frame), new NodeWriter { ActorClass = "Hot::Scene", Writer = n => WriteSceneProperties((Frame)n) } },
				{ typeof(Image), new NodeWriter { ActorClass = "Hot::Image", Writer = n => WriteImageProperties((Image)n) } },
				{ typeof(Audio), new NodeWriter { ActorClass = "Hot::Audio", Writer = n => WriteAudioProperties((Audio)n) } },
				{ typeof(Button), new NodeWriter { ActorClass = "Hot::Button", Writer = n => WriteButtonProperties((Button)n) } },
			};
		}

		public void Dispose()
		{
			writer.Dispose();
		}

		public void Write(Node node)
		{
			NodeWriter w;
			if (!nodeWriters.TryGetValue(node.GetType(), out w)) {
				throw new InvalidOperationException($"Unknown node type: {node.GetType()}");
			}
			writer.WriteLine($"\"{w.ActorClass}\" {{");
			writer.Indent++;
			w.Writer(node);
			if (node.Nodes.Count > 0) {
				writer.WriteLine("Actors [");
				writer.Indent++;
				foreach (var child in node.Nodes) {
					Write(child);
				}
				writer.Indent--;
				writer.WriteLine("]");
			}
			writer.Indent--;
			writer.WriteLine("}");
		}

		void WriteActorProperties(Node node)
		{
			writer.WriteProperty("Name", node.Id);
			writer.WriteProperty("Source", node.ContentsPath);
			writer.WriteProperty("Attributes", (int)node.TangerineFlags);
		}

		void WriteGraphicProperties(Widget widget)
		{
			WriteActorProperties(widget);
			writer.WriteProperty("Visible", widget.Visible, true);
			writer.WriteProperty("Rotation", widget.Rotation);
			writer.WriteProperty("Position", widget.Position);
			writer.WriteProperty("Scale", widget.Scale, Vector2.One);
			writer.WriteProperty("Pivot", widget.Pivot);
			writer.WriteProperty("Size", widget.Size);
			writer.WriteProperty("Anchors", (int)widget.Anchors);
			writer.WriteProperty("Color", widget.Color);
			writer.WriteProperty("BlendMode", GetHotStudioBlending(widget.Blending, widget.Shader));
			writer.WriteProperty("HitTestMethod", (int)widget.HitTestMethod, ((widget is Image) || (widget is NineGrid)) ? 1 : 0);
			writer.WriteProperty("SkinningWeights", widget.SkinningWeights);
		}

		int GetHotStudioBlending(Blending blending, ShaderId shader)
		{
			if (blending == Blending.Inherited && shader == ShaderId.Inherited) {
				return 0;
			}
			if (shader == ShaderId.Diffuse) {
				switch (blending) {
					case Blending.Add: return 2;
					case Blending.Burn: return 3;
					case Blending.Modulate: return 5;
					case Blending.Opaque: return 8;
				}
			}
			if (blending == Blending.Alpha && shader == ShaderId.Silhuette) {
				return 7;
			}
			return 1;
		}

		void WriteSceneProperties(Frame frame)
		{
			WriteGraphicProperties(frame);
			writer.WriteProperty("RenderTarget", (int)frame.RenderTarget);
		}

		void WriteImageProperties(Image image)
		{
			WriteGraphicProperties(image);
			writer.WriteProperty("TexturePath", TransformPath(image.Texture.SerializationPath + ".png"));
		}

		void WriteAudioProperties(Audio audio)
		{
			WriteActorProperties(audio);
		}

		void WriteButtonProperties(Button button)
		{
			WriteGraphicProperties(button);
		}

		static string TransformPath(string path)
		{
			path = path.Replace("/", "\\\\");
			return path;
		}
	}
}
