using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;

using Yuzu;

namespace YuzuTest
{
	[YuzuCompact][ProtoContract]
	public class Color
	{
		[YuzuRequired][ProtoMember(1)]
		public byte R;

		[YuzuRequired][ProtoMember(2)]
		public byte G;

		[YuzuRequired][ProtoMember(3)]
		public byte B;
	}

	[ProtoContract]
	public class SamplePerson
	{
		public static int Counter = 0;

		[YuzuRequired][ProtoMember(1)]
		public string Name;

		[YuzuRequired][ProtoMember(2)]
		public DateTime Birth;

		[YuzuRequired][ProtoMember(3)]

		public List<SamplePerson> Children;

		[YuzuRequired][ProtoMember(4)]
		public Color EyeColor;

		public SamplePerson(Random rnd, int depth)
		{
			Counter++;
			StringBuilder sb = new StringBuilder();
			var len = rnd.Next(1, 40);
			for (int i = 0; i < len; ++i)
				sb.Append((char)rnd.Next((int)'a', (int)'z' + 1));
			Name = sb.ToString();
			Birth = new DateTime(1999, rnd.Next(10) + 1, 13);
			var childCount = rnd.Next(28 / depth);
			Children = new List<SamplePerson>();
			for (int i = 0; i < childCount; ++i)
				Children.Add(new SamplePerson(rnd, depth + 1));
			EyeColor = new Color { R = (byte)rnd.Next(256), G = (byte)rnd.Next(256), B = (byte)rnd.Next(256) };
		}
	}


	[TestClass]
	public class TestSpeed
	{
		[TestMethod]
		public void TestJsonLongListStr()
		{
			var list1 = new SampleList { E = new List<string>() };
			for (int i = 0; i < 100000; ++i)
				list1.E.Add(i.ToString());

			var js = new JsonSerializer();
			var result1 = js.ToString(list1);
			Assert.IsTrue(result1 != "");

			var list2 = new SampleList();
			var jd = new JsonDeserializer();
			jd.FromString(list2, result1);
			Assert.AreEqual(list1.E.Count, list2.E.Count);

			var jdg = new SampleList_JsonDeserializer();
			var list3 = (SampleList)jdg.FromString(result1);
			Assert.AreEqual(list1.E.Count, list3.E.Count);
		}

		[TestMethod]
		public void TestProtobufNetLongListStr()
		{
			var list1 = new SampleList { E = new List<string>() };
			for (int i = 0; i < 100000; ++i)
				list1.E.Add(i.ToString());

			var ms = new MemoryStream();
			ProtoBuf.Serializer.Serialize(ms, list1);
			Assert.IsTrue(ms.Length > 0);

			ms.Position = 0;
			var list2 = ProtoBuf.Serializer.Deserialize<SampleList>(ms);
			Assert.AreEqual(list1.E.Count, list2.E.Count);
		}

		[TestMethod]
		public void TestJsonLongArrayStr()
		{
			var list1 = new SampleArray { A = new string[100000] };
			for (int i = 0; i < list1.A.Length; ++i)
				list1.A[i] = i.ToString();

			var js = new JsonSerializer();
			js.JsonOptions.ArrayLengthPrefix = true;
			var result1 = js.ToString(list1);
			Assert.IsTrue(result1 != "");

			var list2 = new SampleArray();
			var jd = new JsonDeserializer();
			jd.JsonOptions.ArrayLengthPrefix = true;
			jd.FromString(list2, result1);
			Assert.AreEqual(list1.A.Length, list2.A.Length);

			var jdg = new SampleArray_JsonDeserializer();
			var list3 = (SampleArray)jdg.FromString(result1);
			Assert.AreEqual(list1.A.Length, list3.A.Length);
		}

		[TestMethod]
		public void TestJsonLongListInt()
		{
			var list1 = new SampleMatrix { M = new List<List<int>>() };
			for (int i = 0; i < 300; ++i) {
				list1.M.Add(new List<int>());
				for (int j = 0; j < 400; ++j)
					list1.M[i].Add(i * j);
			}

			var js = new JsonSerializer();
			var result1 = js.ToString(list1);
			Assert.IsTrue(result1 != "");

			var list2 = new SampleMatrix();
			var jd = new JsonDeserializer();
			jd.FromString(list2, result1);
			Assert.AreEqual(list1.M.Count, list2.M.Count);

			var jdg = new SampleMatrix_JsonDeserializer();
			var list3 = (SampleMatrix)jdg.FromString(result1);
			Assert.AreEqual(list1.M.Count, list3.M.Count);
		}

	}

	[TestClass]
	public class TestSpeedPerson {
		private static SamplePerson person;

		[ClassInitialize]
		public static void Init(TestContext context) {
			SamplePerson.Counter = 0;
			Random rnd = new Random(20151125);
			person = new SamplePerson(rnd, 1);
		}

		[TestMethod]
		public void TestProtobuf()
		{
			var ms = new MemoryStream();
			ProtoBuf.Serializer.Serialize(ms, person);
			Assert.IsTrue(ms.Length > 0);
		}

		[TestMethod]
		public void TestJson()
		{
			Assert.AreEqual(28076, SamplePerson.Counter);
			var js = new JsonSerializer();
			js.JsonOptions.Indent = "";
			js.JsonOptions.FieldSeparator = "";
			var ms = new MemoryStream();
			js.ToStream(person, ms);
			Assert.IsTrue(ms.Length > 0);
		}
	}

}
