using System;
using System.IO;
using Lime;
using System.Collections.Generic;
using Exception = Lime.Exception;

namespace Orange
{
	public static class HotSceneImporterFactory
	{
		public static System.Type ImporterClass = typeof(HotSceneImporter);

		public static HotSceneImporter CreateImporter(string srcPath)
		{
			var ctr = ImporterClass.GetConstructor(new System.Type[] { typeof(string) });
			return ctr.Invoke(new object[] { srcPath }) as HotSceneImporter;
		}
	}

	public partial class HotSceneImporter
	{
		protected HotLexer lexer;
		protected List<KnownActorType> knownActorTypes;

		public HotSceneImporter(string path)
		{
			RegisterKnownActorTypes();
			using(Stream stream = new FileStream(path, FileMode.Open)) {
				using(TextReader reader = new StreamReader(stream)) {
					string text = reader.ReadToEnd();
					lexer = new HotLexer(path, text);
				}
			}
		}

		#region LimeParse

		protected void ParseActorProperty(Node node, string name)
		{
			switch (name) {
			case "Name":
				node.Id = lexer.ParseQuotedString();
				break;
			case "RuntimeClass":
				lexer.ParseQuotedString();
				break;
			case "Localizable":
				lexer.ParseBool();
				break;
			case "Source":
				var s = lexer.ParsePath();
				node.ContentsPath = s;
				break;
			case "Attributes":
				lexer.ParseInt();
				break;
			case "Trigger":
				lexer.ParseQuotedString();
				break;
			case "Tag":
				node.Tag = lexer.ParseQuotedString();
				break;
			case "Actors":
				lexer.ParseToken('[');
				while (lexer.PeekChar() != ']') {
					var child = ParseNode();
					if (child != null)
						node.Nodes.Add(child);
				}
				lexer.ParseToken(']');
				if (node is Widget) {
					ReorderBones(node as Widget);
				}
				break;
			case "Animators":
				lexer.ParseToken('[');
				while (lexer.PeekChar() != ']') {
					ParseAnimator(node);
				}
				if (node is ParticleModifier) {
					TryMergeScaleAndAspectRatioForParticleTemplate(node as ParticleModifier);
					particleModifierScaleAnimator = null;
					particleModifierAspectRatioAnimator = null;
				}
				lexer.ParseToken(']');
				break;
			case "Markers":
				lexer.ParseToken('[');
				while (lexer.PeekChar() != ']') {
					var marker = ParseMarker();
					if (marker.Action == MarkerAction.Jump && marker.JumpTo == null) {
						throw new Exception("Jump marker '{0}' in node '{1}' have no JumpTo property.", marker.Id ?? "<noname>", node.ToString());
					}
					node.Markers.Add(marker);
				}
				lexer.ParseToken(']');
				break;
			default:
				throw new Exception("Unknown property '{0}'. Parsing: {1}", name, node.GetType());
			}
		}

		// Reorder widget bones with topological sort to maintain correct update
		// order of transformations
		private static void ReorderBones(Widget widget)
		{
			var bones = new Dictionary<int, Bone>();
			int maxIndex = 0;
			for (int i = 0; i < widget.Nodes.Count; i++) {
				var bone = widget.Nodes[i] as Bone;
				if (bone != null) {
					if (bones.ContainsKey(bone.Index)) {
						throw new InvalidOperationException("more than one bone with same index");
					}
					bones[bone.Index] = bone;
					if (bone.Index > maxIndex) {
						maxIndex = bone.Index;
					}
				}
			}
			int n = maxIndex + 1;
			var visited = new bool[n];
			var g = new List<int>[n];
			for (int i = 0; i < n; i++) {
				g[i] = new List<int>();
			}
			foreach (var kv in bones) {
				var b = kv.Value;
				g[b.BaseIndex].Add(b.Index);
			}
			var orderedIndices = new List<int>();
			Action<int> visit = null;
			visit = (index) => {
				visited[index] = true;
				for (int i = 0; i < g[index].Count; i++) {
					if (visited[g[index][i]]) {
						throw new InvalidOperationException("found cycle in bones parent child relations");
					}
					visit(g[index][i]);
				}
				orderedIndices.Add(index);
			};
			for (int i = 0; i < n; i++) {
				if (!visited[i]) {
					visit(i);
				}
			}
			foreach (var i in orderedIndices) {
				// holes in indices and zero index (implicit bone with Identity transformation)
				if (!bones.ContainsKey(i)) {
					continue;
				}
				bones[i].Unlink();
				widget.Nodes.Insert(0, bones[i]);
			}
		}

