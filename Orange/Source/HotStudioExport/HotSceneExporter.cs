using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Lime;

namespace Orange
{
	public interface INodeThumbnailProvider
	{
		string GetThumbnail(Node node);
		void SetThumbnail(Node node, string thumbnailData);
	}

	public class FolderBegin : Node
	{
		public bool Expanded { get; set; }

		public FolderBegin()
		{
			Expanded = true;
		}

		public override void AddToRenderChain(RenderChain chain) { }
	}

	public class FolderEnd : Node
	{
		public override void AddToRenderChain(RenderChain chain) { }
	}

	public class HotSceneExporter
	{
		static string ObjectToString(object value)
		{
			if (value is ITexture) {
				var path = ((ITexture)value).SerializationPath;
				value = !path.StartsWith("#") ? RestorePath(path, ".png") : path;
			}
			if (value is SerializableSample) {
				value = RestorePath(((SerializableSample)value).SerializationPath, ".ogg");
			}
			if (value is SerializableFont) {
				value = ((SerializableFont)value).Name;
			}
			if (value is string) {
				value = ((string)value).Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\"", "\\\"").Replace("\'", "\\\'");
				return $"\"{value}\"";
			}
			if (
				value is int || value is AudioAction || value is MovieAction ||
				value is HAlignment || value is VAlignment || value is EmitterShape ||
				value is EmissionType
			) {
				return ((int)value).ToString();
			}
			if (value is float) {
				return FloatToString((float)value);
			}
			if (value is bool) {
				return ((bool)value) ? "true" : "false";
			}
			if (value is Vector2) {
				var v = (Vector2)value;
				return $"[ {FloatToString(v.X)} {FloatToString(v.Y)} ]";
			}
			if (value is NumericRange) {
				var v = (NumericRange)value;
				return $"[ {FloatToString(v.Median)} {FloatToString(v.Dispersion)} ]";
			}
			if (value is SkinningWeights) {
				var v = (SkinningWeights)value;
				return
					$"[ {v.Bone0.Index} {FloatToString(v.Bone0.Weight)} " +
					$"{v.Bone1.Index} {FloatToString(v.Bone1.Weight)} " +
					$"{v.Bone2.Index} {FloatToString(v.Bone2.Weight)} " +
					$"{v.Bone3.Index} {FloatToString(v.Bone3.Weight)} ]";
			}
			if (value is Color4) {
				var v = (Color4)value;
				return $"0x{v.A:X2}{v.R:X2}{v.G:X2}{v.B:X2}";
			}
			throw new ArgumentException();
		}

		static string FloatToString(float value)
		{
			// Returns negative zero for the lesser diff with the imported scene.
			return value == -float.Epsilon ? "-0.00000" : $"{((double)value):0.00000}";
		}

		class Writer
		{
			readonly TextWriter tw;

			public int Indent { get; set; }

			public Writer(TextWriter target)
			{
				tw = target;
				tw.NewLine = "\n";
			}

			public void WriteProperty(string name, object value, object def)
			{
				if (
					(value is ITexture) && string.IsNullOrEmpty((value as ITexture).SerializationPath) ||
					(value is SerializableSample) && string.IsNullOrEmpty((value as SerializableSample).SerializationPath) ||
					(value is SerializableFont) && string.IsNullOrEmpty((value as SerializableFont).Name) ||
					value == null || value.Equals(def)
				) {
					return;
				}
				WriteLine(name + ' ' + ObjectToString(value));
			}

			public void WriteLine(string value)
			{
				for (int i = 0; i < Indent; i++) {
					tw.Write('\t');
				}
				tw.WriteLine(value);
			}

			public void BeginCollection(string name)
			{
				WriteLine($"{name} [");
				Indent++;
			}

			public void EndCollection()
			{
				Indent--;
				WriteLine("]");
			}

			public void BeginStruct(string name)
			{
				WriteLine($"\"{name}\" {{");
				Indent++;
			}

			public void EndStruct()
			{
				Indent--;
				WriteLine("}");
			}
		}

		Writer writer;

