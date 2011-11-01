using System.IO;
using Lime;
using System.Collections.Generic;

namespace Orange
{
	public partial class HotSceneImporter
	{
		List<int> frames = new List<int> ();
		List<KeyFunction> functions = new List<KeyFunction> ();
		List<object> values = new List<object> ();

		delegate object KeyReader ();

		KeyReader GetKeyReader (string animatorType, string propertyName, string className)
		{
			switch (animatorType) {
			case "Hot::TypedAnimator<Hot::EmitterShape>":
				return delegate() {
					return (EmitterShape)lexer.ParseInt (); };
			case "Hot::TypedAnimator<Hot::BlendMode>":
				return delegate() {
					return lexer.ParseBlendMode (); };
			case "Hot::TypedAnimator<Hot::Color>":
				return delegate() {
					return lexer.ParseColor4 (); };
			case "Hot::TypedAnimator<Hot::Vector2>":
				return delegate() {
					return lexer.ParseVector2 (); };
			case "Hot::TypedAnimator<float>":
				return delegate() {
					return lexer.ParseFloat (); };
			case "Hot::TypedAnimator<int>":
				return delegate() {
					return lexer.ParseInt (); };
			case "Hot::TypedAnimator<bool>":
				return delegate() {
					return lexer.ParseBool (); };
			case "Hot::TypedAnimator<std::basic_string<char,std::char_traits<char>,std::allocator<char>>>":
				switch (propertyName + "@" + className) {
				case "Texture@Hot::Image":
				case "Texture@Hot::DistortionMesh":
				case "Texture@Hot::NineGrid":
					return delegate() {
						return new PersistentTexture (lexer.ParsePath ()); };
				case "File@Audio":
					return delegate() {
						return new PersistentSound (lexer.ParsePath ()); };
				default:
					return delegate() {
						return lexer.ParseQuotedString (); };
				}
			case "Hot::TypedAnimator<Hot::RandomPair>":
				return delegate() {
					return lexer.ParseNumericRange (); };
			case "Hot::TypedAnimator<Hot::Audio::Action>":
				return delegate() {
					return (AudioAction)lexer.ParseInt (); };
			default:
				throw new Exception ("Unknown type of animator '{0}'", animatorType);
			}
		}

		void ParseAnimator (Node node)
		{
			Animator animator = null;
			string type = lexer.ParseQuotedString ();
			frames.Clear ();
			functions.Clear ();
			values.Clear ();
			lexer.ParseToken ('{');
			string propertyName = "";
			string className = "";
			while (lexer.PeekChar() != '}') {
				var name = lexer.ParseWord ();
				switch (name) {
				case "Property":
					string[] s = lexer.ParseQuotedString ().Split ('@');
					propertyName = s [0];
					className = s [1];
					switch (propertyName) {
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
					}
					switch (propertyName + '@' + className) {
					case "TextColor@Hot::Text":
					case "ShadowColor@Hot::Text":
					case "TextColor@Hot::TextPresenter":
					case "ShadowColor@Hot::TextPresenter":
						animator = new Color4Animator ();
						break;
					case "Blending@Hot::MaskedEffect":
						animator = new GenericAnimator<Blending> ();
						break;
					default:
						animator = node.Animators [propertyName]; 
						break;
					}
					break;
				case "Frames":
					lexer.ParseToken ('[');
					while (lexer.PeekChar() != ']')
						frames.Add (lexer.ParseInt ());
					lexer.ParseToken (']');
					break;
				case "Attributes":
					lexer.ParseToken ('[');
					while (lexer.PeekChar() != ']')
						functions.Add ((KeyFunction)lexer.ParseInt ());
					lexer.ParseToken (']');
					break;
				case "Keys":
					KeyReader keyReader = GetKeyReader (type, propertyName, className);
					lexer.ParseToken ('[');
					while (lexer.PeekChar() != ']')
						values.Add (keyReader ());
					lexer.ParseToken (']');
					break;
				default:
					throw new Exception ("Unknown property '{0}'. Parsing: {1}", name, animator);
				}
			}
			lexer.ParseToken ('}');
			for (int i = 0; i < frames.Count; ++i)
				animator.Add (frames [i], values [i], functions [i]);
		}
	}
}