		protected void ParseGraphicProperty(Node node, string name)
		{
			Widget widget = (Widget)node;
			switch (name) {
			case "Anchors":
				widget.Anchors = (Anchors)lexer.ParseInt();
				break;
			case "HitTestMethod":
				widget.HitTestMethod = (HitTestMethod)lexer.ParseInt();
				break;
			case "SkinningWeights":
				widget.SkinningWeights = lexer.ParseSkinningWeights();
				break;
			case "Visible":
				widget.Visible = lexer.ParseBool();
				break;
			case "Position":
				widget.Position = lexer.ParseVector2();
				break;
			case "Pivot":
				widget.Pivot = lexer.ParseVector2();
				break;
			case "Size":
				widget.Size = lexer.ParseVector2();
				break;
			case "Scale":
				widget.Scale = lexer.ParseVector2();
				break;
			case "Color":
				widget.Color = lexer.ParseColor4();
				break;
			case "Rotation":
				widget.Rotation = lexer.ParseFloat();
				break;
			case "BlendMode":
				var t = lexer.ParseBlendMode();
				widget.Blending = t.Item1;
				widget.Shader = t.Item2;
				break;
			case "SuppressSkin":
				lexer.ParseBool();
				break;
			case "VisualName":
				lexer.ParseQuotedString();
				System.Console.WriteLine("WARNING: Citrus doesn't support skins. VisualName must be empty");
				break;
			default:
				ParseActorProperty(node, name);
				break;
			}
		}

		protected void ParseImageProperty(Node node, string name)
		{
			Image img = (Image)node;
			switch (name) {
			case "TexturePath":
				var path = lexer.ParsePath();
				img.Texture = new SerializableTexture(path);
				break;
			case "TexCoordForMins":
				img.UV0 = lexer.ParseVector2();
				break;
			case "TexCoordForMaxs":
				img.UV1 = lexer.ParseVector2();
				break;
			default:
				ParseGraphicProperty(node, name);
				break;
			}
		}

		protected void ParseTextProperty(Node node, string name)
		{
			SimpleText text = (SimpleText)node;
			switch (name) {
			case "FontName":
				text.Font = new SerializableFont(lexer.ParseQuotedString());
				break;
			case "Text":
				text.Text = lexer.ParseQuotedString();
				break;
			case "LineIndent":
				text.Spacing = lexer.ParseFloat();
				break;
			case "FontSize":
				text.FontHeight = lexer.ParseFloat();
				break;
			case "TextColor":
				text.TextColor = lexer.ParseColor4();
				break;
			case "ShadowColor":
				lexer.ParseColor4();
				break;
			case "HAlign":
				text.HAlignment = (HAlignment)lexer.ParseInt();
				break;
			case "VAlign":
				text.VAlignment = (VAlignment)lexer.ParseInt();
				break;
			default:
				ParseGraphicProperty(node, name);
				break;
			}
		}