		class NodeWriter
		{
			public string ActorClass;
			public Action<Node> Writer;
		}

		Dictionary<Type, NodeWriter> nodeWriters;

		/// <summary>
		/// Serves to understand on which node Exception has happened.
		/// </summary>
		private Stack<Node> writeNodesStack = new Stack<Node>();

		public HotSceneExporter()
		{
			nodeWriters = new Dictionary<Type, NodeWriter> {
				{ typeof(Node), new NodeWriter { ActorClass = "Hot::Actor", Writer = WriteNodeProperties } },
				{ typeof(Widget), new NodeWriter { ActorClass = "Hot::Graphic", Writer = n => WriteWidgetProperties((Widget)n) } },
				{ typeof(Frame), new NodeWriter { ActorClass = "Hot::Scene", Writer = n => WriteFrameProperties((Frame)n) } },
				{ typeof(Image), new NodeWriter { ActorClass = "Hot::Image", Writer = n => WriteImageProperties((Image)n) } },
				{ typeof(Audio), new NodeWriter { ActorClass = "Hot::Audio", Writer = n => WriteAudioProperties((Audio)n) } },
				{ typeof(Button), new NodeWriter { ActorClass = "Hot::Button", Writer = n => WriteButtonProperties((Button)n) } },
				{ typeof(ImageCombiner), new NodeWriter { ActorClass = "Hot::MaskedEffect", Writer = n => WriteImageCombinerProperties((ImageCombiner)n) } },
				{ typeof(ParticleEmitter), new NodeWriter { ActorClass = "Hot::ParticleEmitter2", Writer = n => WriteParticleEmitterProperties((ParticleEmitter)n) } },
				{ typeof(ParticleModifier), new NodeWriter { ActorClass = "Hot::ParticleTemplate", Writer = n => WriteParticleModifierProperties((ParticleModifier)n) } },
				{ typeof(EmitterShapePoint), new NodeWriter { ActorClass = "Hot::EmitterShapePoint", Writer = n => WritePointObjectProperties((EmitterShapePoint)n) } },
				{ typeof(ParticlesMagnet), new NodeWriter { ActorClass = "Hot::ParticlesMagnet", Writer = n => WriteParticlesMagnetProperties((ParticlesMagnet)n) } },
				{ typeof(SplineGear), new NodeWriter { ActorClass = "Hot::Gear", Writer = n => WriteSplineGearProperties((SplineGear)n) } },
				{ typeof(Spline), new NodeWriter { ActorClass = "Hot::Spline", Writer = n => WriteWidgetProperties((Spline)n) } },
				{ typeof(SplinePoint), new NodeWriter { ActorClass = "Hot::SplinePoint", Writer = n => WriteSplinePointProperties((SplinePoint)n) } },
				{ typeof(DistortionMesh), new NodeWriter { ActorClass = "Hot::DistortionMesh", Writer = n => WriteDistortionMeshProperties((DistortionMesh)n) } },
				{ typeof(Slider), new NodeWriter {ActorClass = "Hot::Slider", Writer = n => WriteSliderProperties((Slider)n) } },
				{ typeof(DistortionMeshPoint), new NodeWriter { ActorClass = "Hot::MeshPoint", Writer = n => WriteDistortionMeshPointProperties((DistortionMeshPoint)n) } },
				{ typeof(PointObject), new NodeWriter { ActorClass = "Hot::PointObject", Writer = null } },
				{ typeof(SimpleText), new NodeWriter { ActorClass = "Hot::Text", Writer = n => WriteSimpleTextProperties((SimpleText)n) } },
				{ typeof(RichText), new NodeWriter { ActorClass = "Hot::RichText", Writer = n => WriteRichTextProperties((RichText)n) } },
				{ typeof(TextStyle), new NodeWriter { ActorClass = "Hot::TextStyle", Writer = n => WriteTextStyleProperties((TextStyle)n) } },
				{ typeof(NineGrid), new NodeWriter { ActorClass = "Hot::NineGrid", Writer = n => WriteNineGridProperties((NineGrid)n) } },
				{ typeof(LinearLayout), new NodeWriter { ActorClass = "LinearLayout", Writer = n => WriteLinearLayoutProperties((LinearLayout)n) } },
				{ typeof(FolderBegin), new NodeWriter { ActorClass = "Hot::FolderBegin", Writer = n => WriteFolderBeginProperties((FolderBegin)n) } },
				{ typeof(FolderEnd), new NodeWriter { ActorClass = "Hot::FolderEnd", Writer = n => WriteNodeProperties(n) } },
				{ typeof(Bone), new NodeWriter { ActorClass = "Hot::Bone", Writer = n => WriteBoneProperties((Bone)n) } },
			};
		}

