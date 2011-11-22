using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class PersistentSound
	{
		PersistentSoundCore core;

		public PersistentSound ()
		{
			core = SoundPool.Instance.GetPersistentSoundCore ("");
		}

		public PersistentSound (string path)
		{
			core = SoundPool.Instance.GetPersistentSoundCore (path);
		}

		[ProtoMember (1)]
		public string SerializationPath
		{
			get {
				var path = Serialization.ShrinkPath (Path);
				return path;
			}
			set {
				var path = Serialization.ExpandPath (value);
				core = SoundPool.Instance.GetPersistentSoundCore (path);
			}
		}

		public string Path { get { return core.Path; } }
	}

	internal class PersistentSoundCore
	{
		public readonly string Path;

		public PersistentSoundCore (string path)
		{
			Path = path;
		}

		~PersistentSoundCore ()
		{
			Discard ();
		}

		public void Discard ()
		{
			//if (instance != null)
			//{
			//    if (instance is IDisposable)
			//        (instance as IDisposable).Dispose();
			//    instance = null;
			//}
		}

		public void DiscardIfNotUsed ()
		{
			Discard ();
		}
	}

	public sealed class SoundPool
	{
		Dictionary<string, WeakReference> items;
		static readonly SoundPool instance = new SoundPool ();

		public static SoundPool Instance { get { return instance; } }

		private SoundPool ()
		{
			items = new Dictionary<string, WeakReference> ();
		}

		public void DiscardUnused ()
		{
			foreach (WeakReference r in items.Values) {
				if (r.IsAlive)
					(r.Target as PersistentSoundCore).DiscardIfNotUsed ();
			}
		}

		public void DiscardAll ()
		{
			foreach (WeakReference r in items.Values) {
				if (r.IsAlive)
					(r.Target as PersistentSoundCore).Discard ();
			}
		}

		internal PersistentSoundCore GetPersistentSoundCore (string path)
		{
			WeakReference r;
			if (!items.TryGetValue (path, out r) || !r.IsAlive) {
				r = new WeakReference (new PersistentSoundCore (path));
				items [path] = r;
			}
			return r.Target as PersistentSoundCore;
		}
	}
}