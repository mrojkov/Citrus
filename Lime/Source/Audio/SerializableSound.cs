using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class SerializableSound
	{
		public SerializableSound () {}

		public SerializableSound (string path)
		{
			Path = path;
		}

		[ProtoMember (1)]
		public string SerializationPath
		{
			get {
				return Serialization.ShrinkPath (Path);
			}
			set {
				Path = Serialization.ExpandPath (value);
			}
		}

		public string Path;
		
		public AudioChannel Play (AudioChannelGroup group, bool paused, int priority = 0)
		{
			var channel = AudioSystem.LoadSound (Path + ".ogg", group, priority);
			if (!paused) {
				channel.Resume ();
			}
			return channel;
		}
		
		public AudioChannel PlayEffect (bool paused, int priority = 0)
		{
			return Play (AudioChannelGroup.Effects, paused, priority);
		}

		public AudioChannel PlayMusic (bool paused, int priority = 100)
		{
			return Play (AudioChannelGroup.Music, paused, priority);
		}
	}
}