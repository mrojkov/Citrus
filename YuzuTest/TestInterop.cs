using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Yuzu;
using Yuzu.Binary;
using Yuzu.Json;

namespace YuzuTest.Interop
{
	[TestClass]
	public class TestInterop
	{
		[TestMethod]
		public void TestJsonToBinarySimple()
		{
			var bs = new BinarySerializer();
			var bd = new BinaryDeserializer();
			var js = new JsonSerializer();
			js.JsonOptions.FieldSeparator = "";
			js.JsonOptions.Indent = "";
			js.JsonOptions.SaveRootClass = true;
			var jd = new JsonDeserializer();

			var data1 = "{\"class\":\"YuzuTest.Sample1, YuzuTest\",\"X\":98,\"Y\":\"\"}";
			var result1 = js.ToString(bd.FromBytes(bs.ToBytes(jd.FromString(data1))));
			Assert.AreEqual(data1, result1);
		}

		[TestMethod]
		public void TestBinaryToJsonSimple()
		{
			var bs = new BinarySerializer();
			var bd = new BinaryDeserializer();
			var js = new JsonSerializer();
			js.JsonOptions.SaveRootClass = true;
			var jd = new JsonDeserializer();

			var n = "YuzuTest.Sample1, YuzuTest".ToCharArray().Select(ch => (byte)ch).ToList();
			var data1 =
				new byte[] { 0x20, 0x01, 0x00, (byte)n.Count }.Concat(n).
				Concat(new byte[] { 0x02, 0x00, 0x01, (byte)'X',
					(byte)RoughType.Int, 0x01, (byte)'Y', (byte)RoughType.String, 0x01, 0x00,
					98, 0, 0, 0, 0x02, 0x00, 0, 0, 0, 0
				}).ToArray();
			var result1 = bs.ToBytes(jd.FromString(js.ToString(bd.FromBytes(data1))));
			CollectionAssert.AreEqual(data1, result1);
		}

		[TestMethod]
		public void TestJsonToBinaryUnknown()
		{
			var bs = new BinarySerializer();
			var bd = new BinaryDeserializer();
			var js = new JsonSerializer();
			js.JsonOptions.FieldSeparator = "";
			js.JsonOptions.Indent = "";
			js.JsonOptions.SaveRootClass = true;
			var jd = new JsonDeserializer();

			var data1 = "{\"class\":\"NewType\",\"X\":\"abc\"}";
			var result1 = js.ToString(bd.FromBytes(bs.ToBytes(jd.FromString(data1))));
			Assert.AreEqual(data1, result1);
		}

		[TestMethod]
		public void TestBinaryToJsonUnknown()
		{
			var bs = new BinarySerializer();
			var bd = new BinaryDeserializer();
			var js = new JsonSerializer();
			js.JsonOptions.SaveRootClass = true;
			var jd = new JsonDeserializer();

			var n = "NewType".ToCharArray().Select(ch => (byte)ch).ToList();
			var data1 =
				new byte[] { 0x20, 0x01, 0x00, (byte)n.Count }.Concat(n).
				Concat(new byte[] { 0x01, 0x00, 0x01, (byte)'X', (byte)RoughType.String, 0x01, 0x00,
					0x03, (byte)'a', (byte)'b', (byte)'c', 0, 0
				}).ToArray();
			var result1 = bs.ToBytes(jd.FromString(js.ToString(bd.FromBytes(data1))));
			CollectionAssert.AreEqual(data1, result1);
		}
	}
}