using System;
using System.Runtime.CompilerServices;

namespace Lime.Graphics.Platform
{
	internal static unsafe class Murmur3
	{
		private static readonly bool x64 = IntPtr.Size == 8;

		public static Hash128 ComputeHash(IntPtr data, int length, uint seed = 0)
		{
			Hash128 hash;
			if (x64) {
				ComputeHash_x64(data, length, seed, new IntPtr(&hash));
			} else {
				ComputeHash_x86(data, length, seed, new IntPtr(&hash));
			}
			return hash;
		}

		private static void ComputeHash_x86(IntPtr data, int length, uint seed, IntPtr hash)
		{
			unchecked {
				var p = (byte*)data;
				var blockCount = length / 16;

				var h1 = seed;
				var h2 = seed;
				var h3 = seed;
				var h4 = seed;

				const uint c1 = 0x239b961b;
				const uint c2 = 0xab0e9789;
				const uint c3 = 0x38b34ae5;
				const uint c4 = 0xa1e38b93;

				var blocks = (uint*)(p + blockCount * 16);

				uint k1, k2, k3, k4;

				for (var i = -blockCount; i < 0; i++) {
					k1 = *(blocks + i * 4 + 0);
					k2 = *(blocks + i * 4 + 1);
					k3 = *(blocks + i * 4 + 2);
					k4 = *(blocks + i * 4 + 3);

					k1 *= c1;
					k1 = RotateLeft32(k1, 15);
					k1 *= c2;
					h1 ^= k1;

					h1 = RotateLeft32(h1, 19);
					h1 += h2;
					h1 = h1 * 5 + 0x561ccd1b;

					k2 *= c2;
					k2 = RotateLeft32(k2, 16);
					k2 *= c3;
					h2 ^= k2;

					h2 = RotateLeft32(h2, 17);
					h2 += h3;
					h2 = h2 * 5 + 0x0bcaa747;

					k3 *= c3;
					k3 = RotateLeft32(k3, 17);
					k3 *= c4;
					h3 ^= k3;

					h3 = RotateLeft32(h3, 15);
					h3 += h4;
					h3 = h3 * 5 + 0x96cd1c35;

					k4 *= c4;
					k4 = RotateLeft32(k4, 18);
					k4 *= c1;
					h4 ^= k4;

					h4 = RotateLeft32(h4, 13);
					h4 += h1;
					h4 = h4 * 5 + 0x32ac3b17;
				}

				var tail = p + blockCount * 16;

				k1 = 0;
				k2 = 0;
				k3 = 0;
				k4 = 0;

				switch (length & 15) {
					case 15:
						k4 ^= (uint)tail[14] << 16;
						goto case 14;
					case 14:
						k4 ^= (uint)tail[13] << 8;
						goto case 13;
					case 13:
						k4 ^= (uint)tail[12] << 0;
						k4 *= c4;
						k4 = RotateLeft32(k4, 18);
						k4 *= c1;
						h4 ^= k4;
						goto case 12;
					case 12:
						k3 ^= (uint)tail[11] << 24;
						goto case 11;
					case 11:
						k3 ^= (uint)tail[10] << 16;
						goto case 10;
					case 10:
						k3 ^= (uint)tail[9] << 8;
						goto case 9;
					case 9:
						k3 ^= (uint)tail[8] << 0;
						k3 *= c3;
						k3 = RotateLeft32(k3, 17);
						k3 *= c4;
						h3 ^= k3;
						goto case 8;
					case 8:
						k2 ^= (uint)tail[7] << 24;
						goto case 7;
					case 7:
						k2 ^= (uint)tail[6] << 16;
						goto case 6;
					case 6:
						k2 ^= (uint)tail[5] << 8;
						goto case 5;
					case 5:
						k2 ^= (uint)tail[4] << 0;
						k2 *= c2;
						k2 = RotateLeft32(k2, 16);
						k2 *= c3;
						h2 ^= k2;
						goto case 4;
					case 4:
						k1 ^= (uint)tail[3] << 24;
						goto case 3;
					case 3:
						k1 ^= (uint)tail[2] << 16;
						goto case 2;
					case 2:
						k1 ^= (uint)tail[1] << 8;
						goto case 1;
					case 1:
						k1 ^= (uint)tail[0] << 0;
						k1 *= c1;
						k1 = RotateLeft32(k1, 15);
						k1 *= c2;
						h1 ^= k1;
						break;
				};

				h1 ^= (uint)length;
				h2 ^= (uint)length;
				h3 ^= (uint)length;
				h4 ^= (uint)length;

				h1 += h2;
				h1 += h3;
				h1 += h4;

				h2 += h1;
				h3 += h1;
				h4 += h1;

				h1 = FMix32(h1);
				h2 = FMix32(h2);
				h3 = FMix32(h3);
				h4 = FMix32(h4);

				h1 += h2;
				h1 += h3;
				h1 += h4;

				h2 += h1;
				h3 += h1;
				h4 += h1;

				((uint*)hash)[0] = h1;
				((uint*)hash)[1] = h2;
				((uint*)hash)[2] = h3;
				((uint*)hash)[3] = h4;
			}
		}