		private void WritePointObjectProperties(PointObject node)
		{
			WriteNodeProperties(node);
			WriteProperty("Anchor", node.Position, Vector2.Zero);
			WriteProperty("SkinningWeights", node.SkinningWeights, new SkinningWeights());
		}

		private void WriteSliderProperties(Slider slider)
		{
			WriteWidgetProperties(slider);
			WriteProperty("RangeMin", slider.RangeMin, 0);
			WriteProperty("RangeMax", slider.RangeMax, 100);
			WriteProperty("Value", slider.Value, 0);
			WriteProperty("Step", slider.Step, 0);
		}

		private void ReorderBonesRecursive(Widget widget)
		{
			if (widget == null) return;
			var allBones = widget.Nodes.OfType<Bone>();
			foreach (var root in allBones.Where(b => b.BaseIndex == 0)) {
				var bones = BoneUtils.SortBones(BoneUtils.FindBoneDescendats(root, allBones), reverseOrder: true);
				var loc = widget.Nodes.IndexOf(root);
				foreach (var bone in bones) {
					bone.Unlink();
					widget.AsWidget.Nodes.Insert(loc++, bone);
				}
			}

			foreach (var child in widget.Nodes) {
				ReorderBonesRecursive(child.AsWidget);
			}
		}

		public void Export(Stream stream, Node node, INodeThumbnailProvider thumbnailProvider)
		{
			CreateFolderBeginEndNodes(node);
			try {
				ReorderBonesRecursive(node.AsWidget);
				using (var tw = new StreamWriter(stream)) {
					writer = new Writer(tw);
					Write(node);
					var thumbnail = thumbnailProvider.GetThumbnail(node);
					if (thumbnail != null) {
						tw.NewLine = "\r\n";
						tw.WriteLine(HotSceneImporter.ThumbnailMarker);
						tw.Write(thumbnail);
					}
				}
			} finally {
				RemoveFolderBeginEndNodes(node);
			}
		}

		void CreateFolderBeginEndNodes(Node node)
		{
			foreach (var n in node.Nodes) {
				CreateFolderBeginEndNodes(n);
			}
			if (node.Folders == null) {
				return;
			}
			var stack = new Stack<int>();
			var nodes = node.Nodes.ToList();
			node.Nodes.Clear();
			int folderIndex = 0;
			int nodeIndex = 0;
			while (true) {
				if (folderIndex < node.Folders.Count && node.Folders[folderIndex].Index <= nodeIndex) {
					var folder = node.Folders[folderIndex];
					node.Nodes.Add(new FolderBegin { Id = folder.Id, Expanded = folder.Expanded });
					stack.Push(folder.ItemCount);
					folderIndex++;
				} else if (nodeIndex < nodes.Count) {
					node.Nodes.Add(nodes[nodeIndex++]);
				} else {
					break;
				}
				while (stack.Count > 0) {
					var c = stack.Pop();
					if (c > 0) {
						stack.Push(c - 1);
						break;
					}
					node.Nodes.Add(new FolderEnd());
				}
			}
			while (stack.Count > 0) {
				stack.Pop();
				nodes.Add(new FolderEnd());
			}
		}

		void RemoveFolderBeginEndNodes(Node node)
		{
			foreach (var n in node.Nodes) {
				RemoveFolderBeginEndNodes(n);
			}
			if (node.Folders == null) {
				return;
			}
			foreach (var n in node.Nodes.Where(i => i is FolderBegin || i is FolderEnd).ToList()) {
				n.Unlink();
			}
		}

