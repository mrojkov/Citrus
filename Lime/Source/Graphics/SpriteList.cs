using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class Sprite
	{
		public int Tag;
		public IMaterial Material;
		public Color4 Color;
		public Vector2 UV0;
		public Vector2 UV1;
		public Vector2 Position;
		public Vector2 Size;
		
		public interface IMaterialProvider
		{
			IMaterial GetMaterial(ITexture texture, int tag);
		}
		
		public class DefaultMaterialProvider : IMaterialProvider
		{
			public static readonly DefaultMaterialProvider Instance = new DefaultMaterialProvider();

			private Blending blending;
			private ShaderId shader;
			private ITexture texture;
			private IMaterial material;
			
			public void Init(Blending blending, ShaderId shader)
			{
				this.blending = blending;
				this.shader = shader;
				texture = null;
				material = null;
			}
			
			public IMaterial GetMaterial(ITexture texture, int tag)
			{
				if (texture != this.texture) {
					this.texture = texture;
					material = WidgetMaterial.GetInstance(blending, shader, null, texture);
				}
				return material;
			}
		}
		
		public class LcdFontMaterialProvider : IMaterialProvider
		{
			public static readonly LcdFontMaterialProvider Instance = new LcdFontMaterialProvider();

			private ITexture texture;
			private IMaterial material;
			private readonly Dictionary<ITexture, IMaterial> materials = new Dictionary<ITexture, IMaterial>();
			
			public void Init()
			{
				texture = null;
				material = null;
			}
			
			public IMaterial GetMaterial(ITexture texture, int tag)
			{
				if (texture != this.texture) {
					this.texture = texture;
					if (!materials.TryGetValue(texture, out material)) {
						material = new LcdFontMaterial(texture);
						materials.Add(texture, material);
					}
				}
				return material;
			}
		}
		
		public class LcdFontMaterial : IMaterial
		{
			private static string vs = @"
				attribute vec4 inPos;
				attribute vec4 inColor;
				attribute vec2 inTexCoords1;
				varying lowp vec4 color;
				varying lowp vec2 texCoords1;
				uniform mat4 matProjection;
				void main()
				{
					gl_Position = matProjection * inPos;
					color = inColor;
					texCoords1 = inTexCoords1;
				}";

			private static string fsPass1 = @"
				varying lowp vec2 texCoords1;
				uniform lowp sampler2D tex1;
				void main()
				{
					gl_FragColor = texture2D(tex1, texCoords1);
				}";

			private static string fsPass2 = @"
				varying lowp vec2 texCoords1;
				varying lowp vec4 color;
				uniform lowp sampler2D tex1;
				void main()
				{
					gl_FragColor = color * texture2D(tex1, texCoords1);
				}";
		
			public static readonly ShaderProgram shaderProgramPass1;
			public static readonly ShaderProgram shaderProgramPass2;
			
			public readonly ITexture Texture;
			
			public int PassCount => 2;
			
			static LcdFontMaterial()
			{
				shaderProgramPass1 = new ShaderProgram(
					new Shader[] { new VertexShader(vs), new FragmentShader(fsPass1) }, 
					ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers());
					
				shaderProgramPass2 = new ShaderProgram(
					new Shader[] { new VertexShader(vs), new FragmentShader(fsPass2) }, 
					ShaderPrograms.Attributes.GetLocations(), ShaderPrograms.GetSamplers());
			}
			
			public LcdFontMaterial(ITexture texture)
			{
				Texture = texture;
			}
					
			public IMaterial Clone() => (WidgetMaterial)MemberwiseClone();
			
			public void Apply(int pass)
			{
				PlatformRenderer.SetTexture(Texture, 0);
				PlatformRenderer.SetTexture(null, 1);
				if (pass == 0) {
					PlatformRenderer.SetBlending(Blending.LcdTextFirstPass, false);
					PlatformRenderer.SetShaderProgram(shaderProgramPass1);
				} else {
					PlatformRenderer.SetBlending(Blending.LcdTextSecondPass, false);
					PlatformRenderer.SetShaderProgram(shaderProgramPass2);
				}
			}
			
			public void Invalidate() { }
		}
	}

	public class SpriteList
	{
		private List<ISpriteWrapper> items = new List<ISpriteWrapper>();

		private interface ISpriteWrapper
		{
			void AddToList(List<Sprite> sprites, Sprite.IMaterialProvider materialProvider);
			bool HitTest(Vector2 point, out int tag);
		}

		private class NormalSprite : Sprite, ISpriteWrapper
		{
			public ITexture Texture;

			public void AddToList(List<Sprite> sprites, Sprite.IMaterialProvider provider)
			{
				Material = provider.GetMaterial(Texture, Tag);
				sprites.Add(this);
			}

			public bool HitTest(Vector2 point, out int tag)
			{
				var a = Position;
				var b = Position + Size;
				tag = Tag;
				return point.X >= a.X && point.Y >= a.Y && point.X < b.X && point.Y < b.Y;
			}
		}

		public struct CharDef
		{
			public FontChar FontChar;
			public Vector2 Position;

			public Vector2 Size(float fontHeight)
			{
				return new Vector2(fontHeight / FontChar.Height * FontChar.Width, fontHeight);
			}
		}

		private class TextSprite : ISpriteWrapper
		{
			public int Tag;
			public Color4 Color;
			public IFont Font;
			public float FontHeight;
			public CharDef[] CharDefs;
			private static Sprite[] buffer = new Sprite[50];
			public static int Index;

			public void AddToList(List<Sprite> sprites, Sprite.IMaterialProvider provider)
			{
				foreach (CharDef cd in CharDefs) {
					var s = GetNextSprite();
					s.Tag = Tag;
					s.Position = cd.Position;
					s.Size = cd.Size(FontHeight);
					s.UV0 = cd.FontChar.UV0;
					s.UV1 = cd.FontChar.UV1;
					var texture = cd.FontChar.Texture;
					texture.TransformUVCoordinatesToAtlasSpace(ref s.UV0);
					texture.TransformUVCoordinatesToAtlasSpace(ref s.UV1);
					s.Color = Color;
					if (cd.FontChar.RgbIntensity) {
						s.Material = Sprite.LcdFontMaterialProvider.Instance.GetMaterial(texture, Tag);
					} else {
						s.Material = provider.GetMaterial(texture, Tag);
					}
					sprites.Add(s);
				}
			}
			
			private Sprite GetNextSprite()
			{
				if (Index >= buffer.Length) {
					Array.Resize(ref buffer, (int)(buffer.Length * 1.5));
				}
				if (buffer[Index] == null) buffer[Index] = new Sprite();
				return buffer[Index++];
			}

			public bool HitTest(Vector2 point, out int tag)
			{
				foreach (var cd in CharDefs) {
					var a = cd.Position;
					var b = cd.Position + cd.Size(FontHeight);
					if (point.X >= a.X && point.Y >= a.Y && point.X < b.X && point.Y < b.Y) {
						tag = Tag;
						return true;
					}
				}
				tag = -1;
				return false;
			}
		}

		public void Add(
			ITexture texture, Color4 color, Vector2 position, Vector2 size, Vector2 uv0, Vector2 uv1, int tag)
		{
			texture.TransformUVCoordinatesToAtlasSpace(ref uv0);
			texture.TransformUVCoordinatesToAtlasSpace(ref uv1);
			items.Add(new NormalSprite {
				Texture = texture,
				Color = color,
				Position = position,
				Size = size,
				UV0 = uv0,
				UV1 = uv1,
				Tag = tag,
			});
		}

		public void Add(IFont font, Color4 color, float fontHeight, CharDef[] charDefs, int tag)
		{
			items.Add(new TextSprite {
				Font = font,
				Color = color,
				FontHeight = fontHeight,
				CharDefs = charDefs,
				Tag = tag,
			});
		}
		
		public void Clear()
		{
			items.Clear();
		}
		
		private static List<Sprite> sprites = new List<Sprite>();

		public void Render(Color4 color, Blending blending, ShaderId shader)
		{
			Sprite.DefaultMaterialProvider.Instance.Init(blending, shader);
			Sprite.LcdFontMaterialProvider.Instance.Init();
			Render(color, Sprite.DefaultMaterialProvider.Instance);
		}

		public void Render(Color4 color, Sprite.IMaterialProvider materialProvider)
		{
			TextSprite.Index = 0;
			foreach (var w in items) {
				w.AddToList(sprites, materialProvider);
			}
			Renderer.DrawSpriteList(sprites, color);
			sprites.Clear();
		}

		public bool HitTest(Matrix32 worldTransform, Vector2 point, out int tag)
		{
			point = worldTransform.CalcInversed().TransformVector(point);
			foreach (var s in items) {
				if (s.HitTest(point, out tag)) {
					return true;
				}
			}
			tag = -1;
			return false;
		}
	}
}
