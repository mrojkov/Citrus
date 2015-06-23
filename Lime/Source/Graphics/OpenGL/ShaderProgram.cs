#if OPENGL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using OpenTK.Graphics;
#if iOS || ANDROID
using OpenTK.Graphics.ES20;
#else
using OpenTK.Graphics.OpenGL;
#endif

using ProtoBuf;

namespace Lime
{
	public class ShaderProgram : IGLObject
	{
		[ProtoContract]
		public class Asset
		{
			[ProtoMember(1)]
			public string VertexShaderSource;

			[ProtoMember(2)]
			public string FragmentShaderSource;
		}

		public class AttribLocation
		{
			public string Name;
			public int Index;
		}

		public static readonly ShaderProgram Default = LoadFromResources("Default");
		public static readonly ShaderProgram SingleTextureDiffuse = LoadFromResources("SingleTextureDiffuse");
		public static readonly ShaderProgram SingleTextureSilhouette = LoadFromResources("SingleTextureSilhouette");
		public static readonly ShaderProgram DualTextureDiffuse = LoadFromResources("DualTextureDiffuse");
		public static readonly ShaderProgram DualTextureSilhouette = LoadFromResources("DualTextureSilhouette");
		public static readonly ShaderProgram InversedSilhouette = LoadFromResources("InversedSilhouette");

		private int handle;
		private Shader vertexShader;
		private Shader fragmentShader;
		private Dictionary<string, ShaderParameter> parameters;

		public ShaderParameter ProjectionParameter { get; private set; }
		
		internal MaterialPass CachedMaterialPass;

		private ShaderProgram(Shader vertexShader, Shader fragmentShader)
		{
			this.vertexShader = vertexShader;
			this.fragmentShader = fragmentShader;
			GLObjectRegistry.Instance.Add(this);
		}

		private void Create()
		{
			handle = GL.CreateProgram();
			GL.AttachShader(handle, vertexShader.GetHandle());
			GL.AttachShader(handle, fragmentShader.GetHandle());
			BindAttributes();
			Link();
			PrepareParameters();
			BindSamplers();
			CachedMaterialPass = null;
		}

		private void PrepareParameters()
		{
			if (parameters == null) {
				InitializeParameters();
				ProjectionParameter = FindParameter("Projection");
			}
		}

		private void BindAttributes()
		{
			foreach (var i in PlatformMesh.Attributes.GetLocations()) {
				GL.BindAttribLocation(handle, i.Index, i.Name);
			}
		}

		private void BindSamplers()
		{
			Use();
			foreach (var i in GetSamplers()) {
				i.SetInt(i.SamplerStage);
			}
		}

		private IEnumerable<ShaderParameter> GetSamplers()
		{
			return parameters.Values.Where(p => p.IsSampler);
		}

		~ShaderProgram()
		{
			Discard();
		}

		public void Dispose()
		{
			Discard();
			GC.SuppressFinalize(this);
		}

		public void Discard()
		{
			if (handle != 0) {
				Application.InvokeOnMainThread(() => {
					GL.DeleteProgram(handle);
				});
				handle = 0;
			}
		}

		private void Link()
		{
			GL.LinkProgram(handle);
			var result = new int[1];
			GL.GetProgram(handle, ProgramParameter.LinkStatus, result);
			if (result[0] == 0) {
				var infoLog = GetLinkLog();
				Logger.Write("Shader program link log:\n{0}", infoLog);
				throw new Lime.Exception(infoLog.ToString());
			}
			PlatformRenderer.CheckErrors();
		}

		private string GetLinkLog()
		{
			var logLength = new int[1];
			GL.GetProgram(handle, ProgramParameter.InfoLogLength, logLength);
			if (logLength[0] > 0) {
				var infoLog = new System.Text.StringBuilder(logLength[0]);
				unsafe {
					GL.GetProgramInfoLog(handle, logLength[0], (int*)null, infoLog);
				}
				return infoLog.ToString();
			}
			return "";
		}

		public void Use()
		{
			if (handle == 0) {
				Create();
			}
			GL.UseProgram(handle);
		}

		private void InitializeParameters()
		{
			parameters = new Dictionary<string, ShaderParameter>();
			int count;
			GL.GetProgram(handle, ProgramParameter.ActiveUniforms, out count);
			var samplerCount = 0;
			for (var i = 0; i < count; i++) {
				ActiveUniformType type;
				int size;
				var name = GL.GetActiveUniform(handle, i, out size, out type);
				var p = new ShaderParameter {
					Location = GL.GetUniformLocation(handle, name)
				};
				if (type == ActiveUniformType.Sampler2D) {
					p.SamplerStage = samplerCount++;
					if (name.EndsWith(".opaque")) {
						name = name.Remove(name.Length - ".opaque".Length);
						p.SamplerBlendLocation = GL.GetUniformLocation(handle, name + ".blend");
						p.SamplerAlphaLocation = GL.GetUniformLocation(handle, name + ".alpha");
						p.SamplerAlphaStage = samplerCount++;
					}
				}
				parameters[name] = p;
			}
		}

		public ShaderParameter FindParameter(string name)
		{
			ShaderParameter p;
			return parameters.TryGetValue(name, out p) ? p : ShaderParameter.Null;
		}

		public static ShaderProgram LoadFromBundle(string path)
		{
			var asset = Serialization.ReadObject<Asset>(path);
			return LoadFromAsset(asset);
		}

		private static ShaderProgram LoadFromResources(string name)
		{
#if ANDROID
			name = string.Format("Lime.Resources.Shaders.Android.Assets.{0}.shader", name);
#else
			name = string.Format("Lime.Resources.Shaders.Win.iOS.Assets.{0}.shader", name);
#endif
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name)) {
				var asset = Serialization.ReadObject<Asset>(name, stream);
				return LoadFromAsset(asset);
			}
		}

		private static ShaderProgram LoadFromAsset(Asset asset)
		{
			return new ShaderProgram(
				ShaderPool.VertexShaders[asset.VertexShaderSource],
				ShaderPool.FragmentShaders[asset.FragmentShaderSource]
			);
		}
	}

	public sealed class ShaderProgramPool
	{
		public static readonly ShaderProgramPool Instance = new ShaderProgramPool();

		private Dictionary<string, ShaderProgram> items = new Dictionary<string, ShaderProgram>();

		public ShaderProgram this[string path]
		{
			get { return GetProgram(path); }
		}

		private ShaderProgramPool() { }

		private ShaderProgram GetProgram(string path)
		{
			ShaderProgram program;
			if (items.TryGetValue(path, out program)) {
				return program;
			}
			program = ShaderProgram.LoadFromBundle(path);
			items[path] = program;
			return program;
		}
	}
}
#endif