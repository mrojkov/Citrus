using System;
using System.IO;
using System;
using System.Linq;
using Lime;
using System.Collections.Generic;
using Exception = Lime.Exception;

namespace Orange
{
	public class CompatibilityAnimationEngine : AnimationEngine
	{
		public override void AdvanceAnimation(Animation animation, float delta)
		{
			DefaultAnimationEngine.Instance.AdvanceAnimation(animation, delta / 2);
		}

		public override void ApplyAnimators(Animation animation, bool invokeTriggers)
		{
			DefaultAnimationEngine.Instance.ApplyAnimators(animation, invokeTriggers);
		}

		public override bool TryRunAnimation(Animation animation, string markerId)
		{
			return DefaultAnimationEngine.Instance.TryRunAnimation(animation, markerId);
		}
	}

	public partial class HotSceneImporter
	{
		private HotLexer lexer;
		private List<KnownActorType> knownActorTypes;
		public const string ThumbnailMarker = "{8069CDD4-F02F-4981-A3CB-A0BAD4018D00}";
		private readonly bool isTangerine;
		private readonly string sourcePath;

		public HotSceneImporter(bool isTangerine, string sourcePath = null)
		{
			this.isTangerine = isTangerine;
			this.sourcePath = sourcePath;
			if (!isTangerine && string.IsNullOrEmpty(sourcePath)) {
				// When importing scenes from orange you should provide source path, since it doesn't use one in Serialization
				throw new ArgumentException();
			}
			RegisterKnownActorTypes();
		}

		public Node Import(Stream stream, Node node, INodeThumbnailProvider thumbnailProvider)
		{
			using (TextReader reader = new StreamReader(stream)) {
				string text = reader.ReadToEnd();
				lexer = new HotLexer(sourcePath, text, isTangerine);
				var savedDefaultWidgetSize = Widget.DefaultWidgetSize;
				try {
					Widget.DefaultWidgetSize = new Vector2(100, 100);
					node = ParseNode(node);
				} finally {
					Widget.DefaultWidgetSize = savedDefaultWidgetSize;
				}
				lexer.ReadLine();
				thumbnailProvider?.SetThumbnail(node, ReadThumbnail(lexer));
				if (isTangerine) {
					ConvertFolderBeginEndToDescriptors(node);
					ReplaceAnimationEngine(node);
				}
				return node;
			}
		}

		void ReplaceAnimationEngine(Node node)
		{
			node.DefaultAnimation.AnimationEngine = new CompatibilityAnimationEngine();
			foreach (var child in node.Nodes) {
				ReplaceAnimationEngine(child);
			}
		}

		void ConvertFolderBeginEndToDescriptors(Node node)
		{
			var stack = new Stack<Folder.Descriptor>();
			int j = 0;
			foreach (var n in node.Nodes.ToList()) {
				if (n is FolderBegin) {
					var fb = (FolderBegin)n;
					node.Nodes.RemoveAt(j);
					var fd = new Folder.Descriptor { Id = fb.Id, Expanded = fb.Expanded, Index = j };
					stack.Push(fd);
					if (node.Folders == null) {
						node.Folders = new List<Folder.Descriptor>();
					}
					node.Folders.Add(fd);
				} else if (n is FolderEnd) {
					node.Nodes.RemoveAt(j);
					stack.Pop();
					if (stack.Count > 0) {
						stack.Peek().ItemCount++;
					}
				} else {
					ConvertFolderBeginEndToDescriptors(n);
					if (stack.Count > 0) {
						stack.Peek().ItemCount++;
					}
					j++;
				}
			}
		}

		string ReadThumbnail(HotLexer lexer)
		{
			var l = lexer.ReadLine();
			if (l != ThumbnailMarker) {
				return null;
			}
			var sb = new System.Text.StringBuilder();
			var firstLine = true;
			while ((l = lexer.ReadLine()) != null) {
				if (!firstLine) {
					sb.Append('\r');
					sb.Append('\n');
				}
				firstLine = false;
				sb.Append(l);
			}
			return sb.ToString();
		}

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
				node.TangerineFlags = (TangerineFlags)(lexer.ParseInt() & 7);
				break;
			case "Trigger":
				node.Trigger = lexer.ParseQuotedString();
				break;
			case "Tag":
				node.Tag = lexer.ParseQuotedString();
				break;
			case "Actors":
				lexer.ParseToken('[');
				while (lexer.PeekChar() != ']') {
					var child = ParseNode(null);
					if (child != null)
						node.Nodes.Add(child);
				}
				lexer.ParseToken(']');
				if (node is Widget) {
					var allBones = node.AsWidget.Nodes.OfType<Bone>();
					foreach (var root in allBones.Where(b => b.BaseIndex == 0)) {
						var loc = node.AsWidget.Nodes.IndexOf(root);
						if (loc == 0) {
							loc++;
						}
						var bones = BoneUtils.SortBones(BoneUtils.FindBoneDescendats(root, allBones));
						foreach (var bone in bones) {
							bone.Unlink();
							node.AsWidget.Nodes.Insert(loc, bone);
						}
					}
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
				var t = ParseBlendMode();
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
				try {
					var path = Path.ChangeExtension(pm.Texture.SerializationPath, "png");
					using (var s = isTangerine
						? AssetBundle.Instance.OpenFile(Serialization.ExpandPath(path))
						: System.IO.File.OpenRead(path)) {
						using (var b = new Bitmap(s)) {
							pm.Size = new Vector2(b.Width, b.Height);
						}
					}
				} catch (System.Exception e) {
					Console.WriteLine($"Warning: can't extract size for particle modifier from {pm.Texture.SerializationPath}, {e.Message}");
				}
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
					pm.Scale = ParticleEmitter.ApplyAspectRatio(pm.Scale, ar);
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
				var t = ParseBlendMode();
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
				var flags = lexer.ParseInt();
				audio.Looping = (flags & 4) != 0;
				audio.Bumpable = (flags & 1) == 0;
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
				if (!isTangerine) {
					marker.Frame = lexer.ParseInt() * 2;
				} else {
					marker.Frame = lexer.ParseInt();
				}
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
			var fb = (FolderBegin)node;
			switch (name) {
			case "Expanded":
				fb.Expanded = lexer.ParseBool();
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

		private object CreateObject(string className)
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

		private Node ParseNode(Node node)
		{
			string actorClass = lexer.ParseQuotedString();
			foreach (KnownActorType t in knownActorTypes) {
				if (t.ActorClass == actorClass) {
					if (node == null) {
						node = (Node)CreateObject(t.NodeClass);
					}
					lexer.ParseToken('{');
					while (lexer.PeekChar() != '}')
						t.PropReader(node, lexer.ParseWord());
					lexer.ParseToken('}');
					if (!isTangerine) {
						if (t.ActorClass == "Hot::FolderBegin" || t.ActorClass == "Hot::FolderEnd")
							return null;
					}
					return node;
				}
			}
			throw new Exception("Unknown type of actor '{0}'", actorClass);
		}

		private void ParseLinearLayoutProperty(Node node, string name)
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

		private void RegisterKnownActorTypes()
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
				new KnownActorType {ActorClass = "Hot::FolderBegin", NodeClass = "Orange.FolderBegin, Orange", PropReader = ParseFolderBeginProperty},
				new KnownActorType {ActorClass = "Hot::FolderEnd", NodeClass = "Orange.FolderEnd, Orange", PropReader = ParseFolderEndProperty},
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
