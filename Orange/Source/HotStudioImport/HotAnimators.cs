using System.IO;
using Lime;
using System.Collections.Generic;
using System;

namespace Orange
{
	public partial class HotSceneImporter
	{
		readonly List<int> frames = new List<int>();
		readonly List<KeyFunction> functions = new List<KeyFunction>();
		readonly List<object> values = new List<object>();
		private NumericAnimator particleModifierAspectRatioAnimator;
		private NumericAnimator particleModifierScaleAnimator;

		delegate object KeyReader();

		KeyReader GetKeyReader(string animatorType, string propertyName, string className)
		{
			switch (animatorType) {
			case "Hot::TypedAnimator<Hot::HorizontalAlignment>":
				return () => (HAlignment)lexer.ParseInt();
			case "Hot::TypedAnimator<Hot::VerticalAlignment>":
				return () => (VAlignment)lexer.ParseInt();
			case "Hot::TypedAnimator<Hot::EmitterShape>":
				return () => (EmitterShape)lexer.ParseInt();
			case "Hot::TypedAnimator<Hot::BlendMode>":
				return () => ParseBlendMode();
			case "Hot::TypedAnimator<Hot::Color>":
				return () => lexer.ParseColor4();
			case "Hot::TypedAnimator<Hot::Vector2>":
				return () => lexer.ParseVector2();
			case "Hot::TypedAnimator<float>":
				return () => lexer.ParseFloat();
			case "Hot::TypedAnimator<int>":
				return () => lexer.ParseInt();
			case "Hot::TypedAnimator<bool>":
				return () => lexer.ParseBool();
			case "Hot::TypedAnimator<std::basic_string<char,std::char_traits<char>,std::allocator<char>>>":
				switch (propertyName + "@" + className) {
				case "WidgetId@Hot::Gear":
					return () => new NodeReference<Widget>(lexer.ParseQuotedString());
				case "SplineId@Hot::Gear":
					return () => new NodeReference<Spline>(lexer.ParseQuotedString());
				case "Sample@Hot::Audio":
					return () => new SerializableSample(lexer.ParsePath());
				case "Texture@Hot::Image":
				case "Texture@Hot::DistortionMesh":
				case "Texture@Hot::NineGrid":
					return () => new SerializableTexture(lexer.ParsePath());
				default:
					return () => lexer.ParseQuotedString();
				}
			case "Hot::TypedAnimator<Hot::RandomPair>":
				return () => lexer.ParseNumericRange();
			case "Hot::TypedAnimator<Hot::Audio::Action>":
				return () => (AudioAction)lexer.ParseInt();
			case "Hot::TypedAnimator<Hot::Movie::Action>":
				return () => (MovieAction)lexer.ParseInt();
			case "Hot::TypedAnimator<Hot::EmissionType>":
				return () => (EmissionType) lexer.ParseInt();
			default:
				throw new Lime.Exception("Unknown type of animator '{0}'", animatorType);
			}
		}

		Tuple<Blending, ShaderId> ParseBlendMode()
		{
			Blending blending = Blending.Inherited;
			ShaderId shader = ShaderId.Inherited;
			switch(lexer.ParseInt()) {
				case 0:
				break;
				case 2:
					blending = Blending.Add;
					shader = ShaderId.Diffuse;
				break;
				case 3:
					blending = Blending.Burn;
					shader = ShaderId.Diffuse;
				break;
				case 5:
					blending = Blending.Modulate;
					shader = ShaderId.Diffuse;
				break;
				case 7:
					blending = Blending.Alpha;
					shader = ShaderId.Silhuette;
				break;
				case 8:
					blending = Blending.Opaque;
					shader = ShaderId.Diffuse;
				break;
				default:
					blending = Blending.Alpha;
					shader = ShaderId.Diffuse;
				break;
			}
			return new Tuple<Blending, ShaderId>(blending, shader);
		}

