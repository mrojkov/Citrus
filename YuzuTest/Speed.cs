using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;

using Yuzu;
using Yuzu.Json;

namespace YuzuTest
{

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
	public class TestSpeedPerson
	{
		private static SamplePerson person;
		private static MemoryStream jsonStream = new MemoryStream();
		private static MemoryStream protobufStream = new MemoryStream();

		[ClassInitialize]
		public static void Init(TestContext context) {
			SamplePerson.Counter = 0;
			Random rnd = new Random(20151125);
			person = new SamplePerson(rnd, 1);
			var js = new JsonSerializer();
			js.JsonOptions.Indent = "";
			js.JsonOptions.FieldSeparator = "";
			js.Options.TagMode = TagMode.Aliases;
			js.ToStream(person, jsonStream);
			ProtoBuf.Serializer.Serialize(protobufStream, person);
		}

		[TestMethod]
		public void TestProtobufWrite()
		{
			var ms = new MemoryStream();
			ProtoBuf.Serializer.Serialize(ms, person);
			Assert.AreEqual(protobufStream.Length, ms.Length);
		}

		[TestMethod]
		public void TestProtobuRead()
		{
			protobufStream.Position = 0;
			var p = ProtoBuf.Serializer.Deserialize<SamplePerson>(protobufStream);
			Assert.AreEqual(person.Name, p.Name);
		}

		[TestMethod]
		public void TestJsonWrite()
		{
			Assert.AreEqual(28076, SamplePerson.Counter);
			var js = new JsonSerializer();
			js.JsonOptions.Indent = "";
			js.JsonOptions.FieldSeparator = "";
			js.Options.TagMode = TagMode.Aliases;
			var ms = new MemoryStream();
			js.ToStream(person, ms);
			Assert.AreEqual(jsonStream.Length, ms.Length);
		}

		[TestMethod]
		public void TestJsonRead()
		{
			var jd = new JsonDeserializer();
			jd.Options.TagMode = TagMode.Aliases;
			SamplePerson p = new SamplePerson();
			jsonStream.Position = 0;
			jd.FromStream(p, jsonStream);
			Assert.AreEqual(person.Name, p.Name);
		}

		[TestMethod]
		public void TestJsonReadObject()
		{
			var jd = new JsonDeserializer();
			jd.Options.TagMode = TagMode.Aliases;
			jd.Options.AllowEmptyTypes = true;
			object p = new object();
			jsonStream.Position = 0;
			p = jd.FromStream(p, jsonStream);
			Assert.AreEqual(person.Name, ((Dictionary<string, object>)p)["1"]);
		}

		[TestMethod]
		public void TestJsonGenRead()
		{
			jsonStream.Position = 0;
			SamplePerson p = (SamplePerson)SamplePerson_JsonDeserializer.Instance.FromStream(jsonStream);
			Assert.AreEqual(person.Name, p.Name);
		}
	}

}
