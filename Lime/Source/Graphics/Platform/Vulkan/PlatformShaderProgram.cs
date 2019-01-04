using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Lime.Graphics.Platform.Vulkan
{
	internal unsafe class PlatformShaderProgram : IPlatformShaderProgram
	{
		private static long referenceHashCounter = 0;

		private PlatformRenderContext context;
		private UniformInfo[] uniformInfos;
		private UniformBufferInfo[] uniformBufferInfos;
		private BackingBuffer[] uniformBuffers;
		private IntPtr uniformStagingData;
		private DescriptorSetUpdateTemplateEntry[] descriptorSetUpdateTemplate;
		private SharpVulkan.ShaderModule vsModule;
		private SharpVulkan.ShaderModule fsModule;
		private SharpVulkan.DescriptorSetLayout descriptorSetLayout;
		private SharpVulkan.PipelineLayout pipelineLayout;
		private ShaderStageMask dirtyStageMask;
		private int combinedImageSamplerCount;
		private int uniformBufferCount;

		internal readonly long ReferenceHash = System.Threading.Interlocked.Increment(ref referenceHashCounter);
		internal SharpVulkan.ShaderModule VSModule => vsModule;
		internal SharpVulkan.ShaderModule FSModule => fsModule;
		internal SharpVulkan.DescriptorSetLayout DescriptorSetLayout => descriptorSetLayout;
		internal SharpVulkan.PipelineLayout PipelineLayout => pipelineLayout;
		internal DescriptorSetUpdateTemplateEntry[] DescriptorSetUpdateTemplate => descriptorSetUpdateTemplate;
		internal BackingBuffer[] UniformBuffers => uniformBuffers;
		internal int CombinedImageSamplerCount => combinedImageSamplerCount;
		internal int UniformBufferCount => uniformBufferCount;

		public PlatformShaderProgram(
			PlatformRenderContext context, IPlatformShader[] shaders,
			ShaderProgram.AttribLocation[] attribLocations, ShaderProgram.Sampler[] samplers)
		{
			this.context = context;
			LinkProgram(shaders, attribLocations, samplers);
		}

		public void Dispose()
		{
			context.Release(pipelineLayout);
			context.Release(descriptorSetLayout);
			context.Release(vsModule);
			context.Release(fsModule);
			Marshal.FreeHGlobal(uniformStagingData);
		}

		public UniformDesc[] GetUniformDescriptions()
		{
			return uniformInfos.Select(i => new UniformDesc {
				Name = i.Name,
				Type = i.Type,
				ArraySize = i.ArraySize
			}).ToArray();
		}

		private void LinkProgram(IPlatformShader[] shaders, ShaderProgram.AttribLocation[] attribLocations, ShaderProgram.Sampler[] samplers)
		{
			PlatformShader vertexShader = null;
			PlatformShader fragmentShader = null;
			foreach (var shader in shaders.Cast<PlatformShader>()) {
				switch (shader.Stage) {
					case ShaderStageMask.Vertex:
						vertexShader = shader;
						break;
					case ShaderStageMask.Fragment:
						fragmentShader = shader;
						break;
				}
			}
			if (vertexShader == null || fragmentShader == null) {
				throw new InvalidOperationException();
			}
			var program = ShaderCompiler.CreateProgram();
			try {
				foreach (var i in attribLocations) {
					ShaderCompiler.BindAttribLocation(program, i.Name, i.Index);
				}
				if (!ShaderCompiler.LinkProgram(program, vertexShader.Shader, fragmentShader.Shader)) {
					var infoLog = ShaderCompiler.GetProgramInfoLog(program);
					throw new InvalidOperationException($"Shader program link failed:\n{infoLog}");
				}
				vsModule = CreateShaderModule(program, ShaderCompiler.Stage.Vertex);
				fsModule = CreateShaderModule(program, ShaderCompiler.Stage.Fragment);
				var uniformInfoLookup = LinkUniforms(program, samplers);
				uniformInfos = uniformInfoLookup.Values.ToArray();
				uniformBufferInfos = BuildUniformBufferUpdateTemplates(program, uniformInfoLookup);
				CreateUniformBuffers();
				CreateDescriptorSetLayout(program, uniformInfoLookup);
				CreatePipelineLayout();
			} finally {
				ShaderCompiler.DestroyProgram(program);
			}
		}

		private SharpVulkan.ShaderModule CreateShaderModule(IntPtr program, ShaderCompiler.Stage stage)
		{
			var code = ShaderCompiler.GetSpv(program, stage);
			var codeSize = ShaderCompiler.GetSpvSize(program, stage);
			var createInfo = new SharpVulkan.ShaderModuleCreateInfo {
				StructureType = SharpVulkan.StructureType.ShaderModuleCreateInfo,
				CodeSize = codeSize,
				Code = code
			};
			return context.Device.CreateShaderModule(ref createInfo);
		}

		private void CreateUniformBuffers()
		{
			uniformBuffers = new BackingBuffer[uniformBufferInfos.Length];
			for (var i = 0; i < uniformBuffers.Length; i++) {
				var size = uniformBufferInfos[i].UpdateTemplate.Max(entry => entry.BufferOffset + entry.Size);
				var memoryPropertyFlags = SharpVulkan.MemoryPropertyFlags.HostVisible | SharpVulkan.MemoryPropertyFlags.HostCoherent;
				uniformBuffers[i] = new BackingBuffer(context, SharpVulkan.BufferUsageFlags.UniformBuffer, memoryPropertyFlags, (ulong)size);
			}
			var stagingDataSize = uniformInfos.Max(ui => ui.StagingOffset + ui.ColumnStride * ui.ColumnCount * ui.ArraySize);
			uniformStagingData = Marshal.AllocHGlobal(stagingDataSize);
		}

		private static ShaderStageMask ConvertShaderStage(ShaderCompiler.Stage stage)
		{
			switch (stage) {
				case ShaderCompiler.Stage.Vertex:
					return ShaderStageMask.Vertex;
				case ShaderCompiler.Stage.Fragment:
					return ShaderStageMask.Fragment;
				default:
					throw new ArgumentException(nameof(stage));
			}
		}

		private static ShaderVariableType ConvertShaderVariableType(ShaderCompiler.VariableType type)
		{
			switch (type) {
				case ShaderCompiler.VariableType.Bool:
					return ShaderVariableType.Bool;
				case ShaderCompiler.VariableType.BoolVector2:
					return ShaderVariableType.BoolVector2;
				case ShaderCompiler.VariableType.BoolVector3:
					return ShaderVariableType.BoolVector3;
				case ShaderCompiler.VariableType.BoolVector4:
					return ShaderVariableType.BoolVector4;
				case ShaderCompiler.VariableType.Int:
					return ShaderVariableType.Int;
				case ShaderCompiler.VariableType.IntVector2:
					return ShaderVariableType.IntVector2;
				case ShaderCompiler.VariableType.IntVector3:
					return ShaderVariableType.IntVector3;
				case ShaderCompiler.VariableType.IntVector4:
					return ShaderVariableType.IntVector4;
				case ShaderCompiler.VariableType.Float:
					return ShaderVariableType.Float;
				case ShaderCompiler.VariableType.FloatVector2:
					return ShaderVariableType.FloatVector2;
				case ShaderCompiler.VariableType.FloatVector3:
					return ShaderVariableType.FloatVector3;
				case ShaderCompiler.VariableType.FloatVector4:
					return ShaderVariableType.FloatVector4;
				case ShaderCompiler.VariableType.FloatMatrix2:
					return ShaderVariableType.FloatMatrix2;
				case ShaderCompiler.VariableType.FloatMatrix3:
					return ShaderVariableType.FloatMatrix3;
				case ShaderCompiler.VariableType.FloatMatrix4:
					return ShaderVariableType.FloatMatrix4;
				case ShaderCompiler.VariableType.Sampler2D:
					return ShaderVariableType.Sampler2D;
				case ShaderCompiler.VariableType.SamplerCube:
					return ShaderVariableType.SamplerCube;
				default:
					throw new ArgumentException(nameof(type));
			}
		}

		private SharpVulkan.ShaderStageFlags GetVKShaderStageFlags(ShaderCompiler.Stage stage)
		{
			switch (stage) {
				case ShaderCompiler.Stage.Vertex:
					return SharpVulkan.ShaderStageFlags.Vertex;
				case ShaderCompiler.Stage.Fragment:
					return SharpVulkan.ShaderStageFlags.Fragment;
				default:
					throw new ArgumentException(nameof(stage));
			}
		}

		private static string AdjustUniformName(string name)
		{
			var dotIndex = name.LastIndexOf('.');
			var bracketIndex = name.LastIndexOf('[');
			if (bracketIndex > dotIndex) {
				return name.Remove(bracketIndex);
			}
			return name;
		}

		private static Dictionary<string, UniformInfo> LinkUniforms(IntPtr program, ShaderProgram.Sampler[] samplers)
		{
			var infos = new Dictionary<string, UniformInfo>();
			var uniformCount = ShaderCompiler.GetActiveUniformCount(program);
			for (var i = 0; i < uniformCount; i++) {
				var name = AdjustUniformName(ShaderCompiler.GetActiveUniformName(program, i));
				var type = ConvertShaderVariableType(ShaderCompiler.GetActiveUniformType(program, i));
				var stage = ConvertShaderStage(ShaderCompiler.GetActiveUniformStage(program, i));
				var arraySize = ShaderCompiler.GetActiveUniformArraySize(program, i);
				if (infos.TryGetValue(name, out var info)) {
					if (info.Type != type) {
						throw new InvalidOperationException($"Uniform type mismatch: {name}");
					}
					info.ArraySize = Math.Max(info.ArraySize, arraySize);
					info.StageMask |= stage;
				} else {
					infos.Add(name, new UniformInfo {
						Name = name,
						Type = type,
						ArraySize = arraySize,
						StageMask = stage,
						StagingOffset = -1,
						TextureSlot = -1
					});
				}
			}
			var stagingOffset = 0;
			foreach (var info in infos.Values) {
				if (info.Type.IsSampler()) {
					info.TextureSlot = samplers.First(sampler => sampler.Name == info.Name).Stage;
				} else {
					info.StagingOffset = stagingOffset;
					info.ColumnCount = info.Type.GetColumnCount();
					info.ColumnSize = info.Type.GetRowCount() * 4;
					info.ColumnStride = 16;
					stagingOffset += info.ColumnStride * info.ColumnCount * info.ArraySize;
				}
			}
			return infos;
		}

		private UniformBufferInfo[] BuildUniformBufferUpdateTemplates(IntPtr program, Dictionary<string, UniformInfo> uniformInfoLookup)
		{
			var uniformBlockCount = ShaderCompiler.GetActiveUniformBlockCount(program);
			var uniformCount = ShaderCompiler.GetActiveUniformCount(program);
			var uniformBufferUpdateTemplates = new List<UniformBufferUpdateTemplateEntry>[uniformBlockCount];
			for (var i = 0; i < uniformBlockCount; i++) {
				uniformBufferUpdateTemplates[i] = new List<UniformBufferUpdateTemplateEntry>();
			}
			for (var i = 0; i < uniformCount; i++) {
				var blockIndex = ShaderCompiler.GetActiveUniformBlockIndex(program, i);
				if (blockIndex >= 0) {
					var name = AdjustUniformName(ShaderCompiler.GetActiveUniformName(program, i));
					var blockOffset = ShaderCompiler.GetActiveUniformBlockOffset(program, i);
					var arraySize = ShaderCompiler.GetActiveUniformArraySize(program, i);
					var info = uniformInfoLookup[name];
					uniformBufferUpdateTemplates[blockIndex].Add(new UniformBufferUpdateTemplateEntry {
						StagingOffset = info.StagingOffset,
						BufferOffset = blockOffset,
						Size = info.ColumnStride * info.ColumnCount * arraySize
					});
				}
			}
			var uniformBufferInfos = new UniformBufferInfo[uniformBlockCount];
			for (var i = 0; i < uniformBlockCount; i++) {
				uniformBufferInfos[i] = new UniformBufferInfo {
					Stage = ConvertShaderStage(ShaderCompiler.GetActiveUniformBlockStage(program, i)),
					UpdateTemplate = uniformBufferUpdateTemplates[i].ToArray()
				};
			}
			return uniformBufferInfos;
		}

		private void CreateDescriptorSetLayout(IntPtr program, Dictionary<string, UniformInfo> uniformInfoLookup)
		{
			var updateTemplate = new List<DescriptorSetUpdateTemplateEntry>();
			var bindings = new List<SharpVulkan.DescriptorSetLayoutBinding>();
			var uniformBlockCount = ShaderCompiler.GetActiveUniformBlockCount(program);
			for (var i = 0; i < uniformBlockCount; i++) {
				var binding = ShaderCompiler.GetActiveUniformBlockBinding(program, i);
				bindings.Add(new SharpVulkan.DescriptorSetLayoutBinding {
					Binding = (uint)binding,
					DescriptorType = SharpVulkan.DescriptorType.UniformBuffer,
					DescriptorCount = 1,
					StageFlags = GetVKShaderStageFlags(ShaderCompiler.GetActiveUniformBlockStage(program, i))
				});
				updateTemplate.Add(new DescriptorSetUpdateTemplateEntry {
					Binding = binding,
					DescriptorType = SharpVulkan.DescriptorType.UniformBuffer,
					BufferSlot = i,
					TextureSlot = -1
				});
				uniformBufferCount++;
			}
			var uniformCount = ShaderCompiler.GetActiveUniformCount(program);
			for (var i = 0; i < uniformCount; i++) {
				var name = AdjustUniformName(ShaderCompiler.GetActiveUniformName(program, i));
				var info = uniformInfoLookup[name];
				if (info.TextureSlot >= 0) {
					var binding = ShaderCompiler.GetActiveUniformBinding(program, i);
					bindings.Add(new SharpVulkan.DescriptorSetLayoutBinding {
						Binding = (uint)binding,
						DescriptorType = SharpVulkan.DescriptorType.CombinedImageSampler,
						DescriptorCount = 1,
						StageFlags = GetVKShaderStageFlags(ShaderCompiler.GetActiveUniformStage(program, i))
					});
					updateTemplate.Add(new DescriptorSetUpdateTemplateEntry {
						Binding = binding,
						DescriptorType = SharpVulkan.DescriptorType.CombinedImageSampler,
						BufferSlot = -1,
						TextureSlot = info.TextureSlot
					});
					combinedImageSamplerCount++;
				}
			}
			fixed (SharpVulkan.DescriptorSetLayoutBinding* bindingsPtr = bindings.ToArray()) {
				var layoutCreateInfo = new SharpVulkan.DescriptorSetLayoutCreateInfo {
					StructureType = SharpVulkan.StructureType.DescriptorSetLayoutCreateInfo,
					BindingCount = (uint)bindings.Count,
					Bindings = new IntPtr(bindingsPtr)
				};
				descriptorSetLayout = context.Device.CreateDescriptorSetLayout(ref layoutCreateInfo);
				descriptorSetUpdateTemplate = updateTemplate.ToArray();
			}
		}

		private void CreatePipelineLayout()
		{
			var setLayout = descriptorSetLayout;
			var createInfo = new SharpVulkan.PipelineLayoutCreateInfo {
				StructureType = SharpVulkan.StructureType.PipelineLayoutCreateInfo,
				SetLayoutCount = 1,
				SetLayouts = new IntPtr(&setLayout)
			};
			pipelineLayout = context.Device.CreatePipelineLayout(ref createInfo);
		}

		public void SetUniform(int index, IntPtr data, int elementCount)
		{
			var info = uniformInfos[index];
			if (info.StagingOffset < 0) {
				throw new InvalidOperationException();
			}
			elementCount = Math.Min(elementCount, info.ArraySize);
			var dstData = uniformStagingData + info.StagingOffset;
			if (info.ColumnStride == info.ColumnSize) {
				GraphicsUtility.CopyMemory(dstData, data, info.ColumnSize * info.ColumnCount * elementCount);
			} else {
				var totalCols = info.ColumnCount * elementCount;
				for (var i = 0; i < totalCols; i++) {
					GraphicsUtility.CopyMemory(dstData, data, info.ColumnSize);
					dstData += info.ColumnStride;
					data += info.ColumnSize;
				}
			}
			dirtyStageMask |= info.StageMask;
		}

		internal void UpdateUniformBuffers(ulong fenceValue)
		{
			for (var i = 0; i < uniformBuffers.Length; i++) {
				var buffer = uniformBuffers[i];
				var bufferInfo = uniformBufferInfos[i];
				if ((dirtyStageMask & bufferInfo.Stage) == 0) {
					continue;
				}
				buffer.DiscardSlice(fenceValue);
				var bufferData = buffer.MapSlice();
				foreach (var templateEntry in bufferInfo.UpdateTemplate) {
					var dstData = bufferData + templateEntry.BufferOffset;
					var srcData = uniformStagingData + templateEntry.StagingOffset;
					GraphicsUtility.CopyMemory(dstData, srcData, templateEntry.Size);
				}
				buffer.UnmapSlice();
			}
			dirtyStageMask = ShaderStageMask.None;
		}

		internal struct DescriptorSetUpdateTemplateEntry
		{
			public int Binding;
			public SharpVulkan.DescriptorType DescriptorType;
			public int BufferSlot;
			public int TextureSlot;
		}

		private class UniformBufferInfo
		{
			public ShaderStageMask Stage;
			public UniformBufferUpdateTemplateEntry[] UpdateTemplate;
		}

		private struct UniformBufferUpdateTemplateEntry
		{
			public int StagingOffset;
			public int BufferOffset;
			public int Size;
		}

		private class UniformInfo
		{
			public string Name;
			public ShaderVariableType Type;
			public ShaderStageMask StageMask;
			public int ArraySize;
			public int ColumnCount;
			public int ColumnSize;
			public int ColumnStride;
			public int TextureSlot;
			public int StagingOffset;
		}
	}
}