		protected void ParseParticleTemplateProperty(Node node, string name)
		{
			ParticleModifier pm = (ParticleModifier)node;
			switch (name) {
			case "TexturePath":
				pm.Texture = new SerializableTexture(lexer.ParsePath());
				pm.Size = (Vector2)pm.Texture.ImageSize;
				break;
			case "FirstFrame":
				pm.FirstFrame = lexer.ParseInt();
				break;
			case "LastFrame":
				pm.LastFrame = lexer.ParseInt();
				break;
			case "LoopedAnimation":
				pm.LoopedAnimation = lexer.ParseBool();
				break;
			case "AnimationFPS":
				pm.AnimationFps = lexer.ParseFloat();
				break;
			case "Scale": {
				var scale = lexer.ParseFloat();
				pm.Scale = new Vector2(scale, scale);
				break;
			}
			case "AspectRatio": {
				var ar = lexer.ParseFloat();
				if (ar != 1f) {
					pm.Scale = new Vector2(pm.Scale.X * ar, pm.Scale.Y / Math.Max(0.0001f, ar));
				}
				break;
			}
			case "Velocity":
				pm.Velocity = lexer.ParseFloat();
				break;
			case "WindAmount":
				pm.WindAmount = lexer.ParseFloat();
				break;
			case "GravityAmount":
				pm.GravityAmount = lexer.ParseFloat();
				break;
			case "MagnetAmount":
				pm.MagnetAmount = lexer.ParseFloat();
				break;
			case "Spin":
				pm.Spin = lexer.ParseFloat();
				break;
			case "AngularVelocity":
				pm.AngularVelocity = lexer.ParseFloat();
				break;
			case "Color":
				pm.Color = lexer.ParseColor4();
				break;
			default:
				ParseActorProperty(node, name);
				break;
			}
		}

		protected void ParseParticlesMagnetProperty(Node node, string name)
		{
			ParticlesMagnet magnet = (ParticlesMagnet)node;
			switch (name) {
			case "Shape":
				magnet.Shape = (EmitterShape)lexer.ParseInt();
				break;
			case "Attenuation":
				magnet.Attenuation = lexer.ParseFloat();
				break;
			case "Strength":
				magnet.Strength = lexer.ParseFloat();
				break;
			default:
				ParseGraphicProperty(magnet, name);
				break;
			}
		}

		protected void ParseParticleEmitter2Property(Node node, string name)
		{
			ParticleEmitter emitter = (ParticleEmitter)node;
			switch (name) {
			case "Shape":
				emitter.Shape = (EmitterShape)lexer.ParseInt();
				break;
			case "EmissionType":
				emitter.EmissionType = (EmissionType)lexer.ParseInt();
				break;
			case "ParticlesLinkage":
				emitter.ParticlesLinkage = (ParticlesLinkage)lexer.ParseInt();
				break;
			case "LinkageActorName":
				emitter.LinkageWidgetName = lexer.ParseQuotedString();
				break;
			case "Number":
				emitter.Number = lexer.ParseFloat();
				break;
			case "TimeShift":
				emitter.TimeShift = lexer.ParseFloat();
				break;
			case "ImmortalParticles":
				emitter.ImmortalParticles = lexer.ParseBool();
				break;
			case "Speed":
				emitter.Speed = lexer.ParseFloat();
				break;
			case "Life":
				emitter.Lifetime = lexer.ParseNumericRange();
				break;
			case "Velocity":
				emitter.Velocity = lexer.ParseNumericRange();
				break;
			case "Zoom":
				emitter.Zoom = lexer.ParseNumericRange();
				break;
			case "AspectRatio":
				emitter.AspectRatio = lexer.ParseNumericRange();
				break;
			case "Spin":
				emitter.Spin = lexer.ParseNumericRange();
				break;
			case "AngularVelocity":
				emitter.AngularVelocity = lexer.ParseNumericRange();
				break;
			case "Orientation":
				emitter.Orientation = lexer.ParseNumericRange();
				break;
			case "AlongTrackOrientation":
				emitter.AlongPathOrientation = lexer.ParseBool();
				break;
			case "Direction":
				emitter.Direction = lexer.ParseNumericRange();
				break;
			case "WindDirection":
				emitter.WindDirection = lexer.ParseNumericRange();
				break;
			case "WindAmount":
				emitter.WindAmount = lexer.ParseNumericRange();
				break;
			case "GravityDirection":
				emitter.GravityDirection = lexer.ParseNumericRange();
				break;
			case "GravityAmount":
				emitter.GravityAmount = lexer.ParseNumericRange();
				break;
			case "MagnetAmount":
				emitter.MagnetAmount = lexer.ParseNumericRange();
				break;
			case "RandMotionRadius":
				emitter.RandomMotionRadius = lexer.ParseNumericRange();
				break;
			case "RandMotionRotation":
				emitter.RandomMotionRotation = lexer.ParseNumericRange();
				break;
			case "RandMotionSpeed":
				emitter.RandomMotionSpeed = lexer.ParseNumericRange();
				break;
			case "RandMotionAspectRatio":
				emitter.RandomMotionAspectRatio = lexer.ParseFloat();
				break;
			default:
				ParseGraphicProperty(emitter, name);
				break;
			}
		}

