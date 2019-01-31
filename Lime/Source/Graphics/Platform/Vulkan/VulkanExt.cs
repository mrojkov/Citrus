using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SharpVulkan.Ext
{
	internal struct BufferMemoryRequirementsInfo2
	{
		public StructureType StructureType;
		public IntPtr Next;
		public Buffer Buffer;
	}

	internal struct ImageMemoryRequirementsInfo2
	{
		public StructureType StructureType;
		public IntPtr Next;
		public Image Image;
	}

	internal struct MemoryRequirements2
	{
		public StructureType StructureType;
		public IntPtr Next;
		public MemoryRequirements MemoryRequirements;
	}

	internal struct MemoryDedicatedAllocateInfo
	{
		public StructureType StructureType;
		public IntPtr Next;
		public Image Image;
		public Buffer Buffer;
	}

	internal struct MemoryDedicatedRequirements
	{
		public StructureType StructureType;
		public IntPtr Next;
		public RawBool PrefersDedicatedAllocation;
		public RawBool RequiresDedicatedAllocation;
	}

	internal enum StructureType : int
	{
		MemoryDedicatedRequirements = 1000127000,
		MemoryDedicatedAllocateInfo = 1000127001,
		BufferMemoryRequirementsInfo2 = 1000146000,
		ImageMemoryRequirementsInfo2 = 1000146001,
		MemoryRequirements2 = 1000146003
	}
	
	internal struct MVKConfiguration
	{
	    public SharpVulkan.RawBool debugMode;
 		public SharpVulkan.RawBool shaderConversionFlipVertexY;
		public SharpVulkan.RawBool synchronousQueueSubmits;
		public SharpVulkan.RawBool prefillMetalCommandBuffers;
		public uint maxActiveMetalCommandBuffersPerQueue;
		public SharpVulkan.RawBool supportLargeQueryPools;
		public SharpVulkan.RawBool presentWithCommandBuffer;
		public SharpVulkan.RawBool swapchainMagFilterUseNearest;
		public ulong metalCompileTimeout;
		public SharpVulkan.RawBool performanceTracking;
		public uint performanceLoggingFrameCount;
		public SharpVulkan.RawBool displayWatermark;
	}
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	internal unsafe delegate RawBool DebugReportCallbackDelegate(
			DebugReportFlags flags, DebugReportObjectType objectType, ulong @object,
			PointerSize location, int messageCode, string layerPrefix, string message, IntPtr userData);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal unsafe delegate Result CreateDebugReportCallbackDelegate(
		Instance instance, ref DebugReportCallbackCreateInfo createInfo,
		AllocationCallbacks* allocator, out DebugReportCallback callback);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal unsafe delegate Result DestroyDebugReportCallbackDelegate(
		Instance instance, DebugReportCallback debugReportCallback,
		AllocationCallbacks* allocator);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void GetImageMemoryRequirements2Delegate(Device device, ref ImageMemoryRequirementsInfo2 info, ref MemoryRequirements2 memoryRequirements);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void GetBufferMemoryRequirements2Delegate(Device device, ref BufferMemoryRequirementsInfo2 info, ref MemoryRequirements2 memoryRequirements);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate SharpVulkan.Result GetMoltenVKConfigurationDelegate(SharpVulkan.Instance instance, ref MVKConfiguration pConfiguration, ref UIntPtr pConfigurationSize);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate SharpVulkan.Result SetMoltenVKConfigurationDelegate(SharpVulkan.Instance instance, ref MVKConfiguration pConfiguration, ref UIntPtr pConfigurationSize);

	internal unsafe class VulkanExt
	{
		public CreateDebugReportCallbackDelegate CreateDebugReportCallback;
		public DestroyDebugReportCallbackDelegate DestroyDebugReportCallback;
		public GetImageMemoryRequirements2Delegate GetImageMemoryRequirements2;
		public GetBufferMemoryRequirements2Delegate GetBufferMemoryRequirements2;
		public GetMoltenVKConfigurationDelegate GetMoltenVKConfiguration;
		public SetMoltenVKConfigurationDelegate SetMoltenVKConfiguration;

		public void LoadInstanceEntryPoints(Instance instance)
		{
			var loader = new InstanceEntryPointLoader(instance);
			CreateDebugReportCallback = loader.Load<CreateDebugReportCallbackDelegate>("vkCreateDebugReportCallbackEXT");
			DestroyDebugReportCallback = loader.Load<DestroyDebugReportCallbackDelegate>("vkDestroyDebugReportCallbackEXT");
			GetMoltenVKConfiguration = loader.Load<GetMoltenVKConfigurationDelegate>("vkGetMoltenVKConfigurationMVK");
			SetMoltenVKConfiguration = loader.Load<SetMoltenVKConfigurationDelegate>("vkSetMoltenVKConfigurationMVK");
		}

		public void LoadDeviceEntryPoints(Device device)
		{
			var loader = new DeviceEntryPointLoader(device);
			GetImageMemoryRequirements2 = loader.Load<GetImageMemoryRequirements2Delegate>("vkGetImageMemoryRequirements2KHR");
			GetBufferMemoryRequirements2 = loader.Load<GetBufferMemoryRequirements2Delegate>("vkGetBufferMemoryRequirements2KHR");
		}

		private class InstanceEntryPointLoader : EntryPointLoader
		{
			private Instance instance;

			public InstanceEntryPointLoader(Instance instance)
			{
				this.instance = instance;
			}

			protected override IntPtr GetProcAddress(IntPtr name)
			{
				return instance.GetProcAddress((byte*)name);
			}
		}

		private class DeviceEntryPointLoader : EntryPointLoader
		{
			private Device device;

			public DeviceEntryPointLoader(Device device)
			{
				this.device = device;
			}

			protected override IntPtr GetProcAddress(IntPtr name)
			{
				return device.GetProcAddress((byte*)name);
			}
		}

		private abstract class EntryPointLoader
		{
			public T Load<T>(string name) where T : Delegate
			{
				var namePtr = Marshal.StringToHGlobalAnsi(name);
				try {
					var address = GetProcAddress(namePtr);
					if (address != IntPtr.Zero) {
						return Marshal.GetDelegateForFunctionPointer<T>(address);
					}
					return null;
				} finally {
					Marshal.FreeHGlobal(namePtr);
				}
			}
			protected abstract IntPtr GetProcAddress(IntPtr name);
		}
	}
}