		void Write(Node node)
		{
			writeNodesStack.Push(node);

			NodeWriter w;
			if (!nodeWriters.TryGetValue(node.GetType(), out w)) {
				throw new InvalidOperationException($"Unknown node type: {node.GetType()}");
			}
			writer.BeginStruct(w.ActorClass);
			w.Writer(node);
			if (node.Animators.Count > 0) {
				writer.BeginCollection("Animators");
				foreach (var a in node.Animators) {
					WriteAnimator(node, a);
				}
				writer.EndCollection();
			}
			if (node.Nodes.Count > 0) {
				writer.BeginCollection("Actors");
				foreach (var child in node.Nodes) {
					Write(child);
				}
				writer.EndCollection();
			}
			if (node.Markers.Count > 0) {
				writer.BeginCollection("Markers");
				foreach (var m in node.Markers) {
					WriteMarker(m);
				}
				writer.EndCollection();
			}
			writer.EndStruct();

			writeNodesStack.Pop();
		}

		void WriteMarker(Marker marker)
		{
			writer.BeginStruct("Hot::Marker");
			WriteProperty("Name", marker.Id, null);
			WriteProperty("Frame", marker.Frame, 0);
			WriteProperty("Command", (int)marker.Action, 0);
			WriteProperty("OtherMarkerName", marker.JumpTo, null);
			writer.EndStruct();
		}

		void WriteAnimator(Node owner, IAnimator animator)
		{
			if (animator is Animator<ShaderId>) {
				return;
			}
			if (owner is ParticleModifier && animator.TargetProperty == "Scale" && animator is Vector2Animator) {
				NumericAnimator zoomAnimator;
				NumericAnimator aspectRatioAnimator;
				DecomposeParticleModifierScaleAnimator(animator, out zoomAnimator, out aspectRatioAnimator);
				WriteAnimator(owner, zoomAnimator);
				WriteAnimator(owner, aspectRatioAnimator);
				return;
			}
			var type = GetHotStudioValueType(animator.GetValueType());
			writer.BeginStruct($"Hot::TypedAnimator<{type}>");
			WriteProperty("Property", GetAnimatorPropertyReference(owner, animator), null);
			if (animator.ReadonlyKeys.Count == 0) {
				writer.WriteLine("Frames [ ]");
				writer.WriteLine("Attributes [ ]");
				writer.WriteLine("Keys [ ]");
			} else {
				writer.WriteLine("Frames [ " + string.Join(" ", animator.ReadonlyKeys.Select(i => i.Frame)) + " ]");
				writer.WriteLine("Attributes [ " + string.Join(" ", animator.ReadonlyKeys.Select(i => (int)i.Function)) + " ]");
				if (animator is Animator<Blending>) {
					var shaderAnimator = owner.Animators.OfType<Animator<ShaderId>>().First();
					writer.WriteLine("Keys [ " + string.Join(" ",
						animator.ReadonlyKeys.Select((b, i) =>
							GetHotStudioBlending((Blending)b.Value, shaderAnimator.ReadonlyKeys[i].Value).ToString())) + " ]");
				} else if (animator is Animator<string>) {
					writer.WriteLine("Keys [ " + string.Join(" ", animator.ReadonlyKeys.Select(i => ObjectToString(i.Value ?? string.Empty))) + " ]");
				} else {
					writer.WriteLine("Keys [ " + string.Join(" ", animator.ReadonlyKeys.Select(i => ObjectToString(i.Value))) + " ]");
				}
			}
			writer.EndStruct();
		}