		protected void ParseSceneProperty(Node node, string name)
		{
			Frame frame = (Frame)node;
			switch (name) {
			case "Enabled":
				lexer.ParseBool();
				break;
			case "DefaultFocus":
				lexer.ParseBool();
				break;
			case "Modal":
				lexer.ParseBool();
				break;
			case "ControlClass":
				lexer.ParseQuotedString();
				break;
			case "RenderTarget":
				frame.RenderTarget = (RenderTarget)lexer.ParseInt();
				break;
			case "Transparent":
				lexer.ParseBool();
				break;
			default:
				ParseGraphicProperty(node, name);
				break;
			}
		}

		protected void ParseMaskedEffectProperty(Node node, string name)
		{
			ImageCombiner combiner = (ImageCombiner)node;
			switch (name) {
			case "Enabled":
				combiner.Enabled = lexer.ParseBool();
				break;
			case "BlendMode":
				var t = lexer.ParseBlendMode();
				combiner.Blending = t.Item1;
				combiner.Shader = t.Item2;
				break;
			default:
				ParseActorProperty(combiner, name);
				break;
			}
		}

		void ParseDistortionMeshProperty(Node node, string name)
		{
			DistortionMesh mesh = (DistortionMesh)node;
			switch (name) {
			case "NumRows":
				mesh.NumRows = lexer.ParseInt();
				break;
			case "NumCols":
				mesh.NumCols = lexer.ParseInt();
				break;
			case "TexturePath":
				mesh.Texture = new SerializableTexture(lexer.ParsePath());
				break;
			default:
				ParseGraphicProperty(mesh, name);
				break;
			}
		}

		protected void ParseMeshPointProperty(Node node, string name)
		{
			DistortionMeshPoint point = (DistortionMeshPoint)node;
			switch (name) {
			case "Position":
				point.Offset = lexer.ParseVector2();
				break;
			case "Anchor":
				point.Position = lexer.ParseVector2();
				break;
			case "UV":
				point.UV = lexer.ParseVector2();
				break;
			case "Color":
				point.Color = lexer.ParseColor4();
				break;
			case "SkinningWeights":
				point.SkinningWeights = lexer.ParseSkinningWeights();
				break;
			default:
				ParseActorProperty(point, name);
				break;
			}
		}

		protected void ParseBoneProperty(Node node, string name)
		{
			Bone bone = (Bone)node;
			switch (name) {
			case "Position":
				bone.Position = lexer.ParseVector2();
				break;
			case "Rotation":
				bone.Rotation = lexer.ParseFloat();
				break;
			case "Length":
				bone.Length = lexer.ParseFloat();
				break;
			case "IKStopper":
				bone.IKStopper = lexer.ParseBool();
				break;
			case "Index":
				bone.Index = lexer.ParseInt();
				break;
			case "BaseIndex":
				bone.BaseIndex = lexer.ParseInt();
				break;
			case "EffectiveRadius":
				bone.EffectiveRadius = lexer.ParseFloat();
				break;
			case "FadeoutZone":
				bone.FadeoutZone = lexer.ParseFloat();
				break;
			case "RefPosition":
				bone.RefPosition = lexer.ParseVector2();
				break;
			case "RefRotation":
				bone.RefRotation = lexer.ParseFloat();
				break;
			case "RefLength":
				bone.RefLength = lexer.ParseFloat();
				break;
			default:
				ParseActorProperty(bone, name);
				break;
			}
		}

