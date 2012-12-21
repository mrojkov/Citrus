using System.IO;
using System;
using System.Collections.Generic;

namespace Lime
{
	struct AssetDescriptor
	{
		public DateTime Time;
		public Int32 Offset;
		public Int32 Length;
		public Int32 AllocatedSize;
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

	public class AssetStream : Stream
	{
		AssetsBundle bundle;
		AssetDescriptor descriptor;
		Int32 position;
		Stream stream;

		public AssetStream(AssetsBundle bundle, string path)
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

	public class AssetsBundle : IDisposable
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
		public string CurrentLanguage;

		static readonly AssetsBundle instance = new AssetsBundle();

		public static AssetsBundle Instance { get { return instance; } }

		AssetsBundle() {}

		public AssetsBundle(string path, AssetBundleFlags flags)
		{
			Open(path, flags);
		}

		public void Open(string path, AssetBundleFlags flags)
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
		
		private void MoveBlock (int offset, int size, int delta)
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

		public void Close()
		{
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

		void IDisposable.Dispose()
		{
			Close();
		}

		public Stream OpenFile(string path)
		{
			return new AssetStream(this, path);
		}

		public Stream OpenFileLocalized(string path)
		{
			return new AssetStream(this, GetLocalizedPath(path));
		}

		public string GetLocalizedPath(string path)
		{
			if (string.IsNullOrEmpty(CurrentLanguage))
				return path;
			string extension = Path.GetExtension(path);
			string pathWithoutExtension = Path.ChangeExtension(path, null);
			string localizedParth = pathWithoutExtension + "." + CurrentLanguage + extension;
			if (FileExists(localizedParth)) {
				return localizedParth;
			}
			return path;
		}

		public DateTime GetFileLastWriteTime(string path)
		{
			AssetDescriptor desc;
			if (index.TryGetValue(AssetPath.CorrectSlashes(path), out desc)) {
				return desc.Time;
			}
			throw new Exception("Asset '{0}' doesn't exist", path);
		}

		public void DeleteFile(string path)
		{
			path = AssetPath.CorrectSlashes(path);
			AssetDescriptor desc;
			if (!index.TryGetValue(path, out desc)) {
				throw new Exception("Asset '{0}' doesn't exist", path);
			}
			index.Remove(path);
			trash.Add(desc);
		}

		public void ListFiles()
		{
			foreach (var k in index.Keys)
				Console.WriteLine(k);
		}

		public bool FileExists(string path)
		{
			return index.ContainsKey(AssetPath.CorrectSlashes(path));
		}

		public void ImportFile(string srcPath, string dstPath, int reserve)
		{
			using (var stream = new FileStream(srcPath, FileMode.Open)) {
				ImportFile(dstPath, stream, reserve);
			}
		}

		public void ImportFile(string path, Stream stream, int reserve)
		{
			AssetDescriptor d;
			bool reuseExistingDescriptor = index.TryGetValue(AssetPath.CorrectSlashes(path), out d) && 
				(d.AllocatedSize >= stream.Length) && 
				(d.AllocatedSize <= stream.Length + reserve);
			if (reuseExistingDescriptor) {
				d.Length = (int)stream.Length;
				d.Time = DateTime.Now;
				index[AssetPath.CorrectSlashes(path)] = d;
				this.stream.Seek(d.Offset, SeekOrigin.Begin);
				stream.CopyTo(this.stream);
				reserve = d.AllocatedSize - (int)stream.Length;
				if (reserve > 0) {
					byte[] zeroBytes = new byte[reserve];
					this.stream.Write(zeroBytes, 0, zeroBytes.Length);
				}
			} else {
				if (FileExists(path))
					DeleteFile(path);
				d = new AssetDescriptor();
				d.Time = DateTime.Now;
				d.Length = (int)stream.Length;
				d.Offset = indexOffset;
				d.AllocatedSize = d.Length + reserve;
				index[AssetPath.CorrectSlashes(path)] = d;
				indexOffset += d.AllocatedSize;
				this.stream.Seek(d.Offset, SeekOrigin.Begin);
				stream.CopyTo(this.stream);
				byte[] zeroBytes = new byte[reserve];
				this.stream.Write(zeroBytes, 0, zeroBytes.Length);
			}
		}

		private void ReadIndexTable()
		{
			if (stream.Length == 0) {
				indexOffset = sizeof(Int32) * 3;
				index.Clear();
				return;
			}
			stream.Seek(0, SeekOrigin.Begin);
			var signature = reader.ReadInt32();
			if (signature != Signature) {
				throw new Exception("Assets bundle has been corrupted");
			}
			reader.ReadInt32(); // reserved field
			indexOffset = reader.ReadInt32();

			stream.Seek(indexOffset, SeekOrigin.Begin);
			int numDescriptors = reader.ReadInt32();
			index.Clear();
			for (int i = 0; i < numDescriptors; i++) {
				var desc = new AssetDescriptor();
				string name = reader.ReadString();
				desc.Time = DateTime.FromBinary(reader.ReadInt64());
				desc.Offset = reader.ReadInt32();
				desc.Length = reader.ReadInt32();
				desc.AllocatedSize = reader.ReadInt32();
				index.Add(name, desc);
			}
		}

		private void WriteIndexTable()
		{
			stream.Seek(0, SeekOrigin.Begin);
			writer.Write(Signature);
			writer.Write(0);
			writer.Write(indexOffset);
			stream.Seek(indexOffset, SeekOrigin.Begin);
			Int32 numDescriptors = index.Count;
			writer.Write(numDescriptors);
			foreach (KeyValuePair <string, AssetDescriptor> p in index) {
				writer.Write(p.Key);
				writer.Write((Int64)p.Value.Time.ToBinary());
				writer.Write(p.Value.Offset);
				writer.Write(p.Value.Length);
				writer.Write(p.Value.AllocatedSize);
			}
		}
		
		public string[] EnumerateFiles()
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
	}
}