		private void DecomposeParticleModifierScaleAnimator(IAnimator animator, out NumericAnimator zoomAnimator, out NumericAnimator aspectRatioAnimator)
		{
			zoomAnimator = new NumericAnimator {
				TargetProperty = "Scale"
			};
			aspectRatioAnimator = new NumericAnimator() {
				TargetProperty = "AspectRatio"
			};
			if (animator.ReadonlyKeys.Count == 0) {
				return;
			}
			foreach (var key in animator.ReadonlyKeys) {
				float aspectRatio;
				float zoom;
				ParticleEmitter.DecomposeScale((Vector2)key.Value, out aspectRatio, out zoom);
				zoomAnimator.Keys.Add(key.Frame, zoom, key.Function);
				aspectRatioAnimator.Keys.Add(key.Frame, aspectRatio, key.Function);
			}
		}

		string GetAnimatorPropertyReference(Node owner, IAnimator animator)
		{
			var p = GetHotStudioPropertyName(owner.GetType(), animator.TargetProperty) + '@' + GetHotStudioActorName(owner, animator);
			if (p == "Position@Hot::PointObject") {
				return "Anchor@Hot::PointObject";
			}
			return p;
		}

		string GetHotStudioActorName(Node owner, IAnimator animator)
		{
			var t = owner.GetType();
			if (owner is ParticleModifier) {
				if (animator.TargetProperty == "AspectRatio" || animator.TargetProperty == "Scale") {
					return "Hot::ParticleTemplate";
				}
			}
			var nodeType = t.GetProperty(animator.TargetProperty).DeclaringType;
			var a = nodeWriters.First(i => i.Key == nodeType).Value.ActorClass;
			if (a == "Hot::ParticleEmitter2") {
				return "Hot::ParticleEmitter";
			}
			return a;
		}

		void WriteProperty(string name, object value, object def)
		{
			writer.WriteProperty(name, value, def);
		}

		string GetHotStudioPropertyName(Type type, string name)
		{
			switch (name) {
				case "AlongPathOrientation": return "AlongTrackOrientation";
				case "File": return "Sample";
				case "UV0": return "TexCoordForMins";
				case "UV1": return "TexCoordForMaxs";
				case "WidgetId": return "WidgetName";
				case "Texture": return "TexturePath";
				case "Blending": return "BlendMode";
				case "AnimationFps": return "AnimationFPS";
				case "Lifetime": return "Life";
				case "FontHeight": return "FontSize";
				case "HAlignment": return "HAlign";
				case "VAlignment": return "VAlign";
				case "SplineId": return "SplineName";
				case "RandomMotionRadius": return "RandMotionRadius";
				case "RandomMotionRotation": return "RandMotionRotation";
				case "RandomMotionSpeed": return "RandMotionSpeed";
				case "RandomMotionAspectRatio": return "RandMotionAspectRatio";
				default: return name;
			}
		}

		string GetHotStudioValueType(Type type)
		{
			if (type == typeof(ITexture) || type == typeof(string) || type == typeof(SerializableSample))
				return "std::basic_string<char,std::char_traits<char>,std::allocator<char>>";
			if (type == typeof(Vector2)) return "Hot::Vector2";
			if (type == typeof(Color4)) return "Hot::Color";
			if (type == typeof(float)) return "float";
			if (type == typeof(int)) return "int";
			if (type == typeof(bool)) return "bool";
			if (type == typeof(Blending)) return "Hot::BlendMode";
			if (type == typeof(HAlignment)) return "Hot::HorizontalAlignment";
			if (type == typeof(VAlignment)) return "Hot::VerticalAlignment";
			if (type == typeof(NumericRange)) return "Hot::RandomPair";
			if (type == typeof(AudioAction)) return "Hot::Audio::Action";
			if (type == typeof(MovieAction)) return "Hot::Movie::Action";
			if (type == typeof(EmissionType)) return "Hot::EmissionType";
			if (type == typeof(EmitterShape)) return "Hot::EmitterShape";
			throw new ArgumentException($"Unknown type {type}");
		}

		static string RestorePath(string path, string extension)
		{
			if (!string.IsNullOrEmpty(path)) {
				return path.Replace("/", "\\") + extension;
			}
			return path;
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
			if (shader == ShaderId.Silhuette) {
				switch (blending) {
					case Blending.Alpha: return 7;
					default: throw new InvalidOperationException("Can't use Silhuette shader with blending other than Alpha");
				}
			}
			return 1;
		}