		protected void ParseAudioProperty(Node node, string name)
		{
			Audio audio = (Audio)node;
			switch (name) {
			case "File":
				audio.Sample = new SerializableSample(lexer.ParsePath());
				break;
			case "Flags":
				audio.Looping = (lexer.ParseInt() & 4) != 0;
				break;
			case "Action":
				lexer.ParseInt();
				break;
			case "Group":
				var group = lexer.ParseInt();
				audio.Group = (AudioChannelGroup)(group == 1 || group == 2 ? group : 0);
				break;
			case "Priority":
				audio.Priority = (int)lexer.ParseFloat();
				break;
			case "FadeTime":
				audio.FadeTime = lexer.ParseFloat();
				break;
			case "Volume":
				audio.Volume = lexer.ParseFloat();
				break;
			case "Pan":
				audio.Pan = lexer.ParseFloat();
				break;
			default:
				ParseActorProperty(audio, name);
				break;
			}
		}

		protected void ParseVideoProperty(Node node, string name)
		{
			Movie movie = (Movie)node;
			switch (name) {
				case "File":
					movie.Path = lexer.ParsePath();
					break;
				case "Flags":
					movie.Looped = (lexer.ParseInt() & 1) != 0;
					break;
				case "Action":
					lexer.ParseInt();
					break;
				default:
					ParseGraphicProperty(movie, name);
					break;
			}
		}

		protected void ParseMarkerProperty(Marker marker, string name)
		{
			switch (name) {
			case "Name":
				marker.Id = lexer.ParseQuotedString();
				break;
			case "Frame":
				marker.Frame = lexer.ParseInt() * 2;
				break;
			case "EaseInterpolation":
				lexer.ParseBool();
				break;
			case "Command":
				marker.Action = (MarkerAction)lexer.ParseInt();
				break;
			case "OtherMarkerName":
				marker.JumpTo = lexer.ParseQuotedString();
				break;
			case "EaseParameters":
				lexer.ParseToken('[');
				lexer.ParseFloat();
				lexer.ParseFloat();
				lexer.ParseFloat();
				lexer.ParseToken(']');
				break;
			default:
				throw new Exception("Unknown property '{0}'. Parsing: {1}", name, marker.GetType());
			}
		}

		protected Marker ParseMarker()
		{
			string type = lexer.ParseQuotedString();
			if (type != "Hot::Marker")
				throw new Exception("Invalid marker type '{0}'", type);
			var marker = new Marker();
			lexer.ParseToken('{');
			while (lexer.PeekChar() != '}')
				ParseMarkerProperty(marker, lexer.ParseWord());
			lexer.ParseToken('}');
			return marker;
		}

		protected void ParseButtonProperty(Node node, string name)
		{
			Button button = (Button)node;
			switch (name) {
			case "Text":
				button.Text = lexer.ParseQuotedString();
				break;
			case "Enabled":
				button.Enabled = lexer.ParseBool();
				break;
			default:
				ParseGraphicProperty(button, name);
				break;
			}
		}

		protected void ParseNineGridProperty(Node node, string name)
		{
			NineGrid grid = (NineGrid)node;
			switch (name) {
			case "TexturePath":
				grid.Texture = new SerializableTexture(lexer.ParsePath());
				break;
			case "LeftOffset":
				grid.LeftOffset = lexer.ParseFloat();
				break;
			case "TopOffset":
				grid.TopOffset = lexer.ParseFloat();
				break;
			case "RightOffset":
				grid.RightOffset = lexer.ParseFloat();
				break;
			case "BottomOffset":
				grid.BottomOffset = lexer.ParseFloat();
				break;
			default:
				ParseGraphicProperty(grid, name);
				break;
			}
		}

