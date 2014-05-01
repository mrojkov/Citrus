using System.IO;
using System.IO.Compression;
using System;
using System.Collections.Generic;

namespace Lime
{
	[Flags]
	public enum AssetAttributes
	{
		Zipped = 1 << 0,
	}

	struct AssetDescriptor
	{
		public DateTime ModificationTime;
		public Int32 Offset;
		public Int32 Length;
		public Int32 AllocatedSize;
		public AssetAttributes Attributes;
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
			if (path.IndexOf('\\') >= 0) {
				return path.Replace('\\', '/');
			} else {
				return path;
			}
		}
	}

	public sealed class AssetStream : Stream
	{
		PackedAssetsBundle bundle;
		internal AssetDescriptor descriptor;
		Int32 position;
		Stream stream;

		public AssetStream(PackedAssetsBundle bundle, string path)
		{
			this.bundle = bundle;
			if (!bundle.index.TryGetValue(AssetPath.CorrectSlashes(path), out descriptor)) {
				throw new Exception("Can't open asset: {0}", path);
			}
			stream = bundle.AllocStream();
			Seek(0, SeekOrigin.Begin);
		}
		
		public override bool CanRead {
			get {
				return true;
			}
		}
		
		public override bool CanWrite {
			get {
				return false;
			}
		}
		
		public override long Length {
			get {
				return descriptor.Length;
			}
		}
		
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
				position = (Int32)offset;
			} else if (origin == SeekOrigin.Current) {
				position += (Int32)offset;
			} else {
				position = descriptor.Length - (Int32)offset;
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

	public class PackedAssetsBundle : AssetsBundle
	{
		Stack<Stream> streamPool = new Stack<Stream>();
		string path;
		const Int32 Signature = 0x13AF;
		Int32 indexOffset;
		BinaryReader reader;
		BinaryWriter writer;
		FileStream stream;
		internal Dictionary <string, AssetDescriptor> index = new Dictionary<string, AssetDescriptor>();
		List<AssetDescriptor> trash = new List<AssetDescriptor>();

		PackedAssetsBundle() {}

		public PackedAssetsBundle(string path, AssetBundleFlags flags = Lime.AssetBundleFlags.None)
		{
			this.path = path;
			if ((flags & AssetBundleFlags.Writable) != 0) {
				stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
				reader = new BinaryReader(stream);
				writer = new BinaryWriter(stream);
			} else {
				stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				reader = new BinaryReader(stream);
			}
			ReadIndexTable();
		}

		public static int CalcBundleCheckSum(string bundlePath)
		{
			// It would be better read whole file by chunks
			var contents = File.ReadAllBytes(bundlePath);
			if (contents.Length < 8) {
				return 0;
			}
			contents[4] = 0;
			contents[5] = 0;
			contents[6] = 0;
			contents[7] = 0;
			int checkSum = Toolbox.ComputeHash(contents, contents.Length);
			return checkSum;
		}

		public static bool IsBundleCorrupted(string bundlePath)
		{
			using (var stream = new FileStream(bundlePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
				if (stream.Length < 8) {
					return true;
				}
				var reader = new BinaryReader(stream);
				int sign = reader.ReadInt32();
				int storedCheckSum = reader.ReadInt32();
				int actualCheckSum = CalcBundleCheckSum(bundlePath);
				return storedCheckSum != actualCheckSum;
			}
		}

		public static void RefreshBundleCheckSum(string bundlePath)
		{
			int checkSum = CalcBundleCheckSum(bundlePath);
			using (var stream = new FileStream(bundlePath, FileMode.Open, FileAccess.Write)) {
				if (stream.Length > 8) {
					stream.Seek(4, SeekOrigin.Begin);
					var reader = new BinaryWriter(stream);
					reader.Write(checkSum);
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
				CleanupBundle();
				WriteIndexTable();
				writer.Close();
			}
			if (reader != null)
				reader.Close();
			if (stream != null)
				stream.Close();
			reader = null;
			writer = null;
			stream = null;
			index.Clear();
			streamPool.Clear();
		}

		public override Stream OpenFile(string path)
		{
			var stream = new AssetStream(this, path);
			var desc = stream.descriptor;
			if (Application.JerkyMode) {
				ApplicationToolbox.SimulateReadDelay(path, desc.Length);
			}
			if ((desc.Attributes & AssetAttributes.Zipped) != 0) {
				return DecompressStream(stream);
			}
			return stream;
		}

		private static Stream DecompressStream(AssetStream stream)
		{
#if UNITY
			throw new NotImplementedException();
#else
			var deflateStream = new DeflateStream(stream, CompressionMode.Decompress, false);
			return deflateStream;
#endif
		}

		public override DateTime GetFileLastWriteTime(string path)
		{
			return GetDescriptor(path).ModificationTime;
		}

		public override void DeleteFile(string path)
		{
			path = AssetPath.CorrectSlashes(path);
			var desc = GetDescriptor(path);
			index.Remove(path);
			trash.Add(desc);
		}

		public override bool FileExists(string path)
		{
			return index.ContainsKey(AssetPath.CorrectSlashes(path));
		}

		public override void ImportFile(string path, Stream stream, int reserve, bool compress)
		{
			AssetDescriptor d;
			AssetAttributes attributes = 0;
			if (compress) {
				attributes |= AssetAttributes.Zipped;
				stream = CompressStream(stream);
			}
			bool reuseExistingDescriptor = index.TryGetValue(AssetPath.CorrectSlashes(path), out d) && 
				(d.AllocatedSize >= stream.Length) && 
				(d.AllocatedSize <= stream.Length + reserve);
			if (reuseExistingDescriptor) {
				d.Length = (int)stream.Length;
				d.ModificationTime = DateTime.Now;
				d.Attributes = attributes;
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
				d.Length = (int)stream.Length;
				d.Offset = indexOffset;
				d.AllocatedSize = d.Length + reserve;
				d.Attributes = attributes;
				index[AssetPath.CorrectSlashes(path)] = d;
				indexOffset += d.AllocatedSize;
				this.stream.Seek(d.Offset, SeekOrigin.Begin);
				stream.CopyTo(this.stream);
				byte[] zeroBytes = new byte[reserve];
				this.stream.Write(zeroBytes, 0, zeroBytes.Length);
			}
		}

		private static Stream CompressStream(Stream stream)
		{
#if UNITY
			throw new NotImplementedException();
#else
			MemoryStream memStream = new MemoryStream();
			using (var deflateStream = new DeflateStream(memStream, CompressionMode.Compress, true)) {
				stream.CopyTo(deflateStream);
			}
			memStream.Seek(0, SeekOrigin.Begin);
			stream = memStream;
			return stream;
#endif
		}

		private void ReadIndexTable()
		{
			if (stream.Length == 0) {
				indexOffset = sizeof(Int32) * 4;
				index.Clear();
				return;
			}
			stream.Seek(0, SeekOrigin.Begin);
			var signature = reader.ReadInt32();
			if (signature != Signature) {
				throw new Exception("The assets bundle has been corrupted");
			}
			reader.ReadInt32(); // CheckSum
			if (reader.ReadInt32() != Lime.Version.GetBundleFormatVersion()) {
				throw new Exception("The bundle format or serialization scheme has been changed. Please update Orange, rebuild the game and serializer.dll");
			}
			indexOffset = reader.ReadInt32();
			stream.Seek(indexOffset, SeekOrigin.Begin);
			int numDescriptors = reader.ReadInt32();
			index.Clear();
			for (int i = 0; i < numDescriptors; i++) {
				var desc = new AssetDescriptor();
				string name = reader.ReadString();
				desc.ModificationTime = DateTime.FromBinary(reader.ReadInt64());
				desc.Offset = reader.ReadInt32();
				desc.Length = reader.ReadInt32();
				desc.AllocatedSize = reader.ReadInt32();
				desc.Attributes = (AssetAttributes)reader.ReadInt32();
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
			Int32 numDescriptors = index.Count;
			writer.Write(numDescriptors);
			foreach (KeyValuePair <string, AssetDescriptor> p in index) {
				writer.Write(p.Key);
				writer.Write((Int64)p.Value.ModificationTime.ToBinary());
				writer.Write(p.Value.Offset);
				writer.Write(p.Value.Length);
				writer.Write(p.Value.AllocatedSize);
				writer.Write((Int32)p.Value.Attributes);
			}
		}
		
		public override IEnumerable<string> EnumerateFiles()
		{
			string[] files = new string[index.Keys.Count];
			index.Keys.CopyTo(files, 0);
			return files;
		}

		internal Stream AllocStream()
		{
			lock (streamPool) {
				if (streamPool.Count > 0) {
					return streamPool.Pop();
				}
			}
			return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
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