		void WriteNodeProperties(Node node)
		{
			WriteProperty("Name", node.Id, null);
			WriteProperty("Source", RestorePath(node.ContentsPath, ".scene"), null);
			WriteProperty("Attributes", (int)node.TangerineFlags, 0);
			WriteProperty("Trigger", node.Trigger, string.Empty);
			WriteProperty("Tag", node.Tag, string.Empty);
		}

		void WriteWidgetProperties(Widget node)
		{
			WriteNodeProperties(node);
			WriteProperty("Visible", node.Visible, true);
			WriteProperty("Rotation", node.Rotation, 0f);
			WriteProperty("Position", node.Position, Vector2.Zero);
			WriteProperty("Scale", node.Scale, Vector2.One);
			WriteProperty("Pivot", node.Pivot, Vector2.Zero);
			WriteProperty("Size", node.Size, new Vector2(100, 100));
			WriteProperty("Color", node.Color, Color4.White);
			WriteProperty("BlendMode", GetHotStudioBlending(node.Blending, node.Shader), 0);
			WriteProperty("Anchors", (int)node.Anchors, 0);
			WriteProperty("HitTestMethod", (int)node.HitTestMethod, ((node is Image) || (node is NineGrid)) ? 1 : 0);
			WriteProperty("SkinningWeights", node.SkinningWeights, new SkinningWeights());
		}

		void WriteFrameProperties(Frame node)
		{
			WriteWidgetProperties(node);
			WriteProperty("RenderTarget", (int)node.RenderTarget, 0);
		}

		void WriteImageProperties(Image node)
		{
			WriteWidgetProperties(node);
			WriteProperty("TexturePath", node.Texture, null);
			WriteProperty("TexCoordForMins", node.UV0, Vector2.Zero);
			WriteProperty("TexCoordForMaxs", node.UV1, Vector2.One);
		}

		void WriteAudioProperties(Audio node)
		{
			WriteNodeProperties(node);
			WriteProperty("Action", (int)node.Action, (int)AudioAction.Play);
			WriteProperty("File", node.Sample, null);
			WriteProperty("Flags", (node.Looping ? 4 : 0) | (node.Bumpable ? 0 : 1), 0);
			WriteProperty("Group", node.Group == AudioChannelGroup.Music ? 1 : 0, 0);
			WriteProperty("Priority", node.Priority, 0.5f);
			WriteProperty("FadeTime", node.FadeTime, 0f);
			WriteProperty("Volume", node.Volume, 0.5f);
			WriteProperty("Pan", node.Pan, 0f);
		}

		void WriteButtonProperties(Button node)
		{
			WriteWidgetProperties(node);
			WriteProperty("Enabled", node.Enabled, true);
			WriteProperty("Text", node.Text, null);
		}

		void WriteImageCombinerProperties(ImageCombiner node)
		{
			WriteNodeProperties(node);
			WriteProperty("Enabled", node.Enabled, true);
			WriteProperty("BlendMode", GetHotStudioBlending(node.Blending, node.Shader), 0);
		}