		void ParseAnimator(Node node)
		{
			IAnimator animator = null;
			string type = lexer.ParseQuotedString();
			frames.Clear();
			functions.Clear();
			values.Clear();
			lexer.ParseToken('{');
			string propertyName = "";
			string className = "";
			while (lexer.PeekChar() != '}') {
				var name = lexer.ParseWord();
				switch (name) {
				case "Property":
					string[] s = lexer.ParseQuotedString().Split('@');
					propertyName = s[0];
					className = s[1];
					switch (propertyName) {
					case "AlongTrackOrientation":
						propertyName = "AlongPathOrientation";
						break;
					case "File":
						propertyName = "Sample";
						break;
					case "TexCoordForMins":
						propertyName = "UV0";
						break;
					case "TexCoordForMaxs":
						propertyName = "UV1";
						break;
					case "WidgetName":
						propertyName = "WidgetId";
						break;
					case "TexturePath":
						propertyName = "Texture";
						break;
					case "Anchor":
						propertyName = "Position";
						break;
					case "BlendMode":
						propertyName = "Blending";
						break;
					case "AnimationFPS":
						propertyName = "AnimationFps";
						break;
					case "Life":
						propertyName = "Lifetime";
						break;
					case "FontSize":
						propertyName = "FontHeight";
						break;
					case "HAlign":
						propertyName = "HAlignment";
						break;
					case "VAlign":
						propertyName = "VAlignment";
						break;
					case "SplineName":
						propertyName = "SplineId";
						break;
					case "RandMotionRadius":
						propertyName = "RandomMotionRadius";
						break;
					case "RandMotionRotation":
						propertyName = "RandomMotionRotation";
						break;
					case "RandMotionSpeed":
						propertyName = "RandomMotionSpeed";
						break;
					case "RandMotionAspectRatio":
						propertyName = "RandomMotionAspectRatio";
						break;
					case "TextColor":
						propertyName = "TextColor";
						break;
					case "LineIndent":
						propertyName = "Spacing";
						break;
					}
					switch (propertyName + '@' + className) {
					case "ShadowColor@Hot::Text":
					case "TextColor@Hot::TextPresenter":
					case "ShadowColor@Hot::TextPresenter":
						animator = new Color4Animator();
						break;
					case "Blending@Hot::MaskedEffect":
						animator = new Animator<Blending>();
						break;
					case "AlongTrackOrientation@Hot::ParticleEmitter":
						animator = new Animator<bool>();
						break;
					case "WidgetId@Hot::Gear":
						animator = new Animator<NodeReference<Widget>>();
						break;
					case "SplineId@Hot::Gear":
						animator = new Animator<NodeReference<Spline>>();
						break;
					case "AspectRatio@Hot::ParticleTemplate":
					case "Scale@Hot::ParticleTemplate":
						animator = new NumericAnimator();
						break;
					default:
						animator = node.Animators[propertyName];
						break;
					}
					break;
				case "Frames":
					lexer.ParseToken('[');
					while (lexer.PeekChar() != ']')
						if (!isTangerine) {
							frames.Add(lexer.ParseInt() * 2);
						} else {
							frames.Add(lexer.ParseInt());
						}
					lexer.ParseToken(']');
					break;
				case "Attributes":
					lexer.ParseToken('[');
					while (lexer.PeekChar() != ']')
						functions.Add((KeyFunction)lexer.ParseInt());
					lexer.ParseToken(']');
					break;
				case "Keys":
					KeyReader keyReader = GetKeyReader(type, propertyName, className);
					lexer.ParseToken('[');
					while (lexer.PeekChar() != ']')
						values.Add(keyReader());
					lexer.ParseToken(']');
					break;
				default:
					throw new Lime.Exception("Unknown property '{0}'. Parsing: {1}", name, animator);
				}
			}
			lexer.ParseToken('}');
			if (values.Count > 0 && values[0] is Tuple<Blending, ShaderId>) {
				ProcessBlendingAndShaderAnimators(node, animator);
			} else {
				for (int i = 0; i < frames.Count; i++) {
					animator.Keys.Add(frames[i], values[i], functions[i]);
				}
			}
			switch (propertyName + '@' + className) {
			case "AspectRatio@Hot::ParticleTemplate":
				particleModifierAspectRatioAnimator = animator as NumericAnimator;
				break;
			case "Scale@Hot::ParticleTemplate":
				particleModifierScaleAnimator = animator as NumericAnimator;
				break;
			}
			if (animator.ReadonlyKeys.Count == 0) {
				node.Animators.Remove(animator);
			}
		}

