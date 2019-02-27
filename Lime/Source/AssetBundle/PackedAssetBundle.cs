using System.IO;
using System.IO.Compression;
using System;
using System.Collections.Generic;
using System.Linq;
using Lzma;

namespace Lime
{
	struct AssetDescriptor
	{
		public DateTime ModificationTime;
		public byte[] CookingRulesSHA1;
		public int Offset;
		public int Length;
		public int AllocatedSize;
		public AssetAttributes Attributes;
		public string SourceExtension;
	}

	public static class AssetPath
	{
		public static string GetDirectoryName(string path)
		{
			return CorrectSlashes(Path.GetDirectoryName(path));
		}

		public static string Combine(string path1, string path2)
		{
			return CorrectSlashes(Path.Combine(path1, path2));
		}

		public static string CorrectSlashes(string path)
		{
			return path.IndexOf('\\') >= 0 ? path.Replace('\\', '/') : path;
		}
	}

	public sealed class AssetStream : Stream
	{
		readonly PackedAssetBundle bundle;
		internal AssetDescriptor descriptor;
		int position;
		Stream stream;

		public AssetStream(PackedAssetBundle bundle, string path)
		{
			this.bundle = bundle;
			if (!bundle.index.TryGetValue(AssetPath.CorrectSlashes(path), out descriptor)) {
				throw new Exception("Can't open asset: {0}", path);
			}
			stream = bundle.AllocStream();
			Seek(0, SeekOrigin.Begin);
		}

		public override bool CanRead => true;

		public override bool CanWrite => false;

		public override long Length => descriptor.Length;

		public override long Position {
			get {
				return position;
			}
			set {
				Seek(value, SeekOrigin.Begin);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (stream != null) {
				bundle.ReleaseStream(stream);
				stream = null;
			}
		}

		public override bool CanSeek {
			get {
				return true;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			count = Math.Min(count, descriptor.Length - position);
			if (count > 0) {
				count = stream.Read(buffer, offset, count);
				if (count < 0)
					return count;
				position += count;
			} else {
				count = 0;
			}
			return count;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			if (origin == SeekOrigin.Begin) {
				position = (int)offset;
			} else if (origin == SeekOrigin.Current) {
				position += (int)offset;
			} else {
				position = descriptor.Length - (int)offset;
			}
			position = Math.Max(0, Math.Min(position, descriptor.Length));
			stream.Seek(position + descriptor.Offset, SeekOrigin.Begin);
			return position;
		}

		public override void Flush()
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}
	}

	[Flags]
	public enum AssetBundleFlags
	{
		None = 0,
		Writable = 1,
	}

	public class PackedAssetBundle : AssetBundle
	{
		private readonly Stack<Stream> streamPool = new Stack<Stream>();
		const int Signature = 0x13AF;
		private int indexOffset;
		private readonly BinaryReader reader;
		private readonly BinaryWriter writer;
		private readonly Stream stream;
		private AssetBundleFlags flags;
		internal Dictionary <string, AssetDescriptor> index = new Dictionary<string, AssetDescriptor>(StringComparer.OrdinalIgnoreCase);
		private readonly List<AssetDescriptor> trash = new List<AssetDescriptor>();
		private readonly System.Reflection.Assembly resourcesAssembly;
		private bool wasModified { get; set; }
		public string Path { get; private set; }
		public event Action OnModifying;

		PackedAssetBundle() {}

		public PackedAssetBundle(string resourceId, string assemblyName)
		{
			this.Path = resourceId;
			resourcesAssembly = AppDomain.CurrentDomain.GetAssemblies().
				SingleOrDefault(a => a.GetName().Name == assemblyName);
			if (resourcesAssembly == null) {
				throw new Lime.Exception("Assembly '{0}' doesn't exist", assemblyName);
			}
			stream = AllocStream();
			reader = new BinaryReader(stream);
			ReadIndexTable();
		}

		public PackedAssetBundle(string path, AssetBundleFlags flags = Lime.AssetBundleFlags.None)
		{
			this.Path = path;
			this.flags = flags;
			if ((flags & AssetBundleFlags.Writable) != 0) {
				stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
				reader = new BinaryReader(stream);
				writer = new BinaryWriter(stream);
			} else {
				stream = AllocStream();
				reader = new BinaryReader(stream);
			}
			ReadIndexTable();
		}

