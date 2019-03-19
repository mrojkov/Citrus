using System;
using System.Collections.Generic;
using System.IO;

namespace Lime.Graphics.Platform.Vulkan
{
	internal unsafe class PipelineCache
	{
		public const int Version = 1;

		private Dictionary<long, byte[]> shaderSpvCache = new Dictionary<long, byte[]>();

		internal SharpVulkan.PipelineCache VKPipelineCache;

		public PlatformRenderContext Context { get; }

		public PipelineCache(PlatformRenderContext context)
		{
			Context = context;
			VKPipelineCache = CreateVKPipelineCache(new byte[0]);
		}

		private SharpVulkan.PipelineCache CreateVKPipelineCache(byte[] initialData)
		{
			fixed (byte* initialDataPtr = initialData) {
				var createInfo = new SharpVulkan.PipelineCacheCreateInfo {
					StructureType = SharpVulkan.StructureType.PipelineCacheCreateInfo,
					InitialData = new IntPtr(initialDataPtr),
					InitialDataSize = initialData.Length
				};
				return Context.Device.CreatePipelineCache(ref createInfo);
			}
		}

		private void SetVKPipelineCache(SharpVulkan.PipelineCache newCache)
		{
			if (VKPipelineCache == newCache) {
				return;
			}
			Context.Device.DestroyPipelineCache(VKPipelineCache);
			VKPipelineCache = newCache;
		}

		public byte[] GetShaderSpv(long hash)
		{
			return shaderSpvCache.TryGetValue(hash, out var spv) ? spv : null;
		}

		public void AddShaderSpv(long hash, byte[] spv)
		{
			shaderSpvCache.Add(hash, spv);
		}

		public void Serialize(BinaryWriter writer)
		{
			var vkPipelineCacheData = Context.Device.GetPipelineCacheData(VKPipelineCache);
			writer.Write(Version);
			writer.Write(vkPipelineCacheData.Length);
			writer.Write(vkPipelineCacheData);
			writer.Write(shaderSpvCache.Count);
			foreach (var i in shaderSpvCache) {
				writer.Write(i.Key);
				writer.Write(i.Value.Length);
				writer.Write(i.Value);
			}
		}

		public bool Deserialize(BinaryReader reader)
		{
			var version = reader.ReadInt32();
			if (version != Version) {
				return false;
			}
			var vkPipelineCacheDataSize = reader.ReadInt32();
			var vkPipelineCacheData = reader.ReadBytes(vkPipelineCacheDataSize);
			var vkPipelineCache = CreateVKPipelineCache(vkPipelineCacheData);
			SetVKPipelineCache(vkPipelineCache);
			shaderSpvCache.Clear();
			var entryCount = reader.ReadInt32();
			for (var i = 0; i < entryCount; i++) {
				var hash = reader.ReadInt64();
				var size = reader.ReadInt32();
				var spv = reader.ReadBytes(size);
				shaderSpvCache.Add(hash, spv);
			}
			return true;
		}
	}
}