		protected void ParseSplinePointProperty(Node node, string name)
		{
			SplinePoint point = (SplinePoint)node;
			switch (name) {
			case "Position":
				throw new Exception("`Position` property of spline point must not be used. Use `Anchor` instead.");
				break;
			case "Anchor":
				point.Position = lexer.ParseVector2();
				break;
			case "Straight":
				point.Straight = lexer.ParseBool();
				break;
			case "TangentAngle":
				point.TangentAngle = lexer.ParseFloat();
				break;
			case "TangentWeight":
				point.TangentWeight = lexer.ParseFloat();
				break;
			case "SkinningWeights":
				point.SkinningWeights = lexer.ParseSkinningWeights();
				break;
			default:
				ParseActorProperty(point, name);
				break;
			}
		}

		protected void ParseGearProperty(Node node, string name)
		{
			SplineGear gear = (SplineGear)node;
			switch (name) {
			case "WidgetName":
				gear.WidgetRef = new NodeReference<Widget>(lexer.ParseQuotedString());
				break;
			case "SplineName":
				gear.SplineRef = new NodeReference<Spline>(lexer.ParseQuotedString());
				break;
			case "SplineOffset":
				gear.SplineOffset = lexer.ParseFloat();
				break;
			default:
				ParseActorProperty(gear, name);
				break;
			}
		}

		protected void ParseSplineProperty(Node node, string name)
		{
			switch (name) {
			default:
				ParseGraphicProperty(node, name);
				break;
			}
		}

		protected void ParseSliderProperty(Node node, string name)
		{
			Slider slider = (Slider)node;
			switch (name) {
			case "RangeMin":
				slider.RangeMin = lexer.ParseFloat();
				break;
			case "RangeMax":
				slider.RangeMax = lexer.ParseFloat();
				break;
			case "Value":
				slider.Value = lexer.ParseFloat();
				break;
			case "Step":
				lexer.ParseFloat();
				break;
			default:
				ParseGraphicProperty(node, name);
				break;
			}
		}

		protected void ParseTextStyleProperty(Node node, string name)
		{
			TextStyle style = (TextStyle)node;
			switch (name) {
			case "ImagePath":
				style.ImageTexture = new SerializableTexture(lexer.ParsePath());
				break;
			case "ImageSize":
				style.ImageSize = lexer.ParseVector2();
				break;
			case "ImageUsage":
				style.ImageUsage = (TextStyle.ImageUsageEnum)lexer.ParseInt();
				break;
			case "Font":
				style.Font = new SerializableFont(lexer.ParseQuotedString());
				break;
			case "SpaceAfter":
				style.SpaceAfter = lexer.ParseFloat();
				break;
			case "Size":
				style.Size = lexer.ParseFloat();
				break;
			case "Bold":
				style.Bold = lexer.ParseBool();
				break;
			case "TextColor":
				style.TextColor = lexer.ParseColor4();
				break;
			case "DropShadow":
				style.CastShadow = lexer.ParseBool();
				break;
			case "ShadowColor":
				style.ShadowColor = lexer.ParseColor4();
				break;
			case "ShadowOffset":
				style.ShadowOffset = lexer.ParseVector2();
				break;
			default:
				ParseActorProperty(node, name);
				break;
			}
		}
		protected void ParseRichTextProperty(Node node, string name)
		{
			RichText text = (RichText)node;
			switch (name) {
			case "Text":
				text.Text = lexer.ParseQuotedString();
				break;
			case "HAlign":
				text.HAlignment = (HAlignment)lexer.ParseInt();
				break;
			case "VAlign":
				text.VAlignment = (VAlignment)lexer.ParseInt();
				break;
			default:
				ParseGraphicProperty(node, name);
				break;
			}
		}

		protected void ParseFolderBeginProperty(Node node, string name)
		{
			switch (name) {
			case "Expanded":
				lexer.ParseBool();
				break;
			default:
				ParseActorProperty(node, name);
				break;
			}
		}

