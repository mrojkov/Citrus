using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Orange.FbxImporter
{
	public class AnimationData
	{
		public Matrix44[] Transform { get; set; }

		public double[] TimeSteps { get; set; }

		public string Key { get; set; }

		public string MarkerId { get; set; }
	}

	public class Animation
	{
		public List<FbxImporter.AnimationData> Animations = new List<FbxImporter.AnimationData>();

		public string Name { get; private set; }

		public Animation(IntPtr scene)
		{
			var id = FbxSceneGetAnimations(scene);
			if (id != IntPtr.Zero) {
				var animation = id.ToStruct<AnimationWrapper>();
				if (animation.count == 0)
					return;
				var stacks = animation.stacks.FromArrayOfPointersToStructArrayUnsafe<AnimationStack>(animation.count);
				for (int i = 0; i < animation.count; i++) {
					var layers = stacks[i].layers.FromArrayOfPointersToStructArrayUnsafe<AnimationLayer>(stacks[i].count);
					for (int j = 0; j < stacks[i].count; j++) {
						if (layers[j].count == 0)
							continue;
						var animations = layers[j].nodes.ToStructArray<AnimationData>(layers[j].count);
						for (int k = 0; k < layers[j].count; k++) {
							var data = new FbxImporter.AnimationData();
							Animations.Add(data);
							data.MarkerId = stacks[i].name;
							data.Key = animations[k].key.ToCharArray();
							data.TimeSteps = animations[k].times.ToDoubleArray(animations[k].count);
							data.Transform = animations[k].transforms
								.ToStructArray<Mat4x4>(animations[k].count)
								.Select(v => v.ToLime()).ToArray();
						}
					}
				}
			}
		}

		#region Pinvokes

		[DllImport(ImportConfig.LibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FbxSceneGetAnimations(IntPtr pScene);

		#endregion

		#region MarshalingStructures

		[StructLayout(LayoutKind.Sequential)]
		private class AnimationWrapper
		{
			[MarshalAs(UnmanagedType.I4)]
			public int count;

			public IntPtr stacks;
		}

		[StructLayout(LayoutKind.Sequential)]
		private class AnimationStack
		{
			[MarshalAs(UnmanagedType.I4)]
			public int count;

			[MarshalAs(UnmanagedType.LPStr)]
			public string name;

			public IntPtr layers;
		}

		[StructLayout(LayoutKind.Sequential)]
		private class AnimationLayer
		{
			[MarshalAs(UnmanagedType.I4)]
			public int count;

			[MarshalAs(UnmanagedType.LPStr)]
			public string name;

			public IntPtr nodes;
		}

		[StructLayout(LayoutKind.Sequential)]
		private class AnimationData
		{
			public IntPtr transforms;

			public IntPtr times;

			[MarshalAs(UnmanagedType.I4)]
			public int count;

			public IntPtr key;
		}

		#endregion
	}
}
