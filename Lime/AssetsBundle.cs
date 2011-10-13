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
		public Int32 Space;
	}
    	
	public class AssetStream : Stream
	{
		private AssetsBundle bundle;
		private AssetDescriptor descriptor;
		private Int32 position;
		
		public AssetStream (AssetsBundle bundle, string path)
		{
			this.bundle = bundle;
			if (!bundle.indexTable.TryGetValue (path, out descriptor)) {
				throw new RuntimeError ("Can't open asset: {0}", path);
			}
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
				position = (Int32)value;
			}
		}
		
		protected override void Dispose (bool disposing)
		{			 
		}
		
		public override bool CanSeek {
			get {
				return true;
			}
		}
		
		public override int Read (byte[] buffer, int offset, int count)
		{
			lock (bundle) {
				bundle.stream.Seek (position + descriptor.Offset, SeekOrigin.Begin);
				count = Math.Min (count, descriptor.Length - position);
				if (count > 0) {
					count = bundle.stream.Read (buffer, offset, count);
					position += count;
				} else {
					count = 0;
				}
			}
			return count;
		}
		
		public override long Seek (long offset, SeekOrigin origin)
		{
			if (origin == SeekOrigin.Begin) {
				position = (Int32)offset;
			} else if (origin == SeekOrigin.Current) {
				position += (Int32)offset;
			} else {
				position = descriptor.Length - (Int32)offset;
			}
			position = Math.Max(0, Math.Min ((Int32)offset, descriptor.Length));
			return position;
		}

		public override void Flush ()
		{
			throw new NotImplementedException ();
		}
	
		public override void SetLength (long value)
		{
			throw new NotImplementedException ();
		}
		
		public override void Write (byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException ();
		}
	}
	
	public class AssetsBundle : IAssetsSource, IDisposable
	{	
		static readonly AssetsBundle instance = new AssetsBundle();
		public static AssetsBundle Instance { get { return instance; } }
        
		AssetsBundle () {}
		
		public AssetsBundle (string path, bool forWriting) 
		{
			Open (path, forWriting);
		}
		
		public void Open (string path, bool forWriting)
		{
			if (forWriting) {
				stream = new FileStream (path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
				reader = new BinaryReader (stream);
				writer = new BinaryWriter (stream);
			} else {
				stream = new FileStream (path, FileMode.Open, FileAccess.Read);
				reader = new BinaryReader (stream);
			}
			ReadIndexTable ();
		}
		
		public void Close ()
		{
			if (writer != null) {
				WriteIndexTable ();
				writer.Close ();
			}
			if (reader != null)
				reader.Close ();
			if (stream != null)
				stream.Close ();
			reader = null;
			writer = null;
			stream = null;
			indexTable.Clear ();
		}
		
		void IDisposable.Dispose ()
		{
			Close ();
		}

		public Stream OpenFile (string path)
		{
			Stream stream = new AssetStream (this, path);
			return stream;
		}
        
		public DateTime GetFileModificationTime (string path)
		{
			AssetDescriptor desc;
			if (indexTable.TryGetValue (path, out desc)) {
				return desc.Time;
			}
			throw new RuntimeError ("Asset '{0}' doesn't exist", path);
		}
		
		public void RemoveFile (string path)
		{
			indexTable.Remove (path);
		}
		
		public bool FileExists (string path)
		{
			return indexTable.ContainsKey (path);
		}

		public bool FileExists (string path, string extension)
		{
			return indexTable.ContainsKey (System.IO.Path.ChangeExtension (path, extension));
		}
		
		public void ImportFile (string path, Stream stream)
		{
			byte[] buffer = new byte [stream.Length];
			int read = stream.Read (buffer, 0, (int)stream.Length);
			if (read != stream.Length) {
				throw new RuntimeError ("File read failure: {0}", path);
			}
			AssetDescriptor d;
			if (!indexTable.TryGetValue (path, out d) || (d.Space < buffer.Length)) {
				int tailSize = Math.Max (MinExtraBytes, buffer.Length * ExtraBytesPercent / 100);
				d = new AssetDescriptor();
				d.Time = DateTime.Now;
				d.Length = buffer.Length;
				d.Offset = indexTableOffset;
				d.Space = d.Length + tailSize;
				indexTable [path] = d;
				indexTableOffset += d.Space;
				this.stream.Seek (d.Offset, SeekOrigin.Begin);
				this.stream.Write (buffer, 0, buffer.Length);
				byte[] zeroBytes = new byte [tailSize];
				this.stream.Write (zeroBytes, 0, zeroBytes.Length);
			} else {
				d.Length = buffer.Length;
				d.Time = DateTime.Now;
				indexTable [path] = d;
				this.stream.Seek (d.Offset, SeekOrigin.Begin);
				this.stream.Write (buffer, 0, buffer.Length);
				int tailSize = d.Space - buffer.Length;
				if (tailSize > 0) {
					byte[] zeroBytes = new byte [tailSize];
					this.stream.Write (zeroBytes, 0, zeroBytes.Length);
				}
			}			
		}

		public void ImportFile (string path)
		{
			using (Stream stream = File.Open (path, FileMode.Open, FileAccess.Read)) {
				ImportFile (path, stream);
			}
		}
       	
		private void ReadIndexTable ()
		{
			if (stream.Length == 0)	{
				indexTableOffset = sizeof (Int32) * 3;
				indexTable.Clear ();
				return;
			}
			stream.Seek (0, SeekOrigin.Begin);
			var signature = reader.ReadInt32 ();
			if (signature != Signature) {
				throw new RuntimeError ("Assets bundle has been corrupted");
			}
			reader.ReadInt32 (); // reserved field
			indexTableOffset = reader.ReadInt32 ();

			stream.Seek (indexTableOffset, SeekOrigin.Begin);
			int numDescriptors = reader.ReadInt32 ();
			indexTable.Clear ();
			for (int i = 0; i < numDescriptors; i++) {
				var desc = new AssetDescriptor ();
				string name = reader.ReadString ();
				desc.Time = DateTime.FromBinary (reader.ReadInt64 ());
				desc.Offset = reader.ReadInt32 ();
				desc.Length = reader.ReadInt32 ();
				desc.Space = reader.ReadInt32 ();
				indexTable.Add (name, desc);
			}
		}

		private void WriteIndexTable ()
		{
			stream.Seek (0, SeekOrigin.Begin);
			writer.Write (Signature);
			writer.Write (0);
			writer.Write (indexTableOffset);
			stream.Seek (indexTableOffset, SeekOrigin.Begin);
			Int32 numDescriptors = indexTable.Count;
			writer.Write (numDescriptors);
			foreach (KeyValuePair <string, AssetDescriptor> p in indexTable) {
				writer.Write (p.Key);
				writer.Write ((Int64)p.Value.Time.ToBinary ());
				writer.Write (p.Value.Offset);
				writer.Write (p.Value.Length);
				writer.Write (p.Value.Space);
			}
		}
		
		public ICollection<string> EnumerateFiles ()
		{			
			return indexTable.Keys;
		}

		const Int32 Signature = 0x13AF;
		public Int32 indexTableOffset;
		
		// Actually we store additinal bytes after file end. Its neccessary in case if file size grew after update.
		// The formula for calculating additional bytes is: ExtraBytes = Max(MinExtraBytes, (FileSize * ExtraBytesPercent) / 100)
		const int ExtraBytesPercent = 10;
		const int MinExtraBytes = 50;
		
		private BinaryReader reader;
		private BinaryWriter writer;
		internal FileStream stream;
		internal Dictionary <string, AssetDescriptor> indexTable = new Dictionary<string, AssetDescriptor> ();

	}
}