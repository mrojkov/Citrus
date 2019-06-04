using System;
using System.Collections.Generic;
using System.IO;

namespace Lime.Graphics.Platform.Vulkan
{
	internal unsafe class PipelineCache : IDisposable
	{
		private Dictionary<long, byte[]> shaderSpvCache = new Dictionary<long, byte[]>();

		internal SharpVulkan.PipelineCache NativePipelineCache;

		public PlatformRenderContext Context { get; }

		public PipelineCache(PlatformRenderContext context)
		{
			Context = context;
			NativePipelineCache = CreateNativePipelineCache(Array.Empty<byte>());
		}

		public void Dispose()
		{
			Discard();
		}

		private void Discard()
		{
			if (NativePipelineCache != SharpVulkan.PipelineCache.Null) {
				Context.Device.DestroyPipelineCache(NativePipelineCache);
				NativePipelineCache = SharpVulkan.PipelineCache.Null;
			}
			shaderSpvCache = null;
		}

		private SharpVulkan.PipelineCache CreateNativePipelineCache(byte[] initialData)
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

		public byte[] GetShaderSpv(long hash)
		{
			return shaderSpvCache.TryGetValue(hash, out var spv) ? spv : null;
		}

		public void AddShaderSpv(long hash, byte[] spv)
		{
			shaderSpvCache.Add(hash, spv);
		}

		private const int FormatMagicNumber = ((int)'P' << 16) | ((int)'L' << 8) | (int)'C';
		private const int FormatVersion = 100;

		public byte[] GetData()
		{
			var stream = new MemoryStream();
			using (var writer = new BinaryWriter(stream)) {
				writer.Write(FormatMagicNumber);
				writer.Write(FormatVersion);
				var nativeData = Context.Device.GetPipelineCacheData(NativePipelineCache);
				writer.Write(nativeData.Length);
				writer.Write(nativeData);
				writer.Write(shaderSpvCache.Count);
				foreach (var (shaderHash, shaderSpv) in shaderSpvCache) {
					writer.Write(shaderHash);
					writer.Write(shaderSpv.Length);
					writer.Write(shaderSpv);
				}
				writer.Flush();
				return stream.ToArray();
			}
		}

		public bool SetData(byte[] data)
		{
			try {
				using (var reader = new BinaryReader(new MemoryStream(data))) {
					if (reader.ReadInt32() != FormatMagicNumber) {
						return false;
					}
					if (reader.ReadInt32() != FormatVersion) {
						return false;
					}
					var nativeDataSize = reader.ReadInt32();
					var nativeData = reader.ReadBytes(nativeDataSize);
					var shaderSpvCount = reader.ReadInt32();
					var shaderSpvMap = new Dictionary<long, byte[]>(shaderSpvCount);
					for (var i = 0; i < shaderSpvCount; i++) {
						var shaderHash = reader.ReadInt64();
						var shaderSpvSize = reader.ReadInt32();
						var shaderSpv = reader.ReadBytes(shaderSpvSize);
						shaderSpvMap.Add(shaderHash, shaderSpv);
					}
					Discard();
					NativePipelineCache = CreateNativePipelineCache(nativeData);
					shaderSpvCache = shaderSpvMap;
					return true;
				}
			} catch (EndOfStreamException) {
				return false;
			}
		}
	}
}