		protected void ParseFolderEndProperty(Node node, string name)
		{
			switch (name) {
			default:
				ParseActorProperty(node, name);
				break;
			}
		}

		public delegate void ActorPropReader(Node node, string name);

		public struct KnownActorType
		{
			public string ActorClass;
			public string NodeClass;
			public ActorPropReader PropReader;

			public static KnownActorType Generate(System.Type Type, ActorPropReader PropReader)
			{
				string actorClass = Type.Name;
				string nodeClass = string.Format("{0}, {1}", Type.FullName, Type.Namespace);
				KnownActorType result = new KnownActorType() {
					ActorClass = actorClass,
					NodeClass = nodeClass,
					PropReader = PropReader
				};
				return result;
			}
		}

		public object CreateObject(string className)
		{
			var type = System.Type.GetType(className);
			if (type == null) {
				throw new Exception("Unknown type: {0}", className);
			}
			var ctor = type.GetConstructor(System.Type.EmptyTypes);
			if (ctor == null)
				throw new Exception("No public default constructor is defined for: {0}", className);
			var obj = ctor.Invoke(new object[] {});
			return obj;
		}

		public Node ParseNode()
		{
			string actorClass = lexer.ParseQuotedString();
			foreach (KnownActorType t in knownActorTypes) {
				if (t.ActorClass == actorClass) {
					Node n = (Node)CreateObject(t.NodeClass);
					lexer.ParseToken('{');
					while (lexer.PeekChar() != '}')
						t.PropReader(n, lexer.ParseWord());
					lexer.ParseToken('}');
					if (t.ActorClass == "Hot::FolderBegin" || t.ActorClass == "Hot::FolderEnd")
						return null;
					return n;
				}
			}
			throw new Exception("Unknown type of actor '{0}'", actorClass);
		}

		protected void ParseLinearLayoutProperty(Node node, string name)
		{
			var layout = (LinearLayout)node;
			switch (name) {
				case "Horizontal":
					layout.Horizontal = lexer.ParseBool();
					break;
				case "ProcessHidden":
					layout.ProcessHidden = lexer.ParseBool();
					break;
				default:
					ParseActorProperty(node, name);
					break;
			}
		}

		private void ParseModelViewProperty(Node node, string name)
		{
			var viewport = (Viewport3D)node;
			switch (name) {
				case "ModelPath":
					viewport.ContentsPath = lexer.ParsePath();
					break;
				case "Frame":
					viewport.Frame = lexer.ParseFloat();
					break;
				case "Camera":
					lexer.ParseQuotedString();
					break;
				default:
					ParseGraphicProperty(node, name);
					break;
			}
		}

		private void ParseEmitterShapePointProperty(Node node, string name)
		{
			EmitterShapePoint point = (EmitterShapePoint)node;
			switch (name) {
			case "Position":
				throw new Exception("`Position` property of emitter shape point must not be used. Use `Anchor` instead.");
				break;
			case "Anchor":
				point.Position = lexer.ParseVector2();
				break;
			case "SkinningWeights":
				point.SkinningWeights = lexer.ParseSkinningWeights();
				break;
			default:
				ParseActorProperty(point, name);
				break;
			}
		}

		#endregion

