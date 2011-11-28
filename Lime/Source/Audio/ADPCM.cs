// C# IMA ADPCM decoder
// This source code is in the public domain.
// usage: new SoundPlayer(new IMA_ADPCM("imaadpcm.wav")).Play();

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Decoder
{
    public enum WAVE_FORMAT
    {
        UNKNOWN,
        PCM,
        ADPCM,
        IMA_ADPCM = 0x11
    }

    public class IMA_ADPCM : Stream
    {
        private Stream fs;
        private int length, position, blocklen;
        public ushort Channels;
        public int SamplesPerSec;
        public ushort BlockAlign;
        private int DataPosition, DataSize;
        private byte[] Header;

        public IMA_ADPCM(string path)
            : this(new FileStream(path, FileMode.Open))
        {
        }

        public IMA_ADPCM(Stream stream)
        {
            fs = stream;
            if (ReadID() != "RIFF")
                throw Abort("Invalid RIFF header");
            ReadInt32();
            if (ReadID() != "WAVE")
                throw Abort("Wave type is expected");
            int fmtsize = 0;
            DataSize = 0;
            while (fs.Position < fs.Length)
            {
                switch (ReadID())
                {
                    case "fmt ":
                        fmtsize = ReadInt32();
                        if (ReadUInt16() != (ushort)WAVE_FORMAT.IMA_ADPCM)
                            throw Abort("Not IMA ADPCM");
                        Channels = ReadUInt16();
                        SamplesPerSec = ReadInt32();
                        ReadInt32();
                        BlockAlign = ReadUInt16();
                        if (ReadUInt16() != 4)
                            throw Abort("Not 4-bit format");
                        ReadBytes(fmtsize - 16);
                        break;
                    case "data":
                        DataSize = ReadInt32();
                        DataPosition = (int)fs.Position;
                        fs.Position += DataSize;
                        break;
                    default:
                        var size = ReadInt32();
                        fs.Position += size;
                        break;
                }
            }
            if (fmtsize == 0)
                throw Abort("No format information");
            else if (DataSize == 0)
                throw Abort("No data");

            int blocks = (int)(DataSize / BlockAlign);
            blocklen = (BlockAlign - Channels * 4) * 4 + Channels * 2;
            int datalen = blocks * blocklen;
            length = datalen + 44;

            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            bw.Write(Encoding.UTF8.GetBytes("RIFF"));
            bw.Write(length - 8);
            bw.Write(Encoding.UTF8.GetBytes("WAVE"));
            bw.Write(Encoding.UTF8.GetBytes("fmt "));
            bw.Write(16);
            bw.Write((ushort)WAVE_FORMAT.PCM); // FormatTag
            bw.Write(Channels);
            bw.Write(SamplesPerSec);
            bw.Write(SamplesPerSec * Channels * 2); // AvgBytesPerSec
            bw.Write((ushort)(Channels * 2)); // BlockAlign
            bw.Write((ushort)16); // BitsPerSample
            bw.Write(Encoding.UTF8.GetBytes("data"));
            bw.Write(datalen);
            Header = ms.ToArray();
            bw.Close();
            ms.Close();
        }

        private int CacheNo = -1;
        private byte[] Cache;

        public void Decode(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            {
                position = 0;
                fs.Write(Header, 0, Header.Length);
                int blocks = DataSize / BlockAlign;
                for (int i = 0; i < blocks; i++)
                {
                    var block = DecodeBlock(i);
                    fs.Write(block, 0, block.Length);
                }
            }
        }

        public IEnumerable<byte[]> DecodeSamples()
        {
            position = 0;
            int blocks = DataSize / BlockAlign;
            for (int i = 0; i < blocks; i++)
            {
                var block = DecodeBlock(i);
                yield return block;
            }
        }

        private byte[] DecodeBlock(int n)
        {
            if (n >= DataSize / BlockAlign) return null;
            if (CacheNo == n) return Cache;

            int pos = DataPosition + n * BlockAlign;
            if (pos >= fs.Length) return null;

            fs.Position = pos;
            var data = ReadBytes(BlockAlign);
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            var v = new SampleValue[Channels];
            for (int ch = 0; ch < Channels; ch++)
            {
                v[ch] = new SampleValue(data, ch * 4);
                bw.Write(v[ch].Value);
            }
            int ch4 = Channels * 4;
            for (int i = ch4; i < BlockAlign; i += ch4)
            {
                for (int j = 0; j < 4; j++)
                {
                    for (int ch = 0; ch < Channels; ch++)
                        bw.Write(v[ch].Next(data[i + j + ch * 4] & 0xf));
                    for (int ch = 0; ch < Channels; ch++)
                        bw.Write(v[ch].Next(data[i + j + ch * 4] >> 4));
                }
            }
            CacheNo = n;
            Cache = ms.ToArray();
            bw.Close();
            ms.Close();
            return Cache;
        }

        private struct SampleValue
        {
            public short Value;
            public int Index;

            public SampleValue(byte[] value, int startIndex)
            {
                Value = BitConverter.ToInt16(value, startIndex);
                Index = value[startIndex + 2];
            }

            private static int[] StepTable = new[]
            {
                7, 8, 9, 10, 11, 12, 13, 14,
                16, 17, 19, 21, 23, 25, 28, 31,
                34, 37, 41, 45, 50, 55, 60, 66,
                73, 80, 88, 97, 107, 118, 130, 143,
                157, 173, 190, 209, 230, 253, 279, 307,
                337, 371, 408, 449, 494, 544, 598, 658,
                724, 796, 876, 963, 1060, 1166, 1282, 1411,
                1552, 1707, 1878, 2066, 2272, 2499, 2749, 3024,
                3327, 3660, 4026, 4428, 4871, 5358, 5894, 6484,
                7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899,
                15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794,
                32767
            };

            private static int[] IndexTable = new[]
            {
                -1, -1, -1, -1, 2, 4, 6, 8,
                -1, -1, -1, -1, 2, 4, 6, 8
            };

            public short Next(int v)
            {
                int d = 0;
                int s = StepTable[Index];
                if ((v & 4) != 0) d += s;
                if ((v & 2) != 0) d += s >> 1;
                if ((v & 1) != 0) d += s >> 2;
                d += s >> 3;
                if ((v & 8) != 0) d = -d;
                int val = ((int)Value) + d;
                if (val > short.MaxValue) val = short.MaxValue;
                if (val < short.MinValue) val = short.MinValue;

                int idx = Index + IndexTable[v];
                if (idx >= StepTable.Length) idx = StepTable.Length - 1;
                if (idx < 0) idx = 0;

                Value = (short)val;
                Index = idx;
                return Value;
            }
        }

        private byte[] ReadBytes(int length)
        {
            var ret = new byte[length];
            if (length > 0) fs.Read(ret, 0, length);
            return ret;
        }

        private string ReadID() { return Encoding.UTF8.GetString(ReadBytes(4), 0, 4); }
        private int ReadInt32() { return BitConverter.ToInt32(ReadBytes(4), 0); }
        private uint ReadUInt32() { return BitConverter.ToUInt32(ReadBytes(4), 0); }
        private ushort ReadUInt16() { return BitConverter.ToUInt16(ReadBytes(2), 0); }

        protected override void Dispose(bool disposing)
        {
            if (fs != null)
            {
                fs.Close();
                fs = null;
            }
            base.Dispose(disposing);
        }

        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return true; } }
        public override bool CanWrite { get { return false; } }
        public override void Flush() { }
        public override long Length { get { return length; } }

        public override long Position
        {
            get { return position; }
            set
            {
                position = (int)value;
                if (position < 0) position = 0;
                if (position > length) position = length;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int len = Math.Min(count, length - position);
            int hlen = Header.Length;
            for (int i = 0; i < len; )
            {
                int p = position + i;
                if (p < hlen)
                {
                    int len2 = Math.Min(len - i, hlen - p);
                    Array.Copy(Header, p, buffer, offset + i, len2);
                    i += len2;
                }
                else
                {
                    int bn = (p - hlen) / blocklen;
                    int bs = bn * blocklen + hlen;
                    int be = bs + blocklen;
                    int len2 = Math.Min(len - i, be - p);
                    var data = DecodeBlock(bn);
                    Array.Copy(data, p - bs, buffer, offset + i, len2);
                    i += len2;
                }
            }
            position += len;
            return len;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = length + (int)origin;
                    break;
            }
            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public Exception Abort(string message)
        {
            Dispose();
            return new Exception(message);
        }
    }
}