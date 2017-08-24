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
		private IAnimator particleModifierAspectRatioAnimator;
		private IAnimator particleModifierScaleAnimator;

		delegate object KeyReader();

		KeyReader GetKeyReader(string animatorType, string propertyName, string className)
		{
			switch(animatorType) {
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
				switch(propertyName + "@" + className) {
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
						animator = new Animator<float>();
						break;
					default:
						animator = node.Animators[propertyName];
						break;
					}
					break;
				case "Frames":
					lexer.ParseToken('[');
					while (lexer.PeekChar() != ']')
						frames.Add(lexer.ParseInt());
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
				particleModifierAspectRatioAnimator = animator;
				break;
			case "Scale@Hot::ParticleTemplate":
				particleModifierScaleAnimator = animator;
				break;
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
			var sad = particleModifierScaleAnimator;
			var arad = particleModifierAspectRatioAnimator;
			var scaleAnimator = node.Animators["Scale"];
			for (int i = 0; i < sad.Keys.Count; i++) {
				scaleAnimator.Keys.Add(sad.Keys[i].Frame, new Vector2((float)sad.Keys[i].Value), sad.Keys[i].Function);
			}
			var scaleAnimatorClone = scaleAnimator.Clone();
			for (int i = 0; i < arad.Keys.Count; i++) {
				IKeyframe keyframe = null;
				foreach (var k in scaleAnimator.Keys) {
					if (k.Frame == arad.Keys[i].Frame) {
						keyframe = k;
						break;
					}
				}
				var ar = (float)arad.Keys[i].Value;
				if (keyframe != null) {
					var scale = (Vector2)keyframe.Value;
					var newValue = new Vector2(scale.X * ar, scale.Y / Math.Max(0.0001f, ar));
					keyframe.Value = newValue;
				} else {
					if (scaleAnimator.Keys.Count > 1 && arad.Keys[i].Frame > scaleAnimator.Keys[0].Frame) {
						scaleAnimatorClone.Apply(AnimationUtils.FramesToSeconds(arad.Keys[i].Frame));
						var scale = (node as ParticleModifier).Scale;
						var newValue = new Vector2(scale.X * ar, scale.Y / Math.Max(0.0001f, ar));
						scaleAnimator.Keys.AddOrdered(arad.Keys[i].Frame, newValue, arad.Keys[i].Function);
					} else {
						var scale = (node as ParticleModifier).Scale;
						var implyiedAr = Mathf.Sqrt(scale.X / scale.Y);
						var implyiedScale = scale.Y * implyiedAr;
						var newValue = new Vector2(implyiedScale * ar, implyiedScale / Math.Max(0.0001f, ar));
						scaleAnimator.Keys.AddOrdered(arad.Keys[i].Frame, scale, arad.Keys[i].Function);
					}
				}
			}
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
