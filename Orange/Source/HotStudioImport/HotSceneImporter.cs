using System.IO;
using Lime;

namespace Orange
{
	public partial class HotSceneImporter
	{
		HotLexer lexer;
		KnownActorType[] knownActorTypes;
		
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

		void ParseActorProperty(Node node, string name)
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
				break;
			case "Animators":
				lexer.ParseToken('[');
				while (lexer.PeekChar() != ']') {
					ParseAnimator(node);
				}
				lexer.ParseToken(']');
				break;
			case "Markers":
				lexer.ParseToken('[');
				while (lexer.PeekChar() != ']')
					node.Markers.Add(ParseMarker());
				lexer.ParseToken(']');
				break;
			default:
				throw new Exception("Unknown property '{0}'. Parsing: {1}", name, node.GetType());
			}
		}

		void ParseGraphicProperty(Node node, string name)
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
				widget.Blending = lexer.ParseBlendMode();
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

		void ParseImageProperty(Node node, string name)
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

		void ParseTextProperty(Node node, string name)
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
				text.Color = text.Color * lexer.ParseColor4();
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

		void ParseParticleTemplateProperty(Node node, string name)
		{
			ParticleModifier pm = (ParticleModifier)node;
			switch (name) {
			case "TexturePath":
				pm.Texture = new SerializableTexture(lexer.ParsePath());
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
			case "Scale":
				pm.Scale = lexer.ParseFloat();
				break;
			case "AspectRatio":
				pm.AspectRatio = lexer.ParseFloat();
				break;
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

		void ParseParticlesMagnetProperty(Node node, string name)
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

		void ParseParticleEmitter2Property(Node node, string name)
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

		void ParseSceneProperty(Node node, string name)
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
		
		void ParseMaskedEffectProperty(Node node, string name)
		{
			ImageCombiner combiner = (ImageCombiner)node;
			switch (name) {
			case "Enabled":
				combiner.Enabled = lexer.ParseBool();
				break;
			case "BlendMode":
				lexer.ParseInt();
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

		void ParseMeshPointProperty(Node node, string name)
		{
			DistortionMeshPoint point = (DistortionMeshPoint)node;
			switch (name) {
			case "Position":
				lexer.ParseVector2();
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

		void ParseBoneProperty(Node node, string name)
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

		void ParseAudioProperty(Node node, string name)
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
				audio.Group = lexer.ParseInt() == 1 ? AudioChannelGroup.Music : AudioChannelGroup.Effects;
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

		void ParseMarkerProperty(Marker marker, string name)
		{
			switch (name) {
			case "Name":
				marker.Id = lexer.ParseQuotedString();
				break;
			case "Frame":
				marker.Frame = lexer.ParseInt();
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

		Marker ParseMarker()
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

		void ParseButtonProperty(Node node, string name)
		{
			Button button = (Button)node;
			switch (name) {
			case "Text":
				button.Caption = lexer.ParseQuotedString();
				break;
			default:
				ParseGraphicProperty(button, name);
				break;
			}
		}

		void ParseNineGridProperty(Node node, string name)
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

		void ParseSplinePointProperty(Node node, string name)
		{
			SplinePoint point = (SplinePoint)node;
			switch (name) {
			case "Position":
				lexer.ParseVector2();
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
		
		void ParseGearProperty(Node node, string name)
		{
			SplineGear gear = (SplineGear)node;
			switch (name) {
			case "WidgetName":
				gear.WidgetId = lexer.ParseQuotedString();
				break;
			case "SplineName":
				gear.SplineId = lexer.ParseQuotedString();
				break;
			case "SplineOffset":
				gear.SplineOffset = lexer.ParseFloat();
				break;
			default:
				ParseActorProperty(gear, name);
				break;
			}
		}

		void ParseSplineProperty(Node node, string name)
		{
			switch (name) {
			default:
				ParseGraphicProperty(node, name);
				break;
			}
		}

		void ParseSliderProperty(Node node, string name)
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

		void ParseTextStyleProperty(Node node, string name)
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
		void ParseRichTextProperty(Node node, string name)
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

		void ParseEditProperty(Node node, string name)
		{
			TextBox textBox = (TextBox)node;
			switch (name) {
			case "FontName":
				textBox.Font = new SerializableFont(lexer.ParseQuotedString());
				break;
			case "Text":
				textBox.Text = lexer.ParseQuotedString();
				break;
			case "FontSize":
				textBox.FontHeight = lexer.ParseFloat();
				break;
			case "MaxLength":
				textBox.MaxTextLength = lexer.ParseInt();
				break;
			case "ExclusiveMode":
				lexer.ParseBool();
				break;
			case "CaretChar":
				string caret = lexer.ParseQuotedString();
				if (caret != "") {
					textBox.CaretChar = caret[0];
				}
				break;
			default:
				ParseGraphicProperty(node, name);
				break;
			}
		}

		void ParseFolderBeginProperty(Node node, string name)
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

		void ParseFolderEndProperty(Node node, string name)
		{
			switch (name) {
			default:
				ParseActorProperty(node, name);
				break;
			}
		}

		public delegate void ActorPropReader(Node node, string name);

		struct KnownActorType 
		{
			public string ActorClass;
			public string NodeClass;
			public ActorPropReader PropReader;
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
		
		void RegisterKnownActorTypes()
		{
			knownActorTypes = new KnownActorType[] {
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
				new KnownActorType {ActorClass = "Hot::Edit", NodeClass = "Lime.TextBox, Lime", PropReader = ParseEditProperty},
			};
		}
	}
}
