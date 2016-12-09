using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ProtoBuf;

using Yuzu;
using Yuzu.Metadata;

namespace YuzuTest.Metadata
{
	[TestClass]
	public class TestMeta
	{
		[TestMethod]
		public void TestAttributes()
		{
			var opt1 = new CommonOptions {
				Meta = new MetaOptions { }
			};
			var opt2 = new CommonOptions {
				Meta = new MetaOptions {
					RequiredAttribute = typeof(ProtoMemberAttribute),
					OptionalAttribute = null,
					MemberAttribute = null,
					GetAlias = attr => (attr as ProtoMemberAttribute).Tag.ToString(),
				}
			};
			var m1 = Meta.Get(typeof(Sample1), opt1);
			var m2 = Meta.Get(typeof(Sample1), opt2);
			Assert.AreNotEqual(m1, m2);

			Assert.AreEqual(2, m1.Items.Count);
			Assert.AreEqual("X", m1.Items[0].Tag(opt1));
			Assert.AreEqual("Y", m1.Items[1].Tag(opt1));

			Assert.AreEqual(2, m2.Items.Count);
			Assert.AreEqual("1", m2.Items[0].Tag(opt2));
			Assert.AreEqual("2", m2.Items[1].Tag(opt2));
		}
	}
}