		protected virtual void RegisterKnownActorTypes()
		{
			knownActorTypes = new List<KnownActorType> {
				new KnownActorType {ActorClass = "Hot::Scene", NodeClass = "Lime.Frame, Lime", PropReader = ParseSceneProperty},
				new KnownActorType {ActorClass = "Hot::RootScene", NodeClass = "Lime.Frame, Lime", PropReader = ParseSceneProperty},
				new KnownActorType {ActorClass = "Hot::Visual", NodeClass = "Lime.Frame, Lime", PropReader = ParseSceneProperty},
				new KnownActorType {ActorClass = "Hot::Image", NodeClass = "Lime.Image, Lime", PropReader = ParseImageProperty},
				new KnownActorType {ActorClass = "Hot::DistortionMesh", NodeClass = "Lime.DistortionMesh, Lime", PropReader = ParseDistortionMeshProperty},
				new KnownActorType {ActorClass = "Hot::MeshPoint", NodeClass = "Lime.DistortionMeshPoint, Lime", PropReader = ParseMeshPointProperty},
				new KnownActorType {ActorClass = "Hot::Text", NodeClass = "Lime.SimpleText, Lime", PropReader = ParseTextProperty},
				new KnownActorType {ActorClass = "Hot::TextPresenter", NodeClass = "Lime.SimpleText, Lime", PropReader = ParseTextProperty},
				new KnownActorType {ActorClass = "Hot::ParticleEmitter", NodeClass = "Lime.ParticleEmitter, Lime", PropReader = ParseParticleEmitter2Property},
				new KnownActorType {ActorClass = "Hot::ParticleEmitter2", NodeClass = "Lime.ParticleEmitter, Lime", PropReader = ParseParticleEmitter2Property},
				new KnownActorType {ActorClass = "Hot::ParticleTemplate", NodeClass = "Lime.ParticleModifier, Lime", PropReader = ParseParticleTemplateProperty},
				new KnownActorType {ActorClass = "Hot::ParticlesMagnet", NodeClass = "Lime.ParticlesMagnet, Lime", PropReader = ParseParticlesMagnetProperty},
				new KnownActorType {ActorClass = "Hot::MaskedEffect", NodeClass = "Lime.ImageCombiner, Lime", PropReader = ParseMaskedEffectProperty},
				new KnownActorType {ActorClass = "Hot::Bone", NodeClass = "Lime.Bone, Lime", PropReader = ParseBoneProperty},
				new KnownActorType {ActorClass = "Hot::Audio", NodeClass = "Lime.Audio, Lime", PropReader = ParseAudioProperty},
				new KnownActorType {ActorClass = "Hot::Spline", NodeClass = "Lime.Spline, Lime", PropReader = ParseSplineProperty},
				new KnownActorType {ActorClass = "Hot::SplinePoint", NodeClass = "Lime.SplinePoint, Lime", PropReader = ParseSplinePointProperty},
				new KnownActorType {ActorClass = "Hot::Gear", NodeClass = "Lime.SplineGear, Lime", PropReader = ParseGearProperty},
				new KnownActorType {ActorClass = "Hot::Button", NodeClass = "Lime.Button, Lime", PropReader = ParseButtonProperty},
				new KnownActorType {ActorClass = "Hot::FolderBegin", NodeClass = "Lime.Node, Lime", PropReader = ParseFolderBeginProperty},
				new KnownActorType {ActorClass = "Hot::FolderEnd", NodeClass = "Lime.Node, Lime", PropReader = ParseFolderEndProperty},
				new KnownActorType {ActorClass = "Hot::NineGrid", NodeClass = "Lime.NineGrid, Lime", PropReader = ParseNineGridProperty},
				new KnownActorType {ActorClass = "Hot::Slider", NodeClass = "Lime.Slider, Lime", PropReader = ParseSliderProperty},
				new KnownActorType {ActorClass = "Hot::RichText", NodeClass = "Lime.RichText, Lime", PropReader = ParseRichTextProperty},
				new KnownActorType {ActorClass = "Hot::TextStyle", NodeClass = "Lime.TextStyle, Lime", PropReader = ParseTextStyleProperty},
				new KnownActorType {ActorClass = "Hot::Movie", NodeClass = "Lime.Movie, Lime", PropReader = ParseVideoProperty},
				new KnownActorType {ActorClass = "Hot::ModelView", NodeClass = "Lime.Viewport3D, Lime", PropReader = ParseModelViewProperty},
				new KnownActorType {ActorClass = "LinearLayout", NodeClass = "Lime.LinearLayout, Lime", PropReader = ParseLinearLayoutProperty},
				new KnownActorType {ActorClass = "Hot::EmitterShapePoint", NodeClass = "Lime.EmitterShapePoint, Lime", PropReader = ParseEmitterShapePointProperty},
			};
		}
	}
}
