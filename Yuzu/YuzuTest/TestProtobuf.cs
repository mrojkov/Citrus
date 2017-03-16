using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Yuzu;
using Yuzu.Protobuf;

namespace YuzuTest.Protobuf
{
	[TestClass]
	public class TestProtobuf
	{
		private byte[] ProtoBufNetToBytes(object obj)
		{
			var ms = new MemoryStream();
			ProtoBuf.Serializer.Serialize(ms, obj);
			var result = ms.GetBuffer();
			Array.Resize(ref result, (int)ms.Length);
			return result;
		}

		[TestMethod]
		public void TestProtobufSimple()
		{
			var v1 = new Sample1 { X = 150, Y = "test" };
			var ps = new ProtobufSerializer();
			var result = ps.ToBytes(v1);
			CollectionAssert.AreEqual(new byte[] {
				0x08, 0x96, 0x01, 0x12, 0x04, (byte)'t', (byte)'e', (byte)'s', (byte)'t' }, result);
			CollectionAssert.AreEqual(result, ProtoBufNetToBytes(v1));
			var v2 = new Sample1();
			(new ProtobufDeserializer()).FromBytes(v2, result);
			Assert.AreEqual(v1.X, v2.X);
			Assert.AreEqual(v1.Y, v2.Y);
		}

		[TestMethod]
		public void TestProtobufSimpleProps()
		{
			var v1 = new Sample2 { X = 150, Y = "test" };
			var ps = new ProtobufSerializer();
			var result = ps.ToBytes(v1);
			CollectionAssert.AreEqual(new byte[] {
				0x08, 0x96, 0x01, 0x12, 0x04, (byte)'t', (byte)'e', (byte)'s', (byte)'t' }, result);
			var v2 = new Sample2();
			(new ProtobufDeserializer()).FromBytes(v2, result);
			Assert.AreEqual(v1.X, v2.X);
			Assert.AreEqual(v1.Y, v2.Y);
		}

		[TestMethod]
		public void TestProtobufFloat()
		{
			var v1 = new SampleFloat { F = 1e-20f, D = -3.1415E+100 };
			var ps = new ProtobufSerializer();
			var result = ps.ToBytes(v1);
			CollectionAssert.AreEqual(new byte[] {
				0x09, 0x00, 0x00, 0x00, 0x00, 0xA1, 0x9C, 0xC7, 0x3B,
				0x11, 0xCA, 0xDC, 0x09, 0x3E, 0xBE, 0xB9, 0xCC, 0xD4 }, result);
			var v2 = new SampleFloat();
			(new ProtobufDeserializer()).FromBytes(v2, result);
			Assert.AreEqual(v1.F, v2.F);
			Assert.AreEqual(v1.D, v2.D);
		}
	}
}