		private void TryMergeScaleAndAspectRatioForParticleTemplate(Node node)
		{
			if (node == null) {
				throw new ArgumentException("node can't be null");
			}
			if (particleModifierScaleAnimator == null && particleModifierAspectRatioAnimator == null) {
				return;
			}
			var scaleAnimator = node.Animators["Scale"];
			var zoomAnimator = particleModifierScaleAnimator;
			var aspectRatioAnimator = particleModifierAspectRatioAnimator;
			int zoomAnimatorIndex = 0;
			int aspectRatioAnimatorIndex = 0;
			float zoom;
			float aspectRatio;
			ParticleEmitter.DecomposeScale((node as ParticleModifier).Scale, out aspectRatio, out zoom);
			while (true) {
				int state;
				int frame;
				KeyFunction keyFunction;
				IKeyframe aspectRatioKey = null;
				IKeyframe zoomKey = null;
				if (aspectRatioAnimator != null && aspectRatioAnimatorIndex == aspectRatioAnimator.Keys.Count) {
					aspectRatioAnimator = null;
				}
				if (zoomAnimator != null && zoomAnimatorIndex == zoomAnimator.Keys.Count) {
					zoomAnimator = null;
				}
				if (zoomAnimator == null && aspectRatioAnimator == null) {
					break;
				}
				if (zoomAnimator != null) {
					zoomKey = zoomAnimator.Keys[zoomAnimatorIndex];
				}
				if (aspectRatioAnimator != null) {
					aspectRatioKey = aspectRatioAnimator.Keys[aspectRatioAnimatorIndex];
				}
				if (zoomAnimator == null) {
					state = 1;
				} else if (aspectRatioAnimator == null) {
					state = -1;
				} else {
					state = zoomKey.Frame.CompareTo(aspectRatioKey.Frame);
				}
				switch (state) {
				case -1:
					frame = zoomKey.Frame;
					keyFunction = zoomKey.Function;
					zoom = (float)zoomKey.Value;
					aspectRatio = aspectRatioAnimator?.CalcValue(AnimationUtils.FramesToSeconds(frame)) ?? aspectRatio;
					zoomAnimatorIndex++;
					break;
				case 0:
					frame = aspectRatioKey.Frame;
					keyFunction = SelectKeyFunction(aspectRatioKey.Function, zoomKey.Function);
					aspectRatio = (float)aspectRatioKey.Value;
					zoom = (float)zoomKey.Value;
					aspectRatioAnimatorIndex++;
					zoomAnimatorIndex++;
					break;
				case 1:
					frame = aspectRatioKey.Frame;
					keyFunction = aspectRatioKey.Function;
					aspectRatio = (float)aspectRatioKey.Value;
					zoom = zoomAnimator?.CalcValue(AnimationUtils.FramesToSeconds(frame)) ?? zoom;
					aspectRatioAnimatorIndex++;
					break;
				default: throw new InvalidOperationException();
				}
				scaleAnimator.Keys.Add(frame, ParticleEmitter.ApplyAspectRatio(zoom, aspectRatio), keyFunction);
			}
		}

		private KeyFunction SelectKeyFunction(KeyFunction a, KeyFunction b)
		{
			if (a == b) {
				return a;
			}
			if (a == KeyFunction.Steep || b == KeyFunction.Steep) {
				return KeyFunction.Steep;
			}
			if (a == KeyFunction.ClosedSpline || b == KeyFunction.ClosedSpline) {
				return KeyFunction.ClosedSpline;
			}
			if (a == KeyFunction.Spline || b == KeyFunction.Spline) {
				return KeyFunction.Spline;
			}
			return KeyFunction.Linear;
		}

		private void ProcessBlendingAndShaderAnimators(Node node, IAnimator animator)
		{
			var shaderAnimator = node.Animators["Shader"];
			for (int i = 0; i < frames.Count; i++) {
				var type = values[i] as Tuple<Blending, ShaderId>;
				animator.Keys.Add(frames[i], type.Item1, functions[i]);
				shaderAnimator.Keys.Add(frames[i], type.Item2, functions[i]);
			}
		}
	}
}