		public static int CalcBundleCheckSum(string bundlePath)
		{
			// "Modified FNV with good avalanche behavior and uniform distribution with larger hash sizes."
			// see http://papa.bretmulvey.com/post/124027987928/hash-functions for algo
			using (var stream = new FileStream(bundlePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
				var data = new byte[16 * 1024];
				int size = stream.Read(data, 0, data.Length);
				if (size < 8) {
					return 0;
				}
				data[4] = 0;
				data[5] = 0;
				data[6] = 0;
				data[7] = 0;
				unchecked {
					const int p = 16777619;
					int hash = (int)2166136261;
					while (size > 0) {
						for (int i = 0; i < size; i++) {
							hash = (hash ^ data[i]) * p;
						}
						size = stream.Read(data, 0, data.Length);
					}
					// are these actually needed?
					hash += hash << 13;
					hash ^= hash >> 7;
					hash += hash << 3;
					hash ^= hash >> 17;
					hash += hash << 5;
					return hash;
				}
			}
		}

		public static bool IsBundleCorrupted(string bundlePath)
		{
			using (var stream = new FileStream(bundlePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
				if (stream.Length < 8) {
					return true;
				}
				var reader = new BinaryReader(stream);
				// Bundle signature
				reader.ReadInt32();
				int storedCheckSum = reader.ReadInt32();
				int actualCheckSum = CalcBundleCheckSum(bundlePath);
				return storedCheckSum != actualCheckSum;
			}
		}

		public static void RefreshBundleCheckSum(string bundlePath)
		{
			int checkSum = CalcBundleCheckSum(bundlePath);
			using (var stream = new FileStream(bundlePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite)) {
				if (stream.Length > 8) {
					using (var writer = new BinaryWriter(stream)) {
						writer.Seek(4, SeekOrigin.Begin);
						writer.Write(checkSum);
					}
				}
			}
		}

		private void MoveBlock(int offset, int size, int delta)
		{
			if (delta > 0) {
				throw new NotImplementedException();
			}
			byte[] buffer = new byte[4096];
			while (size > 0) {
				stream.Seek(offset, SeekOrigin.Begin);
				int readCount = stream.Read(buffer, 0, Math.Min(size, buffer.Length));
				stream.Seek(offset + delta, SeekOrigin.Begin);
				stream.Write(buffer, 0, readCount);
				size -= readCount;
				offset += readCount;
			}
		}

		public void CleanupBundle()
		{
			if (trash.Count == 0) {
				// return early to avoid modifying Date Modified of bundle file with stream.SetLength
				return;
			}
			trash.Sort((x, y) => {
				return x.Offset - y.Offset; });
			int moveDelta = 0;
			var indexKeys = new string[index.Keys.Count];
			index.Keys.CopyTo(indexKeys, 0);
			for (int i = 0; i < trash.Count; i++) {
				moveDelta += trash[i].AllocatedSize;
				int blockBegin = trash[i].Offset + trash[i].AllocatedSize;
				int blockEnd = (i < trash.Count - 1) ? trash[i + 1].Offset : indexOffset;
				MoveBlock (blockBegin, blockEnd - blockBegin, -moveDelta);
				foreach (var k in indexKeys) {
					var d = index[k];
					if (d.Offset >= blockBegin && d.Offset < blockEnd) {
						d.Offset -= moveDelta;
						index[k] = d;
					}
				}
			}
			trash.Clear();
			indexOffset -= moveDelta;
			stream.SetLength(stream.Length - moveDelta);
		}

		public override void Dispose()
		{
			base.Dispose();
			if (writer != null) {
				if (wasModified) {
					CleanupBundle();
					WriteIndexTable();
					RefreshBundleCheckSum(Path);
				}
				writer.Close();
			}
			if (reader != null) {
				reader.Close();
			}
			if (stream != null) {
				stream.Close();
			}
			index.Clear();
			while (streamPool.Count > 0) {
				streamPool.Pop().Dispose();
			}
		}

		public override Stream OpenFile(string path)
		{
			var stream = new AssetStream(this, path);
			var desc = stream.descriptor;
			if (CommandLineArgs.SimulateSlowExternalStorage) {
				ExternalStorageLagsSimulator.SimulateReadDelay(path, desc.Length);
			}
			if ((desc.Attributes & AssetAttributes.Zipped) != 0) {
				return DecompressAssetStream(stream, desc.Attributes);
			}
			return stream;
		}

		private static Stream DecompressAssetStream(AssetStream stream, AssetAttributes attributes)
		{
			if ((attributes & AssetAttributes.ZippedDeflate) != 0) {
				return new DeflateStream(stream, CompressionMode.Decompress);
			}
			if ((attributes & AssetAttributes.ZippedLZMA) != 0) {
				return new LzmaDecompressionStream(stream);
			}
			throw new NotImplementedException();
		}

		public override DateTime GetFileLastWriteTime(string path)
		{
			return GetDescriptor(path).ModificationTime;
		}

		public override byte[] GetCookingRulesSHA1(string path)
		{
			return GetDescriptor(path).CookingRulesSHA1;
		}

		public override int GetFileSize(string path)
		{
			return GetDescriptor(path).Length;
		}

		public override void DeleteFile(string path)
		{
			OnModifying?.Invoke();
			path = AssetPath.CorrectSlashes(path);
			var desc = GetDescriptor(path);
			index.Remove(path);
			trash.Add(desc);
			wasModified = true;
		}

		public override bool FileExists(string path)
		{
			return index.ContainsKey(AssetPath.CorrectSlashes(path));
		}

		public override AssetAttributes GetAttributes(string path)
		{
			return GetDescriptor(path).Attributes;
		}

		public override void SetAttributes(string path, AssetAttributes attributes)
		{
			OnModifying?.Invoke();
			var desc = GetDescriptor(path);
			index[AssetPath.CorrectSlashes(path)] = desc;
			wasModified = true;
		}

		public override void ImportFile(string path, Stream stream, int reserve, string sourceExtension, AssetAttributes attributes, byte[] cookingRulesSHA1)
		{
			OnModifying?.Invoke();
			AssetDescriptor d;
			if ((attributes & AssetAttributes.Zipped) != 0) {
				stream = CompressAssetStream(stream, attributes);
			}
			bool reuseExistingDescriptor = index.TryGetValue(AssetPath.CorrectSlashes(path), out d) &&
				(d.AllocatedSize >= stream.Length) &&
				(d.AllocatedSize <= stream.Length + reserve);
			if (reuseExistingDescriptor) {
				d.Length = (int)stream.Length;
				d.ModificationTime = DateTime.Now;
				d.CookingRulesSHA1 = cookingRulesSHA1;
				d.Attributes = attributes;
				d.SourceExtension = sourceExtension;
				index[AssetPath.CorrectSlashes(path)] = d;
				this.stream.Seek(d.Offset, SeekOrigin.Begin);
				stream.CopyTo(this.stream);
				reserve = d.AllocatedSize - (int)stream.Length;
				if (reserve > 0) {
					byte[] zeroBytes = new byte[reserve];
					this.stream.Write(zeroBytes, 0, zeroBytes.Length);
				}
			} else {
				if (FileExists(path)) {
					DeleteFile(path);
				}
				d = new AssetDescriptor();
				d.ModificationTime = DateTime.Now;
				d.CookingRulesSHA1 = cookingRulesSHA1;
				d.Length = (int)stream.Length;
				d.Offset = indexOffset;
				d.AllocatedSize = d.Length + reserve;
				d.Attributes = attributes;
				d.SourceExtension = sourceExtension;
				index[AssetPath.CorrectSlashes(path)] = d;
				indexOffset += d.AllocatedSize;
				this.stream.Seek(d.Offset, SeekOrigin.Begin);
				stream.CopyTo(this.stream);
				byte[] zeroBytes = new byte[reserve];
				this.stream.Write(zeroBytes, 0, zeroBytes.Length);
				this.stream.Flush();
			}
			wasModified = true;
		}

		private static Stream CompressAssetStream(Stream stream, AssetAttributes attributes)
		{
			MemoryStream memStream = new MemoryStream();
			using (var compressionStream = CreateCompressionStream(memStream, attributes)) {
				stream.CopyTo(compressionStream);
			}
			memStream.Seek(0, SeekOrigin.Begin);
			stream = memStream;
			return stream;
		}

		private static Stream CreateCompressionStream(Stream stream, AssetAttributes attributes)
		{
			if ((attributes & AssetAttributes.ZippedDeflate) != 0) {
				return new DeflateStream(stream, CompressionMode.Compress, leaveOpen: true);
			}
			if ((attributes & AssetAttributes.ZippedLZMA) != 0) {
				return new LzmaCompressionStream(stream, leaveOpen: true);
			}
			throw new NotImplementedException();
		}

		private void ReadIndexTable()
		{
			if (stream.Length == 0) {
				indexOffset = sizeof(int) * 4;
				index.Clear();
				return;
			}
			stream.Seek(0, SeekOrigin.Begin);
			var signature = reader.ReadInt32();
			if (signature != Signature) {
				throw new Exception($"The asset bundle at \"{Path}\" has been corrupted");
			}
			reader.ReadInt32(); // CheckSum
			var version = reader.ReadInt32();
			if (version != Lime.Version.GetBundleFormatVersion()) {
				throw new Exception(string.Format("The bundle format has been changed. Please update Citrus and rebuild game.\n" +
					"Bundle format version: {0}, but expected: {1}", version, Lime.Version.GetBundleFormatVersion()));
			}
			indexOffset = reader.ReadInt32();
			stream.Seek(indexOffset, SeekOrigin.Begin);
			int numDescriptors = reader.ReadInt32();
			index.Clear();
			for (int i = 0; i < numDescriptors; i++) {
				var desc = new AssetDescriptor();
				string name = reader.ReadString();
				desc.ModificationTime = DateTime.FromBinary(reader.ReadInt64());
				ushort sha1Length = reader.ReadUInt16();
				if (sha1Length != 0) {
					desc.CookingRulesSHA1 = reader.ReadBytes(sha1Length);
				}
				desc.Offset = reader.ReadInt32();
				desc.Length = reader.ReadInt32();
				desc.AllocatedSize = reader.ReadInt32();
				desc.Attributes = (AssetAttributes)reader.ReadInt32();
				desc.SourceExtension = reader.ReadString();
				index.Add(name, desc);
			}
		}

		private void WriteIndexTable()
		{
			stream.Seek(0, SeekOrigin.Begin);
			writer.Write(Signature);
			writer.Write(0);
			writer.Write(Lime.Version.GetBundleFormatVersion());
			writer.Write(indexOffset);
			stream.Seek(indexOffset, SeekOrigin.Begin);
			int numDescriptors = index.Count;
			writer.Write(numDescriptors);
			foreach (KeyValuePair <string, AssetDescriptor> p in index) {
				writer.Write(p.Key);
				writer.Write((Int64)p.Value.ModificationTime.ToBinary());
				if (p.Value.CookingRulesSHA1 != null && p.Value.CookingRulesSHA1.Length > ushort.MaxValue) {
					throw new InvalidOperationException("Invalid Cooking Rules hash in asset descriptor. Must be 20 byte long SHA1");
				}
				ushort sha1Length = (ushort)(p.Value.CookingRulesSHA1?.Length ?? 0);
				writer.Write(sha1Length);
				if (sha1Length != 0) {
					writer.Write(p.Value.CookingRulesSHA1);
				}
				writer.Write(p.Value.Offset);
				writer.Write(p.Value.Length);
				writer.Write(p.Value.AllocatedSize);
				writer.Write((int)p.Value.Attributes);
				writer.Write(p.Value.SourceExtension);
			}
		}

		public override IEnumerable<string> EnumerateFiles(string path = null)
		{
			foreach (var file in index) {
				if (path == null || file.Key.StartsWith(path)) {
					yield return file.Key;
				}
			}
		}

		internal Stream AllocStream()
		{
			lock (streamPool) {
				if (streamPool.Count > 0) {
					return streamPool.Pop();
				}
			}
			if (resourcesAssembly != null) {
				var stream = resourcesAssembly.GetManifestResourceStream(Path);
				if (stream == null) {
					throw new Lime.Exception("Resource '{0}' doesn't exist. Available resources: {1}", Path,
						string.Join(", ", resourcesAssembly.GetManifestResourceNames()));
				}
				return stream;
			} else {
				return new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			}
		}

		internal void ReleaseStream(Stream stream)
		{
			lock (streamPool) {
				streamPool.Push(stream);
			}
		}

		private AssetDescriptor GetDescriptor(string path)
		{
			AssetDescriptor desc;
			if (index.TryGetValue(AssetPath.CorrectSlashes(path), out desc)) {
				return desc;
			}
			throw new Exception("Asset '{0}' doesn't exist", path);
		}
	}
}
