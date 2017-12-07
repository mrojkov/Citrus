using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Orange.FbxImporter
{
	public class AnimationData
	{
		public List<Keyframe<Vector3>> positionKeys { get; set; }

		public List<Keyframe<Vector3>> scaleKeys { get; set; }

		public List<Keyframe<Quaternion>> rotationKeys { get; set; }

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
						var animations = layers[j].nodes.FromArrayOfPointersToStructArrayUnsafe<KeyframeCollection>(layers[j].count);
						for (int k = 0; k < layers[j].count; k++) {
							var data = new FbxImporter.AnimationData();
							Animations.Add(data);
							data.MarkerId = stacks[i].name;
							data.Key = animations[k].id;
							data.positionKeys = animations[k].positionKeys.keyframes
								.FromArrayOfPointersToStructArrayUnsafe<AnimationKeyframe>(animations[k].positionKeys.count)
								.Select(key => new Keyframe<Vector3>(AnimationUtils.SecondsToFrames(key.time), key.data.ToStruct<Vec3>().toLime()))
								.ToList();

							data.rotationKeys = animations[k].rotationKeys.keyframes
								.FromArrayOfPointersToStructArrayUnsafe<AnimationKeyframe>(animations[k].rotationKeys.count)
								.Select(key => new Keyframe<Quaternion>(AnimationUtils.SecondsToFrames(key.time), key.data.ToStruct<Vec4>().toLimeQuaternion()))
								.ToList();

							data.scaleKeys = animations[k].scaleKeys.keyframes
								.FromArrayOfPointersToStructArrayUnsafe<AnimationKeyframe>(animations[k].scaleKeys.count)
								.Select(key => new Keyframe<Vector3>(AnimationUtils.SecondsToFrames(key.time), key.data.ToStruct<Vec3>().toLime()))
								.ToList();
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

		[StructLayout(LayoutKind.Sequential, CharSet = ImportConfig.Charset)]
		private class AnimationStack
		{
			[MarshalAs(UnmanagedType.I4)]
			public int count;

			public string name;

			public IntPtr layers;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = ImportConfig.Charset)]
		private class AnimationLayer
		{
			[MarshalAs(UnmanagedType.I4)]
			public int count;

			public string name;

			public IntPtr nodes;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = ImportConfig.Charset)]
		private class KeyframeCollection
		{
			public string id;

			public AnimationKeyframeSet positionKeys;

			public AnimationKeyframeSet scaleKeys;

			public AnimationKeyframeSet rotationKeys;
		}

		[StructLayout(LayoutKind.Sequential)]
		private class AnimationKeyframeSet
		{
			[MarshalAs(UnmanagedType.I4)]
			public int count;

			public IntPtr keyframes;
		}

		[StructLayout(LayoutKind.Sequential)]
		private class AnimationKeyframe
		{
			public double time;
			public IntPtr data;
		}

		#endregion
	}
}