		void WriteParticleEmitterProperties(ParticleEmitter node)
		{
			WriteWidgetProperties(node);
			WriteProperty("Shape", (int)node.Shape, (int)EmitterShape.Point);
			WriteProperty("EmissionType", (int)node.EmissionType, (int)EmissionType.Outer);
			WriteProperty("ParticlesLinkage", (int)node.ParticlesLinkage, (int)ParticlesLinkage.Parent);
			WriteProperty("LinkageActorName", node.LinkageWidgetName, null);
			WriteProperty("Number", node.Number, 100f);
			WriteProperty("TimeShift", node.TimeShift, 0f);
			WriteProperty("ImmortalParticles", node.ImmortalParticles, false);
			WriteProperty("Speed", node.Speed, 1f);
			WriteProperty("Life", node.Lifetime, new NumericRange(1, 0));
			WriteProperty("Velocity", node.Velocity, new NumericRange(100, 0));
			WriteProperty("Zoom", node.Zoom, new NumericRange(1, 0));
			WriteProperty("AspectRatio", node.AspectRatio, new NumericRange(1, 0));
			WriteProperty("Spin", node.Spin, new NumericRange(0, 0));
			WriteProperty("AngularVelocity", node.AngularVelocity, new NumericRange(0, 0));
			WriteProperty("Orientation", node.Orientation, new NumericRange(0, 360));
			WriteProperty("AlongTrackOrientation", node.AlongPathOrientation, false);
			WriteProperty("Direction", node.Direction, new NumericRange(0, 360));
			WriteProperty("WindDirection", node.WindDirection, new NumericRange(0, 0));
			WriteProperty("WindAmount", node.WindAmount, new NumericRange(0, 0));
			WriteProperty("GravityDirection", node.GravityDirection, new NumericRange(90, 0));
			WriteProperty("GravityAmount", node.GravityAmount, new NumericRange(0, 0));
			WriteProperty("MagnetAmount", node.MagnetAmount, new NumericRange(0, 0));
			WriteProperty("RandMotionRadius", node.RandomMotionRadius, new NumericRange(20, 0));
			WriteProperty("RandMotionRotation", node.RandomMotionRotation, new NumericRange(0, 360));
			WriteProperty("RandMotionSpeed", node.RandomMotionSpeed, new NumericRange(0, 0));
			WriteProperty("RandMotionAspectRatio", node.RandomMotionAspectRatio, 1f);
		}

		void WriteParticleModifierProperties(ParticleModifier node)
		{
			WriteNodeProperties(node);
			WriteProperty("TexturePath", node.Texture, null);
			WriteProperty("FirstFrame", node.FirstFrame, 1);
			WriteProperty("LastFrame", node.LastFrame, 1);
			WriteProperty("LoopedAnimation", node.LoopedAnimation, true);
			WriteProperty("AnimationFPS", node.AnimationFps, 20f);
			float aspectRatio;
			float zoom;
			ParticleEmitter.DecomposeScale(node.Size / (Vector2)node.Texture.ImageSize * node.Scale, out aspectRatio, out zoom);
			WriteProperty("Scale", zoom, 1f);
			WriteProperty("AspectRatio", aspectRatio, 1f);
			WriteProperty("Velocity", node.Velocity, 1f);
			WriteProperty("WindAmount", node.WindAmount, 1f);
			WriteProperty("GravityAmount", node.GravityAmount, 1f);
			WriteProperty("MagnetAmount", node.MagnetAmount, 1f);
			WriteProperty("Spin", node.Spin, 1f);
			WriteProperty("AngularVelocity", node.AngularVelocity, 1f);
			WriteProperty("Color", node.Color, Color4.White);
		}

		void WriteParticlesMagnetProperties(ParticlesMagnet node)
		{
			WriteWidgetProperties(node);
			WriteProperty("Shape", (int)node.Shape, (int)EmitterShape.Area);
			WriteProperty("Strength", node.Strength, 1000f);
			WriteProperty("Attenuation", node.Attenuation, 0f);
		}

		void WriteSplineGearProperties(SplineGear node)
		{
			WriteNodeProperties(node);
			WriteProperty("WidgetName", node.Widget?.Id, null);
			WriteProperty("SplineName", node.Spline?.Id, null);
			WriteProperty("SplineOffset", node.SplineOffset, 0f);
		}

		void WriteSplinePointProperties(SplinePoint node)
		{
			WritePointObjectProperties(node);
			WriteProperty("TangentAngle", node.TangentAngle, 0f);
			WriteProperty("TangentWeight", node.TangentWeight, 0f);
			WriteProperty("Straight", node.Straight, false);
		}

		void WriteDistortionMeshProperties(DistortionMesh node)
		{
			WriteWidgetProperties(node);
			WriteProperty("TexturePath", node.Texture, null);
			WriteProperty("NumRows", node.NumRows, 2);
			WriteProperty("NumCols", node.NumCols, 2);
		}