		private static void ComputeHash_x64(IntPtr data, int length, uint seed, IntPtr hash)
		{
			unchecked {
				var p = (byte*)data;
				var blockCount = length / 16;

				var h1 = (ulong)seed;
				var h2 = (ulong)seed;

				const ulong c1 = 0x87c37b91114253d5;
				const ulong c2 = 0x4cf5ad432745937f;

				var blocks = (ulong*)p;

				ulong k1, k2;

				for (int i = 0; i < blockCount; i++) {
					k1 = *(blocks + i * 2 + 0);
					k2 = *(blocks + i * 2 + 1);

					k1 *= c1;
					k1 = RotateLeft64(k1, 31);
					k1 *= c2;
					h1 ^= k1;

					h1 = RotateLeft64(h1, 27);
					h1 += h2;
					h1 = h1 * 5 + 0x52dce729;

					k2 *= c2;
					k2 = RotateLeft64(k2, 33);
					k2 *= c1;
					h2 ^= k2;

					h2 = RotateLeft64(h2, 31);
					h2 += h1;
					h2 = h2 * 5 + 0x38495ab5;
				}

				var tail = p + blockCount * 16;

				k1 = 0;
				k2 = 0;

				switch (length & 15) {
					case 15:
						k2 ^= ((ulong)tail[14]) << 48;
						goto case 14;
					case 14:
						k2 ^= ((ulong)tail[13]) << 40;
						goto case 13;
					case 13:
						k2 ^= ((ulong)tail[12]) << 32;
						goto case 12;
					case 12:
						k2 ^= ((ulong)tail[11]) << 24;
						goto case 11;
					case 11:
						k2 ^= ((ulong)tail[10]) << 16;
						goto case 10;
					case 10:
						k2 ^= ((ulong)tail[9]) << 8;
						goto case 9;
					case 9:
						k2 ^= ((ulong)tail[8]) << 0;
						k2 *= c2;
						k2 = RotateLeft64(k2, 33);
						k2 *= c1;
						h2 ^= k2;
						goto case 8;
					case 8:
						k1 ^= ((ulong)tail[7]) << 56;
						goto case 7;
					case 7:
						k1 ^= ((ulong)tail[6]) << 48;
						goto case 6;
					case 6:
						k1 ^= ((ulong)tail[5]) << 40;
						goto case 5;
					case 5:
						k1 ^= ((ulong)tail[4]) << 32;
						goto case 4;
					case 4:
						k1 ^= ((ulong)tail[3]) << 24;
						goto case 3;
					case 3:
						k1 ^= ((ulong)tail[2]) << 16;
						goto case 2;
					case 2:
						k1 ^= ((ulong)tail[1]) << 8;
						goto case 1;
					case 1:
						k1 ^= ((ulong)tail[0]) << 0;
						k1 *= c1;
						k1 = RotateLeft64(k1, 31);
						k1 *= c2;
						h1 ^= k1;
						break;
				};

				h1 ^= (ulong)length;
				h2 ^= (ulong)length;

				h1 += h2;
				h2 += h1;

				h1 = FMix64(h1);
				h2 = FMix64(h2);

				h1 += h2;
				h2 += h1;

				((ulong*)hash)[0] = h1;
				((ulong*)hash)[1] = h2;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static uint RotateLeft32(uint x, byte r)
		{
			return (x << r) | (x >> (32 - r));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ulong RotateLeft64(ulong x, byte r)
		{
			return (x << r) | (x >> (64 - r));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static uint FMix32(uint h)
		{
			unchecked {
				h ^= h >> 16;
				h *= 0x85ebca6b;
				h ^= h >> 13;
				h *= 0xc2b2ae35;
				h ^= h >> 16;
				return h;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ulong FMix64(ulong h)
		{
			unchecked {
				h ^= h >> 33;
				h *= 0xff51afd7ed558ccd;
				h ^= h >> 33;
				h *= 0xc4ceb9fe1a85ec53;
				h ^= h >> 33;
				return h;
			}
		}
	}

	internal struct Hash128 : IEquatable<Hash128>
	{
		public ulong H1;
		public ulong H2;

		public override bool Equals(object obj)
		{
			return obj is Hash128 hash && Equals(hash);
		}

		public bool Equals(Hash128 hash)
		{
			return H1 == hash.H1 && H2 == hash.H2;
		}

		public override int GetHashCode()
		{
			return (H1 ^ H2).GetHashCode();
		}

		public static bool operator ==(Hash128 lhs, Hash128 rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(Hash128 lhs, Hash128 rhs)
		{
			return !lhs.Equals(rhs);
		}
	}
}