		void WriteDistortionMeshPointProperties(DistortionMeshPoint node)
		{
			WritePointObjectProperties(node);
			WriteProperty("Anchor", node.Position, Vector2.Zero);
			WriteProperty("Color", node.Color, Color4.White);
			WriteProperty("UV", node.UV, Vector2.Zero);
		}

		void WriteSimpleTextProperties(SimpleText node)
		{
			WriteWidgetProperties(node);
			WriteProperty("FontName", node.Font, null);
			WriteProperty("FontSize", node.FontHeight, 0f);
			WriteProperty("LineIndent", node.Spacing, 0f);
			WriteProperty("Text", node.Text, null);
			WriteProperty("TextColor", node.TextColor, new Color4(0));
			WriteProperty("HAlign", (int)node.HAlignment, (int)HAlignment.Left);
			WriteProperty("VAlign", (int)node.VAlignment, (int)VAlignment.Top);
			WriteProperty("LetterSpacing", node.LetterSpacing, 0f);
		}

		void WriteRichTextProperties(RichText node)
		{
			WriteWidgetProperties(node);
			WriteProperty("Text", node.Text, null);
			WriteProperty("HAlign", (int)node.HAlignment, (int)HAlignment.Left);
			WriteProperty("VAlign", (int)node.VAlignment, (int)VAlignment.Top);
		}

		void WriteTextStyleProperties(TextStyle node)
		{
			WriteNodeProperties(node);
			WriteProperty("ImagePath", node.ImageTexture, null);
			WriteProperty("ImageSize", node.ImageSize, Vector2.Zero);
			WriteProperty("ImageUsage", (int)node.ImageUsage, (int)TextStyle.ImageUsageEnum.Bullet);
			WriteProperty("Font", node.Font, null);
			WriteProperty("Size", node.Size, 15f);
			WriteProperty("SpaceAfter", node.SpaceAfter, 0f);
			WriteProperty("Bold", node.Bold, false);
			WriteProperty("DropShadow", node.CastShadow, false);
			WriteProperty("TextColor", node.TextColor, Color4.White);
			WriteProperty("ShadowColor", node.ShadowColor, Color4.Black);
			WriteProperty("ShadowOffset", node.ShadowOffset, Vector2.One);
			WriteProperty("LetterSpacing", node.LetterSpacing, 0f);
		}

		void WriteNineGridProperties(NineGrid node)
		{
			WriteWidgetProperties(node);
			WriteProperty("TexturePath", node.Texture, null);
			WriteProperty("LeftOffset", node.LeftOffset, 0f);
			WriteProperty("TopOffset", node.TopOffset, 0f);
			WriteProperty("RightOffset", node.RightOffset, 0f);
			WriteProperty("BottomOffset", node.BottomOffset, 0f);
		}

		void WriteLinearLayoutProperties(LinearLayout node)
		{
			WriteNodeProperties(node);
			WriteProperty("Horizontal", node.Horizontal, false);
			WriteProperty("ProcessHidden", node.ProcessHidden, false);
		}

		void WriteFolderBeginProperties(FolderBegin node)
		{
			WriteNodeProperties(node);
			WriteProperty("Expanded", node.Expanded, true);
		}

		void WriteBoneProperties(Bone node)
		{
			WriteNodeProperties(node);
			WriteProperty("Position", node.Position, Vector2.Zero);
			WriteProperty("Rotation", node.Rotation, 0f);
			WriteProperty("Length", node.Length, 100f);
			WriteProperty("IKStopper", node.IKStopper, true);
			WriteProperty("Index", node.Index, 0);
			WriteProperty("BaseIndex", node.BaseIndex, 0);
			WriteProperty("EffectiveRadius", node.EffectiveRadius, 100f);
			WriteProperty("FadeoutZone", node.FadeoutZone, 50f);
			WriteProperty("RefPosition", node.RefPosition, Vector2.Zero);
			WriteProperty("RefRotation", node.RefRotation, 0f);
			WriteProperty("RefLength", node.RefLength, 0f);
		}

		public IEnumerable<Node> EnumerateWriteNodesStack()
		{
			return writeNodesStack;
		}

	}
}
